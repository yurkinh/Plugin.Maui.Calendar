namespace Plugin.Maui.Calendar.Controls;

public partial class Calendar : ContentView, IDisposable
{
	/// <summary>
	/// Calendar plugin for .NET MAUI
	/// </summary>
	public Calendar()
	{
		// Item 16: suppress all intermediate renders triggered by bindable-property
		// callbacks that fire during construction.  One consolidated render executes
		// after the control is fully configured.
		isInitializing = true;

		PrevLayoutUnitCommand = new Command(PrevUnit);
		NextLayoutUnitCommand = new Command(NextUnit);
		PrevYearCommand = new Command(PrevYear, CanExecutePrevYear);
		NextYearCommand = new Command(NextYear, CanExecuteNextYear);
		ShowHideCalendarCommand = new Command(ToggleCalendarSectionVisibility);

		InitializeComponent();

		InitializeViewLayoutEngine();
		InitializeSelectionType();

		isInitializing = false;

		// Single consolidated render at end of construction.
		UpdateSelectedDateLabel();
		UpdateLayoutUnitLabel();
		UpdateEvents();
		RenderLayout();

		calendarSectionAnimateHide = new Lazy<Animation>(() => new Animation(AnimateMonths, 1, 0));
		calendarSectionAnimateShow = new Lazy<Animation>(() => new Animation(AnimateMonths, 0, 1));
	}
}