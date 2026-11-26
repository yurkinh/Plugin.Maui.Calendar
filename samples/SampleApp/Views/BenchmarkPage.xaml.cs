using System.Globalization;
using System.Text;
using SampleApp.Benchmarks;

namespace SampleApp.Views;

public partial class BenchmarkPage : ContentPage
{
	bool autoRunStarted;

	public BenchmarkPage()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		if (BenchmarkLaunch.IsRequested && !autoRunStarted)
		{
			autoRunStarted = true;
			_ = RunAsync();
		}
	}

	void OnRunClicked(object sender, EventArgs e) => _ = RunAsync();

	async Task RunAsync()
	{
		RunButton.IsEnabled = false;
		ResultsLabel.Text = string.Empty;
		try
		{
			// Let the page finish its own layout and the navigation animation first
			await Task.Delay(1000);

			var progress = new Progress<string>(text => StatusLabel.Text = text);
			var results = await CalendarBenchmark.RunAsync(Host, progress);

			var text = new StringBuilder();
			foreach (var result in results)
			{
				var line = CalendarBenchmark.Format(result, BenchmarkLaunch.StartupMilliseconds);
				Console.WriteLine(line);
				text.AppendLine(Describe(result));
			}

			Console.WriteLine($"{CalendarBenchmark.OutputPrefix} done");
			ResultsLabel.Text = text.ToString();
			StatusLabel.Text = "Done";
		}
		catch (Exception exception)
		{
			Console.WriteLine($"{CalendarBenchmark.OutputPrefix} failed {exception}");
			StatusLabel.Text = $"Failed: {exception.Message}";
		}
		finally
		{
			RunButton.IsEnabled = true;
		}
	}

	static string Describe(BenchmarkResult result)
	{
		var warm = result.WarmRenders;
		var changes = result.MonthChanges;
		return string.Create(CultureInfo.InvariantCulture, $"""
			{result.Scenario}
			  first render   cold {result.ColdRender.Milliseconds,7:F1} ms, warm median {CalendarBenchmark.Median(warm.Select(r => r.Milliseconds)),7:F1} ms
			  allocated      {CalendarBenchmark.Median(warm.Select(r => r.AllocatedBytes / 1024.0)),7:F0} KB per render, {result.ColdRender.Views} views
			  month change   {CalendarBenchmark.Median(changes.Select(c => c.SynchronousMilliseconds)),7:F2} ms sync, {CalendarBenchmark.Median(changes.Select(c => c.TotalMilliseconds)),7:F2} ms total, {CalendarBenchmark.Median(changes.Select(c => c.AllocatedBytes / 1024.0)),5:F0} KB

			""");
	}
}
