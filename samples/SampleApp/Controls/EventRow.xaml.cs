using System.Windows.Input;

namespace SampleApp.Controls;

/// <summary>
/// An <see cref="EventModel"/> in the event list under a sample calendar.
/// </summary>
public partial class EventRow : ContentView
{
    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(EventRow), null);

    public EventRow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Executed with the <see cref="EventModel"/> when the row is tapped.
    /// </summary>
    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    void OnTapped(object sender, EventArgs e)
    {
        if (BindingContext is EventModel eventModel)
        {
            Command?.Execute(eventModel);
        }
    }
}
