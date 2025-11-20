using Plugin.Maui.Calendar.Controls.SelectionEngines;

namespace Plugin.Maui.Calendar.Controls;

public partial class Calendar : ContentView, IDisposable
{
	/// <summary>
	/// Calendar plugin for .NET MAUI
	/// </summary>
	public Calendar()
	{
		PrevLayoutUnitCommand = new Command(PrevUnit);
		NextLayoutUnitCommand = new Command(NextUnit);
		PrevYearCommand = new Command(PrevYear, CanExecutePrevYear);
		NextYearCommand = new Command(NextYear, CanExecuteNextYear);
		ShowHideCalendarCommand = new Command(ToggleCalendarSectionVisibility);

		InitializeComponent();

		InitializeViewLayoutEngine();
		InitializeSelectionType();
		UpdateSelectedDateLabel();
		UpdateLayoutUnitLabel();
		UpdateEvents();
		RenderLayout();


		calendarSectionAnimateHide = new Lazy<Animation>(() => new Animation(AnimateMonths, 1, 0));
		calendarSectionAnimateShow = new Lazy<Animation>(() => new Animation(AnimateMonths, 0, 1));
	}
}