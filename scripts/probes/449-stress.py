#!/usr/bin/env python3
"""Run #449's two original tests under CPU contention; retain every rumdl exit.

Build HexIDE.Tests first. Linux only. Outputs are disposable; copy the findings
into 449-investigation.md. No test is retried: every iteration is counted.
"""
import argparse
import collections
import json
import os
from pathlib import Path
import shlex
import subprocess
import sys
import time
import xml.etree.ElementTree as ET

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--iterations", type=int, default=200)
args = parser.parse_args()
root = Path(__file__).resolve().parents[2]
os.chdir(root)
os.environ["PATH"] = ":".join(p for p in os.environ["PATH"].split(":") if not p.startswith("/mnt/"))
out = root / "artifacts/449-stress"
out.mkdir(parents=True, exist_ok=True)
server = root / "artifacts/foreign-lsp/markdown/0.2.79/linux-x64/rumdl"
if not server.is_file():
    sys.exit("Run a foreign Markdown test first to acquire the pinned rumdl.")
exits = out / "server-exits.txt"
exits.write_text("")
wrapper = out / "rumdl-observed"
trace = out / "launches.txt"
trace.write_text("")
wrapper.write_text('#!/bin/sh\nprintf "start %s\\n" "$$" >> ' + shlex.quote(str(trace)) + '\n'
                   + shlex.quote(str(server)) + ' "$@"\n'
                   + 'code=$?\nprintf "exit %s %s\\n" "$$" "$code" >> ' + shlex.quote(str(trace))
                   + '\nprintf "%s\\n" "$code" >> ' + shlex.quote(str(exits))
                   + '\nexit "$code"\n')
wrapper.chmod(0o755)
env = dict(os.environ, HEXIDE_REQUIRE_FOREIGN_LSP="1", HEXIDE_FOREIGN_LSP_DOWNLOAD="0",
           HEXIDE_MARKDOWN_LSP=str(wrapper))
# Contend on two CPUs rather than monopolising a shared WSL host.
cpus = sorted(os.sched_getaffinity(0))[:2]
os.sched_setaffinity(0, cpus)
workers = []
results = []
started = time.monotonic()
try:
    for _ in cpus:
        workers.append(subprocess.Popen([sys.executable, "-c", "while True: pass"]))
    for iteration in range(1, args.iterations + 1):
        with (out / f"{iteration:03}.log").open("w") as log:
            result = subprocess.run([str(root / "IDE/HexIDE.Tests/bin/Debug/net10.0/HexIDE.Tests"),
                                     "-method", "*AServerAttachedOnlyByAConfigurationFileAnswersForReal",
                                     "-method", "*AMarkdownServerAnswersAboutADocumentWithNoFile",
                                     "-failSkips", "-xml", str(out / f"{iteration:03}.xml")],
                                    env=env, stdout=log, stderr=subprocess.STDOUT, timeout=240)
        report = ET.parse(out / f"{iteration:03}.xml").getroot().find("assembly")
        if report is None or report.get("total") != "2" or report.get("skipped") != "0":
            raise RuntimeError(f"Iteration {iteration} did not run both tests without skips")
        results.append(result.returncode)
        print(f"iteration {iteration}: runner exit {result.returncode}", flush=True)
finally:
    for worker in workers:
        worker.terminate()
    for worker in workers:
        worker.wait()
    summary = dict(iterations=len(results), runner_exits=dict(collections.Counter(results)),
                   server_exits=dict(collections.Counter(exits.read_text().splitlines())),
                   cpus=cpus, seconds=round(time.monotonic() - started, 1))
    (out / "summary.json").write_text(json.dumps(summary, indent=2) + "\n")
    print(json.dumps(summary), flush=True)
server_codes = exits.read_text().splitlines()
sys.exit(1 if any(results) or len(server_codes) != 2 * args.iterations
         or any(code != "0" for code in server_codes) else 0)
