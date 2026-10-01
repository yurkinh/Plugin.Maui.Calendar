using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Maui.Controls.Shapes;

namespace Plugin.Maui.Calendar.Converters;

public sealed class StrokeShapeConverter : IValueConverter
{

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is float floatValue)
		{
			return new RoundRectangle() { CornerRadius = new(floatValue) };
		}
		return new RoundRectangle();
	}

	[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Binding.DoNothing is a static marker object; reading it involves no reflection.")]
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return Binding.DoNothing;
	}
}

