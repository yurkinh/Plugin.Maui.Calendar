using System.Diagnostics;
using Plugin.Maui.Calendar.Controls;
using Plugin.Maui.Calendar.Models;

namespace SampleApp.Views;

/// <summary>
/// Phase 0 spike of the DrawnUI migration: compares the native day grid with the drawn one. Opened
/// instead of the shell when the app is launched with CALBENCH=1 (iOS) or the calbench intent extra
/// (Android); the results are shown and written to the console with a CALBENCH prefix.
/// </summary>
public sealed class DrawnSpikeBenchmarkPage : ContentPage
{
	const string drawnDayGridSwitch = "Plugin.Maui.Calendar.DrawnDayGrid";
	const int rounds = 5;
	const int navigations = 24;

	/// <summary>Set by the platform entry points when the benchmark was requested.</summary>
	public static bool Requested { get; set; } = Environment.GetEnvironmentVariable("CALBENCH") == "1";

	readonly ContentView host = new() { HeightRequest = 420 };
	readonly Label results = new() { FontSize = 12, FontFamily = "Courier" };

	public DrawnSpikeBenchmarkPage()
	{
		Content = new VerticalStackLayout
		{
			Padding = new Thickness(8, 50, 8, 8),
			Children = { host, results },
		};
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await Task.Delay(1000);

		// A warm-up of both modes, so JIT and first-use costs are not counted.
		await Measure(drawn: false);
		await Measure(drawn: true);

		var native = new List<Result>();
		var drawn = new List<Result>();
		for (var round = 0; round < rounds; round++)
		{
			native.Add(await Measure(drawn: false));
			drawn.Add(await Measure(drawn: true));
		}

		Report("native", native);
		Report("drawn", drawn);

		// A DayViewTemplate set as XAML sets it: the drawn-enabled calendar renders its drawn grid in
		// the constructor first, then the native grid for the template.
		await Measure(drawn: false, template: true);
		await Measure(drawn: true, template: true);
		var templateNative = new List<Result>();
		var templateDrawn = new List<Result>();
		for (var round = 0; round < rounds; round++)
		{
			templateNative.Add(await Measure(drawn: false, template: true));
			templateDrawn.Add(await Measure(drawn: true, template: true));
		}

		Report("templ-off", templateNative);
		Report("templ-on", templateDrawn);

		ShowSideBySide();
		await NextFrames();
		Log("done");
	}

	// Native above, drawn below, for a visual comparison: the same month, selection and events.
	void ShowSideBySide()
	{
		Calendar Create(bool drawn)
		{
			AppContext.SetSwitch(drawnDayGridSwitch, drawn);
			return new Calendar
			{
				ShownDate = new DateTime(2026, 10, 1),
				SelectedDate = new DateTime(2026, 10, 14),
				Events = CreateEvents(),
				EventsScrollViewVisible = false,
				FooterSectionVisible = false,
				HeaderSectionVisible = false,
			};
		}

		host.HeightRequest = -1;
		results.IsVisible = false;
		host.Content = new VerticalStackLayout { Spacing = 12, Children = { Create(drawn: false), Create(drawn: true) } };
	}

	async Task<Result> Measure(bool drawn, bool template = false)
	{
		AppContext.SetSwitch(drawnDayGridSwitch, drawn);
		host.Content = null;
		await NextFrames();

		// First render: from creating the calendar until it was laid out and drawn.
		var stopwatch = Stopwatch.StartNew();
		var calendar = new Calendar
		{
			ShownDate = new DateTime(2026, 10, 1),
			Events = CreateEvents(),
			EventsScrollViewVisible = false,
		};

		// As XAML does: the template is assigned after the constructor rendered the grid.
		if (template)
		{
			calendar.DayViewTemplate = new DataTemplate(() =>
			{
				var label = new Label { HorizontalTextAlignment = TextAlignment.Center };
				label.SetBinding(Label.TextProperty, static (Plugin.Maui.Calendar.Interfaces.ICalendarDay day) => day.Day);
				return label;
			});
		}

		host.Content = calendar;
		await NextFrames();
		var firstRender = stopwatch.Elapsed.TotalMilliseconds;

		var nativeViews = CountNativeViews(calendar.Handler?.PlatformView);

		// Month changes: the synchronous update (the main thread is blocked) and the time until
		// the change is on screen.
		double sync = 0;
		stopwatch.Restart();
		for (var i = 0; i < navigations; i++)
		{
			var step = Stopwatch.StartNew();
			calendar.NextLayoutUnitCommand.Execute(null);
			sync += step.Elapsed.TotalMilliseconds;
			await NextFrames();
		}
		var navigation = stopwatch.Elapsed.TotalMilliseconds / navigations;

		host.Content = null;
		calendar.Dispose();

		return new Result(firstRender, sync / navigations, navigation, nativeViews);
	}

	static EventCollection CreateEvents()
	{
		var events = new EventCollection();
		var start = new DateTime(2026, 9, 1);
		for (var day = 0; day < 365; day += 3)
		{
			events[start.AddDays(day)] = new List<string> { "Event", "Event" };
		}
		return events;
	}

	// Two frames: the layout pass runs before the next frame is committed, and the drawn canvas
	// paints on the frame after its invalidation.
	Task NextFrames() => FrameWaiter.WaitAsync(2);

	void Report(string mode, List<Result> list)
	{
		static double Median(IEnumerable<double> values)
		{
			var sorted = values.OrderBy(v => v).ToArray();
			return sorted[sorted.Length / 2];
		}

		Log($"{mode,-7} firstRender={Median(list.Select(r => r.FirstRender)),7:F1}ms " +
			$"navSync={Median(list.Select(r => r.NavigationSync)),6:F2}ms " +
			$"navToScreen={Median(list.Select(r => r.NavigationToScreen)),6:F1}ms " +
			$"nativeViews={list[0].NativeViews}");
	}

	void Log(string line)
	{
		Console.WriteLine($"CALBENCH {DeviceInfo.Platform} {line}");
		results.Text += line + Environment.NewLine;
	}

	static int CountNativeViews(object view)
	{
#if IOS || MACCATALYST
		return view is UIKit.UIView uiView ? 1 + uiView.Subviews.Sum(CountNativeViews) : 0;
#elif ANDROID
		if (view is Android.Views.ViewGroup group)
		{
			var count = 1;
			for (var i = 0; i < group.ChildCount; i++)
			{
				count += CountNativeViews(group.GetChildAt(i));
			}
			return count;
		}
		return view is Android.Views.View ? 1 : 0;
#else
		return -1;
#endif
	}

	readonly record struct Result(double FirstRender, double NavigationSync, double NavigationToScreen, int NativeViews);
}

/// <summary>Waits for display frames (vsync callbacks) on the main thread.</summary>
static class FrameWaiter
{
	public static async Task WaitAsync(int frames)
	{
		for (var i = 0; i < frames; i++)
		{
			await NextFrame();
		}
	}

	static Task NextFrame()
	{
		var done = new TaskCompletionSource();
#if IOS || MACCATALYST
		CoreAnimation.CADisplayLink link = null;
		link = CoreAnimation.CADisplayLink.Create(() =>
		{
			link.Invalidate();
			done.TrySetResult();
		});
		link.AddToRunLoop(Foundation.NSRunLoop.Main, Foundation.NSRunLoopMode.Common);
#elif ANDROID
		Android.Views.Choreographer.Instance.PostFrameCallback(new FrameCallback(done));
#else
		Task.Delay(16).ContinueWith(_ => done.TrySetResult());
#endif
		return done.Task;
	}

#if ANDROID
	sealed class FrameCallback(TaskCompletionSource done) : Java.Lang.Object, Android.Views.Choreographer.IFrameCallback
	{
		public void DoFrame(long frameTimeNanos) => done.TrySetResult();
	}
#endif
}
