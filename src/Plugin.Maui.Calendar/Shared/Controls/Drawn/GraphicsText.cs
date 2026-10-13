using GraphicsFont = Microsoft.Maui.Graphics.Font;
#if ANDROID
using Android.Text;
using Microsoft.Maui.Graphics.Platform;
#endif
#if IOS || MACCATALYST
using UIKit;
#endif

namespace Plugin.Maui.Calendar.Controls.Drawn;

/// <summary>
/// A label font as the day grid draws it: the Microsoft.Maui.Graphics font, the size it is drawn at
/// (the label's size after the platform's font scaling), the height of a native label with one line
/// of it, and the platform's own font, which the text is drawn with where there is one.
/// </summary>
readonly record struct GraphicsLabelFont(IFont Font, float Size, float LineHeight)
{
#if ANDROID
	/// <summary>The typeface a native label would use (see <see cref="GraphicsText.Draw"/>).</summary>
	internal Android.Graphics.Typeface PlatformFont { get; init; }
#elif IOS || MACCATALYST
	/// <summary>The font a native label would use (see <see cref="GraphicsText.Draw"/>).</summary>
	internal UIFont PlatformFont { get; init; }
#endif
}

/// <summary>
/// Shows text on a Microsoft.Maui.Graphics canvas as native labels show it. A canvas finds its fonts
/// by name and draws them without character spacing, which leaves out the bold and italic system
/// fonts on iOS and some fonts registered with MAUI: on Android, iOS and Mac Catalyst the text is
/// laid out and drawn by the platform, as a native label's is, into what the canvas draws on. The
/// canvas draws the text itself only where there is no platform canvas under it.
/// </summary>
static class GraphicsText
{
	/// <summary>
	/// The font of a label with <paramref name="family"/> (a font registered with MAUI, a platform
	/// font, or <see langword="null"/> for the default font), <paramref name="attributes"/> and
	/// <paramref name="size"/>, as MAUI's font manager resolves it for a native label.
	/// </summary>
	internal static GraphicsLabelFont Resolve(string family, FontAttributes attributes, double size)
	{
		var bold = attributes.HasFlag(FontAttributes.Bold);
		var italic = attributes.HasFlag(FontAttributes.Italic);
		var font = new GraphicsFont(
			string.IsNullOrEmpty(family) ? null : family,
			bold ? FontWeights.Bold : FontWeights.Normal,
			italic ? FontStyleType.Italic : FontStyleType.Normal);

#if ANDROID || IOS || MACCATALYST
		var mauiFont = Microsoft.Maui.Font.OfSize(
			string.IsNullOrEmpty(family) ? null : family,
			size,
			bold ? FontWeight.Bold : FontWeight.Regular,
			italic ? FontSlant.Italic : FontSlant.Default);
		var fontManager = IPlatformApplication.Current?.Services?.GetService<IFontManager>();
#endif

#if ANDROID
		// A native label's size is in scaled pixels, and it is as high as its font's top and bottom.
		var typeface = fontManager?.GetTypeface(mauiFont) ?? font.ToTypeface();
		var scaledSize = (float)size * Android.App.Application.Context.Resources.Configuration.FontScale;

		using var paint = new TextPaint { TextSize = scaledSize * Density };
		paint.SetTypeface(typeface);
		var metrics = paint.GetFontMetricsInt();

		return new(font, scaledSize, (metrics.Bottom - metrics.Top) / Density) { PlatformFont = typeface };
#elif IOS || MACCATALYST
		// The font manager scales the font as a native label scales it.
		var platformFont = fontManager?.GetFont(mauiFont) ?? UIFont.SystemFontOfSize((nfloat)size);

		// The system fonts cannot be asked for by name (their names start with a dot, and Core Text
		// gives Times New Roman for such a name).
		var name = platformFont.Name.StartsWith('.') ? null : platformFont.Name;
		var scale = UIScreen.MainScreen.Scale;

		return new(
			new GraphicsFont(name, font.Weight, font.StyleType),
			(float)platformFont.PointSize,
			(float)(Math.Ceiling(platformFont.LineHeight * scale) / scale))
		{
			PlatformFont = platformFont,
		};
#else
		// Without a platform (the net10.0 target, where nothing is drawn) there are no fonts to ask.
		return new(font, (float)size, (float)Math.Ceiling(size * 1.2));
#endif
	}

	/// <summary><paramref name="text"/> as a label with <paramref name="transform"/> shows it.</summary>
	internal static string Transform(string text, TextTransform transform) => transform switch
	{
		TextTransform.Uppercase => text?.ToUpperInvariant(),
		TextTransform.Lowercase => text?.ToLowerInvariant(),
		_ => text,
	};

	/// <summary>
	/// The height of a native label that shows <paramref name="text"/> on one line. It is the height
	/// of the font, except on Android for text the font cannot show: the line is then as high as the
	/// system font that shows it (as for day names in Arabic).
	/// </summary>
	internal static float Height(in GraphicsLabelFont font, string text)
	{
#if ANDROID
		if (font.PlatformFont is not null && !string.IsNullOrEmpty(text))
		{
			using var paint = CreatePaint(font, Colors.Black, 0);
			using var layout = CreateLayout(text, paint, int.MaxValue / 2, Android.Text.Layout.Alignment.AlignNormal);
			return layout.Height / Density;
		}
#endif
		return font.LineHeight;
	}

	/// <summary>
	/// Draws one line of <paramref name="text"/> in <paramref name="box"/>, the bounds of the native
	/// label it stands for (as high as <see cref="Height"/> says).
	/// </summary>
	internal static void Draw(
		ICanvas canvas,
		string text,
		in GraphicsLabelFont font,
		Color color,
		RectF box,
		HorizontalAlignment alignment,
		double characterSpacing)
	{
		if (string.IsNullOrEmpty(text) || color is not { Alpha: > 0 } || box.Width <= 0 || box.Height <= 0)
		{
			return;
		}

#if ANDROID
		// Laid out as a TextView lays its text out, and drawn on the canvas of the view. That canvas
		// is in pixels: the Microsoft.Maui.Graphics canvas over it scales the points itself.
		if (font.PlatformFont is not null && PlatformCanvasOf(canvas) is { } platformCanvas)
		{
			using var paint = CreatePaint(font, color, characterSpacing);
			using var layout = CreateLayout(
				text,
				paint,
				(int)Math.Ceiling(box.Width * Density),
				alignment switch
				{
					HorizontalAlignment.Center => Android.Text.Layout.Alignment.AlignCenter,
					HorizontalAlignment.Right => Android.Text.Layout.Alignment.AlignOpposite,
					_ => Android.Text.Layout.Alignment.AlignNormal,
				});

			platformCanvas.Save();
			platformCanvas.Translate(box.X * Density, box.Y * Density);
			layout.Draw(platformCanvas);
			platformCanvas.Restore();
			return;
		}
#elif IOS || MACCATALYST
		// Drawn by UIKit into the context the canvas draws in (the view's, while it is drawn).
		if (font.PlatformFont is { } platformFont && UIGraphics.GetCurrentContext() is not null)
		{
			using var paragraph = new NSMutableParagraphStyle
			{
				Alignment = alignment switch
				{
					HorizontalAlignment.Center => UITextAlignment.Center,
					HorizontalAlignment.Right => UITextAlignment.Right,
					_ => UITextAlignment.Left,
				},
				LineBreakMode = UILineBreakMode.Clip,
			};
			using var platformText = new Foundation.NSString(text);
			platformText.DrawString(
				new CoreGraphics.CGRect(box.X, box.Y, box.Width, box.Height),
				new UIStringAttributes
				{
					Font = platformFont,
					ForegroundColor = UIColor.FromRGBA(color.Red, color.Green, color.Blue, color.Alpha),
					ParagraphStyle = paragraph,
					KerningAdjustment = (float)characterSpacing,
				});
			return;
		}
#endif

		DrawOnCanvas(canvas, text, font, color, box, alignment, characterSpacing);
	}

	// The text as the canvas itself draws it: in the font it finds by name, in the middle of the box.
	static void DrawOnCanvas(
		ICanvas canvas,
		string text,
		in GraphicsLabelFont font,
		Color color,
		RectF box,
		HorizontalAlignment alignment,
		double characterSpacing)
	{
		canvas.Font = font.Font;
		canvas.FontSize = font.Size;
		canvas.FontColor = color;

		if (characterSpacing == 0)
		{
			canvas.DrawString(text, box.X, box.Y, box.Width, box.Height, alignment, VerticalAlignment.Center);
			return;
		}

		// A canvas has no character spacing: the characters are drawn one by one, each after the
		// width of the one before it and the spacing. (Text that needs shaping, as Arabic, loses it.)
		var spacing = (float)characterSpacing;
		var elements = System.Globalization.StringInfo.GetTextElementEnumerator(text);
		var characters = new List<(string Text, float Width)>();
		var width = 0f;
		while (elements.MoveNext())
		{
			var element = elements.GetTextElement();
			var elementWidth = canvas.GetStringSize(element, font.Font, font.Size).Width;
			characters.Add((element, elementWidth));
			width += elementWidth + spacing;
		}

		var x = alignment switch
		{
			HorizontalAlignment.Center => box.X + ((box.Width - width) / 2),
			HorizontalAlignment.Right => box.Right - width,
			_ => box.X,
		};

		foreach (var (element, elementWidth) in characters)
		{
			canvas.DrawString(element, x, box.Y, elementWidth + 1, box.Height, HorizontalAlignment.Left, VerticalAlignment.Center);
			x += elementWidth + spacing;
		}
	}

#if ANDROID
	static float Density => Android.App.Application.Context.Resources.DisplayMetrics.Density;

	// The canvas of the view under a Microsoft.Maui.Graphics canvas, or null when it draws elsewhere.
	static Android.Graphics.Canvas PlatformCanvasOf(ICanvas canvas) => canvas switch
	{
		PlatformCanvas platformCanvas => platformCanvas.Canvas,
		ScalingCanvas scalingCanvas => PlatformCanvasOf(scalingCanvas.ParentCanvas),
		_ => null,
	};

	// The paint of a TextView with this font; MAUI gives a label's CharacterSpacing to it in ems.
	static TextPaint CreatePaint(in GraphicsLabelFont font, Color color, double characterSpacing)
	{
		var paint = new TextPaint(Android.Graphics.PaintFlags.AntiAlias)
		{
			TextSize = font.Size * Density,
			LetterSpacing = (float)characterSpacing * 0.0624f,
			Color = new Android.Graphics.Color(
				(byte)Math.Round(color.Red * 255),
				(byte)Math.Round(color.Green * 255),
				(byte)Math.Round(color.Blue * 255),
				(byte)Math.Round(color.Alpha * 255)),
		};
		paint.SetTypeface(font.PlatformFont);
		return paint;
	}

	// One line as a TextView lays it out: with the font's padding above and below it, and as high as
	// the fonts that show its characters.
	static StaticLayout CreateLayout(string text, TextPaint paint, int width, Android.Text.Layout.Alignment alignment)
	{
		if (!OperatingSystem.IsAndroidVersionAtLeast(23))
		{
#pragma warning disable CA1422 // The constructor is the only way before Android 6
			return new StaticLayout(text, paint, width, alignment, 1f, 0f, true);
#pragma warning restore CA1422
		}

		var builder = StaticLayout.Builder.Obtain(text, 0, text.Length, paint, width)
			.SetAlignment(alignment)
			.SetIncludePad(true)
			.SetMaxLines(1);

		if (OperatingSystem.IsAndroidVersionAtLeast(28))
		{
			builder.SetUseLineSpacingFromFallbacks(true);
		}

		return builder.Build();
	}
#endif
}
