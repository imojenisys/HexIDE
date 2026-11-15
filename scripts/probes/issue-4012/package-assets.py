#!/usr/bin/env python3
"""Summarise central package selections after a .NET 11 restore (#4012)."""
import json
from pathlib import Path
import re
import sys
import xml.etree.ElementTree as ET

root = Path(__file__).resolve().parents[3]
selected = {}
assets_files = [path for area in ("IDE", "LspServer")
                for path in (root / area).glob("*/obj/project.assets.json")]
assets_files.extend(Path(path) for path in sys.argv[1:])
for assets in assets_files:
    data = json.loads(assets.read_text())
    for target, packages in data["targets"].items():
        if target != "net11.0":
            continue
        for name, package in packages.items():
            if package["type"] != "package":
                continue
            paths = list(package.get("compile", {})) + list(package.get("runtime", {}))
            frameworks = {m[1] for path in paths if (m := re.match(r"(?:lib|ref)/([^/]+)/", path))}
            if not frameworks:
                files = data["libraries"][name].get("files", [])
                frameworks = {"analyzer/build assets" if any(
                    path.startswith(("analyzers/", "build/", "buildTransitive/")) for path in files
                ) else "no managed assets"}
            selected.setdefault(name.lower(), set()).update(frameworks)

print("| Central package | Pin | Selected assets for net11.0 |")
print("|---|---|---|")
for package in ET.parse(root / "Directory.Packages.props").iter("PackageVersion"):
    name, pin = package.attrib["Include"], package.attrib["Version"]
    matches = [(key.split("/", 1)[1], value) for key, value in selected.items()
               if key.split("/", 1)[0] == name.lower()]
    result = "; ".join(version + ": " + ", ".join(sorted(value)) for version, value in matches)
    print(f"| {name} | {pin} | {result or 'not restored in these projects'} |")
for name in ("Microsoft.Testing.Platform",):
    for key, value in selected.items():
        if key.split("/", 1)[0] == name.lower():
            print(f"| {name} (transitive) | {key.split('/', 1)[1]} | {', '.join(sorted(value))} |")
