using System.ComponentModel;
using AppoMobi.Gestures;
using DrawnUi.Draw;
using DrawnUi.Models;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls.Drawn;

/// <summary>
/// The drawn built-in day cell, element for element the native one of <c>DayView.xaml</c>:
/// <list type="bullet">
/// <item>the cell itself (the DayView), as wide as its column, painted for <see cref="EventIndicatorType.BackgroundFull"/>;</item>
/// <item>a <c>DayViewSize</c> × <c>DayViewHeight</c> container centered in it (the container Grid);</item>
/// <item>in the container a background shape (the Border) under a column (the FlexLayout) of a
/// spacer, the day label and the event dot row (the HorizontalStackLayout), reversed for
/// <see cref="EventIndicatorType.TopDot"/>.</item>
/// </list>
/// The cell follows its <see cref="DayModel"/> through PropertyChanged rather than bindings.
/// </summary>
sealed class DrawnDayCell : SkiaLayer
{
	// DayModel.EventColors holds at most five colors (see Calendar.AssignIndicatorColors).
	const int maximumDots = 5;
	const double dotSize = 8;

	readonly Action<DayModel> tapped;
	readonly SkiaLayer container;
	readonly SkiaShape background;
	readonly DrawnOutline outline;
	readonly SkiaLayout content;
	readonly SkiaControl spacer;
	readonly SkiaLabel label;
	readonly SkiaRow dots;

	DayModel day;
	bool? topDot;
	Style labelStyle;
	bool applyPending;

	internal DrawnDayCell(Action<DayModel> tapped)
	{
		this.tapped = tapped;
		HorizontalOptions = LayoutOptions.Fill;

		// The Border: its fill, and its stroke drawn over it by an outline of the same size.
		background = new SkiaShape
		{
			Type = ShapeType.Rectangle,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
		};
		outline = new DrawnOutline
		{
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
		};

		spacer = new SkiaControl { HeightRequest = 4 };
		label = new SkiaLabel { HorizontalOptions = LayoutOptions.Center };
		dots = new SkiaRow { HeightRequest = dotSize, Spacing = 0, HorizontalOptions = LayoutOptions.Center };

		for (var i = 0; i < maximumDots; i++)
		{
			dots.AddSubView(new SkiaShape
			{
				Type = ShapeType.Circle,
				WidthRequest = dotSize,
				HeightRequest = dotSize,
				IsVisible = false,
			});
		}

		content = new SkiaLayout
		{
			Type = LayoutType.Column,
			Spacing = 0,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
		};

		container = new SkiaLayer
		{
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
		};
		container.AddSubView(background);
		container.AddSubView(outline);
		container.AddSubView(content);
		AddSubView(container);

		// A screen reader reads each day as a button (phase 7 preview).
		AccessibilityRole = Aria.RoleButton;

		ConsumeGestures += OnCellGestures;
	}

	internal DayModel Day
	{
		get => day;
		set
		{
			if (ReferenceEquals(day, value))
			{
				return;
			}

			if (day is not null)
			{
				day.PropertyChanged -= OnDayChanged;
			}

			day = value;

			if (day is not null)
			{
				day.PropertyChanged += OnDayChanged;
				Apply();
			}
		}
	}

	// An update pass of the calendar changes many properties of every day: the cell shows them
	// once, after the pass, instead of once per property.
	void OnDayChanged(object sender, PropertyChangedEventArgs e)
	{
		if (applyPending)
		{
			return;
		}

		if (Dispatcher is not { } dispatcher)
		{
			Apply();
			return;
		}

		applyPending = true;
		dispatcher.Dispatch(() =>
		{
			applyPending = false;
			if (day is not null)
			{
				Apply();
			}
		});
	}

	/// <summary>Shows the current state of the day, as the bindings of DayView.xaml would.</summary>
	void Apply()
	{
		// The DayView is hidden (IsControlVisible), so it takes no room and a week of hidden days
		// takes no row; inside it the container is hidden (IsVisible) and keeps its room.
		IsVisible = day.IsControlVisible;
		HeightRequest = day.DayViewHeight;
		BackgroundColor = day.BackgroundFullEventColor;

		container.WidthRequest = day.DayViewSize;
		container.HeightRequest = day.DayViewHeight;
		container.Opacity = day.IsVisible ? 1 : 0;

		// The Border fills the container up to DayViewSize (its maximum size), inside its margin.
		var margin = day.DayViewBorderMargin;
		var width = Math.Max(0, Math.Min(day.DayViewSize, day.DayViewSize - margin.HorizontalThickness));
		var height = Math.Max(0, Math.Min(day.DayViewSize, day.DayViewHeight - margin.VerticalThickness));

		background.Margin = margin;
		background.WidthRequest = width;
		background.HeightRequest = height;
		background.CornerRadius = day.DayViewCornerRadius;
		background.BackgroundColor = day.BackgroundColor;

		outline.Margin = margin;
		outline.WidthRequest = width;
		outline.HeightRequest = height;
		outline.CornerRadius = day.DayViewCornerRadius;
		outline.Color = day.OutlineColor;

		// The font of the text depends on the style and the text: both are applied only when they change.
		var styleChanged = !ReferenceEquals(labelStyle, day.DaysLabelStyle);
		if (styleChanged)
		{
			labelStyle = day.DaysLabelStyle;
			DrawnText.ApplyLabelStyle(label, labelStyle, DrawnText.DefaultLabelFontSize);
		}

		if (styleChanged || label.Text != day.Day)
		{
			DrawnText.SetText(label, day.Day, labelStyle);
		}
		label.TextColor = day.TextColor;

		// The dot row shows EventColors whatever the indicator type, like the native cell.
		var colors = day.EventColors ?? [];
		for (var i = 0; i < maximumDots; i++)
		{
			var dot = (SkiaShape)dots.Views[i];
			dot.IsVisible = i < colors.Count;
			if (dot.IsVisible)
			{
				dot.BackgroundColor = colors[i];
			}
		}

		ArrangeContent(day.EventIndicatorType == EventIndicatorType.TopDot);

		AccessibilityLabel = day.Date.ToLongDateString();
		AccessibilityIsPressed = day.IsSelected;
		AccessibilityCanInteract = day.IsVisible && !day.IsDisabled;
	}

	// The dot row sits above the label for TopDot and below it otherwise, in every cell, so the
	// numbers stay aligned across the grid.
	void ArrangeContent(bool dotsOnTop)
	{
		if (topDot == dotsOnTop)
		{
			return;
		}

		topDot = dotsOnTop;
		content.ClearChildren();

		foreach (var child in dotsOnTop ? new SkiaControl[] { dots, label, spacer } : [spacer, label, dots])
		{
			content.AddSubView(child);
		}
	}

	void OnCellGestures(object sender, SkiaGesturesInfo e)
	{
		if (e.Args.Type == TouchActionResult.Tapped && day is not null)
		{
			e.Consumed = true;
			Tap();
		}
	}

	/// <summary>Handles a tap on the cell, as a touch does.</summary>
	internal void Tap()
	{
		if (day is not null)
		{
			tapped(day);
		}
	}

	/// <summary>The label that shows the day number.</summary>
	internal SkiaLabel Label => label;

	/// <summary>The shape behind the day: its background color.</summary>
	internal SkiaShape BackgroundShape => background;

	/// <summary>The today outline over the background.</summary>
	internal DrawnOutline Outline => outline;

	/// <summary>The <c>DayViewSize</c> × <c>DayViewHeight</c> box that holds the background and the content.</summary>
	internal SkiaLayer Container => container;

	/// <summary>The event dots, shown or hidden.</summary>
	internal IEnumerable<SkiaShape> Dots => dots.Views.Cast<SkiaShape>();

	public override void OnDisposing()
	{
		Day = null;
		base.OnDisposing();
	}
}
