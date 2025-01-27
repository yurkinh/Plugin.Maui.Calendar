namespace Plugin.Maui.Calendar.Controls;

public partial class DayCell : Border
{
    private readonly Color DayTextColor = (Color)Application.Current.Resources["TextGray"];
    private readonly Color DisabledDayTextColor = (Color)Application.Current.Resources["OutlineGray"];
    private readonly Color SelectedDayTextColor = (Color)Application.Current.Resources["White"];
    private readonly Color SelectedDayColor = (Color)Application.Current.Resources["LightBlue"];
    private CalendarNeo calendarView;
    public DayCell()
    {
        InitializeComponent();
    }

    private bool IsDisabled => calendarView?.DisabledDates.Contains(Date) ?? false;
    private bool IsSelected => calendarView?.SelectedDates.Contains(Date) ?? false;

    private DateTime date;
    public DateTime Date
    {
        get => date;
        set
        {
            date = value;
            DayLabel.Text = date.Day.ToString();
        }
    }

    public Color TextColor
    {
        get => DayLabel.TextColor;
        set => DayLabel.TextColor = value;
    }

    public Color DayCellBackgroundColor
    {
        get => DayBorder.BackgroundColor;
        set => DayBorder.BackgroundColor = value;
    }

    public void Handle_Tapped(object sender, EventArgs e)
    {
        if (calendarView.DisabledDates.Contains(Date) || Date.Month != calendarView.MonthIndex)
        {
            return;
        }

        if (!calendarView.SelectedDates.Remove(Date))
        {
            calendarView.SelectedDates.Add(Date);
        }

        UpdateBackgroundColor();
        UpdateTextColor();
        calendarView.DayTappedCommand.Execute(Date);
    }

    private void UpdateTextColor() =>
        TextColor = IsSelected ? SelectedDayTextColor
                  : IsDisabled ? DisabledDayTextColor
                  : DayTextColor;

    private void UpdateBackgroundColor() => DayCellBackgroundColor = (IsSelected && !IsDisabled) ? SelectedDayColor : Colors.Transparent;

    private void DayCell_Loaded(object sender, EventArgs e)
    {
        calendarView ??= (Parent?.Parent?.Parent as RaptorCalendarView)!;
    }
}