using System.Collections.ObjectModel;
using FluentAssertions;
using Plugin.Maui.Calendar.Models;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Models;

/// <summary>
/// Verifies that <see cref="EventCollection"/> observes the day collections that raise
/// CollectionChanged (issue #20) exactly while they are stored in it.
/// </summary>
public class EventCollectionTests
{
    static readonly DateTime May12 = new(2025, 5, 12);
    static readonly DateTime May13 = new(2025, 5, 13);

    static List<EventCollection.EventCollectionChangedType> RecordChanges(EventCollection events)
    {
        var changes = new List<EventCollection.EventCollectionChangedType>();
        events.CollectionChanged += (_, e) => changes.Add(e.Type);
        return changes;
    }

    [Fact]
    public void ChangesInsideAnAddedDayCollection_AreReported()
    {
        var dayEvents = new ObservableCollection<string> { "a" };
        var events = new EventCollection();
        events.Add(May12, dayEvents);
        var changes = RecordChanges(events);

        dayEvents.Add("b");
        dayEvents[0] = "c";
        dayEvents.Move(0, 1);
        dayEvents.Remove("b");
        dayEvents.Clear();

        changes.Should().HaveCount(5).And.OnlyContain(type => type == EventCollection.EventCollectionChangedType.DayCollectionChanged);
    }

    [Fact]
    public void ChangesInsideADayCollectionAssignedThroughTheIndexer_AreReported()
    {
        var dayEvents = new ObservableCollection<string>();
        var events = new EventCollection { [May12] = dayEvents };
        var changes = RecordChanges(events);

        dayEvents.Add("a");

        changes.Should().Equal(EventCollection.EventCollectionChangedType.DayCollectionChanged);
    }

    [Fact]
    public void ReplacedRemovedOrClearedDayCollections_AreNoLongerObserved()
    {
        var replaced = new ObservableCollection<string>();
        var removed = new ObservableCollection<string>();
        var cleared = new ObservableCollection<string>();
        var events = new EventCollection { [May12] = replaced, [May13] = removed };
        var changes = RecordChanges(events);

        events[May12] = new List<string>();
        events.Remove(May13);
        replaced.Add("a");
        removed.Add("a");

        changes.Should().Equal(EventCollection.EventCollectionChangedType.Set, EventCollection.EventCollectionChangedType.Remove);

        events[May12] = cleared;
        events.Clear();
        changes.Clear();
        cleared.Add("a");

        changes.Should().BeEmpty();
    }

    [Fact]
    public void DayCollectionStoredForTwoDays_IsObservedOnceUntilItsLastDayIsRemoved()
    {
        var shared = new ObservableCollection<string>();
        var events = new EventCollection { [May12] = shared, [May13] = shared };
        var changes = RecordChanges(events);

        shared.Add("a");
        changes.Should().HaveCount(1, "a collection stored for two days is observed once");

        events.Remove(May12);
        changes.Clear();
        shared.Add("b");
        changes.Should().HaveCount(1, "the collection is still stored for May 13");

        events.Remove(May13);
        changes.Clear();
        shared.Add("c");
        changes.Should().BeEmpty();
    }

    [Fact]
    public void DayCollectionAssignedAgainToItsOwnDay_IsObservedOnce()
    {
        var dayEvents = new ObservableCollection<string>();
        var events = new EventCollection { [May12] = dayEvents };
        events[May12] = dayEvents;
        var changes = RecordChanges(events);

        dayEvents.Add("a");

        changes.Should().HaveCount(1);
    }

    [Fact]
    public void DayCollectionRejectedByAddForAnExistingDate_IsNotObserved()
    {
        var events = new EventCollection { [May12] = new List<string>() };
        var rejected = new ObservableCollection<string>();
        var changes = RecordChanges(events);

        var add = () => events.Add(May12, rejected);

        add.Should().Throw<ArgumentException>();
        rejected.Add("a");
        changes.Should().BeEmpty();
    }
}
