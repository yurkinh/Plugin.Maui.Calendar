using DrawnUi.Draw;
using DrawnUi.Views;
using Plugin.Maui.Calendar.Controls.Drawn;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls;

public partial class Calendar : ContentView, IDisposable
{
	/// <summary>
	/// The <see cref="AppContext"/> switch that draws the day grid with DrawnUI (phase 0 spike of the
	/// hybrid migration). Off by default; it is read every time the layout is rendered.
	/// </summary>
	internal const string DrawnDayGridSwitch = "Plugin.Maui.Calendar.DrawnDayGrid";

	static bool IsDrawnDayGridEnabled => AppContext.TryGetSwitch(DrawnDayGridSwitch, out var enabled) && enabled;

	// Created the first time the drawn grid is shown and kept across layout changes.
	Canvas drawnDayCanvas;
	DrawnDayGrid drawnDayGrid;

	void ShowDrawnDayGrid()
	{
		if (drawnDayGrid is null)
		{
			drawnDayGrid = new DrawnDayGrid
			{
				DayTapped = day => DayView.Tap(day, this),
				Swiped = direction =>
				{
					if (!SwipeDetectionDisabled)
					{
						OnSwiped(this, new SwipedEventArgs(null, direction));
					}
				},
			};

			// Sized by its content (the grid), as the native grid is.
			drawnDayCanvas = new Canvas
			{
				RenderingMode = RenderingModeType.Accelerated,
				Gestures = GesturesMode.Enabled,
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Start,
				Content = drawnDayGrid,
			};
		}

		drawnDayGrid.SetDays([.. dayViews.Select(dayView => (DayModel)dayView.BindingContext)]);
		drawnDayGrid.SetTitles([.. dayTitleLabels.Select(label => label.Text)], [.. dayTitleLabels.Select(label => label.Style)]);

		daysControl.Add(drawnDayCanvas);
	}
}
