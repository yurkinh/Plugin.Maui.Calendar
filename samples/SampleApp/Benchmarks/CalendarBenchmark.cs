using System.Diagnostics;
using System.Globalization;
using System.Text;
using Plugin.Maui.Calendar.Models;
using CalendarControl = Plugin.Maui.Calendar.Controls.Calendar;

namespace SampleApp.Benchmarks;

/// <summary>
/// Measures how long the calendar takes to appear and to change the month, and how much it allocates, so that
/// performance changes in the library can be compared before and after. Run it in Release: Debug builds are
/// several times slower and their numbers say little.
/// </summary>
/// <remarks>
/// Every calendar gets the same input (May 2025, en-US, a fixed "today"), so the runs do not depend on the device's
/// date or language. Times are wall-clock times on the UI thread; allocations are the bytes allocated on the UI
/// thread (<see cref="GC.GetAllocatedBytesForCurrentThread"/>), which is where the calendar builds its views.
/// </remarks>
public static class CalendarBenchmark
{
	/// <summary>The line the benchmark writes to the console; benchmark.sh collects these lines.</summary>
	public const string OutputPrefix = "CALBENCH";

	const int WarmRenderRuns = 10;
	const int WarmupRenders = 5;
	const int MonthChanges = 12;

	static readonly DateTime today = new(2025, 5, 14);
	static readonly CultureInfo culture = new("en-US");
	static readonly TimeProvider timeProvider = new FixedTimeProvider(today.AddHours(10));

	public static IReadOnlyList<BenchmarkScenario> Scenarios { get; } =
	[
		new("Default", _ => { }),
		new("Events", calendar => calendar.Events = Events()),
	];

	/// <summary>
	/// Runs every scenario in <paramref name="host"/>, which must be on screen and give the calendar a width
	/// (a calendar that is not laid out is never measured).
	/// </summary>
	public static async Task<IReadOnlyList<BenchmarkResult>> RunAsync(ContentView host, IProgress<string> progress = null)
	{
		// The first render of each scenario in this process, then a warm-up of all of them, so the measured runs of
		// every scenario see the same, fully compiled code (tiered compilation optimizes methods after their first
		// calls; without the warm-up the scenario measured first would look slower).
		var coldRenders = new Dictionary<BenchmarkScenario, RenderSample>();
		foreach (var scenario in Scenarios)
		{
			progress?.Report($"{scenario.Name}: first render…");
			coldRenders[scenario] = await MeasureRenderAsync(host, scenario);
		}

		progress?.Report("Warming up…");
		for (var i = 0; i < WarmupRenders; i++)
		{
			foreach (var scenario in Scenarios)
			{
				await MeasureRenderAsync(host, scenario);
			}
		}

		var results = new List<BenchmarkResult>();
		foreach (var scenario in Scenarios)
		{
			progress?.Report($"{scenario.Name}: renders…");
			var renders = new List<RenderSample>();
			for (var run = 0; run < WarmRenderRuns; run++)
			{
				renders.Add(await MeasureRenderAsync(host, scenario));
			}

			progress?.Report($"{scenario.Name}: month change…");
			var changes = await MeasureMonthChangesAsync(host, scenario);

			results.Add(new BenchmarkResult(scenario.Name, coldRenders[scenario], renders, changes));
		}

		host.Content = null;
		return results;
	}

	static async Task<RenderSample> MeasureRenderAsync(ContentView host, BenchmarkScenario scenario)
	{
		host.Content = null;
		await SettleAsync();

		var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
		var stopwatch = Stopwatch.StartNew();

		var calendar = CreateCalendar(scenario);
		var laidOut = WaitForLayoutAsync(calendar);
		host.Content = calendar;
		await laidOut;
		await NextUiTickAsync();

		stopwatch.Stop();
		var allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
		return new RenderSample(stopwatch.Elapsed.TotalMilliseconds, allocated, CountViews(calendar));
	}

	static async Task<List<MonthChangeSample>> MeasureMonthChangesAsync(ContentView host, BenchmarkScenario scenario)
	{
		// Replacing a calendar directly with one of the same size can leave the new one without a layout pass
		host.Content = null;
		await SettleAsync();

		var calendar = CreateCalendar(scenario);
		var laidOut = WaitForLayoutAsync(calendar);
		host.Content = calendar;
		await laidOut;

		var samples = new List<MonthChangeSample>();
		// The first changes only warm up the code of the month change; they are not recorded
		const int warmupChanges = 3;
		for (var i = 0; i < warmupChanges + MonthChanges; i++)
		{
			await SettleAsync();

			var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
			var stopwatch = Stopwatch.StartNew();

			// The setter rebuilds the days synchronously; the layout and drawing follow on the UI thread
			calendar.ShownDate = calendar.ShownDate.AddMonths(1);
			var synchronous = stopwatch.Elapsed.TotalMilliseconds;
			await NextUiTickAsync();

			stopwatch.Stop();
			var allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
			if (i < warmupChanges)
			{
				continue;
			}

			samples.Add(new MonthChangeSample(synchronous, stopwatch.Elapsed.TotalMilliseconds, allocated));
		}

		return samples;
	}

	static CalendarControl CreateCalendar(BenchmarkScenario scenario)
	{
		var calendar = new CalendarControl
		{
			Culture = culture,
			TimeProvider = timeProvider,
			ShownDate = today,
			FirstDayOfWeek = DayOfWeek.Sunday,
		};
		scenario.Configure(calendar);
		return calendar;
	}

	/// <summary>Completes when the calendar has been measured and arranged with a real size.</summary>
	static Task WaitForLayoutAsync(VisualElement calendar)
	{
		var laidOut = new TaskCompletionSource();
		void OnSizeChanged(object sender, EventArgs e)
		{
			if (calendar.Width > 0 && calendar.Height > 0)
			{
				calendar.SizeChanged -= OnSizeChanged;
				laidOut.TrySetResult();
			}
		}

		calendar.SizeChanged += OnSizeChanged;
		OnSizeChanged(calendar, EventArgs.Empty);
		return laidOut.Task.WaitAsync(TimeSpan.FromSeconds(30));
	}

	/// <summary>
	/// Waits until the platform has laid out and drawn the pending changes: two display frames, because a change
	/// made during a frame is laid out and drawn in the next one. (Dispatching a callback is not enough: on the UI
	/// thread it runs right away or before the next frame.)
	/// </summary>
	static async Task NextUiTickAsync()
	{
		await DisplayFrames.NextAsync();
		await DisplayFrames.NextAsync();
	}

	/// <summary>Lets the previous run finish and collects its garbage, so it does not slow down the next one.</summary>
	static async Task SettleAsync()
	{
		await Task.Delay(100);
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
		await Task.Delay(50);
	}

	static int CountViews(IVisualTreeElement element) =>
		1 + element.GetVisualChildren().Sum(CountViews);

	static EventCollection Events() => new()
	{
		[new DateTime(2025, 5, 5)] = new List<object> { "Dentist" },
		[new DateTime(2025, 5, 14)] = new List<object> { "Standup", "Review" },
		[new DateTime(2025, 5, 20)] = new List<object> { "Trip", "Hotel", "Museum" },
		[new DateTime(2025, 5, 22)] = new List<object> { "Party" },
		[new DateTime(2025, 5, 28)] = new List<object> { "Holiday" },
		[new DateTime(2025, 6, 2)] = new List<object> { "Holiday" },
		[new DateTime(2025, 6, 11)] = new List<object> { "Dentist" },
		[new DateTime(2025, 6, 18)] = new List<object> { "Trip" },
	};

	/// <summary>One line per scenario: what benchmark.sh collects. Numbers use the invariant culture.</summary>
	public static string Format(BenchmarkResult result, double? startupMilliseconds)
	{
		var text = new StringBuilder();
		text.Append(CultureInfo.InvariantCulture, $"{OutputPrefix} scenario={result.Scenario}");
		text.Append(CultureInfo.InvariantCulture, $" coldRenderMs={result.ColdRender.Milliseconds:F1}");
		text.Append(CultureInfo.InvariantCulture, $" warmRenderMs={Median(result.WarmRenders.Select(r => r.Milliseconds)):F1}");
		text.Append(CultureInfo.InvariantCulture, $" renderKB={Median(result.WarmRenders.Select(r => r.AllocatedBytes / 1024.0)):F0}");
		text.Append(CultureInfo.InvariantCulture, $" views={result.ColdRender.Views}");
		text.Append(CultureInfo.InvariantCulture, $" monthChangeSyncMs={Median(result.MonthChanges.Select(c => c.SynchronousMilliseconds)):F2}");
		text.Append(CultureInfo.InvariantCulture, $" monthChangeMs={Median(result.MonthChanges.Select(c => c.TotalMilliseconds)):F2}");
		text.Append(CultureInfo.InvariantCulture, $" monthChangeKB={Median(result.MonthChanges.Select(c => c.AllocatedBytes / 1024.0)):F0}");
		if (startupMilliseconds is { } startup)
		{
			text.Append(CultureInfo.InvariantCulture, $" startupMs={startup:F0}");
		}

		return text.ToString();
	}

	public static double Median(IEnumerable<double> values)
	{
		var sorted = values.Order().ToArray();
		if (sorted.Length == 0)
		{
			return double.NaN;
		}

		var middle = sorted.Length / 2;
		return sorted.Length % 2 == 1 ? sorted[middle] : (sorted[middle - 1] + sorted[middle]) / 2;
	}

	sealed class FixedTimeProvider(DateTime localNow) : TimeProvider
	{
		public override DateTimeOffset GetUtcNow() => new(localNow, TimeSpan.Zero);

		public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
	}
}

public sealed record BenchmarkScenario(string Name, Action<CalendarControl> Configure);

/// <summary>From creating a calendar until it is laid out and the UI thread has caught up.</summary>
public sealed record RenderSample(double Milliseconds, long AllocatedBytes, int Views);

/// <summary>
/// A move to the next month: the synchronous part (the ShownDate setter rebuilds the days) and the total until the
/// UI thread has caught up.
/// </summary>
public sealed record MonthChangeSample(double SynchronousMilliseconds, double TotalMilliseconds, long AllocatedBytes);

/// <summary>
/// The first render of the scenario in this process (types and templates loaded for the first time), the renders
/// after it, and the month changes.
/// </summary>
public sealed record BenchmarkResult(string Scenario, RenderSample ColdRender, IReadOnlyList<RenderSample> WarmRenders, IReadOnlyList<MonthChangeSample> MonthChanges);
