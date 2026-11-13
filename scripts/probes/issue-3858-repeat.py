#!/usr/bin/env python3
"""Repeat the End-notice theory in independent hosts alongside a full HexIDE.Tests run.

Build first, then run from the repository root:
  python3 scripts/probes/issue-3858-repeat.py artifacts/issue-3858-before 200
Each iteration runs both theory rows; four hosts provide load without sharing Avalonia state.
Add --include-startup to also repeat the deterministic startup cases, using eight hosts.
"""
import concurrent.futures
import os
from pathlib import Path
import subprocess
import sys
import xml.etree.ElementTree as ET

destination = Path(sys.argv[1]).resolve()
destination.mkdir(parents=True, exist_ok=True)
iterations = int(sys.argv[2])
include_startup = "--include-startup" in sys.argv[3:]
expected = "4" if include_startup else "2"
method = "*localized_Immediate_notice" if include_startup else "*End_of_unresponsive_adapter_shows_one_localized_Immediate_notice"
environment = dict(os.environ, HEXIDE_FOREIGN_LSP_DOWNLOAD="0")
environment["PATH"] = ":".join(p for p in environment["PATH"].split(":") if not p.startswith("/mnt/"))


def run(index):
    report = destination / f"{index}.xml"
    command = ["dotnet", "IDE/HexIDE.Tests/bin/Debug/net10.0/HexIDE.Tests.dll",
               "-method", method,
               "-xml", str(report), "-noColor"]
    with (destination / f"{index}.log").open("w") as output:
        result = subprocess.run(command, env=environment, stdout=output, stderr=subprocess.STDOUT)
    assembly = ET.parse(report).getroot().find("assembly") if report.exists() else None
    good = result.returncode == 0 and assembly is not None and all(
        assembly.get(key) == value for key, value in
        [("total", expected), ("passed", expected), ("failed", "0"), ("skipped", "0"), ("errors", "0")])
    return index, good


failed = []
with concurrent.futures.ThreadPoolExecutor(max_workers=8 if include_startup else 4) as pool:
    for completed, (index, good) in enumerate(pool.map(run, range(1, iterations + 1)), 1):
        if not good:
            failed.append(index)
        print(f"{completed}/{iterations} theory runs; failures: {failed}", flush=True)
sys.exit(bool(failed))
