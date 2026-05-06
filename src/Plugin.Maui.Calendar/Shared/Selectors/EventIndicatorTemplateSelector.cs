namespace Plugin.Maui.Calendar.Selectors;

public class EventIndicatorTemplateSelector : DataTemplateSelector
{
	public DataTemplate DotTemplate { get; set; }
	public DataTemplate TextTemplate { get; set; }
	public DataTemplate ImageTemplate { get; set; }

	protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
	{
		var indicator = (Models.EventIndicatorModel)item;

		if (indicator.ImageSource != null)
		{
			return ImageTemplate;
		}

		if (!string.IsNullOrEmpty(indicator.Text))
		{
			return TextTemplate;
		}

		return DotTemplate;
	}
}