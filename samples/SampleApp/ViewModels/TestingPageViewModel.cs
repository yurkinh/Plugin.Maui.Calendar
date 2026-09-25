using CommunityToolkit.Mvvm.ComponentModel;
using Plugin.Maui.Calendar.Enums;
using System.Collections.ObjectModel;
using System.Globalization;

namespace SampleApp.ViewModels;

public partial class TestingPageViewModel : BasePageViewModel
{
	[ObservableProperty]
	public partial CultureInfo Culture { get; set; } = CultureInfo.CreateSpecificCulture("en-US");

	[ObservableProperty]
	public partial DaysTitleMaxLength DaysTitleMaxLength { get; set; } = DaysTitleMaxLength.None;

	[ObservableProperty]
	public partial bool UseAbbreviatedDayNames { get; set; } = true;
	public ObservableCollection<CultureInfo> AvailableCultures { get; } =
	[
		new CultureInfo("en-US"), // English (US)
        new CultureInfo("fr-FR"), // French
        new CultureInfo("de-DE"), // German
        new CultureInfo("es-ES"), // Spanish
        new CultureInfo("it-IT"), // Italian
        new CultureInfo("uk-UA"), // Ukrainian
        new CultureInfo("pl-PL"), // Polish
        new CultureInfo("ar-JO"), // Arabic (Jordan)
        new CultureInfo("ja-JP"), // Japanese
        new CultureInfo("zh-CN"), // Chinese (Simplified)
    ];

	public Array DaysTitleMaxLengthOptions => Enum.GetValues<DaysTitleMaxLength>();
}
