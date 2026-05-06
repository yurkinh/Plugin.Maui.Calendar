using System.Globalization;

namespace Plugin.Maui.Calendar.Converters;

public sealed class ImageSourceToBoolConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		bool hasImage = false;

		if (value != null)
		{
			if (value is string str)
			{
				hasImage = !string.IsNullOrWhiteSpace(str);
			}
			else if (value is FileImageSource fis)
			{
				hasImage = !string.IsNullOrWhiteSpace(fis.File);
			}
			else
			{
				hasImage = true;
			}
		}

		if (parameter is string param && param.Equals("Inverted", StringComparison.OrdinalIgnoreCase))
		{
			return !hasImage;
		}

		return hasImage;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}