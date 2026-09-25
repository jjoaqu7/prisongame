# Invalid first performance attempt

September 25, 2026. This report is retained as failed diagnostic evidence, **not a performance result**. Assigning a camera render texture did not force rendering in the hidden Windows player: draw calls were zero. The route also crossed a bench, producing only 1.4 metres of movement and no footsteps. The harness rejected insufficient scenario coverage.

The corrected harness explicitly calls `Camera.Render()` each LateUpdate, uses an unobstructed route, and requires positive measured draw calls in every phase as well as movement, audio, sales and save/load coverage. Corrected results are in the sibling `performance-720p` and `performance-1080p` folders. Their offscreen measurement still excludes HUD/menu drawing and display presentation.
