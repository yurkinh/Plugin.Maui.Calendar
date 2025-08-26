using CommunityToolkit.Mvvm.ComponentModel;
using Plugin.Maui.Calendar.Enums;
using System.Collections.ObjectModel;
using System.Globalization;

namespace SampleApp.ViewModels;

public partial class TestingPageViewModel : BasePageViewModel
{
	[ObservableProperty]
	CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");

	[ObservableProperty]
	DaysTitleMaxLength daysTitleMaxLength = DaysTitleMaxLength.None;

	[ObservableProperty]
	bool useAbbreviatedDayNames = true;

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

	public Array DaysTitleMaxLengthOptions => Enum.GetValues(typeof(DaysTitleMaxLength));
}
