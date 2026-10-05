# UI tests

These tests drive a real app with [Appium](https://appium.io) and check the calendar the way a user sees it:
they tap days and arrows, swipe, change properties at runtime, read the calendar's state from the screen and
compare screenshots of the calendar with baseline images. Run them before and after a change to see that nothing
else changed.

| Folder | What it is |
| --- | --- |
| `tests/Plugin.Maui.Calendar.UITests.HostApp` | The app under test. Every page is a *scenario*: a calendar in a known state. |
| `tests/Plugin.Maui.Calendar.UITests` | The NUnit tests, Appium (`package.json`) and the baselines (`Baselines/<platform>`). |

The unit tests in `tests/Plugin.Maui.Calendar.Tests` check the logic without a device and cover all of the
library's code. The UI tests check what only a device shows: layout, drawing, gestures and the platform's text.

## How the host app makes screenshots repeatable

* Today is always **Wednesday, May 14 2025** (the calendars get a fixed `TimeProvider`), and every scenario shows
  May 2025 unless it says otherwise. With Sunday first, the month grid runs from April 27 to June 7.
* The culture is `en-US` unless the scenario is about another culture, and the theme is light.
* The window is portrait on phones and 480 × 960 on the desktop.
* Under the calendar the page shows the calendar's state as text (`SelectedDates`, `ShownDate`, `VisibleDates`,
  `LayoutUnit`, `SelectedText`, `SectionShown`, `MonthChanged`, `DayTapped`, `Swiped`, `Arrows`, `Range`), which
  the tests read. The `Interactive` scenario has buttons that change the calendar at runtime.
* The page gives the calendar's own elements automation ids: `PrevMonthArrow`, `NextMonthArrow`, `PrevYearArrow`,
  `NextYearArrow`, `MonthLabel`, `YearLabel`, `SelectedDateLabel`, `FooterArrow`, `DaysGrid`, `Title0`..`Title6`,
  and `Day0`..`Day41` for the day cells by position (`Day0` is the first date of `VisibleDates`).
  `CalendarApp.TapDay(date)` turns a date into its cell.

## Setup

* .NET 10 SDK with the MAUI workload.
* Node.js 20 or later, then once: `npm ci` in this folder. It installs Appium and its drivers here; the tests start
  this Appium themselves (with `APPIUM_HOME` set to this folder), so nothing is installed globally.
* **iOS**: Xcode and the simulator the baselines were recorded on (see below). The first run builds Appium's
  WebDriverAgent, which takes a few minutes.
* **Android**: the Android SDK, an emulator that is running, or an AVD named in `UITEST_DEVICE`, and Java 17 or later.
* **Mac Catalyst**: Xcode, and macOS *Automation Mode*. Without it macOS asks for your password whenever a test run
  starts; to turn the prompt off once, run `sudo automationmodetool enable-automationmode-without-authentication`.
  Also allow the terminal or IDE to control the computer when macOS asks (System Settings › Privacy & Security ›
  Accessibility).
* **Windows**: WinAppDriver (`appium-windows-driver` uses it) and Developer Mode.

## Running

On macOS, `run.sh` builds the host app for the platform and runs the tests:

```bash
cd tests/Plugin.Maui.Calendar.UITests
./run.sh ios
./run.sh android
./run.sh maccatalyst
./run.sh ios --filter "FullyQualifiedName~NavigationTests"
```

Or by hand (also on Windows):

```bash
dotnet build tests/Plugin.Maui.Calendar.UITests.HostApp -f net10.0-ios -c Debug
UITEST_PLATFORM=ios dotnet test tests/Plugin.Maui.Calendar.UITests
```

The tests read these environment variables:

| Variable | Meaning |
| --- | --- |
| `UITEST_PLATFORM` | `ios`, `android`, `maccatalyst` or `windows` (default: `ios` on macOS, `windows` on Windows) |
| `UITEST_DEVICE` | The iOS simulator (default `iPhone 17`) or the Android AVD to start |
| `UITEST_PLATFORM_VERSION` | The iOS or Android version of that device |
| `UITEST_CONFIGURATION` | The configuration the host app was built with (default `Debug`) |
| `UITEST_APP` | An app to test instead of the host app's build output |
| `UITEST_APPIUM_URL` | An Appium server that is already running (otherwise the tests start one) |
| `UITEST_UPDATE_BASELINES` | `1` saves the screenshots as the new baselines instead of comparing them |
| `UITEST_SCREENSHOTS` | `off` skips the screenshot comparisons and checks the behavior only |

The Appium log of a run is in `TestResults/Screenshots/<platform>/appium.log`.

### Native AOT

Native AOT trims every assembly fully and compiles the app ahead of time, so it catches code that only works with
reflection. To run the tests against a Native AOT build of the host app on the iOS simulator:

```bash
dotnet build tests/Plugin.Maui.Calendar.UITests.HostApp -f net10.0-ios -c Release -p:UITestNativeAot=true -p:_IsPublishing=true
UITEST_PLATFORM=ios UITEST_APP="$PWD/tests/Plugin.Maui.Calendar.UITests.HostApp/bin/Release/net10.0-ios/iossimulator-arm64/Plugin.Maui.Calendar.UITests.HostApp.app" \
	dotnet test tests/Plugin.Maui.Calendar.UITests
```

`_IsPublishing` makes the build use Native AOT for the simulator (`dotnet publish` only builds for devices). The
screenshots match the same baselines as a Debug build.

### On GitHub Actions

The *UI tests* workflow (`.github/workflows/ui-tests.yml`) runs the tests for every pull request into `main`, after
every merge into `main`, and on demand from the Actions tab:

* **iOS** on the iPhone 17 simulator with iOS 26.5, the one the iOS baselines were recorded on, so the screenshots
  are compared too.
* **Android** on an API 35 emulator the size of a Pixel 7. That is not the device the Android baselines were
  recorded on, so only the behavior is checked.

The workflow pins the .NET SDK, the workload set and Xcode. Each .NET for iOS version needs one Xcode version, so
update them together. When a run fails, its artifacts hold the screenshots, the test results and the Appium log.

## Screenshots and baselines

A screenshot of the calendar is compared with `Baselines/<platform>/<name>.png`. A pixel counts as different when a
color channel differs by more than 32 (out of 255), and the comparison fails when more than 0.1 % of the pixels
differ. On a failure, `TestResults/Screenshots/<platform>` holds the screenshot (`.actual.png`), the baseline
(`.expected.png`) and an image of the differences in red (`.diff.png`); the test lists them as attachments.

Fonts and drawing differ between devices and OS versions, so the baselines only match on the devices they were
recorded on:

| Platform | Device the baselines were recorded on |
| --- | --- |
| iOS | iPhone 17 simulator, iOS 26.5 (Xcode 26.6) |
| Android | Pixel 9 emulator (`Pixel_9` AVD), Android 17 (API 37) |
| Mac Catalyst, Windows | No baselines yet: record them with `--update-baselines` on the machine that runs the tests |

On another device, run with `UITEST_SCREENSHOTS=off` to check the behavior only, or record baselines for it.

When a change is meant to change the look:

1. Run the tests; the failures show which screenshots changed.
2. Look at the `.diff.png` files and check that only the intended things changed.
3. Run with `--update-baselines` (or `UITEST_UPDATE_BASELINES=1`) to save the new screenshots.
4. Review the changed images in `git diff` before you commit them.

## Adding a test

1. Add a scenario to `ScenarioCatalog` in the host app (or an action to the `Interactive` scenario).
2. Write the test: `App.OpenScenario("Name")`, then `App.TapDay(...)`, `App.Tap("NextMonthArrow")`,
   `App.SwipeOver("DaysGrid", Swipe.Left)`, `App.Text("SelectedDate")` and `CalendarMatchesBaseline("Name")`.
   A look-only scenario just needs its name in `AppearanceTests`.
3. Run it with `--update-baselines` on every platform that has baselines, check the new images and commit them.
