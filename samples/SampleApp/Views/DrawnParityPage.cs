using System.Globalization;
using Calendar = Plugin.Maui.Calendar.Controls.Calendar;
using Plugin.Maui.Calendar.Controls;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Models;
using Microsoft.Maui.Controls.Xaml;
using SampleApp.Model;

namespace SampleApp.Views;

/// <summary>
/// DrawnUI migration, phase 2: one scenario of the default day cell with the native grid (above) and
/// the drawn grid (below), for a side-by-side comparison. Opened instead of the shell when the app is
/// launched with CALPARITY=n (iOS) or the calparity intent extra (Android).
/// </summary>
public sealed class DrawnParityPage : ContentPage
{
	const string drawnDayGridSwitch = "Plugin.Maui.Calendar.DrawnDayGrid";
	const string graphicsDayGridSwitch = "Plugin.Maui.Calendar.GraphicsDayGrid";

	/// <summary>
	/// Whether the grid below is drawn with Microsoft.Maui.Graphics instead of DrawnUI (the proof of
	/// concept of the migration away from DrawnUI): CALGRAPHICS=1 or the calgraphics intent extra.
	/// </summary>
	public static bool Graphics { get; set; } = Environment.GetEnvironmentVariable("CALGRAPHICS") == "1";

	static readonly DateTime october = new(2026, 10, 1);

	/// <summary>The scenario to show, or -1 when the parity page was not requested.</summary>
	public static int Scenario { get; set; } =
		int.TryParse(Environment.GetEnvironmentVariable("CALPARITY"), out var scenario) ? scenario : -1;

	static DateTime Oct(int day) => new(2026, 10, day);

	static readonly (string Name, Func<Calendar> Create)[] scenarios =
	[
		("0 default: events, selected 14, disabled 20-21, minimum 3", () => new Calendar
		{
			Events = Events(), SelectedDate = Oct(14), DisabledDates = [Oct(20), Oct(21)], MinimumDate = Oct(3),
		}),
		("1 TopDot, multi-color dots, personalized colors", () => new Calendar
		{
			EventIndicatorType = EventIndicatorType.TopDot, Events = ColoredEvents(), SelectedDate = Oct(9),
		}),
		("2 Background indicator, selected event day 9", () => new Calendar
		{
			EventIndicatorType = EventIndicatorType.Background, Events = Events(), SelectedDate = Oct(9),
			EventIndicatorColor = Colors.LightPink, EventIndicatorSelectedColor = Colors.DeepPink,
			EventIndicatorTextColor = Colors.DarkRed, EventIndicatorSelectedTextColor = Colors.White,
		}),
		("3 BackgroundFull indicator", () => new Calendar
		{
			EventIndicatorType = EventIndicatorType.BackgroundFull, Events = Events(), SelectedDate = Oct(14),
			EventIndicatorColor = Colors.LightGreen,
		}),
		("4 sizes 32x56, radius 6, margin 2, custom colors", () => new Calendar
		{
			DayViewSize = 32, DayViewHeight = 56, DayViewCornerRadius = 6, DayViewBorderMargin = new Thickness(2),
			Events = Events(), SelectedDate = Oct(14), DisabledDates = [Oct(20)],
			SelectedDayBackgroundColor = Colors.Orange, SelectedDayTextColor = Colors.Black,
			TodayFillColor = Colors.LightYellow, TodayOutlineColor = Colors.Green, TodayTextColor = Colors.Green,
			OtherMonthDayColor = Colors.Red, WeekendDayColor = Colors.Purple, DisabledDayColor = Colors.LightGray,
			DeselectedDayTextColor = Colors.DarkBlue,
		}),
		("5 other month days and weeks hidden", () => new Calendar
		{
			Events = Events(), OtherMonthDayIsVisible = false, OtherMonthWeekIsVisible = false, SelectedDate = Oct(14),
		}),
		("6 range selection 12-18", () => new RangeSelectionCalendar
		{
			Events = Events(), SelectedStartDate = Oct(12), SelectedEndDate = Oct(18),
			SelectedDatesRangeBackgroundColor = Colors.LightSkyBlue,
		}),
		("7 ar-EG, native digits", () => new Calendar
		{
			Culture = new CultureInfo("ar-EG"), UseNativeDigits = true, Events = Events(), SelectedDate = Oct(14),
		}),
		("8 zh-CN, Monday first, abbreviated names", () => new Calendar
		{
			Culture = new CultureInfo("zh-CN"), FirstDayOfWeek = DayOfWeek.Monday, UseAbbreviatedDayNames = true,
			Events = Events(), SelectedDate = Oct(14),
		}),
		("9 hi-IN, native digits", () => new Calendar
		{
			Culture = new CultureInfo("hi-IN"), UseNativeDigits = true, Events = Events(), SelectedDate = Oct(14),
		}),
		("10 two weeks", () => new Calendar
		{
			CalendarLayout = WeekLayout.TwoWeek, Events = Events(), SelectedDate = Oct(14), ShownDate = Oct(8),
		}),
		("11 custom label styles: font family, bold, red weekend titles", () => new Calendar
		{
			Events = Events(), SelectedDate = Oct(14),
			DaysLabelStyle = LabelStyle(fontSize: 16, fontFamily: "OpenSansSemibold", bold: true),
			DaysTitleLabelStyle = LabelStyle(textColor: Colors.Gray, fontFamily: "OpenSansRegular"),
			WeekendTitleStyle = LabelStyle(textColor: Colors.Red, fontSize: 18, bold: true),
		}),
		("12 styles without FontSize", () => new Calendar
		{
			Events = Events(), SelectedDate = Oct(14),
			DaysLabelStyle = LabelStyle(textColor: Colors.Black),
			DaysTitleLabelStyle = LabelStyle(textColor: Colors.Teal),
			WeekendTitleStyle = LabelStyle(textColor: Colors.Teal),
		}),
		("13 theme: implicit app Label style, theme color, dynamic font", () => new Calendar
		{
			Events = Events(), SelectedDate = Oct(14),
			DaysTitleLabelStyle = LabelStyle(fontSize: 15),
			WeekendTitleStyle = ThemeColorStyle(Colors.Red, Colors.Orange),
			DaysLabelStyle = DynamicFontStyle("ParityDayFont"),
		}),
		("14 weekend bands", () => new Calendar
		{
			Events = Events(), SelectedDate = Oct(14), WeekendDayBackgroundColor = Colors.LightGray,
		}),
		("15 rounded bands, Monday first, hidden other weeks, weekend text", () => new Calendar
		{
			Events = Events(), SelectedDate = Oct(17), FirstDayOfWeek = DayOfWeek.Monday,
			WeekendDayBackgroundColor = Color.FromArgb("#FFE3E3"), WeekendDayBackgroundCornerRadius = 10,
			WeekendDayColor = Colors.Red, OtherMonthWeekIsVisible = false,
		}),
		("16 input: inside a ScrollView", () => new Calendar { Events = Events(), SelectedDate = Oct(14) }),
		("17 input: no ScrollView", () => new Calendar { Events = Events(), SelectedDate = Oct(14) }),
		("18 DayViewTemplate set at 3 s, cleared at 6 s", () => new Calendar { Events = Events(), SelectedDate = Oct(14) }),
		("19 text: character spacing, italic, lowercase and uppercase", () => new Calendar
		{
			Events = Events(), SelectedDate = Oct(14),
			DaysLabelStyle = TextStyle(fontSize: 14, attributes: FontAttributes.Italic, characterSpacing: 2),
			DaysTitleLabelStyle = TextStyle(fontSize: 13, characterSpacing: 3, transform: TextTransform.Lowercase),
			WeekendTitleStyle = TextStyle(fontSize: 13, attributes: FontAttributes.Bold | FontAttributes.Italic, transform: TextTransform.Uppercase),
		}),
		("20 text: margin, padding, opacity, end alignment, custom italic font", () => new Calendar
		{
			Events = Events(), SelectedDate = Oct(14),
			DaysLabelStyle = TextStyle(fontSize: 15, fontFamily: "OpenSansRegular", attributes: FontAttributes.Italic, opacity: 0.6),
			DaysTitleLabelStyle = TextStyle(fontSize: 12, margin: new Thickness(2, 6, 8, 4), padding: new Thickness(0, 3, 0, 1), alignment: TextAlignment.End),
			WeekendTitleStyle = TextStyle(fontSize: 12, margin: new Thickness(2, 6, 8, 4), alignment: TextAlignment.Start, opacity: 0.5),
		}),
		("21 text: ar-EG with character spacing and bold", () => new Calendar
		{
			Culture = new CultureInfo("ar-EG"), UseNativeDigits = true, Events = Events(), SelectedDate = Oct(14),
			DaysLabelStyle = TextStyle(fontSize: 15, attributes: FontAttributes.Bold, characterSpacing: 2),
			DaysTitleLabelStyle = TextStyle(fontSize: 14, characterSpacing: 2),
			WeekendTitleStyle = TextStyle(fontSize: 14, attributes: FontAttributes.Bold),
		}),
	];

	// The input scenarios show what each calendar received, under it.
	static bool IsInputScenario => Scenario is >= 16 and <= 18;

	public DrawnParityPage()
	{
		var (name, create) = scenarios[Math.Clamp(Scenario, 0, scenarios.Length - 1)];

		Calendar Create(bool drawn)
		{
			AppContext.SetSwitch(drawnDayGridSwitch, drawn && !Graphics);
			AppContext.SetSwitch(graphicsDayGridSwitch, drawn && Graphics);
			var calendar = create();
			calendar.ShownDate = calendar.ShownDate == default || calendar.CalendarLayout == WeekLayout.Month ? october : calendar.ShownDate;
			calendar.HeaderSectionVisible = false;
			calendar.FooterSectionVisible = false;
			calendar.EventsScrollViewVisible = false;
			return calendar;
		}

		Resources["ParityDayFont"] = "OpenSansSemibold";

		var native = Create(drawn: false);
		var drawn = Create(drawn: true);

		var stack = new VerticalStackLayout
		{
			Padding = new Thickness(8, 50, 8, 8),
			Spacing = 10,
			Children = { new Label { Text = (Graphics ? "[Maui.Graphics] " : "[DrawnUI] ") + name, FontSize = 12 }, native },
		};

		if (IsInputScenario)
		{
			stack.Children.Add(InputLog(native, "NativeLog"));
		}

		stack.Children.Add(new BoxView { HeightRequest = 1, Color = Colors.Gray });
		stack.Children.Add(drawn);

		if (IsInputScenario)
		{
			stack.Children.Add(InputLog(drawn, "DrawnLog"));
		}

		// Scenario 18: both calendars get a DayViewTemplate and lose it again while shown.
		if (Scenario == 18)
		{
			Dispatcher.StartTimer(TimeSpan.FromSeconds(3), () =>
			{
				native.DayViewTemplate = drawn.DayViewTemplate = DayTemplate();
				Dispatcher.StartTimer(TimeSpan.FromSeconds(3), () =>
				{
					native.DayViewTemplate = drawn.DayViewTemplate = null;
					return false;
				});
				return false;
			});
		}

		// CALSHOT=1: the page saves a picture of itself, where the screen cannot be captured from
		// outside the app (Mac Catalyst); the path is written to the console with a CALSHOT prefix.
		if (Environment.GetEnvironmentVariable("CALSHOT") == "1")
		{
			Dispatcher.StartTimer(TimeSpan.FromSeconds(4), () =>
			{
				_ = SavePicture();
				return false;
			});
		}

		// Scenario 16: content below the calendars, so the page scrolls.
		if (Scenario == 16)
		{
			stack.Children.Add(new BoxView { HeightRequest = 1500, Color = Colors.LightBlue });
			Content = new ScrollView { Content = stack };
		}
		else
		{
			Content = stack;
		}
	}

	async Task SavePicture()
	{
		try
		{
			var picture = Screenshot.Default.IsCaptureSupported ? await Screenshot.Default.CaptureAsync() : await this.CaptureAsync();
			var path = Path.Combine(FileSystem.CacheDirectory, $"parity-{(Graphics ? "graphics" : "drawn")}-{Scenario}.png");
			await using (var file = File.Create(path))
			{
				await picture.CopyToAsync(file, ScreenshotFormat.Png);
			}

			Console.WriteLine($"CALSHOT {path}");
		}
		catch (Exception exception)
		{
			Console.WriteLine($"CALSHOT failed: {exception.Message}");
		}
	}

	// A day cell of an app: the day number on a rounded square, orange when selected.
	static DataTemplate DayTemplate() => new(() =>
	{
		var label = new Label { HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, FontSize = 13 };
		label.SetBinding(Label.TextProperty, static (Plugin.Maui.Calendar.Interfaces.ICalendarDay day) => day.Day);
		var border = new Border
		{
			StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 6 },
			Stroke = Colors.Teal,
			Margin = new Thickness(3),
			Content = label,
		};
		border.SetBinding(VisualElement.BackgroundColorProperty, static (Plugin.Maui.Calendar.Interfaces.ICalendarDay day) => day.IsSelected,
			converter: new SelectedColorConverter());
		return border;
	});

	// What a calendar received: its swipes by direction, the shown month and whether its days are shown.
	static Label InputLog(Calendar calendar, string automationId)
	{
		var log = new Label { FontSize = 12, AutomationId = automationId };
		int left = 0, right = 0, up = 0, down = 0;

		void Show() => log.Text =
			$"swipes L{left} R{right} U{up} D{down} | month {calendar.ShownDate:yyyy-MM} | selected {calendar.SelectedDate:dd} | shown {calendar.CalendarSectionShown}";

		calendar.SwipedLeft += (_, _) => { left++; Show(); };
		calendar.SwipedRight += (_, _) => { right++; Show(); };
		calendar.SwipedUp += (_, _) => { up++; Show(); };
		calendar.SwipedDown += (_, _) => { down++; Show(); };
		calendar.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName is nameof(Calendar.ShownDate) or nameof(Calendar.SelectedDate) or nameof(Calendar.CalendarSectionShown))
			{
				Show();
			}
		};
		Show();
		return log;
	}

	static EventCollection Events()
	{
		var events = new EventCollection();
		foreach (var day in new[] { 1, 4, 7, 9, 13, 16, 22, 25, 28, 31 })
		{
			events[Oct(day)] = new List<string> { "Event" };
		}
		events[new DateTime(2026, 9, 28)] = new List<string> { "Event" };
		events[new DateTime(2026, 11, 3)] = new List<string> { "Event" };
		return events;
	}

	static EventCollection ColoredEvents()
	{
		var events = Events();
		events[Oct(9)] = new DayEventCollection<string>(Colors.Orange, Colors.DarkOrange) { "A", "B", "C" };
		events[Oct(13)] = new DayEventCollection<string> { "A", "B", "C" };
		((DayEventCollection<string>)events[Oct(13)]).Colors = [Colors.Red, Colors.Green, Colors.Blue];
		events[Oct(16)] = new DayEventCollection<string> { "A" };
		((DayEventCollection<string>)events[Oct(16)]).Colors = [Colors.Red, Colors.Orange, Colors.Yellow, Colors.Green, Colors.Blue];
		return events;
	}

	// A theme-dependent setter value, written as an app writes it: {AppThemeBinding} in XAML.
	static Style ThemeColorStyle(Color light, Color dark)
	{
		var resources = new ResourceDictionary().LoadFromXaml($$"""
			<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
			    <Style x:Key="Weekend" TargetType="Label">
			        <Setter Property="FontSize" Value="15" />
			        <Setter Property="FontAttributes" Value="Bold" />
			        <Setter Property="HorizontalTextAlignment" Value="Center" />
			        <Setter Property="TextColor" Value="{AppThemeBinding Light={{light.ToArgbHex()}}, Dark={{dark.ToArgbHex()}}}" />
			    </Style>
			</ResourceDictionary>
			""");
		return (Style)resources["Weekend"];
	}

	// A setter value from a dynamic resource, which the page defines (see the constructor).
	static Style DynamicFontStyle(string key)
	{
		var style = LabelStyle(fontSize: 15);
		style.Setters.Add(new Setter { Property = Label.FontFamilyProperty, Value = new Microsoft.Maui.Controls.Internals.DynamicResource(key) });
		return style;
	}

	// A label style that sets only what it is given.
	static Style TextStyle(
		double? fontSize = null,
		string fontFamily = null,
		FontAttributes attributes = FontAttributes.None,
		double characterSpacing = 0,
		TextTransform? transform = null,
		Thickness? margin = null,
		Thickness? padding = null,
		TextAlignment? alignment = null,
		double? opacity = null)
	{
		var style = new Style(typeof(Label));
		void Set(BindableProperty property, object value) => style.Setters.Add(new Setter { Property = property, Value = value });

		if (fontSize is { } size) { Set(Label.FontSizeProperty, size); }
		if (fontFamily is not null) { Set(Label.FontFamilyProperty, fontFamily); }
		if (attributes != FontAttributes.None) { Set(Label.FontAttributesProperty, attributes); }
		if (characterSpacing != 0) { Set(Label.CharacterSpacingProperty, characterSpacing); }
		if (transform is { } textTransform) { Set(Label.TextTransformProperty, textTransform); }
		if (margin is { } labelMargin) { Set(View.MarginProperty, labelMargin); }
		if (padding is { } labelPadding) { Set(Label.PaddingProperty, labelPadding); }
		if (alignment is { } textAlignment) { Set(Label.HorizontalTextAlignmentProperty, textAlignment); }
		if (opacity is { } labelOpacity) { Set(VisualElement.OpacityProperty, labelOpacity); }
		return style;
	}

	static Style LabelStyle(Color textColor = null, double? fontSize = null, string fontFamily = null, bool bold = false)
	{
		var style = new Style(typeof(Label));
		if (textColor is not null)
		{
			style.Setters.Add(new Setter { Property = Label.TextColorProperty, Value = textColor });
		}
		if (fontSize is { } size)
		{
			style.Setters.Add(new Setter { Property = Label.FontSizeProperty, Value = size });
		}
		if (fontFamily is not null)
		{
			style.Setters.Add(new Setter { Property = Label.FontFamilyProperty, Value = fontFamily });
		}
		if (bold)
		{
			style.Setters.Add(new Setter { Property = Label.FontAttributesProperty, Value = FontAttributes.Bold });
		}
		style.Setters.Add(new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center });
		return style;
	}

	sealed class SelectedColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
			value is true ? Colors.Orange : Colors.Transparent;

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
			throw new NotSupportedException();
	}
}
