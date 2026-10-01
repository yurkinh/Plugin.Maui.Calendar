#!/usr/bin/env bash
# Builds the UI test host app for a platform and runs the UI tests against it (macOS).
#
#   ./run.sh ios                          iOS simulator (iPhone 17 by default, UITEST_DEVICE to change)
#   ./run.sh android                      the running Android emulator, or the AVD named by UITEST_DEVICE
#   ./run.sh maccatalyst                  the Mac (needs macOS Automation Mode, see README.md)
#   ./run.sh ios --update-baselines       save the screenshots as the new baselines
#   ./run.sh ios --filter NavigationTests run some of the tests (a dotnet test filter)
set -euo pipefail

usage="Usage: run.sh <ios|android|maccatalyst> [--update-baselines] [--filter <expression>]"
platform="${1:?$usage}"
shift

here="$(cd "$(dirname "$0")" && pwd)"
case "$platform" in
	ios) framework=net10.0-ios ;;
	android) framework=net10.0-android ;;
	maccatalyst) framework=net10.0-maccatalyst ;;
	*) echo "$usage" >&2; exit 1 ;;
esac

update=0
filter=()
while [[ $# -gt 0 ]]; do
	case "$1" in
		--update-baselines) update=1 ;;
		--filter) filter=(--filter "$2"); shift ;;
		*) echo "$usage" >&2; exit 1 ;;
	esac
	shift
done

if [[ ! -d "$here/node_modules" ]]; then
	(cd "$here" && npm ci)
fi

dotnet build "$here/../Plugin.Maui.Calendar.UITests.HostApp/Plugin.Maui.Calendar.UITests.HostApp.csproj" -f "$framework" -c Debug

UITEST_PLATFORM="$platform" UITEST_UPDATE_BASELINES="$update" \
	dotnet test "$here/Plugin.Maui.Calendar.UITests.csproj" --logger "console;verbosity=normal" "${filter[@]+"${filter[@]}"}"
