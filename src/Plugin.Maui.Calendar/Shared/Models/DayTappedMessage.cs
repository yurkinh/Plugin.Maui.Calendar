using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Plugin.Maui.Calendar.Models;

public sealed class DayTappedMessage(DateTime Value) : ValueChangedMessage<DateTime>(Value)
{
	// The day cell that was tapped, so that only the calendar owning it handles the tap. Null for a
	// message sent by other code, which every calendar handles, as before.
	internal object Source { get; init; }
}
