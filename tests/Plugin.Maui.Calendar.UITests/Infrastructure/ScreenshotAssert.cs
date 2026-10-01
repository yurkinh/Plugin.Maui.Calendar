using NUnit.Framework;
using StbImageSharp;
using StbImageWriteSharp;

namespace Plugin.Maui.Calendar.UITests.Infrastructure;

/// <summary>
/// Compares a screenshot with its baseline in Baselines/&lt;platform&gt;. Baselines differ per platform (and per
/// device and OS version, whose fonts and rendering differ), so they are recorded on the devices named in the
/// README. On a mismatch the screenshot, the baseline and an image of the differences (in red) are written to
/// TestResults/Screenshots/&lt;platform&gt; and attached to the test.
/// </summary>
static class ScreenshotAssert
{
	// A pixel counts as different when one of its channels differs by more than this (out of 255), which
	// ignores tiny anti-aliasing differences.
	const int channelTolerance = 32;

	// The share of pixels that may differ.
	const double maxDifferentPixels = 0.001;

	public static void MatchesBaseline(byte[] screenshot, string name)
	{
		if (!UITestSettings.CompareScreenshots)
		{
			TestContext.Out.WriteLine($"Screenshot comparison '{name}' skipped (UITEST_SCREENSHOTS=off).");
			return;
		}

		var baselinePath = Path.Combine(UITestSettings.BaselineDirectory, name + ".png");

		if (UITestSettings.UpdateBaselines)
		{
			Directory.CreateDirectory(UITestSettings.BaselineDirectory);
			File.WriteAllBytes(baselinePath, screenshot);
			TestContext.Out.WriteLine($"Baseline saved: {baselinePath}");
			return;
		}

		if (!File.Exists(baselinePath))
		{
			var newPath = Save(screenshot, name + ".new.png");
			Assert.Fail($"There is no baseline {baselinePath}. The screenshot is in {newPath}; run the tests with UITEST_UPDATE_BASELINES=1 to save it as the baseline, review it and commit it.");
		}

		var expected = ImageResult.FromMemory(File.ReadAllBytes(baselinePath), StbImageSharp.ColorComponents.RedGreenBlueAlpha);
		var actual = ImageResult.FromMemory(screenshot, StbImageSharp.ColorComponents.RedGreenBlueAlpha);

		if (expected.Width != actual.Width || expected.Height != actual.Height)
		{
			var actualPath = Save(screenshot, name + ".actual.png");
			Assert.Fail($"{name}: the screenshot is {actual.Width}x{actual.Height}, the baseline {expected.Width}x{expected.Height}. Screenshot: {actualPath}");
		}

		var diff = new byte[actual.Data.Length];
		var differentPixels = 0;
		for (var i = 0; i < actual.Data.Length; i += 4)
		{
			var difference = 0;
			for (var channel = 0; channel < 4; channel++)
			{
				difference = Math.Max(difference, Math.Abs(actual.Data[i + channel] - expected.Data[i + channel]));
			}

			if (difference > channelTolerance)
			{
				differentPixels++;
				diff[i] = 255;
				diff[i + 3] = 255;
			}
			else
			{
				// The baseline, faded, so the differences stand out.
				diff[i] = diff[i + 1] = diff[i + 2] = (byte)(255 - ((255 - expected.Data[i + 1]) / 4));
				diff[i + 3] = 255;
			}
		}

		var share = (double)differentPixels / (actual.Width * actual.Height);
		if (share > maxDifferentPixels)
		{
			var actualPath = Save(screenshot, name + ".actual.png");
			Save(File.ReadAllBytes(baselinePath), name + ".expected.png");
			Save(EncodePng(diff, actual.Width, actual.Height), name + ".diff.png");
			Assert.Fail($"{name}: {differentPixels} pixels ({share:P2}) differ from the baseline. Screenshot: {actualPath}");
		}
	}

	static string Save(byte[] png, string fileName)
	{
		Directory.CreateDirectory(UITestSettings.ResultsDirectory);
		var path = Path.Combine(UITestSettings.ResultsDirectory, fileName);
		File.WriteAllBytes(path, png);
		TestContext.AddTestAttachment(path);
		return path;
	}

	static byte[] EncodePng(byte[] rgba, int width, int height)
	{
		using var stream = new MemoryStream();
		new ImageWriter().WritePng(rgba, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
		return stream.ToArray();
	}
}
