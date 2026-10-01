namespace Plugin.Maui.Calendar.Controls;

public partial class Calendar : ContentView, IDisposable
{
	/// <summary>
	/// Calendar plugin for .NET MAUI
	/// </summary>
	public Calendar()
	{
		// The arrows of the default header ask the navigation commands whether they can execute while
		// InitializeComponent creates the header, and the answer depends on the layout engine.
		InitializeViewLayoutEngine();

		PrevLayoutUnitCommand = new Command(PrevUnit, CanExecutePrevUnit);
		NextLayoutUnitCommand = new Command(NextUnit, CanExecuteNextUnit);
		PrevYearCommand = new Command(PrevYear, CanExecutePrevYear);
		NextYearCommand = new Command(NextYear, CanExecuteNextYear);
		ShowHideCalendarCommand = new Command(ToggleCalendarSectionVisibility);

		InitializeComponent();

		InitializeSelectionType();

		// OnEventsChanged only observes a collection that is assigned, not the default one.
		ObserveEvents();

		// The first render, once the control is fully configured: no property callback runs during
		// construction (XAML assigns the calendar's properties after the constructor).
		UpdateSelectedDateLabel();
		UpdateLayoutUnitLabel();
		UpdateEvents();
		RenderLayout();

		// Keeps IsToday current across midnight, but only while the calendar is on screen.
		Loaded += OnCalendarLoaded;
		Unloaded += OnCalendarUnloaded;
	}
}
