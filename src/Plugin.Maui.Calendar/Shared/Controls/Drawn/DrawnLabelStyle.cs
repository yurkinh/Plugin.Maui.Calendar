using System.ComponentModel;
using DrawnUi.Draw;

namespace Plugin.Maui.Calendar.Controls.Drawn;

/// <summary>
/// Resolves a <see cref="Label"/> <see cref="Style"/> for drawn labels exactly as a native label of
/// the calendar resolves it, by applying it to a native label that is never shown. That label is a
/// logical child of the calendar, so it gets everything a native day or title label gets: the
/// style's <see cref="Style.BasedOn"/> chain, the app's implicit Label style, theme values
/// (AppThemeBinding) and dynamic resources, and their later changes, which raise <see cref="Changed"/>.
/// </summary>
sealed class DrawnLabelStyle : IDisposable
{
	// The label properties a drawn label shows; a change of any other property is ignored.
	static readonly HashSet<string> shownProperties =
	[
		nameof(Label.TextColor),
		nameof(Label.FontSize),
		nameof(Label.FontAttributes),
		nameof(Label.FontFamily),
		nameof(Label.CharacterSpacing),
		nameof(Label.TextTransform),
		nameof(Label.Margin),
		nameof(Label.Padding),
		nameof(Label.HorizontalTextAlignment),
		nameof(Label.Opacity),
	];

	readonly Element owner;
	readonly Label resolver = new();

	/// <summary>Raised when a value the drawn labels show changes: a new style, theme or resource value.</summary>
	internal event Action Changed;

	internal DrawnLabelStyle(Element owner)
	{
		this.owner = owner;
		owner.AddLogicalChild(resolver);
		resolver.PropertyChanged += OnResolverPropertyChanged;

		// A label whose styles set no text color uses the platform's, which follows the theme
		// without any property of the label changing. (Application raises it as a weak event.)
		if (Application.Current is { } application)
		{
			application.RequestedThemeChanged += OnRequestedThemeChanged;
		}
	}

	void OnRequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
	{
		if (resolver.TextColor is null)
		{
			Version++;
			Changed?.Invoke();
		}
	}

	/// <summary>The style to resolve.</summary>
	internal Style Style
	{
		get => resolver.Style;
		set => resolver.Style = value;
	}

	/// <summary>Increases whenever the resolved values change, so labels can tell they are out of date.</summary>
	internal int Version { get; private set; }

	/// <summary>The resolved font family, or <see langword="null"/> for the default font.</summary>
	internal string FontFamily => resolver.FontFamily;

	void OnResolverPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is { } name && (shownProperties.Contains(name) || name == nameof(Label.Style)))
		{
			Version++;
			Changed?.Invoke();
		}
	}

	/// <summary>
	/// Gives <paramref name="label"/> the resolved text settings. A native label whose styles set no
	/// font size uses the platform's size, and one without a text color the platform's text color.
	/// </summary>
	internal void ApplyTo(SkiaLabel label, TextAlignment defaultAlignment)
	{
		label.TextColor = resolver.TextColor ?? DefaultTextColor;
		label.FontSize = resolver.IsSet(Label.FontSizeProperty) ? resolver.FontSize : DrawnText.DefaultLabelFontSize;
		label.FontAttributes = resolver.FontAttributes;
		// A SkiaLabel spaces its characters by CharacterSpacing - 1 points: 1 is the normal spacing, which
		// is 0 for a native label.
		label.CharacterSpacing = 1 + resolver.CharacterSpacing;
		label.Margin = resolver.Margin;
		label.Padding = resolver.Padding;
		label.Opacity = resolver.Opacity;
		label.TextTransform = resolver.TextTransform switch
		{
			Microsoft.Maui.TextTransform.Uppercase => DrawnUi.Draw.TextTransform.Uppercase,
			Microsoft.Maui.TextTransform.Lowercase => DrawnUi.Draw.TextTransform.Lowercase,
			_ => DrawnUi.Draw.TextTransform.None,
		};
		label.HorizontalTextAlignment = (resolver.IsSet(Label.HorizontalTextAlignmentProperty) ? resolver.HorizontalTextAlignment : defaultAlignment) switch
		{
			TextAlignment.Center => DrawTextAlignment.Center,
			TextAlignment.End => DrawTextAlignment.End,
			_ => DrawTextAlignment.Start,
		};
	}

	// A native label without a TextColor uses the platform's text color, which follows the theme.
	static Color DefaultTextColor => Application.Current?.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black;

	public void Dispose()
	{
		if (Application.Current is { } application)
		{
			application.RequestedThemeChanged -= OnRequestedThemeChanged;
		}

		resolver.PropertyChanged -= OnResolverPropertyChanged;
		owner.RemoveLogicalChild(resolver);
	}
}
