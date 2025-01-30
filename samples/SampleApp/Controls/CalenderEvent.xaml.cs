using System.Windows.Input;

namespace SampleApp.Controls;

public partial class CalenderEvent : ContentView
{
    public static BindableProperty CalenderEventCommandProperty =
        BindableProperty.Create(nameof(CalenderEventCommand), typeof(ICommand), typeof(CalenderEvent), null);

    public CalenderEvent()
    {
        InitializeComponent();
    }

    public ICommand CalenderEventCommand
    {
        get => (ICommand)GetValue(CalenderEventCommandProperty);
        set => SetValue(CalenderEventCommandProperty, value);
    }

    void TapGestureRecognizer_Tapped(object sender, System.EventArgs e)
    {
        if (BindingContext is AdvancedEventModel eventModel)
		{
			CalenderEventCommand?.Execute(eventModel);
		}
	}
}
