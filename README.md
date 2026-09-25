<div align="center">

<img src="https://raw.githubusercontent.com/yurkinh/Plugin.Maui.Calendar/main/nuget.png" alt="Plugin.Maui.Calendar" width="130" />

# Calendar Plugin for .NET MAUI

**Highly customizable Calendar control for .NET MAUI** — events, localization, theming, range &amp; multi-selection.

[![NuGet](https://img.shields.io/nuget/v/Plugin.Maui.Calendar.svg?label=NuGet&logo=nuget)](https://www.nuget.org/packages/Plugin.Maui.Calendar/)
[![Downloads](https://img.shields.io/nuget/dt/Plugin.Maui.Calendar.svg?color=blue&logo=nuget)](https://www.nuget.org/packages/Plugin.Maui.Calendar/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/yurkinh/Plugin.Maui.Calendar/blob/main/LICENSE)
[![.NET MAUI](https://img.shields.io/badge/.NET%20MAUI-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/apps/maui)
[![Stars](https://img.shields.io/github/stars/yurkinh/Plugin.Maui.Calendar?style=flat&logo=github)](https://github.com/yurkinh/Plugin.Maui.Calendar/stargazers)

</div>

A .NET MAUI port of the [lilcodelab](https://github.com/lilcodelab/) Xamarin.Forms [Calendar Plugin](https://github.com/lilcodelab/Xamarin.Plugin.Calendar).

Simple cross-platform plugin for Calendar control featuring:
- Displaying events by binding EventCollection
- Localization support with System.Globalization.CultureInfo
- Customizable colors, day view sizes/label styles, custom Header/Footer template support
- Custom day cells with `DayViewTemplate`
- UI reactive to EventCollection, Culture, and other changes 

### What's new
V3.1.0
* Added **DayViewTemplate** property — draw every day cell with your own `DataTemplate` or `DataTemplateSelector` (check the [DayViewTemplate](#dayviewtemplate) section)
* Added the **ICalendarDay** interface, the binding context of a day template: `Date`, `Day`, `IsSelected`, `IsToday`, `IsWeekend`, `IsThisMonth`, `IsDisabled`, `HasEvents`, `EventCount`, `Events`, `EventColors`, `IsRangeStart`, `IsRangeEnd`
* Added a Day Template sample page
* Fixed: the today highlight now moves to the new day at midnight while the calendar is shown
* Fixed: adding, replacing or removing entries of the `EventCollection` at runtime now updates the day cells right away, also after the calendar's page was shown again
* Fixed: changing `DisabledDates`, `MinimumDate`, `MaximumDate`, `OtherMonthDayIsVisible` or `OtherMonthWeekIsVisible` at runtime (or through a binding) now updates the visible days
* Fixed: `EventIndicatorType="TopDot"` now draws the event dots above the day number (they were always drawn below it)
* Fixed: `EventIndicatorType="BackgroundFull"` no longer paints hidden other-month days, and follows a runtime change of `EventIndicatorType`
* Fixed: with two calendars on screen, tapping a day in one no longer selects that date in the other
* Fixed: calendars no longer share one default `Events` collection and one default `DisabledDates` list
* Fixed: a day whose event collection implements `IMultiEventDay` without providing `Colors` shows its event dot again (in the indicator color)
* Fixed: `RangeSelectionCalendar` keeps `SelectedDatesRangeBackgroundColor` on the range after a color, theme or layout change, and applies a new `SelectedDatesRangeBackgroundColor` right away

V2.0.0
* Updated to .NET 9
* Optimized startup time: iOS 20 % / Android 40 % 
* Fixed memory leaks* 
* Revamped calendar structure
* Added **Styles** (check [Available Styles](#available-styles) section) that replace some **properties**
* Added **WeekendTitleStyle**
* Added sample page (Default calendar) to test memory leaks with [MemoryToolkit.Maui](https://github.com/AdamEssenmacher/MemoryToolkit.Maui) )
* Updated samples
* Added native digits support (added **UseNativeDigits** Property)
* Added **OtherMonthWeekIsVisible** and **DayViewBorderMargin** properties
* Added **AutoChangeMonthOnDayTap** property — allows automatically switching the displayed month when user taps on a day from another month (disabled by default)
* Added **UseAbbreviatedDayNames** property — allows using built-in .NET AbbreviatedDayNames without trimming. When this property is enabled, **DaysTitleMaximumLength** is ignored.


### Breaking  Changes

Properties that Styles have replaced
```xml
DayViewFontSize --> DaysLabelStyle
MonthLabelColor --> MonthLabelStyle
YearLabelColor --> YearLabelStyle

ArrowsBackgroundColor, ArrowsBorderColor, ArrowsBorderWidth, ArrowsFontAttribute, ArrowsFontSize, ArrowsFontFamily, ArrowsColor --> 
ArrowsSymbolPrev --> PreviousMonthArrowButtonStyle
ArrowsSymbolNext --> NextMonthArrowButtonStyle
ArrowsSymbolPrev --> PreviousYearArrowButtonStyle
ArrowsSymbolNext --> NextYearArrowButtonStyle

ArrowsFontFamily, ArrowsColor --> FooterArrowLabelStyle
SelectedDateColor --> SelectedDateLabelStyle

DaysTitleHeight, DaysTitleColor --> DaysTitleLabelStyle
DaysTitleHeight, DaysTitleWeekendColor --> WeekendTitleStyle
```
> ⚠️ **Breaking Change**:  
> The `SelectedDates` property is now an `ObservableCollection<DateTime>`.  
> To ensure the calendar updates when dates are added/removed, use:
>
> ```csharp
> SelectedDates = new ObservableCollection<DateTime> { ... };
> ```
>
> Do not use `List<DateTime>` for `SelectedDates` binding.

V1.0.x
* Removed all the platform-specific code, hence it supports all available .NET MAUI backends: iOS, Android, Windows, Mac, Tizen (not tested yet)
* Added Multiselection support (Latest PR that was not merged previously)
* Refactored and revamped code
* Updated to .NET 8
* Added OnShownDateChangedCommand so we can take action when a date is changed.
* Added new property **OtherMonthSelectedDayColor**
* Fixed bug with **OtherMonthDayIsVisible** property
* Added a weekend calendar sample
* Added a Windows 11 calendar sample
* Added theme support
* Added new property **FirstDayOfWeek**
* Added support for multiple event dots (multidots) in calendar 
* Added **MonthChanged** Event and **MonthChangedCommand**
* Added **AllowDeselecting** property
* Added **SelectedDatesRangeBackgroundColor** property
* Updated samples

## YouTube
[![Free and Complete Calendar Control for .NET MAUI: Plugin.Maui.Calendar](https://img.youtube.com/vi/bmkizbS4jb4/0.jpg)](https://www.youtube.com/watch?v=bmkizbS4jb4)


## Screenshots
| Android | iOS | Win | Mac |
| ------- | ------ | ------ | ------ |
| ![Android Calendar Screenshot](https://raw.githubusercontent.com/yurkinh/Plugin.Maui.Calendar/main/res/android.png) | ![iPhone Calendar Screenshot](https://raw.githubusercontent.com/yurkinh/Plugin.Maui.Calendar/main/res/ios.png) | ![Windiws Calendar Screenshot](https://raw.githubusercontent.com/yurkinh/Plugin.Maui.Calendar/main/res/win.png) | ![Mac Calendar Screenshot](https://raw.githubusercontent.com/yurkinh/Plugin.Maui.Calendar/main/res/mac.png) |

Theme support
| Ligth | Dark | Settings |
| ------- | ------ | ------ |
| ![Light theme Screenshot](https://raw.githubusercontent.com/yurkinh/Plugin.Maui.Calendar/main/res/LightTheme.png) | ![Dark theme Screenshot](https://raw.githubusercontent.com/yurkinh/Plugin.Maui.Calendar/main/res/DarkTheme.png) | ![Settings Page Screenshot](https://raw.githubusercontent.com/yurkinh/Plugin.Maui.Calendar/main/res/ThemeSettingPage.png) |

Culture support
| Android | iOS |
| ------- | ------ |
| ![Android Culture Screenshot](https://raw.githubusercontent.com/yurkinh/Plugin.Maui.Calendar/main/res/Culture_support_android.png) | ![iPhone Culture Screenshot](https://raw.githubusercontent.com/yurkinh/Plugin.Maui.Calendar/main/res/Culture_support_iOS.png) |


# New Samples

Windows 11 calendar
| Win     | Mac    |
| ------- | ------ |
| ![Windiws 11 android Calendar Screenshot](https://raw.githubusercontent.com/yurkinh/Plugin.Maui.Calendar/main/res/W11_android.png) | ![Windiws 11 Calendar IOS Screenshot](https://raw.githubusercontent.com/yurkinh/Plugin.Maui.Calendar/main/res/W11_ios.png) |

Weekend calendars — the `filled` variant uses `WeekendDayBackgroundColor` to fill weekend cells, a transparent selected-day background and a custom header template.

| Weekend (Android) | Weekend (iOS) | Weekend filled |
| ------- | ------ | ------ |
| ![Weekend calendar Android Screenshot](https://raw.githubusercontent.com/yurkinh/Plugin.Maui.Calendar/main/res/WeekendCalendar_android.png) | ![Weekend calendar IOS Screenshot](https://raw.githubusercontent.com/yurkinh/Plugin.Maui.Calendar/main/res/WeekendCalendar_ios.png) | ![Weekend filled calendar Screenshot](https://raw.githubusercontent.com/yurkinh/Plugin.Maui.Calendar/main/res/WeekendFilledCalendar.png) |

### Usage
To get started just install the package via Nuget.
You can take a look on the sample app to get started or continue reading.

Reference the following xmlns to your page:
```xml
xmlns:controls="clr-namespace:Plugin.Maui.Calendar.Controls;assembly=Plugin.Maui.Calendar" 
```

Basic control usage:
```xml
<controls:Calendar
        Day="14"
        Month="5"
        Year="2019"
        VerticalOptions="Fill"
        HorizontalOptions="Fill"/>
```

Bindable properties:
* `Culture` _CultureInfo_ calender culture/language
* `Day` _int_ currently viewing day
* `Month` _int_ currently viewing month
* `Year` _int_ currently viewing year
* `Events` _EventCollection_ (from package) your events for calender
* Custom colors, fonts, sizes ...


__Remark: You can use `ShownDate` as an alternative to `Year`, `Month` and `Day`__
```xml
<controls:Calendar
        ShownDate="2019-05-14"
        VerticalOptions="Fill"
        HorizontalOptions="Fill"/>
```

#### Binding events:
In your XAML, add the data template for events, and bind the events collection, example:
```xml
<controls:Calendar
    Events="{Binding Events}">
    <controls:Calendar.EventTemplate>
        <DataTemplate>
            <StackLayout
                Padding="15,0,0,0">
                <Label
                    Text="{Binding Name}"
                    FontAttributes="Bold"
                    FontSize="Medium" />
                <Label
                    Text="{Binding Description}"
                    FontSize="Small"
                    LineBreakMode="WordWrap" />
            </StackLayout>
        </DataTemplate>
    </controls:Calendar.EventTemplate>
</controls:Calendar>
```

In your ViewModel reference the following namespace:
```csharp
using Plugin.Maui.Calendar.Models;
```

Add property for Events:
```csharp
public EventCollection Events { get; set; }
```

Initialize Events with your data:
```csharp
 Events = new EventCollection
 {
    [DateTime.Now] = new List<EventModel>
    {
        new() { Name = "Cool event1", Description = "This is Cool event1's description!" },
        new() { Name = "Cool event2", Description = "This is Cool event2's description!" }
    },
    // 5 days from today
    [DateTime.Now.AddDays(5)] = new List<EventModel>
    {
        new() { Name = "Cool event3", Description = "This is Cool event3's description!" },
        new() { Name = "Cool event4", Description = "This is Cool event4's description!" }
    },
    // 3 days ago
    [DateTime.Now.AddDays(-3)] = new List<EventModel>
    {
        new() { Name = "Cool event5", Description = "This is Cool event5's description!" }
    },
    // custom date
    [new DateTime(2024, 3, 16)] = new List<EventModel>
    {
        new() { Name = "Cool event6", Description = "This is Cool event6's description!" }
    }
 };
```

Initialize Events with your data and a different dot color per day:
```csharp
Events = new EventCollection
{
    //2 days ago
    [DateTime.Now.AddDays(-2)] = new DayEventCollection<EventModel>(Colors.Purple, Colors.Purple)
    {
        new() { Name = "Cool event1", Description = "This is Cool event1's description!" },
        new() { Name = "Cool event2", Description = "This is Cool event2's description!" }
    },
    // 5 days ago
    [DateTime.Now.AddDays(-5)] = new DayEventCollection<EventModel>(Colors.Blue, Colors.Blue)
    {
        new() { Name = "Cool event3", Description = "This is Cool event3's description!" },
        new() { Name = "Cool event4", Description = "This is Cool event4's description!" }
    },
};
//4 days ago
Events.Add(DateTime.Now.AddDays(-4), new DayEventCollection<EventModel>(GenerateEvents(10, "Cool")) { EventIndicatorColor = Colors.Green, EventIndicatorSelectedColor = Colors.Green });
```

Where `EventModel` is just an example, it can be replaced by any data model you desire.

`EventsCollection` is just a wrapper over `Dictionary<DateTime, ICollection>` exposing custom `Add` method and `this[DateTime]` indexer which internally extracts the `.Date` component of `DateTime` values and uses it as a key in this dictionary.

`DayEventCollection` is just a wrapper over `List<T>` exposing custom properties `EventIndicatorColor` and `EventIndicatorSelectedColor` for assigning a custom color to the dot.


#### DayTappedCommand
The **DayTappedCommand** is triggered when a user taps on a specific day in the calendar.

XAML Usage:
```xml
DayTappedCommand="{Binding DayTappedCommand}"
```


#### Set up culture

In your ViewModel add property for Culture:
```csharp
public CultureInfo Culture => new CultureInfo("hr-HR")
```

In XAML add Culture binding
```xml
<controls:Calendar
    Culture="{Binding Culture}"/>
</controls:Calendar>
```

#### Available color customization
Sample properties:
```xml
EventIndicatorColor="Red"
EventIndicatorSelectedColor="White"
DeselectedDayTextColor="Blue"
OtherMonthDayColor="Gray"
SelectedDayTextColor="Cyan"
SelectedDayBackgroundColor="DarkCyan"
SelectedTodayTextColor="Green"
TodayOutlineColor="Blue"
TodayFillColor="Silver"
TodayTextColor="Yellow"
OtherMonthSelectedDayColor="HotPink"
```

##### Weekend column background

`WeekendDayBackgroundColor` fills the background of each weekend (Saturday/Sunday) day cell.
The day-of-week title row is left uncovered, and vertically-consecutive weekend days touch
with no gap while remaining individual rounded boxes. It is opt-in and defaults to
`Transparent`, so existing calendars are unaffected. The weekend columns are derived from
`FirstDayOfWeek`. Use `WeekendDayBackgroundCornerRadius` to round each box.

```xml
<controls:Calendar
    WeekendDayBackgroundColor="#EEF0F4"
    WeekendDayBackgroundCornerRadius="12"/>
```

#### Available Styles
| Style Key                       | Based On (`DefaultStyles`)                           |
| ------------------------------- | ---------------------------------------------------- |
| `DaysLabelStyle`                | `DefaultStyles.DefaultLabelStyle`                    |
| `MonthLabelStyle`               | `DefaultStyles.DefaultMonthLabelStyle`               |
| `YearLabelStyle`                | `DefaultStyles.DefaultYearLabelStyle`                |
| `PreviousMonthArrowButtonStyle` | `DefaultStyles.DefaultPreviousMonthArrowButtonStyle` |
| `NextMonthArrowButtonStyle`     | `DefaultStyles.DefaultNextMonthArrowButtonStyle`     |
| `PreviousYearArrowButtonStyle`  | `DefaultStyles.DefaultPreviousYearArrowButtonStyle`  |
| `NextYearArrowButtonStyle`      | `DefaultStyles.DefaultNextYearArrowButtonStyle`      |
| `FooterArrowLabelStyle`         | `DefaultStyles.DefaultFooterArrowLabelStyle`         |
| `SelectedDateLabelStyle`        | `DefaultStyles.DefaultSelectedDateLabelStyle`        |
| `WeekdayTitleStyle`             | `DefaultStyles.DefaultDaysTitleLabelStyle`           |
| `WeekendTitleStyle`             | `DefaultStyles.DefaultWeekendTitleStyle`             |


#### Available customization properties
```xml
FirstDayOfWeek="Monday"
UseNativeDigits="True"
OtherMonthWeekIsVisible="False"
DayViewBorderMargin
AutoChangeMonthOnDayTap="True"
UseAbbreviatedDayNames="True"  <!-- Uses built-in .NET AbbreviatedDayNames; when enabled, DaysTitleMaximumLength is ignored -->
```

#### Calendar Layout customizations
You can set the layout of the calendar with the property `CalendarLayout`

- Available layouts are: 

    `Week` - only one week is shown

    `TwoWeek` - two weeks are shown

    `Month` - the whole month is shown (default value)

```xml
CalendarLayout="Month"
```

You can also choose to display the shown week number instead of the month name

```xml
CalendarLayout="Week"
WeekViewUnit="WeekNumber"
```

##### Event indicator customizations
You can customize how the event indication will look with the property `EventIndicatorType`

- Available indicators are: 
`BottomDot` - event indicator as dot bellow of date in the calendar (default value)
`TopDot` - event indicator as the dot on top of the date in the calendar
`Background` - event indicator as colored background in calendar
`BackgroundFull` // event indicator as larger size colored background in the calendar

```xml
EventIndicatorType="Background"
```
##### Calendar swipe customizations
You can write your own customizations commands for swipe. 
```xml
SwipeLeftCommand="{Binding SwipeLeftCommand}"
SwipeRightCommand="{Binding SwipeRightCommand}"
SwipeUpCommand="{Binding SwipeUpCommand}"
```

You can also disable default swipe actions.
```xml
SwipeToChangeMonthEnabled="False"
SwipeUpToHideEnabled="False"
```

##### Selection type of calendar

You can either use the `Calender` class implementation for a single selection mode, multiselection mode, `RangeSelectionCalendar` for a range selection mode, or `WeekSelectionCalendar` for whole-week selection after tapping one day.

```xml
    <plugin:Calendar
        SelectedDate="{Binding SelectedDate}"/>
```
On the `RangeSelectionCalendar` you can use binding for start date `SelectedStartDate` and end date `SelectedEndDate` or get list of selected dates with `SelectedDates`.
```xml
    <plugin:RangeSelectionCalendar
        x:Name="rangedCalendar"
        SelectedDates="{Binding SelectedDates}"
        SelectedEndDate="{Binding SelectedEndDate}"
        SelectedStartDate="{Binding SelectedStartDate}">
```
On the `MultiselectionCalendar` you can select multiple separate dates

```xml
    <plugin:MultiSelectionCalendar
        Events="{Binding Events}"
        MaximumDate="{Binding MaximumDate}"
        MinimumDate="{Binding MinimumDate}"
        Month="{Binding Month}" >
```

On the `WeekSelectionCalendar`, tapping one day selects the entire week based on `FirstDayOfWeek`.

```xml
    <plugin:WeekSelectionCalendar
        FirstDayOfWeek="Monday"
        SelectedDates="{Binding SelectedDates}" />
```

__Remark: Don't use both `SelectedDates` and `SelectedStartDate`/`SelectedEndDate`__

##### Other customizations

Enable/Disable the visibility of the Events scrollview panel at the bottom
Sample properties:
```xml
EventsScrollViewVisible="True"
```

#### Section templates
There are several templates that can be used to customize the calendar. You can find an example for each one in the AdvancedPage.xaml.
You can create your own custom control file or you can also write customization directly inside of Templates.

##### Calendar control sections
These sections provide customization over appearance of the controls of the calendar, like showing the selected month and year, month selection controls etc.

###### HeaderSectionTemplate
Customize the header section (top of the calendar control). Example from AdvancedPage.xaml
```xml
<plugin:Calendar.HeaderSectionTemplate>
    <controls:CalendarHeader />
</plugin:Calendar.HeaderSectionTemplate>
```

###### FooterSectionTemplate
Customize the footer section (under the calendar part, above the events list). Example from AdvancedPage.xaml
```xml
<plugin:Calendar.FooterSectionTemplate>
    <DataTemplate>
        <controls:CalendarFooter />
    </DataTemplate>
</plugin:Calendar.FooterSectionTemplate>
```

###### BottomSectionTemplate
Customize the bottom section (at the bottom of the calendar control, below the events list). Example from AdvancedPage.xaml
```xml
<plugin:Calendar.BottomSectionTemplate>
    <controls:CalendarBottom />
</plugin:Calendar.BottomSectionTemplate>
```

##### Event templates
These templates provide customization for the events list.

###### EventTemplate
Customize the appearance of the events section. Example from AdvancedPage.xaml
```xml
<plugin:Calendar.EventTemplate>
    <DataTemplate>
        <controls:CalenderEvent CalenderEventCommand="{Binding BindingContext.EventSelectedCommand, Source={x:Reference advancedCalendarPage}}" />
    </DataTemplate>
</plugin:Calendar.EventTemplate>
```

###### EmptyTemplate
Customize what to show in case the selected date has no events. Example from AdvancedPage.xaml
```xml
<plugin:Calendar.EmptyTemplate>
    <DataTemplate>
        <StackLayout>
            <Label Text="NO EVENTS FOR THE SELECTED DATE" HorizontalTextAlignment="Center" Margin="0,5,0,5" />
        </StackLayout>
    </DataTemplate>
</plugin:Calendar.EmptyTemplate>
```

##### Day templates
This template provides full customization over how each individual day cell is rendered.

###### DayViewTemplate
Set `DayViewTemplate` to draw every day cell yourself. The template's `BindingContext` is an `ICalendarDay` (namespace `Plugin.Maui.Calendar.Interfaces`), so the cell can react to the day's date, selection, events and state.

The example below is a trimmed version of the "Photo" template from the sample app's `DayViewTemplatePage.xaml`: the selected day shows a picture, today gets a tinted tile, weekend numbers are green, disabled days are struck out and event days get one dot per event color. `monkey.png` is an image in the app's `Resources/Images` folder.
```xml
xmlns:plugin="clr-namespace:Plugin.Maui.Calendar.Controls;assembly=Plugin.Maui.Calendar"
xmlns:interfaces="clr-namespace:Plugin.Maui.Calendar.Interfaces;assembly=Plugin.Maui.Calendar"
...
<plugin:Calendar x:Name="calendar" DayViewSize="50">
    <plugin:Calendar.DayViewTemplate>
        <DataTemplate x:DataType="interfaces:ICalendarDay">
            <Border Margin="3" Stroke="Transparent" StrokeShape="RoundRectangle 16" StrokeThickness="0">
                <Border.Triggers>
                    <DataTrigger TargetType="Border" Binding="{Binding IsThisMonth}" Value="False">
                        <Setter Property="Opacity" Value="0.3" />
                    </DataTrigger>
                    <DataTrigger TargetType="Border" Binding="{Binding IsToday}" Value="True">
                        <Setter Property="BackgroundColor" Value="#DCFCE7" />
                    </DataTrigger>
                    <DataTrigger TargetType="Border" Binding="{Binding IsSelected}" Value="True">
                        <Setter Property="Stroke" Value="#22C55E" />
                        <Setter Property="StrokeThickness" Value="2" />
                        <Setter Property="Opacity" Value="1" />
                    </DataTrigger>
                </Border.Triggers>
                <Grid>
                    <!-- the picture and a dark overlay are only shown on the selected day -->
                    <Image Aspect="AspectFill" IsVisible="{Binding IsSelected}" Source="monkey.png" />
                    <BoxView IsVisible="{Binding IsSelected}" Color="#80064E2B" />
                    <Grid Padding="0,4,0,6" RowDefinitions="*,Auto">
                        <Label FontAttributes="Bold" HorizontalOptions="Center" Text="{Binding Day}" VerticalOptions="Center">
                            <Label.Triggers>
                                <DataTrigger TargetType="Label" Binding="{Binding IsDisabled}" Value="True">
                                    <Setter Property="TextDecorations" Value="Strikethrough" />
                                </DataTrigger>
                                <!-- later triggers win: the selection overrides the weekend color -->
                                <DataTrigger TargetType="Label" Binding="{Binding IsWeekend}" Value="True">
                                    <Setter Property="TextColor" Value="#15803D" />
                                </DataTrigger>
                                <DataTrigger TargetType="Label" Binding="{Binding IsSelected}" Value="True">
                                    <Setter Property="TextColor" Value="White" />
                                </DataTrigger>
                            </Label.Triggers>
                        </Label>
                        <!-- one dot per event color -->
                        <HorizontalStackLayout Grid.Row="1" BindableLayout.ItemsSource="{Binding EventColors}"
                                               HeightRequest="5" HorizontalOptions="Center" Spacing="3">
                            <BindableLayout.ItemTemplate>
                                <DataTemplate x:DataType="Color">
                                    <Ellipse Fill="{Binding .}" HeightRequest="5" WidthRequest="5" />
                                </DataTemplate>
                            </BindableLayout.ItemTemplate>
                        </HorizontalStackLayout>
                    </Grid>
                </Grid>
            </Border>
        </DataTemplate>
    </plugin:Calendar.DayViewTemplate>
</plugin:Calendar>
```

The sample page also has a "Tiles" template (weekday name from `Date`, weekend tint from `IsWeekend`, an `EventCount` badge, a lock on disabled days), an "Agenda" template that writes the day's event names inside the cell (`Events`), and switches between the templates at runtime.

**Showing the day's events inside the cell**

`Events` holds the objects you stored for the day, so a template can write their text right in the cell:
```xml
<DataTemplate x:DataType="interfaces:ICalendarDay">
    <VerticalStackLayout Padding="2" Spacing="1">
        <Label FontAttributes="Bold" HorizontalOptions="Center" Text="{Binding Day}" />
        <VerticalStackLayout BindableLayout.ItemsSource="{Binding Events}" Spacing="1">
            <BindableLayout.ItemTemplate>
                <DataTemplate x:DataType="model:EventModel">
                    <Label FontSize="8" LineBreakMode="TailTruncation" MaxLines="1" Text="{Binding Name}" />
                </DataTemplate>
            </BindableLayout.ItemTemplate>
        </VerticalStackLayout>
    </VerticalStackLayout>
</DataTemplate>
```

**`ICalendarDay` members**

| Member | Type | Description |
|--------|------|-------------|
| `Date` | `DateTime` | The date the cell shows |
| `Day` | `string` | The day-of-month text (`"1"`..`"31"`), formatted with `Culture` and `UseNativeDigits` |
| `IsSelected` | `bool` | The day is selected. In a `RangeSelectionCalendar` this is `true` for every day of the range, both ends included |
| `IsToday` | `bool` | The day is today. Moves to the new day at midnight while the calendar is shown |
| `IsWeekend` | `bool` | The day is a Saturday or a Sunday. Does not depend on `WeekendDayColor` or `FirstDayOfWeek` |
| `IsThisMonth` | `bool` | The day belongs to the shown month. Always `true` in the `Week` and `TwoWeek` layouts |
| `IsDisabled` | `bool` | The day is before `MinimumDate`, after `MaximumDate` or in `DisabledDates`. Disabled days can't be selected |
| `HasEvents` | `bool` | `Events` has an entry for the day (even an empty one) |
| `EventCount` | `int` | The number of events in the day's `Events` entry; `0` when there is none. Changes inside that entry's own collection are not observed: assign the entry again (`Events[date] = dayEvents`) to refresh the count |
| `Events` | `IReadOnlyList<object>` | The day's events: the objects stored in its `Events` entry, in order, or an empty list. Use it to show event text inside the cell. Like `EventCount`, assign the entry again to refresh it after changing the entry's own collection |
| `EventColors` | `IReadOnlyList<Color>` | The day's event indicator colors: up to five from an event collection that implements `IMultiEventDay` and provides colors, otherwise the single indicator color. Empty when the day has no events |
| `IsRangeStart` | `bool` | The day is the first day of the selected range. Only set by `RangeSelectionCalendar`, `false` in the other calendars |
| `IsRangeEnd` | `bool` | The day is the last day of the selected range. Only set by `RangeSelectionCalendar`, `false` in the other calendars. For a one-day range both `IsRangeStart` and `IsRangeEnd` are `true` |

Day cells are reused: navigating to another month or week gives the same cells new dates. The calendar assigns the members whenever it updates its days, and every member raises `PropertyChanged` when it changes, so bind to the members (or use triggers) instead of reading them once in code.

**Switching templates at runtime**

`DayViewTemplate` can be changed at any time. Only the content of each cell is replaced; the shown month and the selection are kept. Setting it to `null` restores the built-in cell.
```csharp
calendar.DayViewTemplate = (DataTemplate)Resources["TileDayTemplate"];

// back to the built-in day cell
calendar.DayViewTemplate = null;
```

**DataTemplateSelector**

`DayViewTemplate` also accepts a `DataTemplateSelector`. The selector receives the `ICalendarDay` as `item` and the day cell as `container`:
```csharp
public class EventDayTemplateSelector : DataTemplateSelector
{
    public DataTemplate PlainDayTemplate { get; set; }
    public DataTemplate EventDayTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container) =>
        ((ICalendarDay)item).HasEvents ? EventDayTemplate : PlainDayTemplate;
}
```
* The selector is asked when a cell is created and again for every cell each time the calendar updates its days: after navigating, and when the selection, the events, the disabled dates, the minimum or maximum date or today (at midnight) change.
* It always sees the complete, current state of the day, so it can choose by any `ICalendarDay` member.
* A cell only replaces its content when the selector returns a different template instance. Return the same instances every time (like the properties above) and keep the selector cheap.
* Returning `null` shows the built-in cell for that day.

**Good to know**
* The root of the template must be a `View`, such as a `Grid`, `Border` or `Label`. Any other root (for example a `ViewCell`) throws an `InvalidOperationException` when the cell is created.
* The calendar still sizes every cell to `DayViewSize` (the template fills that square), hides the days that `OtherMonthDayIsVisible` and `OtherMonthWeekIsVisible` hide, selects a day when its cell is tapped (`DayTappedCommand`, `AllowDeselecting` and `AutoChangeMonthOnDayTap` work as usual) and draws `WeekendDayBackgroundColor` behind the cells.
* `EventIndicatorColor` and `EventIndicatorSelectedColor` still provide `EventColors` for days whose events don't set their own colors.
* These properties only style the built-in cell and are **ignored** when a template is set: `DaysLabelStyle`, `DayViewCornerRadius`, `DayViewBorderMargin`, `EventIndicatorType` (including the `BackgroundFull` cell background), `SelectedDayBackgroundColor`, `SelectedDayTextColor`, `SelectedTodayTextColor`, `DeselectedDayTextColor`, `TodayOutlineColor`, `TodayFillColor`, `TodayTextColor`, `WeekendDayColor`, `OtherMonthDayColor`, `OtherMonthSelectedDayColor`, `DisabledDayColor`, `EventIndicatorTextColor`, `EventIndicatorSelectedTextColor`, and `SelectedDatesRangeBackgroundColor` on `RangeSelectionCalendar`. Draw these states in the template from the `ICalendarDay` members.
* Taps are handled by the cell around the template. A child that handles input itself, such as a `Button`, a `CheckBox` or a view with its own gesture recognizers, takes the tap and the day is not selected. Set `InputTransparent="True"` on such a child if tapping it should select the day.
* In the `Week` and `TwoWeek` layouts `IsThisMonth` is always `true`, so a template that fades other-month days shows every day normally there.
* In a `RangeSelectionCalendar`, style the whole range with `IsSelected` and its first and last days with `IsRangeStart` and `IsRangeEnd`.
