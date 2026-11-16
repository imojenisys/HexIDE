#!/usr/bin/env python3
"""Manual mutation checks for #4068; run alone from the repository root.

Restores sources after each check. Logs go to artifacts/4068. This is an
extraction check, not a new source of VB6 measurements.
"""
import os
import re
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[2]
os.chdir(ROOT)
os.environ['PATH'] = os.pathsep.join(p for p in os.environ['PATH'].split(os.pathsep) if not p.startswith('/mnt/'))
os.environ['UseSharedCompilation'] = 'false'
os.environ['NUGET_HTTP_CACHE_PATH'] = str(ROOT / 'artifacts/nuget-http')
LOGS = ROOT / 'artifacts/4068'
LOGS.mkdir(parents=True, exist_ok=True)


def check(label, project, pattern, changes):
    originals = {Path(name): Path(name).read_bytes() for name in changes}
    try:
        for name, transform in changes.items():
            path = Path(name)
            before = path.read_text(encoding='utf-8-sig')
            after = transform(before)
            assert before != after, f'{label}: mutation did not apply to {name}'
            path.write_text(after)
        with (LOGS / f'{label}.log').open('w') as log:
            result = subprocess.run(['dotnet', 'test', project, '--', '--filter-method', pattern],
                                    stdout=log, stderr=subprocess.STDOUT)
        text = (LOGS / f'{label}.log').read_text()
        # A build error or a missing test is not a killed mutation.
        killed = result.returncode != 0 and re.search(r'Failed: [1-9]', text) is not None
        print(f'{label}: {"KILLED" if killed else "NOT PROVEN"}', flush=True)
        if not killed:
            raise RuntimeError(f'See {LOGS / (label + ".log")}')
    finally:
        for path, contents in originals.items():
            path.write_bytes(contents)


HOST = 'IDE/HexIDE/IDE/ProgramHost.cs'
STMT = 'IDE/HexIDE.Runtime/Interpreter/StatementExecutor.cs'
INTEGRATION = 'IDE/HexIDE.Integration.Tests/'
check('hold', INTEGRATION, '*A_held_window_stops_real_pointer_and_key_input_and_reports_the_press',
      {HOST: lambda s: s.replace('held = value;', 'held = false;').replace('content.IsHitTestVisible = !value;', 'content.IsHitTestVisible = true;')})
check('close-refusal', INTEGRATION, '*The_close_box_can_be_refused_but_the_program_can_close_without_asking',
      {HOST: lambda s: s.replace('if (Closing?.Invoke() != true)', 'if (true)')})
check('minimised-extent', INTEGRATION, '*A_minimised_extent_is_empty_even_with_a_pending_size_assignment',
      {HOST: lambda s: s.replace('if (state == WindowState.Minimized)', 'if (state == (WindowState)(-1))')})
check('withevents-root', INTEGRATION, '*A_WithEvents_variable_hears_a_single_view_forms_control',
      {STMT: lambda s: s.replace('FormRoot.Execution(evtListener.FormWindow)', '(evtListener.FormWindow as IModuleExecutionRoot)')})
check('unload-root', INTEGRATION, '*Unload_Me_finds_the_runtime_root_inside_the_single_view_window',
      {STMT: lambda s: s.replace('FormRoot.Execution(form.FormWindow)', '(form.FormWindow as IModuleExecutionRoot)')})

# Restore the unmodified #4066 engine and constructor callers, leaving the new guards in place.
BASE_FILES = ['IDE/HexIDE/IDE/InterpreterRunEngine.cs',
              'IDE/HexIDE.Tests/TestRunners.cs',
              'IDE/HexIDE.Tests/DebugAdapter/RunEngineSeamTests.cs']
base = {name: (lambda _, name=name: subprocess.check_output(['git', 'show', f'f7cef45d1f3ea932d23641a8478b2ebf1a2f2c9f:{name}']).decode('utf-8-sig'))
        for name in BASE_FILES}
check('base-host-guard', 'IDE/HexIDE.Tests/',
      '*Run_engine_shows_program_dialogs_and_gets_failure_texts_through_Core', base)
check('base-root-guard', 'IDE/HexIDE.Tests/', '*The_IDE_does_not_own_runtime_form_roots', base)
