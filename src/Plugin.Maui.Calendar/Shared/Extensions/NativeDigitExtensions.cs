using System.Globalization;
using System.Text;

namespace Plugin.Maui.Calendar.Shared.Extensions;

public static class NativeDigitExtensions
{
	public static string ToNativeDigitString(this DateTime date, string format, CultureInfo culture)
	{
		var formatted = date.ToString(format, culture);
		var nativeDigits = GetNativeDigits(culture);

		if (nativeDigits[0] == "0")
		{
			return formatted;
		}

		var result = new StringBuilder();
		foreach (var c in formatted)
		{
			if (char.IsDigit(c))
			{
				result.Append(nativeDigits[c - '0']);
			}
			else
			{
				result.Append(c);
			}
		}

		return result.ToString();
	}

	public static string ToNativeDigitString(this int number, CultureInfo culture)
	{
		var nativeDigits = GetNativeDigits(culture);

		if (nativeDigits[0] == "0")
		{
			return number.ToString();
		}

		var result = new StringBuilder();
		foreach (var c in number.ToString())
		{
			if (char.IsDigit(c))
			{
				result.Append(nativeDigits[c - '0']);
			}
			else
			{
				result.Append(c);
			}
		}

		return result.ToString();
	}

#if IOS || MACCATALYST
    private static readonly Dictionary<string, string[]> NativeDigitsMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ar"] = new[] { "٠", "١", "٢", "٣", "٤", "٥", "٦", "٧", "٨", "٩" },
        ["fa-ir"] = new[] { "۰", "۱", "۲", "۳", "۴", "۵", "۶", "۷", "۸", "۹" },
        ["ur-pk"] = new[] { "۰", "۱", "۲", "۳", "۴", "۵", "۶", "۷", "۸", "۹" },
        ["hi-in"] = new[] { "०", "१", "२", "३", "४", "५", "६", "७", "८", "९" },
        ["bn-bd"] = new[] { "০", "১", "২", "৩", "৪", "৫", "৬", "৭", "৮", "৯" },
        ["ta-in"] = new[] { "௦", "௧", "௨", "௩", "௪", "௫", "௬", "௭", "௮", "௯" },
        ["ml-in"] = new[] { "൦", "൧", "൨", "൩", "൪", "൫", "൬", "൭", "൮", "൯" },
        ["te-in"] = new[] { "౦", "౧", "౨", "౩", "౪", "౫", "౬", "౭", "౮", "౯" },
        ["kn-in"] = new[] { "೦", "೧", "೨", "೩", "೪", "೫", "೬", "೭", "೮", "೯" },
        ["gu-in"] = new[] { "૦", "૧", "૨", "૩", "૪", "૫", "૬", "૭", "૮", "૯" },
        ["pa-in"] = new[] { "੦", "੧", "੨", "੩", "੪", "੫", "੬", "੭", "੮", "੯" },
        ["ne-np"] = new[] { "०", "१", "२", "३", "४", "५", "६", "७", "८", "९" },
        ["as-in"] = new[] { "০", "১", "২", "৩", "৪", "৫", "৬", "৭", "৮", "৯" },
        ["or-in"] = new[] { "୦", "୧", "୨", "୩", "୪", "୫", "୬", "୭", "୮", "୯" },
        ["lo-la"] = new[] { "໐", "໑", "໒", "໓", "໔", "໕", "໖", "໗", "໘", "໙" },
        ["my-mm"] = new[] { "၀", "၁", "၂", "၃", "၄", "၅", "၆", "၇", "၈", "၉" },
        ["th-th"] = new[] { "๐", "๑", "๒", "๓", "๔", "๕", "๖", "๗", "๘", "๙" },
        ["bo-cn"] = new[] { "༠", "༡", "༢", "༣", "༤", "༥", "༦", "༧", "༨", "༩" }
    };
#endif

	static string[] GetNativeDigits(CultureInfo culture)
	{
#if IOS || MACCATALYST
        if (culture.Name.StartsWith("ar", StringComparison.OrdinalIgnoreCase))
            return NativeDigitsMap["ar"];
			
        if (NativeDigitsMap.TryGetValue(culture.Name, out var digits))
            return digits;
#endif
		return culture.NumberFormat.NativeDigits;
	}
}
