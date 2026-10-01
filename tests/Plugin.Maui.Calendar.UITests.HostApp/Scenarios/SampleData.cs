using System.Globalization;
using Microsoft.Maui.Controls.Shapes;
using Plugin.Maui.Calendar.Interfaces;
using Plugin.Maui.Calendar.Models;
using CalendarControl = Plugin.Maui.Calendar.Controls.Calendar;

namespace Plugin.Maui.Calendar.UITests.HostApp;

/// <summary>An event shown by the scenarios.</summary>
public sealed record CalendarEvent(string Name)
{
	public override string ToString() => Name;
}

/// <summary>The events of a day with one indicator color per event (up to five are shown).</summary>
public sealed class MultiColorDayEvents(params Color[] colors) : List<CalendarEvent>, IMultiEventDay
{
	public IReadOnlyList<Color> Colors { get; } = colors;
}

/// <summary>The events of a day that pick their own indicator colors.</summary>
public sealed class PersonalizedDayEvents : List<CalendarEvent>, IPersonalizableDayEvent
{
	public Color? EventIndicatorColor { get; set; }

	public Color? EventIndicatorSelectedColor { get; set; }

	public Color? EventIndicatorTextColor { get; set; }

	public Color? EventIndicatorSelectedTextColor { get; set; }
}

/// <summary>Events, templates and styles shared by the scenarios.</summary>
public static class SampleData
{
	public static DateTime May(int day) => new(2025, 5, day);

	public static DateTime June(int day) => new(2025, 6, day);

	/// <summary>
	/// Plain events on May 5, two events today (May 14), three colors on May 20, a personalized day on
	/// May 22 and an event in the next month (June 2), which the May grid shows too.
	/// </summary>
	public static EventCollection Events() => new()
	{
		[May(5)] = new List<CalendarEvent> { new("Dentist") },
		[May(14)] = new List<CalendarEvent> { new("Standup"), new("Review") },
		[May(20)] = new MultiColorDayEvents(Colors.Red, Colors.Green, Colors.Blue) { new("Trip"), new("Hotel"), new("Museum") },
		[May(22)] = Personalized(new CalendarEvent("Party")),
		[June(2)] = new List<CalendarEvent> { new("Holiday") },
	};

	static PersonalizedDayEvents Personalized(params CalendarEvent[] events)
	{
		// The text color stays readable on white (dot indicators) and on the indicator color (background indicators).
		var dayEvents = new PersonalizedDayEvents
		{
			EventIndicatorColor = Colors.MediumPurple,
			EventIndicatorSelectedColor = Colors.Orange,
			EventIndicatorTextColor = Colors.Indigo,
			EventIndicatorSelectedTextColor = Colors.Black,
		};
		dayEvents.AddRange(events);
		return dayEvents;
	}

	/// <summary>A row of the events list under the calendar.</summary>
	public static DataTemplate EventTemplate() => new(() =>
	{
		var name = new Label { FontSize = 14, Padding = new Thickness(8, 4), TextColor = Colors.Black };
		name.SetBinding(Label.TextProperty, static (CalendarEvent calendarEvent) => calendarEvent.Name);
		return name;
	});

	public static DataTemplate EmptyEventsTemplate() => new(() => new Label
	{
		Text = "No events",
		FontSize = 14,
		Padding = new Thickness(8, 4),
		TextColor = Colors.Gray,
	});

	/// <summary>A day cell drawn by the app: a rounded box that is filled when selected, with a dot for events.</summary>
	public static DataTemplate DayTemplate(Color selectedColor) => new(() =>
	{
		var box = new Border
		{
			StrokeShape = new RoundRectangle { CornerRadius = 6 },
			Stroke = Colors.LightGray,
			Margin = new Thickness(2),
		};
		box.SetBinding(VisualElement.BackgroundColorProperty, static (ICalendarDay day) => day.IsSelected, converter: new BoolToColorConverter(selectedColor, Colors.Transparent));

		var number = new Label { HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, FontSize = 14 };
		number.SetBinding(Label.TextProperty, static (ICalendarDay day) => day.Day);
		number.SetBinding(Label.TextColorProperty, static (ICalendarDay day) => day.IsThisMonth, converter: new BoolToColorConverter(Colors.Black, Colors.LightGray));

		var dot = new Ellipse { Fill = Colors.OrangeRed, WidthRequest = 6, HeightRequest = 6, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.End, Margin = new Thickness(0, 0, 0, 3) };
		dot.SetBinding(VisualElement.IsVisibleProperty, static (ICalendarDay day) => day.HasEvents);

		box.Content = new Grid { Children = { number, dot } };
		return box;
	});

	/// <summary>Weekends get a striped look, the other days <see cref="DayTemplate"/>.</summary>
	public static DataTemplateSelector WeekendSelector() => new WeekendTemplateSelector
	{
		Weekday = DayTemplate(Colors.SteelBlue),
		Weekend = new DataTemplate(() =>
		{
			var number = new Label
			{
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
				FontSize = 14,
				FontAttributes = FontAttributes.Italic,
				TextColor = Colors.DarkRed,
				BackgroundColor = Color.FromArgb("#FDECEA"),
			};
			number.SetBinding(Label.TextProperty, static (ICalendarDay day) => day.Day);
			return number;
		}),
	};

	/// <summary>A tall cell that lists the names of the day's first two events.</summary>
	public static DataTemplate EventNamesTemplate() => new(() =>
	{
		var number = new Label { FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Colors.Black };
		number.SetBinding(Label.TextProperty, static (ICalendarDay day) => day.Day);

		var names = new VerticalStackLayout { Spacing = 1 };
		BindableLayout.SetItemTemplate(names, new DataTemplate(() =>
		{
			var name = new Label { FontSize = 8, TextColor = Colors.White, BackgroundColor = Colors.SteelBlue, LineBreakMode = LineBreakMode.TailTruncation, Padding = new Thickness(2, 0) };
			name.SetBinding(Label.TextProperty, static (CalendarEvent calendarEvent) => calendarEvent.Name);
			return name;
		}));
		names.SetBinding(BindableLayout.ItemsSourceProperty, static (ICalendarDay day) => day.Events, converter: new FirstItemsConverter(2));

		var cell = new VerticalStackLayout { Padding = new Thickness(2), Spacing = 2, Children = { number, names } };
		cell.SetBinding(VisualElement.BackgroundColorProperty, static (ICalendarDay day) => day.IsSelected, converter: new BoolToColorConverter(Color.FromArgb("#DCEBFA"), Colors.Transparent));
		return cell;
	});

	/// <summary>A header with its own arrows and title, bound to the calendar.</summary>
	public static DataTemplate CustomHeader() => new(() =>
	{
		var previous = new Button { AutomationId = "CustomPrev", Text = "Previous", FontSize = 12 };
		previous.SetBinding(Button.CommandProperty, static (CalendarControl calendar) => calendar.PrevLayoutUnitCommand);

		var next = new Button { AutomationId = "CustomNext", Text = "Next", FontSize = 12 };
		next.SetBinding(Button.CommandProperty, static (CalendarControl calendar) => calendar.NextLayoutUnitCommand);

		var title = new Label { AutomationId = "CustomTitle", FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = Colors.DarkSlateBlue, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
		title.SetBinding(Label.TextProperty, static (CalendarControl calendar) => calendar.LayoutUnitText);

		var header = new Grid { ColumnDefinitions = [new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto)], Padding = new Thickness(4) };
		header.Add(previous, 0);
		header.Add(title, 1);
		header.Add(next, 2);
		return header;
	});

	/// <summary>A footer that only shows the selected dates.</summary>
	public static DataTemplate CustomFooter() => new(() =>
	{
		var text = new Label { AutomationId = "CustomFooterText", FontSize = 14, TextColor = Colors.DarkSlateBlue, Padding = new Thickness(8), BackgroundColor = Color.FromArgb("#EEF2FF") };
		text.SetBinding(Label.TextProperty, static (CalendarControl calendar) => calendar.SelectedDateText, stringFormat: "Selected: {0}");
		return text;
	});

	public static Style LabelStyle(Color color, double fontSize, FontAttributes attributes = FontAttributes.None) => new(typeof(Label))
	{
		Setters =
		{
			new Setter { Property = Label.TextColorProperty, Value = color },
			new Setter { Property = Label.FontSizeProperty, Value = fontSize },
			new Setter { Property = Label.FontAttributesProperty, Value = attributes },
			new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center },
			new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center },
		},
	};

	public static Style ArrowStyle(string text, Color background) => new(typeof(Button))
	{
		Setters =
		{
			new Setter { Property = Button.TextProperty, Value = text },
			new Setter { Property = Button.TextColorProperty, Value = Colors.White },
			new Setter { Property = Button.BackgroundColorProperty, Value = background },
			new Setter { Property = Button.CornerRadiusProperty, Value = 16 },
			new Setter { Property = Button.FontSizeProperty, Value = 14 },
			new Setter { Property = Button.PaddingProperty, Value = new Thickness(0) },
			new Setter { Property = Button.HeightRequestProperty, Value = 32 },
			new Setter { Property = Button.WidthRequestProperty, Value = 32 },
			new Setter { Property = View.HorizontalOptionsProperty, Value = LayoutOptions.Center },
		},
	};

	sealed class WeekendTemplateSelector : DataTemplateSelector
	{
		public required DataTemplate Weekday { get; init; }

		public required DataTemplate Weekend { get; init; }

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container) =>
			((ICalendarDay)item).IsWeekend ? Weekend : Weekday;
	}

	sealed class BoolToColorConverter(Color whenTrue, Color whenFalse) : IValueConverter
	{
		public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is true ? whenTrue : whenFalse;

		public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
	}

	sealed class FirstItemsConverter(int count) : IValueConverter
	{
		public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
			value is IEnumerable<object> items ? items.Take(count).ToList() : new List<object>();

		public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
	}
}
