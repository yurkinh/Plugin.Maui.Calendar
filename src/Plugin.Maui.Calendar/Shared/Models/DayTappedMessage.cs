using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Plugin.Maui.Calendar.Models;

public sealed class DayTappedMessage(DateTime Value) : ValueChangedMessage<DateTime>(Value);

