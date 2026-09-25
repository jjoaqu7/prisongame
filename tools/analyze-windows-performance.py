"""Summarize diagnostic CSVs; deduplicate delayed FrameTiming samples by timestamp."""
import csv
import json
import math
import sys
from pathlib import Path


def percentile(values, fraction):
    values = sorted(values)
    return values[max(0, math.ceil(len(values) * fraction) - 1)] if values else None


for argument in sys.argv[1:]:
    folder = Path(argument)
    report = json.loads((folder / "report.json").read_text(encoding="utf-8-sig"))
    with (folder / "frames.csv").open() as source:
        rows = [r for r in csv.DictReader(source) if r["previous_operation"] != "3"]
    # FrameTimingManager results arrive later than the elapsed-frame/recorder row.
    # Analyze them as their own dataset, never as same-row GPU attribution.
    timings = list({r["timing_timestamp"]: r for r in rows
                    if r.get("timing_timestamp", "0") != "0"}.values())
    slow = [r for r in timings if float(r["cpu_ms"]) > 1000 / 30]
    gpu = [float(r["gpu_ms"]) for r in timings if float(r["gpu_ms"]) > 0]
    result = {
        "folder": str(folder), "status": report["status"],
        "frameCount": len(rows),
        "framesOver33ms": sum(float(r["frame_ms"]) > 1000 / 30 for r in rows),
        "framesOver50ms": sum(float(r["frame_ms"]) > 50 for r in rows),
        "maximumFrameMs": max(float(r["frame_ms"]) for r in rows),
        "uniqueTimingSamples": len(timings), "validGpuSamples": len(gpu),
        "gpuP95Ms": percentile(gpu, .95), "gpuP99Ms": percentile(gpu, .99),
        "slowCpuTimingSamples": len(slow),
        "slowCpuSamplesOver75PercentPresentWait": sum(
            float(r["timing_present_wait_ms"]) > .75 * float(r["cpu_ms"]) for r in slow),
        "maximumMarkerMs": {name: max(float(r[name]) for r in rows) / 1e6
                            for name in ["loop_ns", "scripts_ns", "gui_ns", "gc_ns", "present_ns", "render_wait_ns"]
                            if rows and name in rows[0]},
    }
    (folder / "analysis.json").write_text(json.dumps(result, indent=2) + "\n")
    print(json.dumps(result, indent=2))
