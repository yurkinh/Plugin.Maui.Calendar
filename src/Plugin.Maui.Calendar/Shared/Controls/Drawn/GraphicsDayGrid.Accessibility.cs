using Plugin.Maui.Calendar.Models;
#if ANDROID
using Android.OS;
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using AndroidX.CustomView.Widget;
#elif IOS || MACCATALYST
using CoreGraphics;
using UIKit;
#endif

namespace Plugin.Maui.Calendar.Controls.Drawn;

/// <summary>
/// What a screen reader finds in the grid: one element per title and per shown day, where they are drawn.
/// A day is a button named with its full date in the calendar's culture, selected while the day is
/// selected and unavailable while it is disabled; the platform speaks the role and the states in its own
/// language. A title is text. Hidden days are left out. (A native cell has no description of its own: a
/// screen reader reads its day number.)
/// </summary>
/// <param name="Id">Which element: the index of a day (from 0), or <see cref="GraphicsDayGrid.TitleId"/> plus the column of a title.</param>
/// <param name="Bounds">Where it is drawn, in points from the top left corner of the grid.</param>
readonly record struct GraphicsGridElement(int Id, RectF Bounds, string Label, bool IsDay, bool IsSelected, bool IsEnabled, int Row, int Column);

partial class GraphicsDayGrid
{
	/// <summary>The first <see cref="GraphicsGridElement.Id"/> of the titles, after every possible day.</summary>
	internal const int TitleId = 1000;

	/// <summary>
	/// The elements a screen reader finds in the grid, in reading order: the titles, then the days of each
	/// week from the first day of the week.
	/// </summary>
	internal IReadOnlyList<GraphicsGridElement> AccessibleElements()
	{
		var width = (float)Width;
		var elements = new List<GraphicsGridElement>();
		if (width <= 0)
		{
			return elements;
		}

		var titles = drawable.ShownTitles.ToList();
		for (var column = 0; column < titles.Count; column++)
		{
			elements.Add(new(TitleId + column, drawable.TitleBounds(column, width), titles[column], false, false, true, 0, column));
		}

		var days = drawable.Days;
		for (var index = 0; index < days.Count; index++)
		{
			var day = days[index];
			if (day.IsControlVisible && day.IsVisible && !string.IsNullOrEmpty(day.Day))
			{
				elements.Add(new(
					index,
					drawable.CellBounds(index, width),
					day.Date.ToString("D", Culture),
					true,
					day.IsSelected,
					!day.IsDisabled,
					1 + (index / 7),
					index % 7));
			}
		}

		return elements;
	}

	/// <summary>Handles a screen reader's activation of the element <paramref name="id"/>, as a tap does.</summary>
	internal bool Activate(int id)
	{
		if (id is < 0 or >= TitleId || id >= drawable.Days.Count)
		{
			return false;
		}

		DayTapped?.Invoke(drawable.Days[id]);
		return true;
	}

	DayModel DayOf(int id) => id >= 0 && id < drawable.Days.Count ? drawable.Days[id] : null;

	protected override void OnHandlerChanged()
	{
		base.OnHandlerChanged();
		AttachAccessibility(Handler?.PlatformView);
	}

	partial void AttachAccessibility(object platformView);

	partial void UpdateAccessibility();

#if ANDROID
	// The elements are virtual views of the GraphicsView, which TalkBack (and UI Automator) find through
	// the helper, and the helper is given the hover events TalkBack's explore by touch sends.
	ElementHelper accessibilityHelper;

	partial void AttachAccessibility(object platformView)
	{
		if (platformView is not Android.Views.View view)
		{
			accessibilityHelper = null;
			return;
		}

		accessibilityHelper = new ElementHelper(this, view);
		ViewCompat.SetAccessibilityDelegate(view, accessibilityHelper);
		view.ImportantForAccessibility = Android.Views.ImportantForAccessibility.Yes;
		view.Hover += (_, e) => e.Handled = accessibilityHelper?.DispatchHoverEvent(e.Event) == true;
	}

	partial void UpdateAccessibility() => accessibilityHelper?.InvalidateRoot();

	sealed class ElementHelper(GraphicsDayGrid grid, Android.Views.View view) : ExploreByTouchHelper(view)
	{
		static float Density => Android.App.Application.Context.Resources.DisplayMetrics.Density;

		GraphicsGridElement? Find(int id)
		{
			foreach (var element in grid.AccessibleElements())
			{
				if (element.Id == id)
				{
					return element;
				}
			}

			return null;
		}

		protected override int GetVirtualViewAt(float x, float y)
		{
			foreach (var element in grid.AccessibleElements())
			{
				if (element.Bounds.Contains(x / Density, y / Density))
				{
					return element.Id;
				}
			}

			return InvalidId;
		}

		protected override void GetVisibleVirtualViews(IList<Java.Lang.Integer> virtualViewIds)
		{
			foreach (var element in grid.AccessibleElements())
			{
				virtualViewIds.Add(Java.Lang.Integer.ValueOf(element.Id));
			}
		}

		// The grid is a table: a screen reader says where a day is in it.
		protected override void OnPopulateNodeForHost(AccessibilityNodeInfoCompat node)
		{
			base.OnPopulateNodeForHost(node);
			node.SetCollectionInfo(AccessibilityNodeInfoCompat.CollectionInfoCompat.Obtain(1 + grid.drawable.Weeks, 7, false));
		}

		protected override void OnPopulateNodeForVirtualView(int virtualViewId, AccessibilityNodeInfoCompat node)
		{
			// A view that no longer exists (the layout changed): still given bounds, which the helper requires.
			if (Find(virtualViewId) is not { } element)
			{
				node.ContentDescription = string.Empty;
#pragma warning disable CA1422 // The helper reads the bounds in the parent
				node.SetBoundsInParent(new Android.Graphics.Rect());
#pragma warning restore CA1422
				return;
			}

			var bounds = element.Bounds;
#pragma warning disable CA1422
			node.SetBoundsInParent(new Android.Graphics.Rect(
				(int)Math.Round(bounds.Left * Density),
				(int)Math.Round(bounds.Top * Density),
				(int)Math.Round(bounds.Right * Density),
				(int)Math.Round(bounds.Bottom * Density)));
#pragma warning restore CA1422

			node.SetCollectionItemInfo(AccessibilityNodeInfoCompat.CollectionItemInfoCompat.Obtain(
				element.Row, 1, element.Column, 1, !element.IsDay, element.IsSelected));

			if (!element.IsDay)
			{
				node.ClassName = "android.widget.TextView";
				node.Text = element.Label;
				return;
			}

			node.ClassName = "android.widget.Button";
			node.ContentDescription = element.Label;
			node.Selected = element.IsSelected;
			node.Enabled = element.IsEnabled;
			node.Clickable = element.IsEnabled;
			if (element.IsEnabled)
			{
				node.AddAction(AccessibilityNodeInfoCompat.AccessibilityActionCompat.ActionClick);
			}
		}

		protected override bool OnPerformActionForVirtualView(int virtualViewId, int action, Bundle arguments)
		{
			if (action != AccessibilityNodeInfoCompat.ActionClick || grid.DayOf(virtualViewId) is not { IsDisabled: false })
			{
				return false;
			}

			grid.Activate(virtualViewId);
			SendEventForVirtualView(virtualViewId, (int)Android.Views.Accessibility.EventTypes.ViewClicked);
			return true;
		}
	}
#elif IOS || MACCATALYST
	// The elements are accessibility elements of the GraphicsView, which VoiceOver (and XCUITest) find in it.
	// They are kept from one update to the next, so VoiceOver keeps its place when the days change.
	WeakReference<UIView> accessibilityView;
	readonly List<GridElement> platformElements = [];

	partial void AttachAccessibility(object platformView)
	{
		accessibilityView = platformView is UIView view ? new WeakReference<UIView>(view) : null;
		platformElements.Clear();
		UpdateAccessibility();
	}

	partial void UpdateAccessibility()
	{
		if (accessibilityView?.TryGetTarget(out var view) != true)
		{
			return;
		}

		var elements = AccessibleElements();
		while (platformElements.Count < elements.Count)
		{
			platformElements.Add(new GridElement(view, this));
		}

		if (platformElements.Count > elements.Count)
		{
			platformElements.RemoveRange(elements.Count, platformElements.Count - elements.Count);
		}

		for (var i = 0; i < elements.Count; i++)
		{
			var element = elements[i];
			var platformElement = platformElements[i];
			var traits = element.IsDay ? UIAccessibilityTrait.Button : UIAccessibilityTrait.StaticText;
			if (element.IsSelected)
			{
				traits |= UIAccessibilityTrait.Selected;
			}

			if (!element.IsEnabled)
			{
				traits |= UIAccessibilityTrait.NotEnabled;
			}

			platformElement.Id = element.Id;
			platformElement.AccessibilityLabel = element.Label;
			platformElement.AccessibilityTraits = (ulong)traits;
			platformElement.AccessibilityFrameInContainerSpace = new CGRect(element.Bounds.X, element.Bounds.Y, element.Bounds.Width, element.Bounds.Height);
		}

		view.IsAccessibilityElement = false;
		// UIView's accessibilityElements (of the UIAccessibilityContainer protocol, which the binding gives
		// views no member for).
		view.SetValueForKey(Foundation.NSArray.FromNSObjects([.. platformElements]), new Foundation.NSString("accessibilityElements"));
	}

	sealed class GridElement(UIView container, GraphicsDayGrid grid) : UIAccessibilityElement(container)
	{
		readonly WeakReference<GraphicsDayGrid> owner = new(grid);

		internal int Id { get; set; }

		// VoiceOver's double tap (UIAccessibilityAction's accessibilityActivate, an informal protocol).
		[Foundation.Export("accessibilityActivate")]
		public bool Activate() =>
			owner.TryGetTarget(out var target) && target.DayOf(Id) is { IsDisabled: false } && target.Activate(Id);
	}
#endif
}
