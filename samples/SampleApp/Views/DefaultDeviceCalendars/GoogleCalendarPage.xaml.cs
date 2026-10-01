using System.ComponentModel;

namespace SampleApp.Views;

/// <summary>
/// A look-alike of the Google Calendar app with its Schedule, Day, Week and Month views. The view model
/// holds the data; this class sizes the days of the Month view and runs the animations of the drop-down
/// month, the drawer and the event details.
/// </summary>
public partial class GoogleCalendarPage : ContentPage
{
	const string monthPanelAnimation = "GoogleCalendarMonthPanel";
	const uint animationLength = 250;

	readonly GoogleCalendarViewModel viewModel;
	bool monthExpanded;

	// Cancelled when the page disappears: animations that end later leave the page alone. Its source
	// lives from OnAppearing to OnDisappearing, so the page (transient, never disposed) owns no disposable.
	CancellationToken lifetime;
	Action endLifetime = () => { };

	public GoogleCalendarPage(GoogleCalendarViewModel vm)
	{
		InitializeComponent();
		BindingContext = viewModel = vm;

		vm.ScrollRequested += OnScrollRequested;
		vm.PropertyChanged += OnViewModelPropertyChanged;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		var lifetimeSource = new CancellationTokenSource();
		lifetime = lifetimeSource.Token;
		endLifetime = () =>
		{
			lifetimeSource.Cancel();
			lifetimeSource.Dispose();
		};

#if ANDROID
		SetStatusBarColor((CommunityToolkit.Maui.AppThemeColor)Resources["GcBackgroundColor"]);

		if (Application.Current is { } app)
		{
			app.RequestedThemeChanged += OnRequestedThemeChanged;
		}
#endif
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();

		endLifetime();
		endLifetime = () => { };
		viewModel.AddEventCommand.Cancel();
		viewModel.SearchCommand.Cancel();

#if ANDROID
		if (Application.Current is { } app)
		{
			app.RequestedThemeChanged -= OnRequestedThemeChanged;
			SetStatusBarColor((CommunityToolkit.Maui.AppThemeColor)app.Resources["PageBackgroundColor"]);
		}
#endif
	}

#if ANDROID
	// App.UpdateStatusBar paints the Android status bar in the color of the sample pages;
	// this page gives it Google's color while it is shown, the app's color again when it leaves.
	// Runs after the app's own handler, which has just set the app's color.
	void OnRequestedThemeChanged(object sender, AppThemeChangedEventArgs e) =>
		SetStatusBarColor((CommunityToolkit.Maui.AppThemeColor)Resources["GcBackgroundColor"]);

	static void SetStatusBarColor(CommunityToolkit.Maui.AppThemeColor color)
	{
		if (OperatingSystem.IsAndroidVersionAtLeast(23) && Application.Current is { } app)
		{
			CommunityToolkit.Maui.Core.Platform.StatusBar.SetColor(app.RequestedTheme == AppTheme.Dark ? color.Dark : color.Light);
		}
	}
#endif

	// Android back closes what is open over the schedule first
	protected override bool OnBackButtonPressed()
	{
		if (detailsLayer.IsVisible)
		{
			viewModel.CloseEventCommand.Execute(null);
			return true;
		}

		if (drawerLayer.IsVisible)
		{
			_ = CloseDrawerAsync(lifetime);
			return true;
		}

		return base.OnBackButtonPressed();
	}

	void OnScrollRequested(ScheduleRow row, bool animate) =>
		// Lets the list lay out a new ItemsSource before scrolling it
		Dispatcher.DispatchDelayed(
			TimeSpan.FromMilliseconds(50),
			() => schedule.ScrollTo(row, position: ScrollToPosition.Start, animate: animate));

	void OnScheduleScrolled(object sender, ItemsViewScrolledEventArgs e) =>
		viewModel.OnScheduleScrolled(e.FirstVisibleItemIndex);

	// The title opens and closes a panel under it, as in Google Calendar: the months to pick in the
	// Month view, the days of a month in the other views
	void OnTitleTapped(object sender, TappedEventArgs e) => ToggleMonthPanel();

	void ToggleMonthPanel()
	{
		monthExpanded = !monthExpanded;
		_ = monthArrow.RotateToAsync(monthExpanded ? 180 : 0, animationLength, Easing.CubicInOut);

		var panel = viewModel.IsMonthView ? monthStripPanel : monthPanel;
		var content = viewModel.IsMonthView ? (View)monthStrip : calendar;

		panel.AbortAnimation(monthPanelAnimation);

		var from = panel.IsVisible ? panel.Height : 0;
		var to = monthExpanded ? content.Measure(Width, double.PositiveInfinity).Height : 0;

		panel.HeightRequest = from;
		panel.IsVisible = true;

		if (monthExpanded && viewModel.IsMonthView)
		{
			ScrollMonthStripToShownMonth(animate: false);
		}

		panel.Animate(
			monthPanelAnimation,
			height => panel.HeightRequest = height,
			from,
			to,
			length: animationLength,
			easing: Easing.CubicInOut,
			finished: (_, cancelled) =>
			{
				if (cancelled)
				{
					return;
				}

				if (monthExpanded)
				{
					// Follows the content from now on: months have 4 to 6 weeks
					panel.HeightRequest = -1;
				}
				else
				{
					panel.IsVisible = false;
				}
			});
	}

	// Another view has its own panel under the title: the open one closes
	void CloseMonthPanels()
	{
		monthExpanded = false;
		monthArrow.Rotation = 0;

		foreach (var panel in new[] { monthPanel, monthStripPanel })
		{
			panel.AbortAnimation(monthPanelAnimation);
			panel.HeightRequest = 0;
			panel.IsVisible = false;
		}
	}

	// The shown month second in the strip, the month before it cut off, as in Google Calendar
	void ScrollMonthStripToShownMonth(bool animate)
	{
		var index = viewModel.MonthStripItems.ToList().FindIndex(item => item.IsSelected);

		if (index < 0)
		{
			return;
		}

		// Lets a strip that has just been shown lay out its items first
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(50), () =>
		{
			if (monthStripItems.Children[index] is VisualElement item)
			{
				_ = monthStrip.ScrollToAsync(Math.Max(0, item.X - item.Width * 0.85), 0, animate);
			}
		});
	}

	async void OnMenuTapped(object sender, TappedEventArgs e)
	{
		drawer.TranslationX = -drawer.WidthRequest;
		drawerScrim.Opacity = 0;
		drawerLayer.IsVisible = true;

		await Task.WhenAll(
			drawer.TranslateToAsync(0, 0, animationLength, Easing.CubicOut),
			drawerScrim.FadeToAsync(1, animationLength));
	}

	void OnDrawerScrimTapped(object sender, TappedEventArgs e) => _ = CloseDrawerAsync(lifetime);

	async void OnXamlSourceTapped(object sender, TappedEventArgs e)
	{
		var token = lifetime;
		await CloseDrawerAsync(token);

		if (!token.IsCancellationRequested)
		{
			await Navigation.PushModalAsync(new XamlSourcePage(nameof(GoogleCalendarPage)));
		}
	}

	async void OnBackToSamplesTapped(object sender, TappedEventArgs e) => await Shell.Current.GoToAsync("..");

	async void OnViewTapped(object sender, TappedEventArgs e)
	{
		if (sender is BindableObject { BindingContext: GoogleCalendarViewOption option })
		{
			viewModel.SelectViewCommand.Execute(option);
		}

		await CloseDrawerAsync(lifetime);
	}

	void OnMonthViewSizeChanged(object sender, EventArgs e) => UpdateMonthDaySize();

	// The days of the Month view fill the screen, as in Google Calendar: as wide as their column
	// (DayViewSize) and as tall as their share of the height (DayViewHeight)
	void UpdateMonthDaySize()
	{
		if (monthView.Width <= 0 || monthView.Height <= 0)
		{
			return;
		}

		// Above the days: the weekday row (22, drawn by the page); above every week: a gap of 6.
		// The empty gaps under the last week (footer and event list) are cut off by the view.
		const double weekdayRowHeight = 22;
		const double weekGap = 6;
		const int weeks = GoogleCalendarViewModel.MonthWeekCount;

		var dayHeight = Math.Floor((monthView.Height - weekdayRowHeight - weekGap * weeks) / weeks);

		// The calendar has a padding of 3 on both sides
		monthCalendar.DayViewSize = (monthView.Width - 6) / 7;
		monthCalendar.DayViewHeight = dayHeight;

		// Under the date (4 + 17 + 4), every line takes 12 and 2.5 of spacing
		viewModel.SetMonthChipCapacity(Math.Max(1, (int)((dayHeight - 25 + 2.5) / 14.5)));
	}

	void OnCurrentViewChanged()
	{
		if (monthExpanded)
		{
			CloseMonthPanels();
		}

		// The hours open at the current time, as in Google Calendar
		if ((viewModel.IsDayView ? dayScroll : viewModel.IsWeekView ? weekScroll : null) is { } hours)
		{
			var y = Math.Max(0, DateTime.Now.TimeOfDay.TotalHours - 1.5) * GoogleCalendarViewModel.HourHeight;
			Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(50), () => _ = hours.ScrollToAsync(0, y, false));
		}
	}

	async Task CloseDrawerAsync(CancellationToken token)
	{
		await Task.WhenAll(
			drawer.TranslateToAsync(-drawer.WidthRequest, 0, animationLength, Easing.CubicIn),
			drawerScrim.FadeToAsync(0, animationLength));

		if (!token.IsCancellationRequested)
		{
			drawerLayer.IsVisible = false;
		}
	}

	void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(GoogleCalendarViewModel.OpenedEvent):
				_ = ShowOpenedEventAsync(lifetime);
				break;
			case nameof(GoogleCalendarViewModel.CurrentView):
				OnCurrentViewChanged();
				break;
			case nameof(GoogleCalendarViewModel.ShownDate) when monthExpanded && viewModel.IsMonthView:
				ScrollMonthStripToShownMonth(animate: true);
				break;
		}
	}

	async Task ShowOpenedEventAsync(CancellationToken token)
	{
		if (viewModel.OpenedEvent is { } opened)
		{
			// Set here, not bound: the closing animation keeps showing the event
			detailsContent.BindingContext = opened;
			detailsLayer.Opacity = 0;
			detailsLayer.TranslationY = 48;
			detailsLayer.IsVisible = true;

			await Task.WhenAll(
				detailsLayer.FadeToAsync(1, 200, Easing.CubicOut),
				detailsLayer.TranslateToAsync(0, 0, 200, Easing.CubicOut));
		}
		else
		{
			await Task.WhenAll(
				detailsLayer.FadeToAsync(0, 150, Easing.CubicIn),
				detailsLayer.TranslateToAsync(0, 48, 150, Easing.CubicIn));

			// A new event may have opened during the animation
			if (!token.IsCancellationRequested)
			{
				detailsLayer.IsVisible = viewModel.OpenedEvent is not null;
			}
		}
	}
}