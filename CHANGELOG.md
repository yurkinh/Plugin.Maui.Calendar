# Changelog

All notable changes to [Plugin.Maui.Calendar](https://www.nuget.org/packages/Plugin.Maui.Calendar/) are listed here, newest first.
Every version is on [NuGet](https://www.nuget.org/packages/Plugin.Maui.Calendar/#versions-body-tab) and has a matching tag in this repository.

## [3.1.0] - 2026-09-25

### Added
- `DayViewTemplate`: draw every day cell with your own `DataTemplate` or `DataTemplateSelector`. Setting it to `null` restores the built-in cell, and the template can be switched at runtime. By @MykhailoDav
- The public `ICalendarDay` interface, the binding context of a day template: `Date`, `Day`, `IsSelected`, `IsToday`, `IsWeekend`, `IsThisMonth`, `IsDisabled`, `HasEvents`, `EventCount`, `Events`, `EventColors`, `IsRangeStart`, `IsRangeEnd`. By @MykhailoDav
- Day Template sample page. By @MykhailoDav

### Fixed
- The today highlight moves to the new day at midnight while the calendar is shown, and is checked again when the app resumes. By @MykhailoDav
- Adding, replacing or removing entries of the `EventCollection` at runtime refreshes the day cells (event dots and colors) right away, also after the calendar's page was shown again. By @MykhailoDav
- Changing `DisabledDates`, `MinimumDate`, `MaximumDate`, `OtherMonthDayIsVisible` or `OtherMonthWeekIsVisible` at runtime (or through a binding) updates the visible days. By @MykhailoDav
- `EventIndicatorType="TopDot"` draws the event dots above the day number; they were always drawn below it. By @MykhailoDav
- `EventIndicatorType="BackgroundFull"` no longer paints hidden other-month days, and follows a runtime change of `EventIndicatorType`. By @MykhailoDav
- With two calendars on screen, tapping a day in one no longer selects that date in the other. By @MykhailoDav
- Calendars no longer share one default `Events` collection and one default `DisabledDates` list. By @MykhailoDav
- A day whose event collection implements `IMultiEventDay` without providing `Colors` shows its event dot again, in the indicator color. By @MykhailoDav
- `RangeSelectionCalendar` keeps `SelectedDatesRangeBackgroundColor` on the range after a color, theme or layout change, and applies a new `SelectedDatesRangeBackgroundColor` right away. By @MykhailoDav
- `RangeSelectionCalendar` no longer ignores `SelectedStartDate`/`SelectedEndDate` set from code after the range was extended at its start. By @MykhailoDav
- `HeaderSectionTemplate`/`FooterSectionTemplate` are no longer modified when their content is created (the template kept a reference to the calendar), and a `BindingContext` set on the template root is kept. By @MykhailoDav

### Changed
- Updated Microsoft.Maui.Controls to 10.0.110.

## [3.0.3] - 2026-06-25

### Added
- `WeekendDayBackgroundColor` and `WeekendDayBackgroundCornerRadius` fill the weekend columns. By @MykhailoDav in [#258](https://github.com/yurkinh/Plugin.Maui.Calendar/pull/258)
- The visible date range: `VisibleStartDate`, `VisibleEndDate`, the `ShownDatesChanged` event and `ShownDatesChangedCommand`. By @yurkinh in [#253](https://github.com/yurkinh/Plugin.Maui.Calendar/pull/253)
- `WeekSelectionCalendar`: tapping one day selects its whole week ([#166](https://github.com/yurkinh/Plugin.Maui.Calendar/issues/166)).

### Fixed
- The boundary day was disabled when `MinimumDate`/`MaximumDate` had a time other than midnight ([#135](https://github.com/yurkinh/Plugin.Maui.Calendar/issues/135)). By @yurkinh in [#251](https://github.com/yurkinh/Plugin.Maui.Calendar/pull/251)
- Weekday titles use the culture's `AbbreviatedDayNames` when they are shortened ([#227](https://github.com/yurkinh/Plugin.Maui.Calendar/issues/227)). By @yurkinh in [#252](https://github.com/yurkinh/Plugin.Maui.Calendar/pull/252)
- Arabic weekday titles are normalized before they are shortened. By @yurkinh in [#250](https://github.com/yurkinh/Plugin.Maui.Calendar/pull/250)
- The calendar no longer causes a `List` to `ObservableCollection` binding warning. By @MykhailoDav in [#260](https://github.com/yurkinh/Plugin.Maui.Calendar/pull/260)
- Package references and font handling. By @yurkinh in [#256](https://github.com/yurkinh/Plugin.Maui.Calendar/pull/256)

### Changed
- Updated Microsoft.Maui.Controls to 10.0.71.

## [3.0.2] - 2026-05-08
- Updated Microsoft.Maui.Controls to 10.0.60.
- Performance improvements and bug fixes.

## [3.0.1] - 2026-01-26
- Updated Microsoft.Maui.Controls to 10.0.30.
- Fixed: day colors were not applied on the first load.

## [3.0.0] - 2026-01-02
- Updated to .NET 10 (Microsoft.Maui.Controls 10.0.20).
- Compatible with the XAML source generator (`MauiXamlInflator=SourceGen`), the default in .NET 10.
- Added Native AOT support with trimming analyzers (disabled by default).

## [2.0.12] - 2025-12-01
- Fixed an `ArgumentOutOfRangeException` when navigating to the year boundaries.
- Fixed a `NullReferenceException` and another exception in `RangeSelectionCalendar`.

## [2.0.11] - 2025-10-28
- Fixed: changing `CalendarLayout` created multiple layout instances.
- Updated MAUI to 9.0.120.

## [2.0.10] - 2025-09-16
- Added `UseAbbreviatedDayNames`: use the culture's `AbbreviatedDayNames` as they are. When it is on, `DaysTitleMaximumLength` is ignored.
- Updated MAUI to 9.0.100.

## [2.0.9] - 2025-08-22
- Fixed: the `MultiSelectionCalendar` UI did not update when `SelectedDates` was cleared.

## [2.0.8] - 2025-08-06
- Added `AutoChangeMonthOnDayTap`: tapping a day of another month shows that month (off by default).
- Bug fixes. Updated MAUI to 9.0.90.

## [2.0.7] - 2025-06-25
- Bug fixes. Updated MAUI to 9.0.80 and the other NuGet packages.

## [2.0.6] - 2025-05-30
- Fixed: `SelectedDates` was not filled in `RangeSelectionCalendar`.

## [2.0.4] - 2025-05-08
- Added `UseNativeDigits`: show numbers in the native digits of the culture.
- Fixed the calendar height when `OtherMonthDayIsVisible="False"`.

## [2.0.3] - 2025-04-15
- Fixed: event indicators were not updated when events were added.

## [2.0.2] - 2025-04-14
- Fixed event initialization and updates at startup.

## [2.0.1] - 2025-04-11
- Fixed day names ([#158](https://github.com/yurkinh/Plugin.Maui.Calendar/issues/158)).

## [2.0.0] - 2025-04-04

### Added
- Style properties (see [Styles](README.md#styles)) that replace several color and font properties.
- `WeekendTitleStyle`.
- `OtherMonthWeekIsVisible` and `DayViewBorderMargin`.
- A sample page (Default Calendar) to test memory leaks with [MemoryToolkit.Maui](https://github.com/AdamEssenmacher/MemoryToolkit.Maui).

### Changed
- Updated to .NET 9.
- Startup is faster: 20 % on iOS, 40 % on Android.
- Revamped the calendar structure and the samples.

### Fixed
- Memory leaks.

### Breaking changes
Properties replaced by styles:

| Removed property | Use instead |
| --- | --- |
| `DayViewFontSize` | `DaysLabelStyle` |
| `MonthLabelColor` | `MonthLabelStyle` |
| `YearLabelColor` | `YearLabelStyle` |
| `ArrowsBackgroundColor`, `ArrowsBorderColor`, `ArrowsBorderWidth`, `ArrowsFontAttribute`, `ArrowsFontSize`, `ArrowsFontFamily`, `ArrowsColor`, `ArrowsSymbolPrev`, `ArrowsSymbolNext` | `PreviousMonthArrowButtonStyle`, `NextMonthArrowButtonStyle`, `PreviousYearArrowButtonStyle`, `NextYearArrowButtonStyle` |
| `ArrowsFontFamily`, `ArrowsColor` (footer arrow) | `FooterArrowLabelStyle` |
| `SelectedDateColor` | `SelectedDateLabelStyle` |
| `DaysTitleHeight`, `DaysTitleColor` | `DaysTitleLabelStyle` |
| `DaysTitleHeight`, `DaysTitleWeekendColor` | `WeekendTitleStyle` |

`SelectedDates` is an `ObservableCollection<DateTime>`. Bind it to an `ObservableCollection<DateTime>`, not to a `List<DateTime>`, so the calendar sees dates being added and removed.

## [1.2.6] - 2025-01-28
- Added `SelectedDatesRangeBackgroundColor`.
- Fixed [#76](https://github.com/yurkinh/Plugin.Maui.Calendar/issues/76) and [#99](https://github.com/yurkinh/Plugin.Maui.Calendar/issues/99).

## [1.2.5] - 2025-01-14
- Added `AllowDeselecting`.

## [1.2.4] - 2024-12-17
- Added the `MonthChanged` event and `MonthChangedCommand`.

## [1.2.3] - 2024-11-25
- Added several event dots per day (up to five colors).
- Updated MAUI to 8.0.100.

## [1.2.2] - 2024-09-03
- Added `FirstDayOfWeek`.
- Updated MAUI to 8.0.72.

## [1.2.1] - 2024-08-06
- Fixed `OtherMonthDayIsVisible="False"` ([#77](https://github.com/yurkinh/Plugin.Maui.Calendar/issues/77)).
- Theme support and new samples in the sample app: Windows 11 Calendar and Weekend Calendar.
- Updated MAUI to 8.0.61.

## [1.2.0] - 2024-06-24
- Added `OtherMonthSelectedDayColor`.
- Downgraded MAUI to 8.0.40.

## [1.1.9] - 2024-06-19
- Added `OnShownDateChangedCommand`, executed when the shown date changes.
- Selected days can be cleared from code (`ClearSelection()`).
- `RangeSelectionCalendar` respects `DisabledDates`.
- Added `Dispose()` to force the handler to disconnect.
- Fixed: the calendar was empty inside a Shell tab ([#63](https://github.com/yurkinh/Plugin.Maui.Calendar/issues/63)).

## 1.0.x
- Port of the Xamarin.Forms [Calendar Plugin](https://github.com/lilcodelab/Xamarin.Plugin.Calendar) by [lilcodelab](https://github.com/lilcodelab/) without platform-specific code, so it runs on every .NET MAUI backend: Android, iOS, Mac Catalyst and Windows (Tizen is not tested).
- Added `MultiSelectionCalendar`.
- Updated to .NET 8. Refactored the code.

[3.1.0]: https://github.com/yurkinh/Plugin.Maui.Calendar/compare/3.0.3...3.1.0
[3.0.3]: https://github.com/yurkinh/Plugin.Maui.Calendar/compare/3.0.2...3.0.3
[3.0.2]: https://github.com/yurkinh/Plugin.Maui.Calendar/compare/3.0.1...3.0.2
[3.0.1]: https://github.com/yurkinh/Plugin.Maui.Calendar/compare/3.0.0...3.0.1
[3.0.0]: https://github.com/yurkinh/Plugin.Maui.Calendar/compare/2.0.12...3.0.0
[2.0.12]: https://github.com/yurkinh/Plugin.Maui.Calendar/compare/2.0.11...2.0.12
[2.0.11]: https://github.com/yurkinh/Plugin.Maui.Calendar/compare/2.0.10...2.0.11
[2.0.10]: https://github.com/yurkinh/Plugin.Maui.Calendar/compare/2.0.9...2.0.10
[2.0.9]: https://github.com/yurkinh/Plugin.Maui.Calendar/compare/2.0.8...2.0.9
[2.0.8]: https://github.com/yurkinh/Plugin.Maui.Calendar/compare/2.0.7...2.0.8
[2.0.7]: https://github.com/yurkinh/Plugin.Maui.Calendar/compare/2.0.6...2.0.7
[2.0.6]: https://github.com/yurkinh/Plugin.Maui.Calendar/compare/2.0.4...2.0.6
[2.0.4]: https://github.com/yurkinh/Plugin.Maui.Calendar/compare/2.0.3...2.0.4
[2.0.3]: https://github.com/yurkinh/Plugin.Maui.Calendar/compare/2.0.2...2.0.3
[2.0.2]: https://github.com/yurkinh/Plugin.Maui.Calendar/compare/2.0.1...2.0.2
[2.0.1]: https://github.com/yurkinh/Plugin.Maui.Calendar/compare/2.0.0...2.0.1
[2.0.0]: https://github.com/yurkinh/Plugin.Maui.Calendar/compare/1.2.6...2.0.0
[1.2.6]: https://github.com/yurkinh/Plugin.Maui.Calendar/compare/1.2.5...1.2.6
[1.2.5]: https://github.com/yurkinh/Plugin.Maui.Calendar/compare/1.2.4...1.2.5
[1.2.4]: https://github.com/yurkinh/Plugin.Maui.Calendar/compare/1.2.3...1.2.4
[1.2.3]: https://github.com/yurkinh/Plugin.Maui.Calendar/compare/1.2.2...1.2.3
[1.2.2]: https://github.com/yurkinh/Plugin.Maui.Calendar/compare/1.2.1...1.2.2
[1.2.1]: https://github.com/yurkinh/Plugin.Maui.Calendar/compare/1.2.0...1.2.1
[1.2.0]: https://github.com/yurkinh/Plugin.Maui.Calendar/compare/1.1.9...1.2.0
[1.1.9]: https://github.com/yurkinh/Plugin.Maui.Calendar/releases/tag/1.1.9
