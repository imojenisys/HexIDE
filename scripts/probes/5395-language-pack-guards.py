#!/usr/bin/env python3
"""Exercise the #5395 source guards on deliberate faults, restoring each file.

Build HexIDE.Tests first; run from any folder with dotnet on PATH. Do not run
concurrently with a test suite: these mutations temporarily break the tree.
"""
import json
import os
import re
from pathlib import Path
import subprocess
import sys
import tempfile

root = Path(sys.argv[1]).resolve() if len(sys.argv) > 1 else Path(__file__).resolve().parents[2]
provider = root / 'IDE/HexIDE.Runtime/Localization/Packs'
ide = root / 'IDE/HexIDE/Localization/Packs'
dll = root / 'IDE/HexIDE.Tests/bin/Debug/net11.0/HexIDE.Tests.dll'
os.environ['UseSharedCompilation'] = 'false'
os.environ['PATH'] = ':'.join(p for p in os.environ['PATH'].split(':') if not p.startswith('/mnt/'))
os.environ['XDG_RUNTIME_DIR'] = tempfile.mkdtemp(prefix='hexide-5395-')


methods = [
    'EveryShippedTranslationHasEveryCanonicalKey',
    'EveryCSharpGetStringLiteralKey_ExistsInEnglishPack',
    'Areas_never_overlap_and_the_provider_stays_inside_its_grant',
    'Every_provider_string_literal_is_owned_by_the_provider',
    'IDE_readers_of_provider_text_can_only_shrink',
    'Every_in_tree_area_is_guarded',
    'Standalone_reads_only_its_two_provider_keys',
]
clean = subprocess.run(['dotnet', str(dll)] + [arg for method in methods for arg in ['-method', '*' + method]],
                       cwd=root, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True)
if clean.returncode != 0 or not re.search(r'Total:\s+7,', clean.stdout):
    raise AssertionError('Clean tree must pass all seven guards:\n' + clean.stdout)
print('clean tree: seven guards passed')


def check(label, method, file, change):
    original = file.read_bytes() if file.exists() else None
    try:
        change(file)
        result = subprocess.run(['dotnet', str(dll), '-method', '*' + method], cwd=root,
                                stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True)
        if result.returncode == 0 or '[FAIL]' not in result.stdout:
            raise AssertionError(label + ' did not fail:\n' + result.stdout)
        print(label + ': refused')
    finally:
        if original is None:
            file.unlink(missing_ok=True)
        else:
            file.write_bytes(original)


def edit_key(file, key, value=None):
    data = json.loads(file.read_text(encoding='utf-8'))
    if value is None:
        del data['strings'][key]
    else:
        data['strings'][key] = value
    file.write_text(json.dumps(data), encoding='utf-8')


check('parity: provider German loses Caption', 'EveryShippedTranslationHasEveryCanonicalKey',
      provider / 'de.json', lambda f: edit_key(f, 'Str.PropDesc.Caption'))
check('coverage: provider English loses a GetString key', 'EveryCSharpGetStringLiteralKey_ExistsInEnglishPack',
      provider / 'en.json', lambda f: edit_key(f, 'Str.Vb6Toolchain.Vb6NotFound'))
check('overlap: shell duplicates a provider key', 'Areas_never_overlap_and_the_provider_stays_inside_its_grant',
      ide / 'en.json', lambda f: edit_key(f, 'Str.PropDesc.Caption', 'duplicate'))
check('grant: provider adds an ungranted key', 'Areas_never_overlap_and_the_provider_stays_inside_its_grant',
      provider / 'en.json', lambda f: edit_key(f, 'Str.Unowned.Probe', 'probe'))
check('provider ownership: literal has no provider definition', 'Every_provider_string_literal_is_owned_by_the_provider',
      root / 'IDE/HexIDE.Runtime/OwnershipProbe.cs',
      lambda f: f.write_text('class OwnershipProbe { string Key = "Str.Unowned.Probe"; }'))
check('shell ownership: a new reader', 'IDE_readers_of_provider_text_can_only_shrink',
      root / 'IDE/HexIDE/OwnershipProbe.cs',
      lambda f: f.write_text('class OwnershipProbe { string Key = "Str.PropDesc.Caption"; }'))
check('area discovery: a third production area', 'Every_in_tree_area_is_guarded',
      root / 'IDE/HexIDE.Core/Localization/Packs/en.json',
      lambda f: (f.parent.mkdir(parents=True, exist_ok=True), f.write_text('{"strings":{}}')))
# Remove only the empty directories this probe made.
for folder in [root / 'IDE/HexIDE.Core/Localization/Packs', root / 'IDE/HexIDE.Core/Localization']:
    if folder.exists() and not any(folder.iterdir()):
        folder.rmdir()
check('Standalone: an extra foreign key', 'Standalone_reads_only_its_two_provider_keys',
      root / 'IDE/HexIDE.Standalone/OwnershipProbe.cs',
      lambda f: f.write_text('class OwnershipProbe { string Key = "Str.Menu.File"; }'))
