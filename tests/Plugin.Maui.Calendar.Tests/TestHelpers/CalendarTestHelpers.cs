using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Plugin.Maui.Calendar.Controls;
using Plugin.Maui.Calendar.Interfaces;
using Plugin.Maui.Calendar.Models;
using CalendarControl = Plugin.Maui.Calendar.Controls.Calendar;

namespace Plugin.Maui.Calendar.Tests.TestHelpers;

/// <summary>Exposes the day cells of a calendar under test (<c>Calendar.dayViews</c> is protected).</summary>
interface IDayCells
{
    /// <summary>The live list of day cells, in grid order. A layout rebuild replaces its items.</summary>
    IReadOnlyList<DayView> DayViews { get; }
}

sealed class TestCalendar : CalendarControl, IDayCells
{
    public IReadOnlyList<DayView> DayViews => dayViews;

    /// <summary>Calls the protected <c>Dispose(bool)</c>, as a finalizer of a derived class would with <see langword="false"/>.</summary>
    public void CallDispose(bool disposing) => Dispose(disposing);
}

sealed class TestMultiSelectionCalendar : MultiSelectionCalendar, IDayCells
{
    public IReadOnlyList<DayView> DayViews => dayViews;
}

sealed class TestRangeSelectionCalendar : RangeSelectionCalendar, IDayCells
{
    public IReadOnlyList<DayView> DayViews => dayViews;
}

sealed class TestWeekSelectionCalendar : WeekSelectionCalendar, IDayCells
{
    public IReadOnlyList<DayView> DayViews => dayViews;
}

/// <summary>A <see cref="DataTemplate"/> that counts the views it creates.</summary>
sealed class TemplateSpy
{
    public TemplateSpy(Func<View> createView)
    {
        Template = new DataTemplate(() =>
        {
            CreatedCount++;
            return createView();
        });
    }

    public DataTemplate Template { get; }

    public int CreatedCount { get; private set; }
}

/// <summary>A <see cref="DataTemplateSelector"/> that records every call and the container it received.</summary>
sealed class RecordingTemplateSelector(Func<ICalendarDay, DataTemplate?> choose) : DataTemplateSelector
{
    public List<(DateTime Date, BindableObject Container)> Calls { get; } = [];

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        var day = (ICalendarDay)item;
        Calls.Add((day.Date, container));
        return choose(day)!;
    }
}

/// <summary>
/// A handler that does nothing, standing in for a platform handler. Attaching it makes a view act
/// as if it were about to be rendered; for example a DayView then creates its content.
/// </summary>
class FakeViewHandler : IViewHandler
{
    public bool HasContainer { get; set; }

    public object? ContainerView => null;

    public IView? VirtualView { get; private set; }

    IElement? IElementHandler.VirtualView => VirtualView;

    public object? PlatformView => null;

    public IMauiContext? MauiContext { get; init; }

    public Size GetDesiredSize(double widthConstraint, double heightConstraint) => Size.Zero;

    public void PlatformArrange(Rect frame) { }

    public void SetMauiContext(IMauiContext mauiContext) { }

    public void SetVirtualView(IElement view) => VirtualView = (IView)view;

    public void UpdateValue(string property) { }

    public virtual void Invoke(string command, object? args = null) { }

    public void DisconnectHandler() => VirtualView = null;
}

/// <summary>
/// A layout handler that, like a platform one, gives every child added to the layout its own
/// handler right away. Attach it to the days grid to simulate a calendar that is on screen.
/// </summary>
sealed class FakeLayoutHandler : FakeViewHandler
{
    public override void Invoke(string command, object? args = null)
    {
        if (args is Microsoft.Maui.Handlers.LayoutHandlerUpdate { View: Element { Handler: null } child })
        {
            child.Handler = new FakeViewHandler();
        }
    }
}

/// <summary>
/// A ticker that never ticks on its own. With <see cref="SystemEnabled"/> off (animations turned off
/// in the system settings) MAUI finishes every animation as soon as it starts; with it on, an
/// animation runs until a test aborts it.
/// </summary>
sealed class FakeTicker : ITicker
{
    public bool IsRunning { get; private set; }

    public int MaxFps { get; set; } = 60;

    public Action? Fire { get; set; }

    public bool SystemEnabled { get; set; }

    public void Start() => IsRunning = true;

    public void Stop() => IsRunning = false;

    /// <summary>
    /// Lets the running animations reach their end, as the next frames would. The animation manager
    /// measures the time between frames with the system clock, and <see cref="FakeHandlers.WithAnimations"/>
    /// speeds its animations up so that a few milliseconds are enough.
    /// </summary>
    public void FinishFrame()
    {
        Thread.Sleep(30);
        Fire?.Invoke();
    }
}

static class FakeHandlers
{
    /// <summary>
    /// A handler whose <see cref="IMauiContext"/> provides an animation manager driven by
    /// <paramref name="ticker"/>, so the view can run animations.
    /// </summary>
    public static FakeViewHandler WithAnimations(FakeTicker ticker)
    {
        var services = new ServiceCollection()
            .AddSingleton<IAnimationManager>(new AnimationManager(ticker) { SpeedModifier = 1000 })
            .BuildServiceProvider();

        return new FakeViewHandler { MauiContext = new MauiContext(services) };
    }
}

/// <summary>Event entry whose indicator colors come from the day itself (see <see cref="IMultiEventDay"/>).</summary>
sealed class MultiColorEvents(params Color[] colors) : List<string>, IMultiEventDay
{
    public IReadOnlyList<Color> Colors { get; } = colors;
}

/// <summary>Event entry that sets its own indicator colors (see <see cref="IPersonalizableDayEvent"/>).</summary>
sealed class PersonalizedEvents : List<string>, IPersonalizableDayEvent
{
    public Color? EventIndicatorColor { get; set; }

    public Color? EventIndicatorSelectedColor { get; set; }

    public Color? EventIndicatorTextColor { get; set; }

    public Color? EventIndicatorSelectedTextColor { get; set; }
}

static class CalendarTestExtensions
{
    const BindingFlags instanceMembers = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    /// <summary>A view whose Label shows <see cref="ICalendarDay.Day"/> through a binding.</summary>
    public static View CreateDayLabel()
    {
        var label = new Label();
        label.SetBinding(Label.TextProperty, nameof(ICalendarDay.Day));
        return label;
    }

    public static DataTemplate DayLabelTemplate() => new(CreateDayLabel);

    public static DayModel Day(this DayView dayView) => (DayModel)dayView.BindingContext;

    public static List<DayModel> Days(this IDayCells calendar) => [.. calendar.DayViews.Select(Day)];

    public static DayView CellFor(this IDayCells calendar, DateTime date) =>
        calendar.DayViews.Single(dayView => dayView.Day().Date == date.Date);

    public static DayModel DayFor(this IDayCells calendar, DateTime date) => calendar.CellFor(date).Day();

    /// <summary>The Grid inside a DayView that holds the cell content and the tap recognizer.</summary>
    public static Grid Container(this DayView dayView) => (Grid)dayView.Content;

    /// <summary>
    /// Creates the content of every cell, as happens right before the cells are first rendered
    /// (there is no handler in unit tests).
    /// </summary>
    public static void EnsureAllContent(this IDayCells calendar)
    {
        foreach (var dayView in calendar.DayViews)
        {
            dayView.EnsureContent();
        }
    }

    /// <summary>Whether the cell shows the built-in content: a background Border and a content FlexLayout.</summary>
    public static bool ShowsBuiltInCell(this DayView dayView)
    {
        var container = dayView.Container();
        return dayView.IsContentCreated
            && dayView.TemplateContent is null
            && container.Count == 2
            && container[0] is Border
            && container[1] is FlexLayout;
    }

    /// <summary>Whether the cell shows exactly one view created from a template, and nothing else.</summary>
    public static bool ShowsTemplateContent(this DayView dayView)
    {
        var container = dayView.Container();
        return dayView.IsContentCreated
            && dayView.TemplateContent is not null
            && container.Count == 1
            && ReferenceEquals(container[0], dayView.TemplateContent);
    }

    /// <summary>Taps the cell through the tap recognizer on its container, as a touch would.</summary>
    public static void Tap(this DayView dayView)
    {
        var container = dayView.Container();
        var recognizer = container.GestureRecognizers.OfType<TapGestureRecognizer>().Single();
        var sendTapped = typeof(TapGestureRecognizer).GetMethod("SendTapped", instanceMembers)
            ?? throw new MissingMethodException(nameof(TapGestureRecognizer), "SendTapped");

        sendTapped.Invoke(recognizer, [container, null]);
    }

    /// <summary>
    /// Runs <paramref name="action"/> while the calendar listens for day taps. The calendar
    /// normally subscribes when it gets a handler, which never happens in unit tests.
    /// </summary>
    public static void WithTapHandling(this CalendarControl calendar, Action action)
    {
        InvokePrivate(calendar, "AttachHandler");
        try
        {
            action();
        }
        finally
        {
            InvokePrivate(calendar, "DetachHandler");
        }
    }

    /// <summary>Does what the calendar does when it gets a handler (it never gets one in unit tests).</summary>
    public static void SimulateHandlerAttached(this CalendarControl calendar) => InvokePrivate(calendar, "AttachHandler");

    /// <summary>Does what the calendar does when its handler is removed (for example when its page is popped).</summary>
    public static void SimulateHandlerDetached(this CalendarControl calendar) => InvokePrivate(calendar, "DetachHandler");

    /// <summary>Raises what the calendar does on its Loaded event (never raised without a window).</summary>
    public static void SimulateLoaded(this CalendarControl calendar) =>
        InvokePrivate(calendar, "OnCalendarLoaded", calendar, EventArgs.Empty);

    /// <summary>Raises what the calendar does on its Unloaded event.</summary>
    public static void SimulateUnloaded(this CalendarControl calendar) =>
        InvokePrivate(calendar, "OnCalendarUnloaded", calendar, EventArgs.Empty);

    /// <summary>The element named <paramref name="name"/> (x:Name) in the calendar's own XAML.</summary>
    public static T Named<T>(this CalendarControl calendar, string name) where T : Element =>
        calendar.FindByName<T>(name) ?? throw new InvalidOperationException($"The calendar has no element named {name}.");

    /// <summary>
    /// Gives the calendar a handler, as happens when it is about to be shown on screen. Unlike
    /// <see cref="SimulateHandlerAttached"/> this goes through <c>OnHandlerChanging</c>, and the
    /// calendar's <c>Handler</c> is set afterwards.
    /// </summary>
    public static FakeViewHandler AttachHandler(this CalendarControl calendar, FakeViewHandler? handler = null)
    {
        handler ??= new FakeViewHandler();
        calendar.Handler = handler;
        return handler;
    }

    /// <summary>The swipe recognizers the calendar added to itself.</summary>
    public static List<SwipeGestureRecognizer> SwipeGestures(this CalendarControl calendar) =>
        [.. calendar.GestureRecognizers.OfType<SwipeGestureRecognizer>()];

    /// <summary>Swipes over the calendar in <paramref name="direction"/>, through its swipe recognizer.</summary>
    public static void Swipe(this CalendarControl calendar, SwipeDirection direction) =>
        calendar.SwipeGestures().Single(gesture => gesture.Direction == direction).SendSwiped(calendar, direction);

    /// <summary>Collects the names of the properties for which <paramref name="source"/> raises PropertyChanged.</summary>
    public static List<string> RecordPropertyChanges(this INotifyPropertyChanged source)
    {
        var names = new List<string>();
        source.PropertyChanged += (_, e) => names.Add(e.PropertyName ?? string.Empty);
        return names;
    }

    /// <summary>
    /// Runs a test that depends on <see cref="DateTime.Today"/> and runs it once more if the local
    /// date changed while it ran, so a run that crosses midnight cannot fail spuriously.
    /// </summary>
    public static void OnStableDay(Action<DateTime> test)
    {
        var today = DateTime.Today;
        try
        {
            test(today);
        }
        catch when (DateTime.Today != today)
        {
            test(DateTime.Today);
            return;
        }

        if (DateTime.Today != today)
        {
            test(DateTime.Today);
        }
    }

    static void InvokePrivate(CalendarControl calendar, string methodName, params object?[] args)
    {
        var method = typeof(CalendarControl).GetMethod(methodName, instanceMembers)
            ?? throw new MissingMethodException(nameof(CalendarControl), methodName);

        method.Invoke(calendar, args);
    }
}
