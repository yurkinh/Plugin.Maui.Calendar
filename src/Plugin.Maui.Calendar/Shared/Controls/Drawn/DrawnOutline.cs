using DrawnUi.Draw;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace Plugin.Maui.Calendar.Controls.Drawn;

/// <summary>
/// The stroke of a native Border: a rounded rectangle outline, <see cref="StrokeWidth"/> points wide,
/// drawn inside the control's bounds. A <see cref="SkiaShape"/> stroke loses part of a 1 point
/// stroke to pixel snapping (DrawnUi 1.10.7.3), so the today outline is drawn by this control.
/// </summary>
sealed class DrawnOutline : SkiaControl
{
	readonly SKPaint paint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke };

	Color color = Colors.Transparent;
	float cornerRadius;

	/// <summary>The width of the stroke, in points.</summary>
	internal const float StrokeWidth = 1f;

	internal Color Color
	{
		get => color;
		set
		{
			if (color != value)
			{
				color = value;
				Update();
			}
		}
	}

	internal float CornerRadius
	{
		get => cornerRadius;
		set
		{
			if (cornerRadius != value)
			{
				cornerRadius = value;
				Update();
			}
		}
	}

	protected override void Paint(DrawingContext ctx)
	{
		base.Paint(ctx);

		if (color is not { Alpha: > 0 } || ctx.Destination.Width <= 0 || ctx.Destination.Height <= 0)
		{
			return;
		}

		var stroke = StrokeWidth * ctx.Scale;
		var half = stroke / 2f;
		var rect = SKRect.Inflate(ctx.Destination, -half, -half);
		var radius = Math.Max(0, (cornerRadius * ctx.Scale) - half);

		paint.Color = color.ToSKColor();
		paint.StrokeWidth = stroke;
		ctx.Context.Canvas.DrawRoundRect(rect, radius, radius, paint);
	}

	public override void OnDisposing()
	{
		paint.Dispose();
		base.OnDisposing();
	}
}
