using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.Messaging;
using DrawnUi.Draw;
using DrawnUi.Models;
using Microsoft.Maui.Layouts;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls.Drawn;

/// <summary>
/// Drawn replacement for the former native <c>DayView</c>. Reads the same <see cref="DayModel"/>
/// and reproduces its visual tree:
/// <code>
/// cell   (BackgroundFullEventColor, visible = IsControlVisible, fills column)
/// └ body (DayViewSize square, centered, visible = IsVisible, tap target)
///   ├ border  (BackgroundColor, OutlineColor stroke, DayViewCornerRadius, DayViewBorderMargin)
///   └ column  (centered: 4pt spacer, day label, 8pt event dots; reversed for TopDot)
/// </code>
/// </summary>
sealed class DayCell : SkiaLayout
{
	const double spacerHeight = 4;

	readonly SkiaLayout body;
	readonly SkiaShape border;
	readonly SkiaLayout column;
	readonly SkiaControl spacer;
	readonly SkiaRichLabel label;
	readonly EventDots dots;
	FlexDirection appliedDirection = FlexDirection.Column;

	public DayCell(DayModel model)
	{
		Model = model;

		Type = LayoutType.Absolute;
		HorizontalOptions = LayoutOptions.Fill;
		VerticalOptions = LayoutOptions.Start;
		// one bitmap per day: selecting a day re-renders only the cells that changed
		UseCache = SkiaCacheType.Image;

		border = new SkiaShape
		{
			Type = ShapeType.Rectangle,
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Fill,
			StrokeWidth = 1,
			UseCache = SkiaCacheType.None
		};

		spacer = new SkiaControl { HeightRequest = spacerHeight, WidthRequest = 0 };

		label = new SkiaRichLabel
		{
			// plain text with per-glyph system font fallback (native digits, any script)
			MarkdownEnabled = false,
			HorizontalOptions = LayoutOptions.Center,
			UseCache = SkiaCacheType.None
		};

		dots = new EventDots();

		column = new SkiaLayout
		{
			Type = LayoutType.Column,
			Spacing = 0,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			Children = { spacer, label, dots }
		};

		body = new SkiaLayout
		{
			Type = LayoutType.Absolute,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Start,
			AccessibilityRole = Aria.RoleButton,
			Children = { border, column }
		};
		body.Tapped += OnTapped;

		Children.Add(body);

		model.PropertyChanged += OnModelPropertyChanged;
		ApplyAll();
	}

	public DayModel Model { get; }

	/// <summary>Culture used for the screen-reader description of the date.</summary>
	public CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;

	/// <summary>
	/// Copies the resolved <c>DaysLabelStyle</c> onto the day label. Text color stays with the
	/// day state (as the former XAML binding overrode the style's TextColor).
	/// </summary>
	public void ApplyLabelStyle(LabelStyleBridge style)
	{
		style.ApplyTo(label, applyTextColor: false, applyLayoutOptions: false);
		label.HorizontalOptions = LayoutOptions.Center;
		label.TextColor = Model.TextColor;
	}

	public void Detach()
	{
		Model.PropertyChanged -= OnModelPropertyChanged;
		body.Tapped -= OnTapped;
	}

	void OnTapped(object sender, ControlTappedEventArgs e)
	{
		if (MainThread.IsMainThread)
		{
			HandleTap();
		}
		else
		{
			MainThread.BeginInvokeOnMainThread(HandleTap);
		}
	}

	// Same rules as the former DayView.OnTapped
	void HandleTap()
	{
		var dayModel = Model;
		if (dayModel.IsDisabled || !dayModel.IsVisible)
		{
			return;
		}

		if (!dayModel.AllowDeselect && dayModel.IsSelected)
		{
			return;
		}

		dayModel.IsSelected = !dayModel.IsSelected;
		dayModel.DayTappedCommand?.Execute(dayModel.Date);
		WeakReferenceMessenger.Default.Send(new DayTappedMessage(dayModel.Date));
	}

	void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(DayModel.Day):
				label.Text = Model.Day;
				break;
			case nameof(DayModel.Date):
			case nameof(DayModel.IsSelected):
			case nameof(DayModel.IsDisabled):
				UpdateAccessibility();
				break;
			case nameof(DayModel.TextColor):
				label.TextColor = Model.TextColor;
				break;
			case nameof(DayModel.BackgroundColor):
				border.BackgroundColor = Model.BackgroundColor;
				break;
			case nameof(DayModel.OutlineColor):
				border.StrokeColor = Model.OutlineColor;
				break;
			case nameof(DayModel.BackgroundFullEventColor):
				BackgroundColor = Model.BackgroundFullEventColor;
				break;
			case nameof(DayModel.IsVisible):
				body.IsVisible = Model.IsVisible;
				break;
			case nameof(DayModel.IsControlVisible):
				IsVisible = Model.IsControlVisible;
				break;
			case nameof(DayModel.DayViewSize):
				body.WidthRequest = Model.DayViewSize;
				body.HeightRequest = Model.DayViewSize;
				break;
			case nameof(DayModel.DayViewBorderMargin):
				border.Margin = Model.DayViewBorderMargin;
				break;
			case nameof(DayModel.DayViewCornerRadius):
				border.CornerRadius = Model.DayViewCornerRadius;
				break;
			case nameof(DayModel.EventColors):
				dots.SetColors(Model.EventColors);
				break;
			case nameof(DayModel.HasEvents):
			case nameof(DayModel.EventIndicatorType):
				UpdateDirection();
				break;
		}

		// these feed computed colors whose own notifications are raised separately
		if (e.PropertyName is nameof(DayModel.HasEvents) or nameof(DayModel.EventIndicatorType) or nameof(DayModel.IsSelected))
		{
			label.TextColor = Model.TextColor;
		}
	}

	void ApplyAll()
	{
		label.Text = Model.Day;
		label.TextColor = Model.TextColor;
		border.BackgroundColor = Model.BackgroundColor;
		border.StrokeColor = Model.OutlineColor;
		border.Margin = Model.DayViewBorderMargin;
		border.CornerRadius = Model.DayViewCornerRadius;
		BackgroundColor = Model.BackgroundFullEventColor;
		body.WidthRequest = Model.DayViewSize;
		body.HeightRequest = Model.DayViewSize;
		body.IsVisible = Model.IsVisible;
		IsVisible = Model.IsControlVisible;
		dots.SetColors(Model.EventColors);
		UpdateDirection();
		UpdateAccessibility();
	}

	void UpdateDirection()
	{
		var direction = Model.EventLayoutDirection;
		if (direction == appliedDirection)
		{
			return;
		}

		appliedDirection = direction;
		column.Children.Clear();
		if (direction == FlexDirection.ColumnReverse)
		{
			column.Children.Add(dots);
			column.Children.Add(label);
			column.Children.Add(spacer);
		}
		else
		{
			column.Children.Add(spacer);
			column.Children.Add(label);
			column.Children.Add(dots);
		}
	}

	void UpdateAccessibility()
	{
		var model = Model;
		if (model.Date == DateTime.MaxValue.Date && string.IsNullOrEmpty(model.Day))
		{
			body.AccessibilityLabel = null;
			return;
		}

		body.AccessibilityLabel = model.Date.ToString("D", Culture);
		body.AccessibilityIsPressed = model.IsSelected;
		body.AccessibilityCanInteract = !model.IsDisabled;
	}
}
