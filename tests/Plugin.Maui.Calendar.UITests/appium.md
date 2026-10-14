# Running Appium: known problems and workarounds

Things that stop a UI test run before any test starts, how to recognize them and what to do. The README covers
the normal setup; this file covers what goes wrong on a developer machine.

## Node.js version

**Symptom:** Appium does not start, or every test fails in `OneTimeSetUp` right away; the Appium log
(`TestResults/Screenshots/<platform>/appium.log`) or the console says the Node.js version is not supported.

Appium 3.8 needs Node.js `^20.19`, `^22.12` or `>=24`. Odd-numbered releases (for example 23.x) are not supported.
Check with `node --version`.

**Fix:** install Node.js 22 LTS or 24. To try a version without changing the one installed on the machine, put a
downloaded Node.js first on `PATH` only for the test run (the tests start Appium with the `node` they find on `PATH`):

```bash
mkdir -p ~/node24 && cd ~/node24
name=$(curl -sf https://nodejs.org/dist/latest-v24.x/SHASUMS256.txt | grep -o "node-v24[0-9.]*-darwin-arm64.tar.gz" | head -1)
curl -sfO "https://nodejs.org/dist/latest-v24.x/$name"
curl -sf https://nodejs.org/dist/latest-v24.x/SHASUMS256.txt | grep " $name\$" | shasum -a 256 -c -
tar xzf "$name"

cd <repo>/tests
PATH="$HOME/node24/${name%.tar.gz}/bin:$PATH" UITEST_PLATFORM=ios dotnet test Plugin.Maui.Calendar.UITests
```

Use `darwin-x64` instead of `darwin-arm64` on an Intel Mac. `node_modules` installed with another Node.js version
works too: Appium's native parts do not depend on the Node.js version.

## iOS: the wrong simulator

**Symptom:** every test fails in `OneTimeSetUp` with `Unable to start WebDriverAgent session` and
`connect ECONNREFUSED 127.0.0.1:8100`. Earlier in the Appium log: `Cannot launch com.plugin.maui.calendar.uitests
application`, and the device id is not the simulator you expected.

Appium chooses the simulator by name (`iPhone 17` by default). When several iOS runtimes are installed, more
than one simulator has that name, and Appium may take the newest one. On the iOS 27.0 simulator the host app fails
to launch, so WebDriverAgent cannot open it.

**Fix:** name the iOS version as well; 26.5 is the one the baselines were recorded on:

```bash
UITEST_PLATFORM=ios UITEST_PLATFORM_VERSION=26.5 dotnet test Plugin.Maui.Calendar.UITests
```

Appium boots that simulator if it is not running. Shut down a simulator Appium started by mistake with
`xcrun simctl shutdown <udid>` (the udid is in the Appium log).

## iOS: the first run is slow

The first run on a simulator builds WebDriverAgent with `xcodebuild`, which takes a few minutes. Until it has
started, the Appium log repeats `connect ECONNREFUSED 127.0.0.1:8100`. That is normal; wait for `WebDriverAgent
successfully started`. Only when a failure follows it is it one of the problems above.

## Android: a phone and an emulator connected together

**Symptom:** the tests install and run the host app on your phone instead of the emulator.

Without a device named, Appium uses the first device `adb devices` lists.

**Fix:** name the emulator's AVD; Appium uses that emulator and starts it if it is not running:

```bash
UITEST_PLATFORM=android UITEST_DEVICE=Pixel_9 dotnet test Plugin.Maui.Calendar.UITests
```

## Android: the emulator is still booting

**Symptom:** every test fails in `OneTimeSetUp` with `adb: device offline`, or on a later run with `Cannot start
the 'io.appium.settings' application` and `Activity class {io.appium.settings/io.appium.settings.Settings} does
not exist`.

`adb devices` lists an emulator as `device` before it has finished booting. A run that starts then can be cut off
while Appium installs its helper apps on the emulator, which leaves them half installed; later runs then fail
too.

**Fix:** wait for the boot to finish before running the tests:

```bash
adb -s emulator-5554 shell getprop sys.boot_completed   # prints 1 when the emulator is ready
```

If the helper apps were left half installed, remove them; Appium installs them again on the next run:

```bash
adb -s emulator-5554 uninstall io.appium.settings
adb -s emulator-5554 uninstall io.appium.uiautomator2.server
adb -s emulator-5554 uninstall io.appium.uiautomator2.server.test
```

## Android: checking with TalkBack on the emulator

Turning TalkBack on from the command line:

```bash
adb -s emulator-5554 shell settings put secure enabled_accessibility_services \
	com.google.android.marvin.talkback/com.google.android.marvin.talkback.TalkBackService
adb -s emulator-5554 shell settings put secure accessibility_enabled 1
```

and off again:

```bash
adb -s emulator-5554 shell settings delete secure enabled_accessibility_services
adb -s emulator-5554 shell settings put secure accessibility_enabled 0
```

Two things get in the way:

* The first time TalkBack starts, it asks for permission to send notifications. The dialog covers the app and takes
  the taps, and it cannot be answered with TalkBack on. Grant the permission before turning TalkBack on
  (`adb -s emulator-5554 shell pm grant com.google.android.marvin.talkback android.permission.POST_NOTIFICATIONS`),
  or, once the dialog is open, close it with `adb -s emulator-5554 shell am force-stop com.google.android.permissioncontroller`.
* Taps sent with `adb shell input tap` (and Appium's taps) do not go through TalkBack's explore by touch: they tap
  the app directly, as without TalkBack. TalkBack's own gestures (one tap to move the focus, a double tap to
  activate) cannot be made from the command line; check them by hand.

What a screen reader finds can be checked without TalkBack: `adb shell uiautomator dump` lists the accessibility
tree, the elements of the drawn day grid included (with their content descriptions and bounds).

## Checking behavior only

On a device the baselines were not recorded on, the screenshot comparisons fail even when nothing is wrong. To
check the behavior only, skip the comparisons and the look-only tests:

```bash
UITEST_SCREENSHOTS=off dotnet test Plugin.Maui.Calendar.UITests --filter "FullyQualifiedName!~AppearanceTests"
```

## A complete run, on macOS

The behavior tests against the day grid drawn with Microsoft.Maui.Graphics, on both mobile platforms, with a
Node.js 24 that is not installed globally:

```bash
cd <repo>/tests
dotnet build Plugin.Maui.Calendar.UITests.HostApp -f net10.0-ios -c Debug
dotnet build Plugin.Maui.Calendar.UITests.HostApp -f net10.0-android -c Debug
export PATH="$HOME/node24/node-v24.21.0-darwin-arm64/bin:$PATH"   # only if the installed Node.js is not supported

UITEST_PLATFORM=ios UITEST_PLATFORM_VERSION=26.5 UITEST_DRAWN=graphics UITEST_SCREENSHOTS=off \
	dotnet test Plugin.Maui.Calendar.UITests --filter "FullyQualifiedName!~AppearanceTests"

UITEST_PLATFORM=android UITEST_DEVICE=Pixel_9 UITEST_DRAWN=graphics UITEST_SCREENSHOTS=off \
	dotnet test Plugin.Maui.Calendar.UITests --filter "FullyQualifiedName!~AppearanceTests"
```
