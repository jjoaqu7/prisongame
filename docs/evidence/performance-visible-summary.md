# Visible-window profiling and frame-limit mitigation

September 25, 2026. PERF-02 follows the [offscreen baseline](performance-summary.md).

## Finding

The uncapped visible 1080p player reproduced intermittent hitches while rendering the normal HUD and settings menu. Of 16 unique CPU timing samples over 33.33 ms, 15 spent more than 75% of their time waiting for presentation. In those long samples, valid GPU rendering times were approximately 1.1–1.5 ms. This points to presentation waiting under the uncapped workload, rather than expensive GPU rendering, as the dominant observed stall. It does not identify a particular driver or Windows scheduling defect, nor prove that every earlier offscreen hitch has the same cause.

Unity defines `cpuMainThreadPresentWaitTime` as time waiting for Present on the main thread; its timing counters distinguish active CPU work, render-thread work and GPU work. [Unity timing-counter reference](https://docs.unity.com/en-us/engine/6000.7/manual/analysis/graphics-performance-profiling/profile-rendering/frame-timing-manager/counter-reference).

The uncapped run measured 16 elapsed frames above 33.33 ms, including two above 50 ms; the worst was 60.12 ms during save/load. Measured `GC.Collect` peaked at 3.08 ms. Not every stall was presentation waiting: one long CPU sample was dominated by main-thread work, and saving/loading includes synchronous file and initialization work. No allocation or game-rule rewrite was justified by these results.

## Controlled comparison

Same PC, scene, resolution, graphics API, eight-second warmup and 76-second measured movement/action/save-load workload as PERF-01. Runs were sequential. Windows 11, i9-14900HX, RTX 4070 Laptop GPU, Direct3D 12, Mono Development player; Editor and other desktop applications remained open. The game window could be partially occluded. This is not an isolated-machine or exclusive-fullscreen benchmark.

| Run | Measured frames | Frames >33.33 ms | Frames >50 ms | Worst frame | GPU P95 |
| --- | ---: | ---: | ---: | ---: | ---: |
| Visible uncapped | 21,218 | 16 | 2 | 60.12 ms | 1.80 ms |
| Visible temporary 60 FPS limit | 4,554 | 0 | 0 | 20.55 ms | 2.50 ms |
| Rebuilt sample, actual default 60 | 4,544 | 1 | 1 | 225.89 ms | 2.62 ms |
| Rebuilt sample, unchanged repeat | 4,556 | 1 | 0 | 35.63 ms | 2.45 ms |

The 60 FPS run averaged 59.92–59.98 FPS across phases. Phase P99 frame times were 16.91–17.11 ms. Its longest frame was a save; save calls peaked at 18.62 ms and load calls at 6.64 ms. All coverage checks passed: approximately 121 m walking, 88 footsteps, ten sales, eight saves and seven loads. No runtime errors were recorded. The lower frame count is intentional; the comparison covers the same elapsed duration, not the same number of frames. A strict count above 16.6667 ms is not a failure criterion for a 60 FPS timer because tiny scheduling differences cross that threshold.

The comparison supports limiting the frame rate as a mitigation on this machine. It does not prove a universal fix for all hardware. GPU times rising slightly at the cap do not outweigh the much more consistent frame intervals; clock/power-state changes were not measured.

The first final-sample run confirmed an applied and preferred limit of 60 without a command-line override, and frame-limit selection/fallback checks passed. All scenario coverage passed again. It also recorded one distinct 225.89 ms walking interval: the delayed CPU sample reported 225.89 ms main-thread time, while the nearby script/UI/GC markers did not account for it. There was no runtime error. Its cause is unresolved, and it is retained rather than removed as an outlier. Recurring presentation-wait stalls did not recur; the rest of that run's elapsed frames were at most 19.56 ms. This is a mitigation, not a hitch-free certification.

The unchanged rebuilt sample was repeated because of that pause. The 225.89 ms pause did not recur. Walking, activity and post-load walking maxima were 17.09, 17.61 and 17.56 ms. One 35.63 ms interval occurred during the paused settings/save-load phase, tagged ordinary rather than a direct save/load call; a delayed CPU sample was dominated by waiting. Thus the limit substantially reduces the recurring stall pattern but does not eliminate every timing outlier. Phase averages were 59.94–59.98 FPS. Coverage and frame-control checks passed again, with zero runtime errors. A longer controlled session should investigate the isolated main-thread pause if it recurs; no specific game-code cause has been established.

## Implementation

`FirstPersonController` owns the local display preference, separate from prison clock/game rules. The Windows settings menu offers 60 FPS, 120 FPS and Unlimited. A missing or invalid preference uses 60; choices persist in `PrisonGame.FrameLimit`. The prototype default is a provisional technical choice, not a user-approved minimum-spec or final performance requirement. Existing sensitivity/audio preferences and game saves are preserved. The Editor's frame limit is not changed.

The diagnostic harness now supports a normal visible window, records CPU/GPU FrameTiming timestamps and six profiler markers, and captures only the game's own rendered buffer during warmup. Screenshot encoding is outside measured phases. It verifies frame-limit selection/fallback without persisting test choices. The ordinary build does not start this diagnostic unless explicitly given `-sample-performance`.

## Evidence and reproduction

- [Uncapped report](performance-visible-uncapped/report.json), [frames](performance-visible-uncapped/frames.csv), [analysis](performance-visible-uncapped/analysis.json).
- [Temporary 60 FPS report](performance-visible-60/report.json), [frames](performance-visible-60/frames.csv), [analysis](performance-visible-60/analysis.json), [actual rendered HUD](performance-visible-60/window.png).
- [Rebuilt sample report](performance-sample-60/report.json), [frames](performance-sample-60/frames.csv), [analysis](performance-sample-60/analysis.json), [actual settings menu](performance-sample-60/settings.png). The 1080p settings capture was inspected: frame choices, sensitivity/audio controls, save/load, Resume and Quit fit without overlap.
- [Unchanged sample repeat report](performance-sample-60-repeat/report.json), [frames](performance-sample-60-repeat/frames.csv), [analysis](performance-sample-60-repeat/analysis.json).
- Each run folder retains its player log, available profiler marker names and external Windows memory samples. Phase memory snapshots and diagnostic overhead are retained, but these short runs do not establish long-session leak freedom.
- Build Save01_Progress as Windows x64 Development with DetailedBuildReport. `tools/test-windows-performance.ps1 -Width 1920 -Height 1080 -Visible` explicitly overrides the limit to unlimited; add `-FrameRate 60` for the controlled cap. Use `-BuildDirectory WindowsSample -Visible -FrameRate 60 -UseSavedFrameLimit` to test the rebuilt normal sample without a limit override when its saved/default setting is 60.
- Run `python tools/analyze-windows-performance.py <evidence-folder>` to regenerate analysis. FrameTiming results arrive later than the elapsed-frame row: the analysis deduplicates by timestamp and treats them as a separate dataset, rather than attributing a same-row GPU time to that elapsed frame. Recorder marker samples and FrameTiming counters have different synchronization boundaries and must not be subtracted as if perfectly aligned.

The raw `COMPLETE` status means scenario coverage passed, not that performance was hitch-free. Functional approval, subjective sound approval and performance evidence remain separate.

The final `WindowsSample/PrisonGame.exe` build succeeded with zero errors and the known missing Pipeline runtime-config warning. The existing standalone regression suite then passed [16 write-process checks](performance-sample-write.txt) and [21 read-process checks](performance-sample-read.txt), covering actual E interactions, deadlines/inspection, suspicion, cross-process persistence, corrupt-save rejection, shelf purchase and nonzero audio output. The test files are isolated; user saves and preference values were not replaced. Frame-limit choices/fallback were exercised without persistence; cross-process preference-button clicks were not automated. 120 FPS is an available choice but has not received this performance comparison.
