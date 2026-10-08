using DrawnUi.Draw;

namespace Plugin.Maui.Calendar.Controls.Drawn;

/// <summary>
/// Lets the public <see cref="Style"/> properties (which target MAUI <see cref="Label"/>)
/// drive drawn <see cref="SkiaLabel"/>s.
/// <para>
/// The style is applied to a hidden MAUI <see cref="Label"/> that is never rendered; MAUI
/// itself resolves <c>BasedOn</c>, implicit styles, <c>DynamicResource</c> and
/// <c>AppThemeBinding</c> setters on it. The resolved values are then copied onto the drawn
/// labels, and copied again whenever any of them changes (theme switch, resource swap).
/// </para>
/// </summary>
sealed class LabelStyleBridge
{
	readonly Label probe;
	bool applyingStyle;

	/// <summary>Raised when any resolved value changed and drawn labels must be refreshed.</summary>
	public event EventHandler Changed;

	/// <param name="owner">Logical parent, so resource lookups walk the same tree as before.</param>
	public LabelStyleBridge(Element owner)
	{
		// HorizontalTextAlignment was set locally in the former XAML, so it wins over the style
		probe = new Label { HorizontalTextAlignment = TextAlignment.Center, Parent = owner };
		probe.PropertyChanged += OnProbePropertyChanged;
	}

	public Style Style
	{
		get => probe.Style;
		set
		{
			if (!ReferenceEquals(probe.Style, value))
			{
				// one notification for the whole style instead of one per setter
				applyingStyle = true;
				try
				{
					probe.Style = value;
				}
				finally
				{
					applyingStyle = false;
				}
				Changed?.Invoke(this, EventArgs.Empty);
			}
		}
	}

	public Color TextColor => probe.TextColor;

	public Thickness Margin => probe.Margin;

	/// <summary>
	/// Copies text attributes onto <paramref name="label"/>.
	/// </summary>
	/// <param name="applyTextColor">False when the text color is driven by day state instead of the style.</param>
	/// <param name="applyLayoutOptions">False inside the day cell, which (as the former FlexLayout did) always centers its label.</param>
	public void ApplyTo(SkiaLabel label, bool applyTextColor, bool applyLayoutOptions)
	{
		if (applyTextColor)
		{
			label.TextColor = probe.TextColor ?? Colors.Black;
		}

		label.FontSize = probe.FontSize > 0 ? probe.FontSize : 14;
		label.FontFamily = probe.FontFamily;
		label.FontAttributes = probe.FontAttributes;
		label.CharacterSpacing = probe.CharacterSpacing;
		label.TextTransform = ToDrawn(probe.TextTransform);
		label.LineBreakMode = probe.LineBreakMode;
		label.MaxLines = probe.MaxLines;
		label.HorizontalTextAlignment = ToDrawn(probe.HorizontalTextAlignment);
		label.VerticalTextAlignment = probe.VerticalTextAlignment;
		label.Margin = probe.Margin;
		label.Padding = probe.Padding;
		label.Opacity = probe.Opacity;

		if (probe.BackgroundColor is not null)
		{
			label.BackgroundColor = probe.BackgroundColor;
		}

		if (applyLayoutOptions)
		{
			label.HorizontalOptions = probe.HorizontalOptions;
			label.VerticalOptions = probe.VerticalOptions;
		}
	}

	public void Detach()
	{
		probe.PropertyChanged -= OnProbePropertyChanged;
		probe.Parent = null;
	}

	void OnProbePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (!applyingStyle && e.PropertyName != nameof(Label.Style))
		{
			Changed?.Invoke(this, EventArgs.Empty);
		}
	}

	// enum values differ between MAUI and DrawnUI, map by name
	static DrawnUi.Draw.TextTransform ToDrawn(Microsoft.Maui.TextTransform transform) => transform switch
	{
		Microsoft.Maui.TextTransform.Lowercase => DrawnUi.Draw.TextTransform.Lowercase,
		Microsoft.Maui.TextTransform.Uppercase => DrawnUi.Draw.TextTransform.Uppercase,
		_ => DrawnUi.Draw.TextTransform.None
	};

	static DrawTextAlignment ToDrawn(TextAlignment alignment) => alignment switch
	{
		TextAlignment.Center => DrawTextAlignment.Center,
		TextAlignment.End => DrawTextAlignment.End,
		_ => DrawTextAlignment.Start
	};
}
