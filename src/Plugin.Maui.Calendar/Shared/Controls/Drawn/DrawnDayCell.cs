using System.ComponentModel;
using AppoMobi.Gestures;
using DrawnUi.Draw;
using DrawnUi.Models;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls.Drawn;

/// <summary>
/// The drawn built-in day cell, element for element the native one of <c>DayView.xaml</c>:
/// a background shape (the Border) under a column (the FlexLayout) of a spacer, the day label and
/// the event dot row (the HorizontalStackLayout), reversed for <see cref="EventIndicatorType.TopDot"/>.
/// The cell follows its <see cref="DayModel"/> through PropertyChanged rather than bindings.
/// </summary>
sealed class DrawnDayCell : SkiaLayer
{
	// DayModel.EventColors holds at most five colors (see Calendar.AssignIndicatorColors).
	const int maximumDots = 5;
	const double dotSize = 8;

	readonly Action<DayModel> tapped;
	readonly SkiaShape background;
	readonly SkiaLayout content;
	readonly SkiaControl spacer;
	readonly SkiaLabel label;
	readonly SkiaRow dots;

	DayModel day;
	bool? topDot;
	bool applyPending;

	internal DrawnDayCell(Action<DayModel> tapped)
	{
		this.tapped = tapped;
		HorizontalOptions = LayoutOptions.Fill;

		background = new SkiaShape
		{
			Type = ShapeType.Rectangle,
			StrokeWidth = 1,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
		};

		spacer = new SkiaControl { HeightRequest = 4 };
		label = new SkiaLabel { HorizontalTextAlignment = DrawTextAlignment.Center, HorizontalOptions = LayoutOptions.Center };
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

		AddSubView(background);
		AddSubView(content);

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
		// Hidden days keep their place in the grid: Opacity, not IsVisible, so no cell is measured again.
		Opacity = day.IsControlVisible ? 1 : 0;
		HeightRequest = day.DayViewHeight;
		WidthRequest = day.DayViewSize;
		HorizontalOptions = LayoutOptions.Center;

		// EventIndicatorType.BackgroundFull paints the whole cell, the rest only the DayViewSize square.
		BackgroundColor = day.BackgroundFullEventColor;

		background.Opacity = day.IsVisible ? 1 : 0;
		background.WidthRequest = day.DayViewSize;
		background.HeightRequest = Math.Min(day.DayViewSize, day.DayViewHeight);
		background.Margin = day.DayViewBorderMargin;
		background.CornerRadius = day.DayViewCornerRadius;
		background.BackgroundColor = day.BackgroundColor;
		background.StrokeColor = day.OutlineColor;

		content.Opacity = day.IsVisible ? 1 : 0;
		StyleValues.ApplyLabelStyle(label, day.DaysLabelStyle);
		label.Text = day.Day;
		label.TextColor = day.TextColor;

		var colors = day.EventColors ?? [];
		var showDots = day.EventIndicatorType is EventIndicatorType.BottomDot or EventIndicatorType.TopDot;
		for (var i = 0; i < maximumDots; i++)
		{
			var dot = (SkiaShape)dots.Views[i];
			dot.IsVisible = showDots && i < colors.Count;
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
			tapped(day);
		}
	}

	public override void OnDisposing()
	{
		Day = null;
		base.OnDisposing();
	}
}
