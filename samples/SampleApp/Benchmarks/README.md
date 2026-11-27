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
The iOS numbers here and in the baseline come from untrimmed simulator builds (the simulator default, `TrimMode=copy`);
`benchmark.sh` now trims them like a device Release build, which renders faster (see below), so compare new iOS runs
with the build settings table, not with these two.
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

## Build settings (phase 4)

The same app (branch `perf/net11-performance`) built with different settings, timed interleaved: each round launches
every variant once, 5 rounds, on the same warmed-up emulator or simulator; medians. Sizes are the APK (both ABIs) and the
simulator `.app`. The library itself gives no trim or AOT warning in any variant.

Android (Pixel 9 emulator, API 37):

| | .NET 11 default | `MauiEnableFullReadyToRun=true` | `TrimMode=full` |
| --- | --- | --- | --- |
| APK | 32 199 KB | 41 811 KB (+30 %) | 32 087 KB (−0.3 %) |
| androidLaunchMs | 313 | 287 (−8 %) | 292 (−7 %) |
| startupMs | 225 | 214 | 210 |
| coldRenderMs (Default / Events) | 98.4 / 81.2 | 82.4 / 74.4 | 93.9 / 67.5 |
| warmRenderMs | 64.5 / 54.4 | 61.4 / 56.8 | 63.9 / 56.9 |
| renderKB | 7832 / 7868 | 7712 / 7865 | 7735 / 7819 |
| monthChangeKB | 166 / 165 | 166 / 165 | 125 / 124 |

iOS (iPhone 17 simulator, iOS 26.5; all three trimmed like a device build):

| | `TrimMode=partial` (device default) | + `DynamicCodeSupport=false` | Native AOT (`-p:SampleNativeAot=true`) |
| --- | --- | --- | --- |
| .app | 96 904 KB | 96 624 KB (−0.3 %) | 79 344 KB (−18 %) |
| coldRenderMs (Default / Events) | 183.7 / 165.3 | 186.9 / 165.4 | 180.4 / 177.5 |
| warmRenderMs | 161.2 / 161.7 | 161.6 / 163.4 | 175.3 / 176.9 (+9 %) |
| renderKB | 7880 / 7973 | 7877 / 7970 | 13 565 / 13 864 (+72 %) |
| monthChangeSyncMs | 11.8 / 12.0 | 7.6 / 12.2 | 11.8 / 12.6 |
| monthChangeKB | 322 / 322 | 320 / 320 | 1606 / 1610 (5×) |

What this means:
* Full ReadyToRun on Android starts the app ~8 % faster and renders the first calendar ~16 % faster, for a 30 % bigger
  APK. Worth it for an app where startup matters more than download size.
* Full trimming saves little for this app, but the calendar works under it (all sample pages checked) and allocates
  25 % less per month change. Each round launched the default variant first, so part of the launch differences may be
  order; the allocation numbers do not depend on it.
* `DynamicCodeSupport=false` changes nothing measurable.
* Native AOT makes the iOS app 18 % smaller, but the calendar renders ~9 % slower and allocates 1.7× as much per render
  and 5× as much per month change. That points to a slower path somewhere in the calendar or MAUI under Native AOT;
  it is not investigated yet.
