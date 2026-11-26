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

<!-- TODO: fill in the medians of 5 launches on the same devices as the baseline (shared day templates, sample
     expressions). Same columns as the baseline table. -->

| | Android (Pixel 9 emulator, API 37) | | iOS (iPhone 17 simulator, iOS 26.5) | |
| --- | --- | --- | --- | --- |
| | Default | Events | Default | Events |
| coldRenderMs | TODO | TODO | TODO | TODO |
| warmRenderMs | TODO | TODO | TODO | TODO |
| renderKB | TODO | TODO | TODO | TODO |
| views | TODO | TODO | TODO | TODO |
| monthChangeSyncMs | TODO | TODO | TODO | TODO |
| monthChangeMs | TODO | TODO | TODO | TODO |
| monthChangeKB | TODO | TODO | TODO | TODO |
| startupMs | TODO | | – | |
| androidLaunchMs | TODO | | – | |
