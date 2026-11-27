#!/usr/bin/env bash
# Builds the sample app in Release, starts it several times in benchmark mode and prints the results of every run
# (macOS). Each start runs the Benchmark page (Benchmarks/CalendarBenchmark.cs) and writes CALBENCH lines.
#
#   ./benchmark.sh android [runs]   the running emulator (set ANDROID_SERIAL when more than one device is attached)
#   ./benchmark.sh ios [runs]       the booted simulator (set SIMULATOR_UDID when more than one is booted)
#
# Close other heavy work (builds, other simulators) while it runs: the numbers are wall-clock times.
# Set NO_BUILD=1 to reuse the last build, and BUILD_ARGS to compare build settings, for example
# BUILD_ARGS="-p:MauiEnableFullReadyToRun=true" (it starts from a clean build so no output of another variant is reused).
# APP_PATH=<.apk or .app> runs an app built elsewhere instead of building one.
set -euo pipefail

usage="Usage: benchmark.sh <android|ios> [runs]"
platform="${1:?$usage}"
runs="${2:-5}"
here="$(cd "$(dirname "$0")" && pwd)"
project="$here/SampleApp.csproj"
app_id="com.companyname.sampleapp"
results="$(mktemp -t calbench)"

build() {
	if [[ "${NO_BUILD:-0}" != 1 ]]; then
		rm -rf "$here/bin/Release/$1" "$here/obj/Release/$1"
		# shellcheck disable=SC2086 # BUILD_ARGS holds several arguments
		dotnet build "$project" -c Release -f "$1" "${@:2}" ${BUILD_ARGS:-} >&2
	fi
}

collect() {
	# Waits for "CALBENCH done" (or failed) in the output file, then prints the run's CALBENCH lines.
	local output="$1" run="$2"
	for _ in $(seq 1 240); do
		if grep -q "CALBENCH \(done\|failed\)" "$output"; then
			break
		fi
		sleep 1
	done
	grep -o "CALBENCH scenario=.*" "$output" | sed "s/^CALBENCH /run=$run /" | tee -a "$results"
	if ! grep -q "CALBENCH done" "$output"; then
		echo "Run $run did not finish:" >&2
		grep -o "CALBENCH failed.*" "$output" >&2 || echo "(no result within 4 minutes)" >&2
		exit 1
	fi
}

case "$platform" in
android)
	if [[ -n "${APP_PATH:-}" ]]; then
		apk="$APP_PATH"
	else
		build net11.0-android -p:AndroidPackageFormat=apk
		apk="$(ls "$here"/bin/Release/net11.0-android/*-Signed.apk | head -1)"
	fi
	echo "apkKB=$(( $(stat -f %z "$apk") / 1024 )) buildArgs=${BUILD_ARGS:-none}" | tee -a "$results"
	adb install -r "$apk" >/dev/null
	activity="$(adb shell cmd package resolve-activity --brief "$app_id" | tail -1 | tr -d '\r')"
	for run in $(seq 1 "$runs"); do
		adb shell am force-stop "$app_id"
		adb logcat -c
		output="$(mktemp -t calbench-run)"
		adb logcat -v raw >"$output" &
		logcat=$!
		# TotalTime: Android's own launch time (process start until the first frame)
		launch="$(adb shell am start -W -n "$activity" --ez calbench true | tr -d '\r' | sed -n 's/^TotalTime: //p')"
		echo "run=$run androidLaunchMs=$launch" | tee -a "$results"
		collect "$output" "$run"
		kill "$logcat" 2>/dev/null || true
		adb shell am force-stop "$app_id"
	done
	;;
ios)
	udid="${SIMULATOR_UDID:-$(xcrun simctl list devices booted -j | python3 -c 'import json,sys; d=[x for r in json.load(sys.stdin)["devices"].values() for x in r if x["state"]=="Booted"]; print(d[0]["udid"] if d else "")')}"
	[[ -n "$udid" ]] || { echo "Boot a simulator first (or set SIMULATOR_UDID)." >&2; exit 1; }
	if [[ -n "${APP_PATH:-}" ]]; then
		app="$APP_PATH"
	else
		build net11.0-ios -p:RuntimeIdentifier=iossimulator-arm64
		app="$(ls -d "$here"/bin/Release/net11.0-ios/iossimulator-arm64/*.app | head -1)"
	fi
	echo "appKB=$(du -sk "$app" | cut -f1) buildArgs=${BUILD_ARGS:-none}" | tee -a "$results"
	xcrun simctl install "$udid" "$app"
	for run in $(seq 1 "$runs"); do
		xcrun simctl terminate "$udid" "$app_id" 2>/dev/null || true
		output="$(mktemp -t calbench-run)"
		SIMCTL_CHILD_CALBENCH=1 xcrun simctl launch --console-pty "$udid" "$app_id" >"$output" 2>&1 &
		launcher=$!
		collect "$output" "$run"
		xcrun simctl terminate "$udid" "$app_id" 2>/dev/null || true
		kill "$launcher" 2>/dev/null || true
	done
	;;
*)
	echo "$usage" >&2
	exit 1
	;;
esac

echo
echo "All runs: $results"
