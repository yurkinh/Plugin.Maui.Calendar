using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls.Drawn;

/// <summary>
/// Paints the day grid of a <see cref="GraphicsDayGrid"/> on a Microsoft.Maui.Graphics canvas: what
/// the native grid of <c>ViewLayoutBase.GenerateWeekLayout</c> shows, a row of day-of-week titles
/// over the built-in day cell of <c>DayView.xaml</c> for every <see cref="DayModel"/>. It holds what
/// is shown and where (the places of the rows and cells are computed here); the view gives it the
/// values, is as high as <see cref="Measure"/> says, and asks it for the day under a tap.
/// </summary>
sealed class GraphicsDayGridDrawable : IDrawable
{
	const int daysInWeek = 7;

	// The row spacing of the native grid.
	internal const float RowSpacing = 6;

	// The built-in cell (see DayView.xaml): a spacer, the day label and the row of event dots.
	const float spacerHeight = 4;
	const float dotSize = 8;
	const int maximumDots = 5;
	const float outlineWidth = 1;

	// A native Border paints inside its stroke, also when the stroke is transparent or not set: its
	// fill is this much smaller on every side, and the middle of its 1 point stroke this far inside.
	const float borderFillInset =
#if ANDROID
		1;
#else
		0.5f;
#endif
	const float borderStrokeInset = 1;

	// The same for the Border of an event dot, as measured on a native cell.
	const float dotInset =
#if ANDROID
		0.75f;
#else
		0.5f;
#endif

	readonly StyleFont dayLabel;
	readonly StyleFont title;
	readonly StyleFont weekendTitle;

	DayModel[] days = [];
	string[] titleTexts = [];
	bool[] weekendTitles = [];

	Color bandColor;
	float bandCornerRadius;
	DayOfWeek bandFirstDayOfWeek;

	bool isRightToLeft;

	// The rows, from the top of the grid: the title row, then the top and height of every week.
	float titleRowHeight;
	float[] weekTops = [];
	float[] weekHeights = [];

	/// <summary>
	/// The styles of the day labels, of the titles and of the weekend titles, resolved as the native
	/// labels resolve them.
	/// </summary>
	internal GraphicsDayGridDrawable(DrawnLabelStyle dayLabelStyle, DrawnLabelStyle titleStyle, DrawnLabelStyle weekendTitleStyle)
	{
		dayLabel = new StyleFont(dayLabelStyle);
		title = new StyleFont(titleStyle);
		weekendTitle = new StyleFont(weekendTitleStyle);
	}

	/// <summary>The days, in grid order.</summary>
	internal IReadOnlyList<DayModel> Days
	{
		get => days;
		set => days = [.. value];
	}

	/// <summary>Whether the week runs from right to left: the first day of the week is then in the rightmost column.</summary>
	internal bool IsRightToLeft
	{
		get => isRightToLeft;
		set => isRightToLeft = value;
	}

	/// <summary>The 7 day-of-week titles, from the first day of the week, and which of them are weekend days.</summary>
	internal void SetTitles(IReadOnlyList<string> texts, IReadOnlyList<bool> weekend)
	{
		titleTexts = [.. texts];
		weekendTitles = [.. weekend];
	}

	/// <summary>The band behind the weekend days of every week; a transparent color shows none.</summary>
	internal void SetWeekendBands(Color color, float cornerRadius, DayOfWeek firstDayOfWeek)
	{
		bandColor = color;
		bandCornerRadius = cornerRadius;
		bandFirstDayOfWeek = firstDayOfWeek;
	}

	/// <summary>
	/// Places the rows as the native grid does: the title row as high as its highest title, then
	/// every week as high as its highest shown day (a week of hidden days takes no height), with
	/// the row spacing between them. Returns the height of the rows: the view is as high as they are.
	/// </summary>
	internal float Measure()
	{
		titleRowHeight = 0;
		for (var i = 0; i < titleTexts.Length; i++)
		{
			var style = TitleOf(i);
			titleRowHeight = Math.Max(
				titleRowHeight,
				style.HeightOf(TitleText(i)) + Vertical(style.Style.Margin) + Vertical(style.Style.Padding));
		}

		var weeks = days.Length / daysInWeek;
		if (weekTops.Length != weeks)
		{
			weekTops = new float[weeks];
			weekHeights = new float[weeks];
		}

		var y = titleRowHeight;
		for (var week = 0; week < weeks; week++)
		{
			var height = 0f;
			for (var column = 0; column < daysInWeek; column++)
			{
				var day = days[(week * daysInWeek) + column];
				if (day.IsControlVisible)
				{
					height = Math.Max(height, (float)day.DayViewHeight);
				}
			}

			y += RowSpacing;
			weekTops[week] = y;
			weekHeights[week] = height;
			y += height;
		}

		return y;
	}

	// A title as its label shows it.
	string TitleText(int column) => GraphicsText.Transform(titleTexts[column], TitleOf(column).Style.TextTransform);

	StyleFont TitleOf(int column) => column < weekendTitles.Length && weekendTitles[column] ? weekendTitle : title;

	// The grid column of a day, counted from the first day of the week.
	int ColumnOf(int dayOfWeekColumn) => isRightToLeft ? daysInWeek - 1 - dayOfWeekColumn : dayOfWeekColumn;

	static float Vertical(Thickness thickness) => (float)thickness.VerticalThickness;

	/// <summary>The place of a day's cell in a grid <paramref name="width"/> wide: its column, as high as its week.</summary>
	internal RectF CellBounds(int index, float width)
	{
		var columnWidth = width / daysInWeek;
		var week = index / daysInWeek;
		return new RectF(ColumnOf(index % daysInWeek) * columnWidth, weekTops[week], columnWidth, weekHeights[week]);
	}

	/// <summary>The day whose cell is at <paramref name="point"/> in a grid <paramref name="width"/> wide, or <see langword="null"/>.</summary>
	internal DayModel DayAt(Point point, double width)
	{
		for (var i = 0; i < days.Length; i++)
		{
			if (days[i].IsControlVisible && CellBounds(i, (float)width).Contains((float)point.X, (float)point.Y))
			{
				return days[i];
			}
		}

		return null;
	}

	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		var width = dirtyRect.Width;
		if (width <= 0)
		{
			return;
		}

		canvas.Antialias = true;

		DrawWeekendBands(canvas, width);

		DrawDays(canvas, width);
		DrawTitles(canvas, width);
	}

	// One band per weekend column and week, as wide as the column, reaching half the row spacing
	// above and below its row so the bands of consecutive weeks touch (see Calendar.CreateWeekendBands).
	void DrawWeekendBands(ICanvas canvas, float width)
	{
		if (bandColor is not { Alpha: > 0 })
		{
			return;
		}

		var columnWidth = width / daysInWeek;
		var bleed = RowSpacing / 2;
		canvas.FillColor = bandColor;

		for (var week = 0; week < weekTops.Length; week++)
		{
			for (var column = 0; column < daysInWeek; column++)
			{
				if (ViewLayoutEngines.ViewLayoutBase.IsWeekendColumn(bandFirstDayOfWeek, column))
				{
					var height = weekHeights[week] + (2 * bleed);
					canvas.FillRoundedRectangle(
						ColumnOf(column) * columnWidth,
						weekTops[week] - bleed,
						columnWidth,
						height,
						Math.Min(bandCornerRadius, Math.Min(columnWidth, height) / 2));
				}
			}
		}
	}

	void DrawDays(ICanvas canvas, float width)
	{
		for (var i = 0; i < days.Length; i++)
		{
			if (days[i].IsControlVisible)
			{
				DrawDay(canvas, days[i], CellBounds(i, width));
			}
		}
	}

	void DrawTitles(ICanvas canvas, float width)
	{
		var columnWidth = width / daysInWeek;

		for (var i = 0; i < titleTexts.Length && i < daysInWeek; i++)
		{
			var style = TitleOf(i);
			var label = style.Style;

			// The native titles are always centered: their labels are given the alignment themselves
			// (see ViewLayoutBase), which a style cannot change.
			var box = new RectF(
				(ColumnOf(i) * columnWidth) + (float)(label.Margin.Left + label.Padding.Left),
				(float)(label.Margin.Top + label.Padding.Top),
				columnWidth - (float)(label.Margin.HorizontalThickness + label.Padding.HorizontalThickness),
				style.HeightOf(TitleText(i)));

			GraphicsText.Draw(
				canvas,
				TitleText(i),
				style.Font,
				label.TextColor.MultiplyAlpha((float)label.Opacity),
				box,
				HorizontalAlignment.Center,
				label.CharacterSpacing);
		}
	}

	/// <summary>
	/// Paints a day as the built-in cell of DayView.xaml shows it: the cell (painted for
	/// <see cref="EventIndicatorType.BackgroundFull"/>), in its middle a <c>DayViewSize</c> ×
	/// <c>DayViewHeight</c> container with the background and outline (the Border) under a column
	/// (the FlexLayout) of a spacer, the day label and the event dots, reversed for
	/// <see cref="EventIndicatorType.TopDot"/>.
	/// </summary>
	void DrawDay(ICanvas canvas, DayModel day, RectF cell)
	{
		if (day.BackgroundFullEventColor is { Alpha: > 0 } fullColor)
		{
			canvas.FillColor = fullColor;
			canvas.FillRectangle(cell.X, cell.Y, cell.Width, (float)day.DayViewHeight);
		}

		// A hidden day (IsVisible) keeps its room and shows nothing.
		if (!day.IsVisible)
		{
			return;
		}

		var size = (float)day.DayViewSize;
		var height = (float)day.DayViewHeight;
		var container = new RectF(cell.X + ((cell.Width - size) / 2), cell.Y, size, height);

		// The Border fills the container up to DayViewSize (its maximum size), inside its margin.
		var margin = day.DayViewBorderMargin;
		var inner = new RectF(
			container.X + (float)margin.Left,
			container.Y + (float)margin.Top,
			Math.Max(0, size - (float)margin.HorizontalThickness),
			Math.Max(0, height - (float)margin.VerticalThickness));
		var borderWidth = Math.Min(size, inner.Width);
		var borderHeight = Math.Min(size, inner.Height);
		var border = new RectF(
			inner.X + ((inner.Width - borderWidth) / 2),
			inner.Y + ((inner.Height - borderHeight) / 2),
			borderWidth,
			borderHeight);
		var radius = Math.Min(day.DayViewCornerRadius, Math.Min(border.Width, border.Height) / 2);

		if (border.Width > 0 && border.Height > 0)
		{
			if (day.BackgroundColor is { Alpha: > 0 } background)
			{
				canvas.FillColor = background;
				canvas.FillRoundedRectangle(border.Inflate(-borderFillInset, -borderFillInset), Math.Max(0, radius - borderFillInset));
			}

			if (day.OutlineColor is { Alpha: > 0 } outline)
			{
				canvas.StrokeColor = outline;
				canvas.StrokeSize = outlineWidth;
				canvas.DrawRoundedRectangle(border.Inflate(-borderStrokeInset, -borderStrokeInset), Math.Max(0, radius - borderStrokeInset));
			}
		}

		// The column in the middle of the container: the dot row sits above the label for TopDot and
		// below it otherwise, in every cell, so the numbers stay aligned across the grid.
		// A FlexLayout counts the margin of a child twice (as part of the child's size, and again
		// around it), so the label takes its margin twice and its text starts after twice its top margin.
		var label = dayLabel.Style;
		var font = dayLabel.Font;
		var text = GraphicsText.Transform(day.Day, label.TextTransform);
		var textHeight = dayLabel.HeightOf(text);
		var labelHeight = textHeight + (2 * Vertical(label.Margin)) + Vertical(label.Padding);
		var top = container.Y + ((height - (spacerHeight + labelHeight + dotSize)) / 2);
		var dotsOnTop = day.EventIndicatorType == EventIndicatorType.TopDot;
		var labelTop = top + (dotsOnTop ? dotSize : spacerHeight);
		var dotsTop = dotsOnTop ? top : top + spacerHeight + labelHeight;
		var center = container.Center.X;

		// The label is as wide as its text and in the middle of the column: the text may be wider
		// than the container, so it is centered in the whole cell.
		var shift = (float)((label.Margin.Left - label.Margin.Right) + ((label.Padding.Left - label.Padding.Right) / 2));
		GraphicsText.Draw(
			canvas,
			text,
			font,
			// The day's state decides the color, whatever the style says (see DayView.xaml).
			day.TextColor?.MultiplyAlpha((float)label.Opacity),
			new RectF(cell.X + shift, labelTop + (float)((2 * label.Margin.Top) + label.Padding.Top), cell.Width, textHeight),
			HorizontalAlignment.Center,
			label.CharacterSpacing);

		// The dot row shows EventColors whatever the indicator type, like the native cell.
		if (day.EventColors is { Count: > 0 } colors)
		{
			var count = Math.Min(maximumDots, colors.Count);
			var x = center - (count * dotSize / 2);
			for (var i = 0; i < count; i++)
			{
				canvas.FillColor = colors[i];
				// Each dot is a Border too.
				canvas.FillEllipse(
					x + (i * dotSize) + dotInset,
					dotsTop + dotInset,
					dotSize - (2 * dotInset),
					dotSize - (2 * dotInset));
			}
		}
	}

	/// <summary>A label style and the font it resolves to, resolved again when the style changes.</summary>
	sealed class StyleFont(DrawnLabelStyle style)
	{
		// The heights of the texts shown in this font (the day numbers of a month, or the titles).
		readonly Dictionary<string, float> heights = [];

		int version = -1;
		GraphicsLabelFont font;

		internal DrawnLabelStyle Style => style;

		/// <summary>The height of a native label with this style that shows <paramref name="text"/>.</summary>
		internal float HeightOf(string text)
		{
			var labelFont = Font;
			if (string.IsNullOrEmpty(text))
			{
				return labelFont.LineHeight;
			}

			if (!heights.TryGetValue(text, out var height))
			{
				heights[text] = height = GraphicsText.Height(labelFont, text);
			}

			return height;
		}

		internal GraphicsLabelFont Font
		{
			get
			{
				if (version != style.Version)
				{
					version = style.Version;
					font = GraphicsText.Resolve(style.FontFamily, style.FontAttributes, style.FontSize);
					heights.Clear();
				}

				return font;
			}
		}
	}
}
