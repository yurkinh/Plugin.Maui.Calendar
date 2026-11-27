# Calendar benchmark

The sample app's **Benchmark** page (Samples › Performance › Benchmark) measures the calendar itself, so
performance changes in the library can be compared before and after. It creates calendars in code with fixed input
(May 2025, `en-US`, a fixed "today") and measures, on the UI thread:

| Value | What it is |
| --- | --- |
| `coldRenderMs` | The first calendar of the scenario in the process: from creating it until it is laid out and drawn (two display frames after its layout). Includes loading and compiling the code. |
| `warmRenderMs` | The same for later calendars (median of 10), after every scenario has been rendered 5 times so the code is fully compiled. |
| `renderKB` | Bytes allocated on the UI thread for one warm render (median). |
| `views` | Elements in the calendar's visual tree. |
| `monthChangeSyncMs` | The `ShownDate` setter moving to the next month (median of 12, after 3 unrecorded changes). |
| `monthChangeMs` | The same until the change is laid out and drawn. It includes waiting for two display frames, so it is never below ~17 ms at 60 Hz; compare it between builds, not as an absolute. |
| `monthChangeKB` | Bytes allocated on the UI thread for one month change. |
| `startupMs` | Android only: process start until the first page appeared. |
| `androidLaunchMs` | Android only: `am start -W` TotalTime, Android's own launch time. |

Scenarios: `Default` (no properties set besides the fixed input) and `Events` (event dots on 8 days).

## Running it

Use a Release build; Debug numbers are several times higher. Close other heavy work while it runs.

```bash
cd samples/SampleApp
ANDROID_SERIAL=emulator-5554 ./benchmark.sh android 5   # the running emulator
SIMULATOR_UDID=<udid> ./benchmark.sh ios 5              # a booted simulator
```

The script builds the app, starts it 5 times in benchmark mode (Android: intent extra `calbench`; iOS: environment
variable `CALBENCH=1`), prints one line per run and scenario and the file that holds them all. `NO_BUILD=1` reuses
the last build. Compare the medians of the runs; a single run varies by about ±10 %.

## Baseline

.NET 11 / MAUI 11.0.10, library at `main` after the .NET 11 update (PR #291), before the performance work. Apple M4
Pro, medians of 5 launches.

| | Android (Pixel 9 emulator, API 37) | | iOS (iPhone 17 simulator, iOS 26.5) | |
| --- | --- | --- | --- | --- |
| | Default | Events | Default | Events |
| coldRenderMs | 98 | 73 | 218 | 203 |
| warmRenderMs | 63.6 | 55.5 | 190.8 | 199.2 |
| renderKB | 8159 | 8289 | 7963 | 8197 |
| views | 365 | 377 | 365 | 377 |
| monthChangeSyncMs | 0.83 | 0.77 | 6.40 | 7.45 |
| monthChangeMs | 24.6 | 24.2 | 38.3 | 38.8 |
| monthChangeKB | 167 | 165 | 207 | 207 |
| startupMs | 314 | | – | |
| androidLaunchMs | 396 | | – | |

Simulator and emulator numbers show relative changes; they are not what a phone measures.

## After phases 1–3

Shared built-in day templates (phase 2) and the sample's XAML expressions (phase 3), branch `perf/net11-performance`.
Android: medians of an interleaved A/B on the same warmed-up emulator (5 launches of the baseline build alternating
with 5 of this one), which removes the drift between separate sessions; the baseline build measured 105.3 / 70.8 ms
cold and 62.7 / 54.8 ms warm in that run. iOS: medians of 5 launches.

| | Android (Pixel 9 emulator, API 37) | | iOS (iPhone 17 simulator, iOS 26.5) | |
| --- | --- | --- | --- | --- |
| | Default | Events | Default | Events |
| coldRenderMs | 105.0 | 69.8 | 202 | 202 |
| warmRenderMs | 63.1 | 55.4 | 188.4 | 192.7 |
| renderKB | 7775 (−4.6 %) | 7869 (−5.0 %) | 7663 (−3.8 %) | 7888 (−3.8 %) |
| views | 365 | 377 | 365 | 377 |
| monthChangeSyncMs | 0.80 | 0.95 | 7.63 | 7.27 |
| monthChangeMs | 24.8 | 24.9 | 38.0 | 40.2 |
| monthChangeKB | 166 | 165 | 207 | 207 |
| startupMs | 324 | | – | |
| androidLaunchMs | 404 | | – | |

Result: about 300–420 KB (4–5 %) less allocated per calendar render on both platforms, the same in every run. The
times are within the run-to-run variation (±5–10 %): the per-cell objects that are gone were cheap to create, and the
render time is spent in the 365 views themselves.
