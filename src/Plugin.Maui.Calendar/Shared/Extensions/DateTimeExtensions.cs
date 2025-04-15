using System.Globalization;
using System.Text;

namespace Plugin.Maui.Calendar.Shared.Extensions;
public static class DateTimeExtensions
{
	public static string ToLocalizedString(this DateTime date, string format, CultureInfo culture)
	{
		var formatted = date.ToString(format, culture);
		var nativeDigits = culture.NumberFormat.NativeDigits;

		if (nativeDigits[0] == "0")
		{
			return formatted;
		}
		var result = new StringBuilder();
		foreach (var c in formatted)
		{
			if (char.IsDigit(c))
			{
				int digit = c - '0';
				result.Append(nativeDigits[digit]);
			}
			else
			{
				result.Append(c);
			}
		}

		return result.ToString();
	}
	public static string ToLocalizedString(this int number, CultureInfo culture)
	{
		var nativeDigits = culture.NumberFormat.NativeDigits;

		if (nativeDigits[0] == "0")
		{
			return number.ToString();
		}
		var result = new StringBuilder();
		foreach (char c in number.ToString())
		{
			if (char.IsDigit(c))
			{
				int digit = c - '0';
				result.Append(nativeDigits[digit]);
			}
			else
			{
				result.Append(c);
			}
		}
		return result.ToString();
	}
}
