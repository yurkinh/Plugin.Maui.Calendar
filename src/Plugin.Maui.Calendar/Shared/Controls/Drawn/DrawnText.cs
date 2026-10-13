using DrawnUi.Draw;
using SkiaSharp;

namespace Plugin.Maui.Calendar.Controls.Drawn;

/// <summary>
/// Shows text in drawn labels as native labels would: in the label's font, or in a system font
/// for text that font cannot show. The other text settings come from <see cref="DrawnLabelStyle"/>.
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

	/// <summary>
	/// Shows <paramref name="text"/> in <paramref name="fontFamily"/> (the label's resolved font).
	/// A drawn label only uses its own font, while a native label falls back to the system fonts:
	/// text the font cannot show (day names in Arabic, Chinese or Thai, native digits) is drawn with
	/// the system font that has its first character. Call it whenever the text or the font changes.
	/// </summary>
	internal static void SetText(SkiaLabel label, string text, string fontFamily)
	{
		var styleFamily = fontFamily;
		label.Text = text;

#if ANDROID || IOS || MACCATALYST || WINDOWS
		// Bold or italic without a font family: a native label uses the bold or italic system font,
		// and so does the drawn one, rather than a synthetic bold of the regular font.
		if (string.IsNullOrEmpty(styleFamily) && label.FontAttributes != FontAttributes.None)
		{
			styleFamily = DefaultFontAlias(label.FontAttributes);
			label.FontAttributes = FontAttributes.None;
		}
#endif

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

#if ANDROID || IOS || MACCATALYST || WINDOWS
	// The alias under which DrawnUI's font manager knows the system's default font with these attributes.
	static string DefaultFontAlias(FontAttributes attributes)
	{
		var alias = $"Plugin.Maui.Calendar.Default.{attributes}";
		if (!SkiaFontManager.Instance.Fonts.ContainsKey(alias))
		{
			SkiaFontManager.Instance.Fonts[alias] = SKTypeface.FromFamilyName(
				SkiaFontManager.DefaultTypeface.FamilyName,
				attributes.HasFlag(FontAttributes.Bold) ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
				SKFontStyleWidth.Normal,
				attributes.HasFlag(FontAttributes.Italic) ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);
		}

		return alias;
	}
#endif

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
