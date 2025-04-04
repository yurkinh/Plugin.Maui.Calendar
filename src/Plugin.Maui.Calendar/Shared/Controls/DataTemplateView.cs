namespace Plugin.Maui.Calendar.Controls;

public class DataTemplateView : ContentView
{
    public static readonly BindableProperty ItemTemplateProperty =
        BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(DataTemplateView), null, propertyChanged: OnItemTemplateChanged);

    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public DataTemplateView() { }

    static void OnItemTemplateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DataTemplateView view)
		{
			view.CreateContent();
		}
	}

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        CreateContent();
    }

    void CreateContent()
    {
        if (BindingContext == null)
		{
			return;
		}

		var itemContent = ItemTemplate?.CreateContent(BindingContext);

        Content = itemContent as View;
    }
}
