using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using DrawnUi.Views;

namespace Plugin.Maui.Calendar.Controls.Drawn;

/// <summary>
/// Canvas hosting the drawn day grid.
/// <para>
/// Works around a DrawnUI (1.10.7.2) iOS / Mac Catalyst crash: when the handler disconnects (page
/// popped) <c>DrawnView.ReleaseAccessibility</c> calls <c>SetValueForKey(null, "accessibilityElements")</c>,
/// which current .NET for iOS rejects with <see cref="ArgumentNullException"/>. Clearing the private
/// accessibility view reference first makes DrawnUI skip that call while the rest of its cleanup
/// (event unsubscription, timers, element release) still runs; the native view is being torn down anyway.
/// Remove once DrawnUI guards the call.
/// </para>
/// </summary>
sealed class CalendarCanvas : Canvas
{
#if IOS || MACCATALYST
	static readonly FieldInfo a11yViewField =
		typeof(DrawnView).GetField("_a11yView", BindingFlags.Instance | BindingFlags.NonPublic);

	[DynamicDependency("_a11yView", typeof(DrawnView))]
	protected override void OnHandlerChanging(HandlerChangingEventArgs args)
	{
		if (args.NewHandler is null || args.OldHandler is not null)
		{
			a11yViewField?.SetValue(this, null);
		}

		base.OnHandlerChanging(args);
	}
#endif
}
