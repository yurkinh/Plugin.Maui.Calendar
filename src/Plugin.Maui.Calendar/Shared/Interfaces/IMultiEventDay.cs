using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Interfaces;

public interface IMultiEventDay
{
    IReadOnlyList<EventIndicatorModel> EventIndicators { get; }
}