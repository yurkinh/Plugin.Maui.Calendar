using DrawnUi.Draw;
using SkiaSharp;

namespace Plugin.Maui.Calendar.Controls.Drawn;

/// <summary>
/// Gives drawn labels the text settings the native labels of the calendar get from their
/// <see cref="Style"/>s, and a system font for text their font cannot show.
/// </summary>
static class DrawnText
{
	/// <summary>
	/// The font size of a native day or title label whose style sets none: the platform's own label
	/// size, which the labels resolve before they are on screen (see DayView.AddBuiltInContent).
	/// </summary>
	internal const double DefaultLabelFontSize =
#if IOS || MACCATALYST
		17;
#else
		14;
#endif

	/// <summary>The value <paramref name="style"/> (or a style it is based on) sets for <paramref name="property"/>.</summary>
	internal static T Get<T>(Style style, BindableProperty property, T fallback)
	{
		for (var current = style; current is not null; current = current.BasedOn)
		{
			foreach (var setter in current.Setters)
			{
				if (setter.Property == property && setter.Value is T value)
				{
					return value;
				}
			}
		}

		return fallback;
	}

	/// <summary>
	/// Gives <paramref name="label"/> the text settings of a <see cref="Label"/> style. Only plain
	/// setter values are read; theme values and dynamic resources come with phase 3.
	/// </summary>
	internal static void ApplyLabelStyle(SkiaLabel label, Style style, double defaultFontSize, TextAlignment defaultAlignment = TextAlignment.Start)
	{
		label.TextColor = Get(style, Label.TextColorProperty, DefaultTextColor);
		label.FontSize = Get(style, Label.FontSizeProperty, defaultFontSize);
		label.FontAttributes = Get(style, Label.FontAttributesProperty, FontAttributes.None);
		SetFontFamily(label, Get<string>(style, Label.FontFamilyProperty, null), force: false);
		label.CharacterSpacing = Get(style, Label.CharacterSpacingProperty, 0d);
		label.Margin = Get(style, View.MarginProperty, new Thickness(0));
		label.Padding = Get(style, Label.PaddingProperty, new Thickness(0));
		label.HorizontalTextAlignment = Get(style, Label.HorizontalTextAlignmentProperty, defaultAlignment) switch
		{
			TextAlignment.Center => DrawTextAlignment.Center,
			TextAlignment.End => DrawTextAlignment.End,
			_ => DrawTextAlignment.Start,
		};
	}

	// A native label without a TextColor uses the platform's text color, which follows the theme.
	static Color DefaultTextColor => Application.Current?.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black;

	/// <summary>
	/// Shows <paramref name="text"/> in the font of <paramref name="style"/>. A drawn label only uses
	/// its own font, while a native label falls back to the system fonts: text the font cannot show
	/// (day names in Arabic, Chinese or Thai, native digits) is drawn with the system font that has
	/// its first character. Call it after <see cref="ApplyLabelStyle"/>, whenever the text or the
	/// style changes.
	/// </summary>
	internal static void SetText(SkiaLabel label, string text, Style style)
	{
		var styleFamily = Get<string>(style, Label.FontFamilyProperty, null);
		label.Text = text;

		// The label is first given the style's font, so the check looks at that font.
		var autoFont = label.AutoFont;
		label.AutoFont = false;
		SetFontFamily(label, styleFamily, force: autoFont || label.FontFamily != styleFamily);

		if (!NeedsSystemFont(label, text))
		{
			return;
		}

		if (text.Any(char.IsLetter))
		{
			// Words (day names) need shaping: AutoFont shapes the text, joining Arabic letters.
			label.AutoFont = true;
		}
		else if (SystemFontAlias(char.ConvertToUtf32(text, 0)) is { } alias)
		{
			// Numbers must not be shaped: DrawnUI shapes Arabic-block text right to left, which
			// reverses the Arabic-Indic digits of a number. Without AutoFont the label draws the
			// glyphs left to right. The font is named through FontFamily: a label without one
			// goes back to the default typeface on its next update.
			SetFontFamily(label, alias, force: false);
		}
	}

	/// <summary>
	/// The alias under which DrawnUI's font manager knows the system font that has
	/// <paramref name="character"/>, or <see langword="null"/> when there is none. The fonts are
	/// registered under aliases of their own: a system font found by character cannot always be
	/// found again by its family name (on Android the default font would be used instead).
	/// </summary>
	static string SystemFontAlias(int character)
	{
#if ANDROID || IOS || MACCATALYST || WINDOWS
		if (SkiaFontManager.MatchCharacter(character) is not { } typeface)
		{
			return null;
		}

		var alias = $"Plugin.Maui.Calendar.System.{typeface.FamilyName}";
		SkiaFontManager.Instance.Fonts.TryAdd(alias, typeface);
		return alias;
#else
		return null;
#endif
	}

	// A label keeps the typeface AutoFont chose until its font family changes. Without a platform
	// (the net10.0 target, where nothing is drawn) DrawnUI's font manager throws for any family.
	static void SetFontFamily(SkiaLabel label, string family, bool force)
	{
#if ANDROID || IOS || MACCATALYST || WINDOWS
		if (force && label.FontFamily == family)
		{
			label.FontFamily = family is null ? string.Empty : null;
		}

		label.FontFamily = family;
#endif
	}


	static bool NeedsSystemFont(SkiaLabel label, string text)
	{
#if ANDROID || IOS || MACCATALYST || WINDOWS
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}

		var weight = label.FontAttributes.HasFlag(FontAttributes.Bold) ? (int)SKFontStyleWeight.Bold : (int)SKFontStyleWeight.Normal;
		var typeface = SkiaFontManager.Instance.GetFont(label.FontFamily, weight) ?? SkiaFontManager.DefaultTypeface;
		using var font = new SKFont(typeface);
		return !font.ContainsGlyphs(text);
#else
		// Without a platform there are no fonts to look in (and nothing is drawn).
		return false;
#endif
	}
}
