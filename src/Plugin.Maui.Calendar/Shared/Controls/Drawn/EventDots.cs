using DrawnUi.Draw;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace Plugin.Maui.Calendar.Controls.Drawn;

/// <summary>
/// Row of 8pt event-indicator dots, painted directly instead of one child per dot
/// (replaces the former HorizontalStackLayout of rounded Borders).
/// </summary>
sealed class EventDots : SkiaControl
{
	const double dotSize = 8;

	IReadOnlyList<Color> colors = [];

	public EventDots()
	{
		HeightRequest = dotSize;
		WidthRequest = 0;
		HorizontalOptions = LayoutOptions.Center;
		UseCache = SkiaCacheType.Operations;
	}

	public void SetColors(IReadOnlyList<Color> value)
	{
		value ??= [];
		if (value.Count == colors.Count && value.SequenceEqual(colors))
		{
			return;
		}

		colors = value;
		WidthRequest = dotSize * colors.Count;
		Update();
	}

	protected override void Paint(DrawingContext ctx)
	{
		base.Paint(ctx);

		if (colors.Count == 0)
		{
			return;
		}

		var diameter = (float)(dotSize * ctx.Scale);
		var radius = diameter / 2f;
		var x = ctx.Destination.Left + radius;
		var y = ctx.Destination.MidY;

		using var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
		foreach (var color in colors)
		{
			paint.Color = (color ?? Colors.Transparent).ToSKColor();
			ctx.Context.Canvas.DrawCircle(x, y, radius, paint);
			x += diameter;
		}
	}
}
