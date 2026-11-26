using Plugin.Maui.Calendar.Converters;

namespace Plugin.Maui.Calendar.Controls;

/// <summary>
/// The templates of the built-in day cell, shared by every <see cref="DayView"/>: a calendar has up to 42
/// cells, and each used to create its own dictionary, two templates and a converter.
/// </summary>
internal sealed partial class DefaultDayTemplates : ResourceDictionary
{
	const string backgroundTemplateKey = "DefaultDayBackgroundTemplate";
	const string contentTemplateKey = "DefaultDayContentTemplate";

	DefaultDayTemplates()
	{
		InitializeComponent();
		BackgroundTemplate = (DataTemplate)this[backgroundTemplateKey];
		ContentTemplate = (DataTemplate)this[contentTemplateKey];
	}

	/// <summary>Turns <c>DayViewCornerRadius</c> into the shape of the background (stateless, so one is enough).</summary>
	internal static readonly StrokeShapeConverter StrokeShapeConverter = new();

	static readonly Lazy<DefaultDayTemplates> shared = new(() => new DefaultDayTemplates());

	/// <summary>The one instance all cells use, created when the first built-in cell is.</summary>
	internal static DefaultDayTemplates Shared => shared.Value;

	/// <summary>The rounded background (selection, today outline, event background) of a day.</summary>
	internal DataTemplate BackgroundTemplate { get; }

	/// <summary>The day number with its event dots.</summary>
	internal DataTemplate ContentTemplate { get; }
}
