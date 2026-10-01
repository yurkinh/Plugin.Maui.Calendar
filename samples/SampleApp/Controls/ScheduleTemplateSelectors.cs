namespace SampleApp.Controls;

/// <summary>
/// Picks the template of a row of the Google Calendar schedule list.
/// </summary>
public class ScheduleRowTemplateSelector : DataTemplateSelector
{
	public DataTemplate MonthTemplate { get; set; }

	public DataTemplate WeekTemplate { get; set; }

	public DataTemplate DayTemplate { get; set; }

	protected override DataTemplate OnSelectTemplate(object item, BindableObject container) => item switch
	{
		ScheduleMonthRow => MonthTemplate,
		ScheduleWeekRow => WeekTemplate,
		_ => DayTemplate,
	};
}

/// <summary>
/// Picks the template of an item of a schedule day: an event chip or the current time line.
/// </summary>
public class ScheduleItemTemplateSelector : DataTemplateSelector
{
	public DataTemplate EventTemplate { get; set; }

	public DataTemplate NowLineTemplate { get; set; }

	protected override DataTemplate OnSelectTemplate(object item, BindableObject container) =>
		item is ScheduleNowLine ? NowLineTemplate : EventTemplate;
}