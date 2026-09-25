# Windows performance baseline — September 25, 2026

The offscreen world/gameplay benchmark completed at 720p and twice at 1080p without runtime errors. Typical frame times were low on this PC, but both 1080p runs had intermittent hitches, up to 81.37 ms. Their cause remains unresolved. This is a diagnostic baseline, not approval of visible-window performance or a minimum-spec claim.

## Method and reproduction

- Unity 6000.3.24f1, Windows x64 Mono Development, Direct3D 12; i9-14900HX, RTX 4070 Laptop GPU, approximately 32 GB RAM / 8 GB VRAM. Editor remained open. Existing uncapped settings: vSync 0, targetFrameRate -1. The 16.67 ms reference is a diagnostic 60 FPS budget, not a user-approved requirement.
- Build Save01_Progress with Unity MCP `build`: outputPath `Builds/WindowsPerf/PrisonGame.exe`, target `StandaloneWindows64`, scenes `["Assets/Scenes/Save01_Progress.unity"]`, options `["Development", "DetailedBuildReport"]`, confirm true. Poll `build_status`. Both diagnostic builds succeeded, with only the known missing RuntimePipelineConfig warning.
- Run `tools/test-windows-performance.ps1 -Width 1280 -Height 720`, then `-Width 1920 -Height 1080` with normal Windows file access, sequentially. The runner uses an isolated save beside the diagnostic build. Ordinary WindowsSample, user save and persisted preferences are preserved.
- `SamplePerformanceCheck` is opt-in via `-sample-performance`, excluded from non-Development players. Eight seconds warmup, twenty walking, twenty repeated crafting/sales, sixteen alternating paused saves/loads, twenty post-load walking. Real controller input and collisions run; activity methods are called directly, bypassing interaction distance. Inspection time is advanced to allow repeat actions. This tests workload, not natural play pacing.
- Explicit `Camera.Render()` every LateUpdate renders the world into a full-resolution texture even though the window is hidden. **HUD/menu drawing, display presentation and input latency are excluded.** Screenshots confirm nonblank world rendering at both resolutions. GPU timing returned no valid samples; CPU timings may lag, so the table uses elapsed frame intervals. No separate GPU-cost conclusion is supported.
- Transition/setup frames are tagged 3 and excluded from summary percentiles, retained in CSV. Operation tags: 0 ordinary, 1 save, 2 load, 3 setup, 4 action. Preallocated sample buffers and input/render helpers add diagnostic overhead. No deep profiler was enabled. End-of-run JSON/CSV/image output is outside frame sampling.
- The [first attempt](performance-invalid-hidden/README.md) failed: no rendering and a route crossing a bench. Its timings are invalid and retained separately. Corrected runs require movement, footsteps, sales, saves/loads and positive draw counts.

## Measurements

All times below are milliseconds. P95 means 95% of measured frames completed within that time.

| Resolution / phase | Mean | P95 | P99 | Maximum | Frames >33.33 ms |
| --- | ---: | ---: | ---: | ---: | ---: |
| 720p walking | 2.24 | 3.37 | 3.95 | 14.67 | 0 |
| 720p activity | 2.27 | 2.97 | 3.32 | 17.17 | 0 |
| 720p save/load | 2.29 | 2.99 | 3.37 | 23.53 | 0 |
| 720p post-load walking | 2.55 | 3.72 | 4.41 | 13.58 | 0 |
| 1080p walking | 2.71 | 4.01 | 10.62 | 38.70 | 1 |
| 1080p activity | 2.79 | 3.53 | 14.65 | 58.65 | 5 |
| 1080p save/load | 2.67 | 3.51 | 11.08 | 43.03 | 1 |
| 1080p post-load walking | 2.89 | 4.24 | 8.98 | 57.56 | 5 |
| 1080p repeat walking | 2.71 | 4.01 | 12.51 | 33.41 | 1 |
| 1080p repeat activity | 2.81 | 3.44 | 15.43 | 81.37 | 7 |
| 1080p repeat save/load | 2.64 | 3.39 | 12.05 | 43.39 | 1 |
| 1080p repeat post-load walking | 2.91 | 4.35 | 9.78 | 50.91 | 4 |

Each run covered approximately 120.8 metres, 48 route corners, 88 footsteps, eight successful saves and seven successful loads. Sales were 11 at 720p and 10 at 1080p; the timed workload can finish at a different assembly stage. Average draw calls ranged from 234 to 311 across phases. No error/exception/assert events were recorded.

Save calls took 4.95–19.52 ms at 720p and 4.31–22.29 ms at 1080p. First loads took 7.77 / 7.30 ms; subsequent loads were 0.55–1.01 ms. The largest 720p frame coincided with a save. The twelve first-run 1080p frames above 33.33 ms were tagged ordinary, not direct save/load/action calls; their allocation samples were 0–112 bytes. This does not establish their cause or rule out garbage collection, driver stalls or other processes.

Unity allocated memory rose 111.9→119.3 MiB at 720p and 121.1→129.0 MiB at 1080p between warmup and end. Managed memory remained approximately 8.7–9.5 MiB at phase boundaries, with collections observed. Windows external samples peaked at 743 / 1022 MiB working set and 1112 / 1490 MiB private bytes respectively; these process peaks include startup and report/image generation. Mono's process working-set field returned zero and is unavailable, not zero consumption. Eighty-four seconds is insufficient to establish leak-free long sessions.

Both runs logged 73 notices that URP reduced punctual-light shadow resolution to fit the 2048 atlas as the view changed. This is a shadow-quality observation; no atlas-size or art change was made without a measured need. No gameplay optimization was justified by typical frame times alone.

Raw evidence: [720p report](performance-720p/report.json), [720p frames](performance-720p/frames.csv), [720p process memory](performance-720p/process-memory.json), [1080p report](performance-1080p/report.json), [1080p frames](performance-1080p/frames.csv), [1080p process memory](performance-1080p/process-memory.json). Each folder also retains its player log and inspected render.

The unchanged [1080p repeat report](performance-1080p-repeat/report.json) and [frames](performance-1080p-repeat/frames.csv) confirm hitches recur under this diagnostic setup: 13 frames above 33.33 ms and four above 50 ms. Coverage again passed, with 11 sales, 88 footsteps, eight saves and seven loads. Saves peaked at 19.16 ms and loads at 6.72 ms, below the worst frame. This does not isolate a game-code, graphics-driver, background-process or offscreen-rendering cause. Native allocated memory ended at 123.4 MiB and managed memory at 9.0 MiB. No speculative optimization or change to lighting, gameplay, graphics API or frame cap was made.

## Remaining verification

A visible-window capture including HUD/menu and valid GPU timing is the next technical step (**High**, because the intermittent stall needs attribution before a fix). Compare an ordinary frame budget against this uncapped offscreen setup, collect CPU/GPU profiler evidence at the hitch, and test a specific correction only after locating the cause. A longer session and another/lower-spec PC remain untested. Specific sound quality still awaits the user's listening judgment; an uncoached new-player test remains part of milestone 6.
