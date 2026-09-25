using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Plugin.Maui.Calendar.Models;

public sealed class DayTappedMessage(DateTime Value) : ValueChangedMessage<DateTime>(Value)
{
	// The calendar that owns the tapped cell, so that only that calendar handles the tap. Null for a
	// message sent by other code, which every calendar handles, as before.
	internal object Source { get; init; }
}
