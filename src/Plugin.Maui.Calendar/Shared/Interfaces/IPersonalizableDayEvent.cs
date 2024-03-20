namespace Plugin.Maui.Calendar.Interfaces;

/// <summary>
/// Interface for customize DayEvent colors
/// </summary>
public interface IPersonalizableDayEvent
{
    #region PersonalizableProperties
    Style EventIndicatorStyle { get; set; }
    Style EventIndicatorSelectedStyle { get; set; }
    Style EventIndicatorLabelStyle { get; set; }
    Style EventIndicatorSelectedLabelStyle { get; set; }
    #endregion
}
