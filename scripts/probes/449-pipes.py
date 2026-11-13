#!/usr/bin/env python3
"""Distinguish an undrained stderr pipe from a closed one on Linux (#449)."""
import subprocess

command = ["/bin/sh", "-c", "head -c 33554432 /dev/zero >&2"]
with subprocess.Popen(command, stdout=subprocess.PIPE, stderr=subprocess.PIPE) as child:
    try:
        child.wait(timeout=1)
        raise RuntimeError("The burst unexpectedly fit in an undrained pipe")
    except subprocess.TimeoutExpired:
        print("Undrained stderr: still alive after 1 s")
    _, error = child.communicate(timeout=10)
    assert len(error) == 33554432 and child.returncode == 0
    print(f"After draining: {len(error)} bytes, exit {child.returncode}")

with subprocess.Popen(command, stdout=subprocess.PIPE, stderr=subprocess.PIPE) as child:
    child.stderr.close()
    code = child.wait(timeout=10)
    assert code == 141
    print(f"Closed stderr read end: shell exit {code}")
