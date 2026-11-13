#!/usr/bin/env python3
"""Run the #778 diagnostic patch's two controls under the same bounded CPU load."""
import multiprocessing
import os
from pathlib import Path
import subprocess
import time


def burn():
    while True:
        sum(i * i for i in range(10000))


if __name__ == "__main__":
    root = Path(__file__).resolve().parents[3]
    output = root / "artifacts" / "778"
    output.mkdir(parents=True, exist_ok=True)
    env = os.environ.copy()
    env["PATH"] = os.pathsep.join(p for p in env["PATH"].split(os.pathsep) if not p.startswith("/mnt/"))
    # Build the diagnostic patch before starting the load, so both modes run against the same assembly.
    subprocess.run(["dotnet", "build", "IDE/HexIDE.Tests/"], cwd=root, env=env, check=True)
    context = multiprocessing.get_context("fork" if os.name == "posix" else "spawn")
    workers = [context.Process(target=burn) for _ in range(48)]
    try:
        for worker in workers:
            worker.start()
        for mode in ("pooled-load", "no-pool-load"):
            run_env = env.copy()
            run_env.pop("HEXIDE_778_NO_POOL", None)
            run_env["HEXIDE_778_TRACE"] = str(output / (mode + ".events"))
            Path(run_env["HEXIDE_778_TRACE"]).unlink(missing_ok=True)
            if mode == "no-pool-load":
                run_env["HEXIDE_778_NO_POOL"] = "1"
            started = time.monotonic()
            with (output / (mode + ".log")).open("w") as log:
                result = subprocess.run(
                    ["dotnet", "test", "IDE/HexIDE.Tests/", "--no-build", "--no-restore", "--",
                     "--filter-method", "*A_client_attached_before_a_restart_is_answered_after_it"],
                    cwd=root, env=run_env, stdout=log, stderr=subprocess.STDOUT, timeout=600)
            print(f"{mode}: exit {result.returncode}, {time.monotonic() - started:.1f}s", flush=True)
    finally:
        for worker in workers:
            if worker.pid is not None:
                worker.terminate()
                worker.join()
