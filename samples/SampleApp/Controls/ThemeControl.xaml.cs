namespace SampleApp.Controls;

public partial class ThemeControl : ContentView
{
	public ThemeControl()
	{
		InitializeComponent();
	}
	
	public static readonly BindableProperty IsSystemThemeCheckedProperty = 
		BindableProperty.Create(nameof(IsSystemThemeChecked), typeof(bool), typeof(ThemeControl), true, BindingMode.TwoWay);

	public bool IsSystemThemeChecked
	{
		get => (bool)GetValue(IsSystemThemeCheckedProperty);
		set => SetValue(IsSystemThemeCheckedProperty, value);
	}


	public static readonly BindableProperty IsDarkThemeCheckedProperty = 
		BindableProperty.Create(nameof(IsDarkThemeChecked), typeof(bool), typeof(ThemeControl), false, BindingMode.TwoWay);

	public bool IsDarkThemeChecked
	{
		get =>(bool)GetValue(IsDarkThemeCheckedProperty);
		set =>SetValue(IsDarkThemeCheckedProperty, value);
	}
	
	public static readonly BindableProperty IsLightThemeCheckedProperty = 
		BindableProperty.Create(nameof(IsLightThemeChecked), typeof(bool), typeof(ThemeControl), false, BindingMode.TwoWay);

	public bool IsLightThemeChecked
	{
		get =>(bool)GetValue(IsLightThemeCheckedProperty);
		set =>SetValue(IsLightThemeCheckedProperty, value);
	}
}