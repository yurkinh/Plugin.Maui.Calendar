namespace SampleApp.Views;

public partial class DayViewTemplatePage : ContentPage
{
	public DayViewTemplatePage()
	{
		InitializeComponent();
		ApplyTemplate((DataTemplate)Resources["PhotoDayTemplate"], photoSegment, photoLegend);
	}

	void OnPhotoTemplateClicked(object sender, EventArgs e) =>
		ApplyTemplate((DataTemplate)Resources["PhotoDayTemplate"], photoSegment, photoLegend);

	void OnTileTemplateClicked(object sender, EventArgs e) =>
		ApplyTemplate((DataTemplate)Resources["TileDayTemplate"], tileSegment, tileLegend);

	void OnAgendaTemplateClicked(object sender, EventArgs e) =>
		ApplyTemplate((DataTemplate)Resources["AgendaDayTemplate"], agendaSegment, agendaLegend);

	// null restores the built-in day cell, styled by the Calendar's color properties again.
	void OnDefaultTemplateClicked(object sender, EventArgs e) =>
		ApplyTemplate(null, defaultSegment, builtInLegend);

	void ApplyTemplate(DataTemplate template, Button activeSegment, View activeLegend)
	{
		// DayViewTemplate can be swapped at any time: only the content of each day cell is replaced.
		calendar.DayViewTemplate = template;

		foreach (var segment in new[] { photoSegment, tileSegment, agendaSegment, defaultSegment })
		{
			var isActive = segment == activeSegment;
			segment.BackgroundColor = isActive ? Color.FromArgb("#16A34A") : Colors.Transparent;
			segment.SetAppThemeColor(
				Button.TextColorProperty,
				isActive ? Colors.White : Color.FromArgb("#6B8577"),
				isActive ? Colors.White : Color.FromArgb("#86A393"));
		}

		foreach (var legend in new View[] { photoLegend, tileLegend, agendaLegend, builtInLegend })
		{
			legend.IsVisible = legend == activeLegend;
		}
	}
}
