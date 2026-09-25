using CommunityToolkit.Mvvm.Messaging;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls;

public sealed partial class DayView : ContentView
{
	const string defaultDayBackgroundTemplateKey = "DefaultDayBackgroundTemplate";
	const string defaultDayContentTemplateKey = "DefaultDayContentTemplate";

	// The Calendar's DayViewTemplate, possibly a DataTemplateSelector; null for the built-in cell.
	DataTemplate dayViewTemplate;

	// The concrete template whose content is shown (a selector's choice for a selector); null
	// while the built-in cell is shown.
	DataTemplate shownTemplate;
	View templateContent;
	bool isContentCreated;

	internal DayView(DataTemplate dayViewTemplate = null)
	{
		this.dayViewTemplate = dayViewTemplate;
		InitializeComponent();
	}

	/// <summary>Whether the cell content (built-in or templated) has been created.</summary>
	internal bool IsContentCreated => isContentCreated;

	/// <summary>
	/// The view created from the <c>DayViewTemplate</c> (or from the template its selector chose),
	/// or <see langword="null"/> while the built-in cell is shown or no content was created yet.
	/// </summary>
	internal View TemplateContent => templateContent;

	/// <summary>
	/// Creates the cell content if it does not exist yet. This happens automatically right before
	/// the cell is first rendered (see <see cref="OnHandlerChanging"/>); without a handler, as in
	/// unit tests, it has to be called explicitly.
	/// </summary>
	internal void EnsureContent()
	{
		if (!isContentCreated)
		{
			UpdateContent();
		}
	}

	/// <summary>
	/// Switches the cell to <paramref name="template"/> (<see langword="null"/> for the built-in
	/// cell). Only the content inside the cell is replaced; the cell itself, its day model and its
	/// size, visibility and tap handling are kept. Content that was not created yet is created
	/// later, directly from the new template.
	/// </summary>
	internal void SetDayViewTemplate(DataTemplate template)
	{
		if (ReferenceEquals(dayViewTemplate, template))
		{
			return;
		}

		dayViewTemplate = template;

		if (isContentCreated)
		{
			UpdateContent();
		}
	}

	// The content is created on first render instead of in the constructor: the Calendar builds its
	// cells before XAML assigns DayViewTemplate, so eager creation would build, and then throw
	// away, a built-in cell for every day whenever the template is set in XAML. OnHandlerChanging
	// runs before the handler maps Content, so the content is in place for the first render.
	protected override void OnHandlerChanging(HandlerChangingEventArgs args)
	{
		base.OnHandlerChanging(args);

		if (args.NewHandler is not null)
		{
			EnsureContent();
		}
	}

	protected override void OnBindingContextChanged()
	{
		base.OnBindingContextChanged();

		// A selector's choice depends on the day, so a new day model needs a new choice.
		RefreshTemplateSelection();
	}

	/// <summary>
	/// Asks a <see cref="DataTemplateSelector"/> <c>DayViewTemplate</c> again for the current state of
	/// the cell's day and swaps the content when it picks another template. The calendar calls this
	/// after every pass that updates the days (navigation, selection, events, disabled dates, ...),
	/// once all the day's state is assigned, so the selector never sees a half-updated day (cells are
	/// reused, and the date is assigned before the rest of a day's state). Does nothing for a plain
	/// template, for content that was not created yet, or while the binding context is not a day
	/// (for example while a page tear-down clears binding contexts).
	/// </summary>
	internal void RefreshTemplateSelection()
	{
		if (isContentCreated && dayViewTemplate is DataTemplateSelector && BindingContext is DayModel)
		{
			UpdateContent();
		}
	}

	/// <summary>
	/// Resolves the template for the current day and replaces the content inside the container when
	/// it differs from the one shown. The container keeps the size, visibility and tap handling, so
	/// any content stays sized, hidden and tappable exactly like the built-in cell.
	/// </summary>
	void UpdateContent()
	{
		// A selector is only given a day: without one (never the case for a cell of the calendar)
		// the built-in cell is shown. A selector returning null also falls back to the built-in cell.
		var template = dayViewTemplate is DataTemplateSelector && BindingContext is not DayModel
			? null
			: dayViewTemplate?.SelectDataTemplate(BindingContext, this);

		if (isContentCreated && ReferenceEquals(template, shownTemplate))
		{
			return;
		}

		// Created before anything is replaced, so an invalid template leaves the current cell intact.
		var view = template is null ? null : CreateTemplateView(template);
		var wasBuiltIn = isContentCreated && shownTemplate is null;

		container.Clear();

		if (view is null)
		{
			AddBuiltInContent();

			// EventIndicatorType.BackgroundFull paints the whole grid cell (this view, not only the
			// DayViewSize square), and only for the built-in cell: a template draws its own background.
			this.SetBinding(BackgroundColorProperty, static (DayModel day) => day.BackgroundFullEventColor, BindingMode.OneWay);
		}
		else
		{
			// The created view inherits the day model as its binding context from the container.
			container.Add(view);

			if (wasBuiltIn)
			{
				RemoveBinding(BackgroundColorProperty);

				// RemoveBinding keeps the last value the binding produced, so it is replaced explicitly.
				BackgroundColor = null;
			}
		}

		shownTemplate = template;
		templateContent = view;
		isContentCreated = true;
	}

	void AddBuiltInContent()
	{
		var background = (View)((DataTemplate)Resources[defaultDayBackgroundTemplateKey]).CreateContent();
		var content = (Layout)((DataTemplate)Resources[defaultDayContentTemplateKey]).CreateContent();

		// Label.FontSize computes its default the first time it is read, from the nearest rendered
		// ancestor: 0 (the platform's own label size, 17pt on iOS and Mac Catalyst) while detached,
		// but the font manager's default (14) once attached. The day label used to always resolve it
		// while detached, so it is read here, before the content is attached, to keep day numbers the
		// same size when DaysLabelStyle does not set a FontSize.
		foreach (var label in content.OfType<Label>())
		{
			_ = label.FontSize;
		}

		container.Add(background);
		container.Add(content);
	}

	static View CreateTemplateView(DataTemplate template)
	{
		var content = template.CreateContent();

		return content as View
			?? throw new InvalidOperationException(
				$"{nameof(Calendar)}.{nameof(Calendar.DayViewTemplate)} must create a {nameof(View)} (for example a " +
				$"{nameof(Grid)}, {nameof(Border)} or {nameof(Label)}), but the template created " +
				$"{(content is null ? "null" : $"a {content.GetType().FullName}")}.");
	}

	void OnTapped(object sender, EventArgs e)
	{
		if (BindingContext is DayModel dayModel && !dayModel.IsDisabled && dayModel.IsVisible)
		{
			if (!dayModel.AllowDeselect && dayModel.IsSelected)
			{
				return;
			}

			// Resolved before DayTappedCommand runs: the command may rebuild the layout (e.g. switch
			// CalendarLayout), which takes this cell out of its calendar.
			var owner = FindOwningCalendar();

			dayModel.IsSelected = !dayModel.IsSelected;
			dayModel.DayTappedCommand?.Execute(dayModel.Date);
			// Source lets only the calendar that owns this cell handle the tap; the message is
			// broadcast to every calendar that is on screen.
			WeakReferenceMessenger.Default.Send(new DayTappedMessage(dayModel.Date) { Source = owner });
		}
	}

	Calendar FindOwningCalendar()
	{
		for (var element = Parent; element is not null; element = element.Parent)
		{
			if (element is Calendar calendar)
			{
				return calendar;
			}
		}

		return null;
	}
}
