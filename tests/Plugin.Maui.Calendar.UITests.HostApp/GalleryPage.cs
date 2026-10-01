namespace Plugin.Maui.Calendar.UITests.HostApp;

/// <summary>
/// The first page: the UI tests type a scenario name in <c>ScenarioEntry</c> and press <c>GoButton</c>.
/// The list below is for people browsing the scenarios.
/// </summary>
sealed class GalleryPage : ContentPage
{
	public GalleryPage()
	{
		var entry = new Entry
		{
			AutomationId = "ScenarioEntry",
			Placeholder = "Scenario name",
			Keyboard = Keyboard.Plain,
			IsSpellCheckEnabled = false,
			IsTextPredictionEnabled = false,
			ClearButtonVisibility = ClearButtonVisibility.Never,
		};
		var go = new Button { AutomationId = "GoButton", Text = "Go" };
		var error = new Label { AutomationId = "GalleryError", TextColor = Colors.Red };

		void Open()
		{
			var scenario = ScenarioCatalog.Find(entry.Text?.Trim() ?? string.Empty);
			if (scenario is null)
			{
				error.Text = $"No scenario named '{entry.Text}'";
				return;
			}

			App.Show(scenario.Create());
		}

		go.Clicked += (_, _) => Open();
		entry.Completed += (_, _) => Open();

		var list = new CollectionView
		{
			ItemsSource = ScenarioCatalog.All,
			SelectionMode = SelectionMode.Single,
			ItemTemplate = new DataTemplate(() =>
			{
				var name = new Label { FontSize = 16, Padding = new Thickness(16, 10) };
				name.SetBinding(Label.TextProperty, static (Scenario scenario) => scenario.Name);
				return name;
			}),
		};
		list.SelectionChanged += (_, e) =>
		{
			if (e.CurrentSelection.FirstOrDefault() is Scenario scenario)
			{
				App.Show(scenario.Create());
			}
		};

		Content = new Grid
		{
			RowDefinitions = [new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Star)],
			ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto)],
			Padding = new Thickness(12),
			ColumnSpacing = 8,
			Children =
			{
				entry,
				Cell(go, column: 1),
				Cell(error, row: 1, columnSpan: 2),
				Cell(list, row: 2, columnSpan: 2),
			},
		};
	}

	static View Cell(View view, int row = 0, int column = 0, int columnSpan = 1)
	{
		Grid.SetRow(view, row);
		Grid.SetColumn(view, column);
		Grid.SetColumnSpan(view, columnSpan);
		return view;
	}
}
