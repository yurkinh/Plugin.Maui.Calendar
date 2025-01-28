using System.Runtime.CompilerServices;
using Plugin.Maui.Calendar.Controls.SelectionEngines;

namespace Plugin.Maui.Calendar.Controls;


public class RangeSelectionCalendar : Calendar
{
    /// <summary> Bindable property for StartDate </summary>
    public static readonly BindableProperty SelectedStartDateProperty = BindableProperty.Create(
        nameof(SelectedStartDate),
        typeof(DateTime?),
        typeof(RangeSelectionCalendar),
        null,
        BindingMode.TwoWay,
        propertyChanged: OnSelectedStartDateChanged
    );

    /// <summary>
    /// Beggining of selected interval
    /// </summary>
    public DateTime? SelectedStartDate
    {
        get => (DateTime?)GetValue(SelectedStartDateProperty);
        set => SetValue(SelectedStartDateProperty, value);
    }

    /// <summary> Bindable property for EndDate </summary>
    public static readonly BindableProperty SelectedEndDateProperty = BindableProperty.Create(
        nameof(SelectedEndDate),
        typeof(DateTime?),
        typeof(RangeSelectionCalendar),
        null,
        BindingMode.TwoWay,
        propertyChanged: OnSelectedEndDateChanged
    );

    /// <summary> End of selected interval </summary>
    public DateTime? SelectedEndDate
    {
        get => (DateTime?)GetValue(SelectedEndDateProperty);
        set => SetValue(SelectedEndDateProperty, value);
    }

    private bool _isSelectionDatesChanging = false;
    private readonly RangedSelectionEngine _selectionEngine;

    /// <summary>
    /// Constructor
    /// </summary>
    public RangeSelectionCalendar()
        : base()
    {
        CurrentSelectionEngine = new RangedSelectionEngine();
        _selectionEngine = CurrentSelectionEngine as RangedSelectionEngine;
    }

    /// <summary>
    /// Method that is called when a bound property is changed.
    /// </summary>
    /// <param name="propertyName">The name of the bound property that changed.</param>
    protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName is nameof(SelectedDates) && !_isSelectionDatesChanging)
        {
            var first = _selectionEngine.GetDateRange(DisabledDates);

            if (first.Count > 0)
            {
                _isSelectionDatesChanging = true;
                SetValue(SelectedStartDateProperty, first.First());
                SetValue(SelectedEndDateProperty, first.Last());
            }
        }
    }

    private static void OnSelectedStartDateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var rangeSelectionCalendar = (RangeSelectionCalendar)bindable;
        if (!rangeSelectionCalendar._isSelectionDatesChanging)
        {
            rangeSelectionCalendar._isSelectionDatesChanging = true;
            rangeSelectionCalendar._selectionEngine.SelectDateRange(
                (DateTime?)newValue,
                rangeSelectionCalendar.DisabledDates
            );
            rangeSelectionCalendar.SelectedDates =
                rangeSelectionCalendar._selectionEngine.GetDateRange(
                    rangeSelectionCalendar.DisabledDates
                );
            rangeSelectionCalendar._isSelectionDatesChanging = false;
        }
    }

    private static void OnSelectedEndDateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var rangeSelectionCalendar = (RangeSelectionCalendar)bindable;
        if (!rangeSelectionCalendar._isSelectionDatesChanging)
        {
            rangeSelectionCalendar._isSelectionDatesChanging = true;
            rangeSelectionCalendar._selectionEngine.SelectDateRange(
                (DateTime?)newValue,
                rangeSelectionCalendar.DisabledDates
            );
            rangeSelectionCalendar.SelectedDates =
                rangeSelectionCalendar._selectionEngine.GetDateRange();
        }
        rangeSelectionCalendar._isSelectionDatesChanging = false;
    }
}
