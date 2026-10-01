using System.Runtime.CompilerServices;
using Microsoft.Maui.Dispatching;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.TestHelpers;

/// <summary>
/// Lets MAUI controls be created without a running app. A <c>BindableObject</c> needs a
/// dispatcher for the current thread (the <c>Calendar</c> constructor already evaluates
/// bindings), and the test host has none, so a synchronous one is registered before any test runs.
/// </summary>
static class MauiTestEnvironment
{
    [ModuleInitializer]
    internal static void Initialize() => DispatcherProvider.SetCurrent(new SyncDispatcherProvider());
}

/// <summary>
/// Groups every test that creates MAUI controls and runs them one at a time, never alongside
/// other tests. Controls share process-wide state that is not safe to use from parallel test
/// threads: the library's default styles, <c>WeakReferenceMessenger.Default</c> (day taps) and
/// the timers recorded by <see cref="SyncDispatcher"/>.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class MauiControlsCollection
{
    public const string Name = "MAUI controls";
}

sealed class SyncDispatcherProvider : IDispatcherProvider
{
    [ThreadStatic]
    static IDispatcher? dispatcherOverride;

    /// <summary>
    /// Makes controls created on this thread use <paramref name="dispatcher"/> until the returned
    /// scope is disposed.
    /// </summary>
    public static IDisposable Use(IDispatcher dispatcher)
    {
        dispatcherOverride = dispatcher;
        return new OverrideScope();
    }

    public IDispatcher GetForCurrentThread() => dispatcherOverride ?? SyncDispatcher.Instance;

    sealed class OverrideScope : IDisposable
    {
        public void Dispose() => dispatcherOverride = null;
    }
}

/// <summary>
/// Stands in for the UI thread's dispatcher as seen from a background thread: while
/// <see cref="IsDispatchRequired"/> is true, dispatched work is queued until
/// <see cref="RunQueued"/> runs it the way the UI thread later would.
/// </summary>
sealed class QueueingDispatcher : IDispatcher
{
    readonly Queue<Action> queued = new();

    public bool IsDispatchRequired { get; set; }

    public int QueuedCount => queued.Count;

    public bool Dispatch(Action action)
    {
        if (IsDispatchRequired)
        {
            queued.Enqueue(action);
        }
        else
        {
            action();
        }
        return true;
    }

    public bool DispatchDelayed(TimeSpan delay, Action action) => Dispatch(action);

    public IDispatcherTimer CreateTimer() => new FakeDispatcherTimer();

    /// <summary>Runs the queued work as the UI thread, where no dispatch is required.</summary>
    public void RunQueued()
    {
        IsDispatchRequired = false;
        while (queued.TryDequeue(out var action))
        {
            action();
        }
    }
}

/// <summary>
/// Runs dispatched work inline and hands out <see cref="FakeDispatcherTimer"/>s, so tests can
/// fire timer ticks themselves instead of waiting for them.
/// </summary>
sealed class SyncDispatcher : IDispatcher
{
    public static readonly SyncDispatcher Instance = new();

    readonly List<FakeDispatcherTimer> createdTimers = [];

    /// <summary>Every timer created through this dispatcher, oldest first.</summary>
    public IReadOnlyList<FakeDispatcherTimer> CreatedTimers => createdTimers;

    public bool IsDispatchRequired => false;

    public bool Dispatch(Action action)
    {
        action();
        return true;
    }

    public bool DispatchDelayed(TimeSpan delay, Action action)
    {
        action();
        return true;
    }

    public IDispatcherTimer CreateTimer()
    {
        var timer = new FakeDispatcherTimer();
        createdTimers.Add(timer);
        return timer;
    }
}

/// <summary>A dispatcher timer that only ticks when a test calls <see cref="Fire"/>.</summary>
sealed class FakeDispatcherTimer : IDispatcherTimer
{
    public TimeSpan Interval { get; set; }

    public bool IsRepeating { get; set; }

    public bool IsRunning { get; private set; }

    public event EventHandler? Tick;

    /// <summary>How many handlers are subscribed to <see cref="Tick"/>.</summary>
    public int TickSubscriberCount => Tick?.GetInvocationList().Length ?? 0;

    public void Start() => IsRunning = true;

    public void Stop() => IsRunning = false;

    /// <summary>
    /// Simulates the interval elapsing the way MAUI's platform timers do: a stopped timer does
    /// nothing, Tick is raised, and a one-shot timer is stopped after Tick returns (which also
    /// stops a one-shot timer that a Tick handler restarted).
    /// </summary>
    public void Fire()
    {
        if (!IsRunning)
        {
            return;
        }

        Tick?.Invoke(this, EventArgs.Empty);

        if (!IsRepeating)
        {
            Stop();
        }
    }
}
