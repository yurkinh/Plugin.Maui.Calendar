namespace SampleApp.Controls;

public partial class ThemeButton : ContentView
{
    public ThemeButton()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty IsCheckedProperty =
        BindableProperty.Create(nameof(IsChecked), typeof(bool), typeof(ThemeButton), false, BindingMode.TwoWay);
    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public static readonly BindableProperty GroupNameProperty =
        BindableProperty.Create(nameof(GroupName), typeof(string), typeof(ThemeButton), string.Empty);
    public string GroupName
    {
        get => (string)GetValue(GroupNameProperty);
        set => SetValue(GroupNameProperty, value);
    }

    public static readonly BindableProperty TitleProperty =
                BindableProperty.Create(nameof(Title), typeof(string), typeof(ThemeButton), string.Empty);
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty IconProperty =
                BindableProperty.Create(nameof(Icon), typeof(string), typeof(ThemeButton), string.Empty);
    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}