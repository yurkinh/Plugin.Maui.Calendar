using Plugin.Maui.Calendar.Controls.SelectionEngines;
using System.Collections.Specialized;
using System.Collections.ObjectModel;


namespace Plugin.Maui.Calendar.Controls;

public partial class Calendar : ContentView, IDisposable
{
	/// <summary>
	/// Bindable property for SelectedDate
	/// </summary>
	public static readonly BindableProperty SelectedDateProperty = BindableProperty.Create(
		nameof(SelectedDate),
		typeof(DateTime?),
		typeof(Calendar),
		null,
		BindingMode.TwoWay,
		propertyChanged: OnSelectedDateChanged
	);
	/// <summary>
	/// Selected date in single date selection mode
	/// </summary>
	public DateTime? SelectedDate
	{
		get => (DateTime?)GetValue(SelectedDateProperty);
		set
		{
			SetValue(
				SelectedDatesProperty,
				value.HasValue ? new List<DateTime> { value.Value } : null
			);
			SetValue(SelectedDateProperty, value);
		}
	}
	static void OnSelectedDateChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var control = (Calendar)bindable;
		var dateToSet = (DateTime?)newValue;

		control.SetValue(SelectedDateProperty, dateToSet);
		if (!control.isSelectingDates || control.CurrentSelectionEngine is SingleSelectionEngine)
		{
			if (dateToSet.HasValue)
			{
				control.SelectedDates = [dateToSet.Value];
			}
			else
			{
				control.SetValue(SelectedDatesProperty, new List<DateTime>());
			}
		}
		else
		{
			control.isSelectingDates = false;
		}
		control.UpdateDays(true);

	}

	bool isSelectingDates = false;

	/// <summary>
	/// Bindable property for SelectedDates
	/// </summary>
	public static readonly BindableProperty SelectedDatesProperty = BindableProperty.Create(
			 nameof(SelectedDates),
			 typeof(ObservableCollection<DateTime>),
			 typeof(Calendar),
			defaultValue: null,
			 BindingMode.TwoWay,
			propertyChanged: SelectedDatesChanged,
			defaultValueCreator: (bindable) => new ObservableCollection<DateTime>());


	public ObservableCollection<DateTime> SelectedDates
	{
		get => (ObservableCollection<DateTime>)GetValue(SelectedDatesProperty);
		set
		{
			var oldCollection = SelectedDates;
			if (oldCollection != null)
			{
				oldCollection.CollectionChanged -= OnSelectedDatesCollectionChanged;
			}

			SetValue(SelectedDatesProperty, value);
			if (value != null)
			{
				value.CollectionChanged += OnSelectedDatesCollectionChanged;
			}

			isSelectingDates = true;
			SelectedDate = value?.Count > 0 ? value.First() : null;
		}
	}

	void OnSelectedDatesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => UpdateSelectedDatesCollection(SelectedDates?.ToList());


	static void SelectedDatesChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var calendar = (Calendar)bindable;
		if (oldValue is ObservableCollection<DateTime> oldCollection)
		{
			oldCollection.CollectionChanged -= calendar.OnSelectedDatesCollectionChanged;
		}

		if (newValue is ObservableCollection<DateTime> newCollection)
		{
			newCollection.CollectionChanged += calendar.OnSelectedDatesCollectionChanged;
		}
		calendar.UpdateSelectedDatesCollection(calendar.SelectedDates?.ToList());
	}

	void UpdateSelectedDatesCollection(List<DateTime> SelectedDates)
	{
		CurrentSelectionEngine.UpdateDateSelection(SelectedDates ?? []);
		UpdateDays(true);
		UpdateSelectedDateLabel();
		UpdateEvents();
		if (CurrentSelectionEngine is RangedSelectionEngine)
		{
			UpdateRangeSelection();
		}
	}

	protected virtual void UpdateRangeSelection() { }
}
