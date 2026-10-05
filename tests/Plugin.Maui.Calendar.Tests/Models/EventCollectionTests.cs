using System.Collections;
using System.Collections.ObjectModel;
using FluentAssertions;
using Plugin.Maui.Calendar.Models;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Models;

/// <summary>
/// Verifies <see cref="EventCollection"/>: entries are kept by day (the time of a key is ignored), every
/// change is reported to the calendars that show the collection, and the day collections that raise
/// CollectionChanged (issue #20) are observed exactly while they are stored in it.
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

    // ── Keys and the notifications of the collection itself ──────────────────

    static readonly DateTime May10 = new(2025, 5, 10);

    static List<(DateTime Item, string Type)> RecordChangesWithDays(EventCollection events)
    {
        var changes = new List<(DateTime, string)>();
        events.CollectionChanged += (_, e) => changes.Add((e.Item, e.Type.ToString()));
        return changes;
    }

    [Fact]
    public void Keys_IgnoreTheTimeOfDay()
    {
        var events = new EventCollection { [May10.AddHours(9)] = new List<string> { "Breakfast" } };

        events.ContainsKey(May10.AddHours(18)).Should().BeTrue();
        events[May10.AddMinutes(1)].Should().BeEquivalentTo(new[] { "Breakfast" });
        events.TryGetValue(May10.AddHours(23), out var dayEvents).Should().BeTrue();
        dayEvents.Should().BeSameAs(events[May10]);
        events.Keys.Should().Equal(May10);
    }

    [Fact]
    public void Capacity_CreatesAnEmptyCollection()
    {
        new EventCollection(10).Should().BeEmpty();
    }

    [Fact]
    public void AddSetRemoveAndClear_AreReported()
    {
        var events = new EventCollection();
        var changes = RecordChangesWithDays(events);

        events.Add(May10.AddHours(8), new List<string>());
        events[May10] = new List<string> { "Lunch" };
        events.Remove(May10.AddHours(20)).Should().BeTrue();
        events.Add(May10, new List<string>());
        events.Clear();

        changes.Should().Equal(
            (May10, "Add"),
            (May10, "Set"),
            (May10, "Remove"),
            (May10, "Add"),
            (default(DateTime), "Clear"));
    }

    [Fact]
    public void RemovingAMissingDayAndClearingAnEmptyCollection_AreNotReported()
    {
        var events = new EventCollection();
        var changes = RecordChangesWithDays(events);

        events.Remove(May10).Should().BeFalse();
        events.Clear();

        changes.Should().BeEmpty();
    }

    [Fact]
    public void Changes_WithoutObservers_DoNotThrow()
    {
        var events = new EventCollection();

        var dayEvents = new ObservableCollection<string>();

        var change = () =>
        {
            events.Add(May10, new List<string>());
            events[May10] = dayEvents;
            dayEvents.Add("observed, but nobody listens");
            events.Remove(May10);
            events.Add(May10, new List<string>());
            events.Clear();
        };

        change.Should().NotThrow();
    }

    [Fact]
    public void TryGetValues_CollectsTheEventsOfTheGivenDays()
    {
        var events = new EventCollection
        {
            [May10] = new List<string> { "a", "b" },
            [May10.AddDays(2)] = new List<string> { "c" },
        };

        events.TryGetValues([May10, May10.AddDays(1), May10.AddDays(2)], out var values).Should().BeTrue();
        values.Cast<object>().Should().Equal("a", "b", "c");

        events.TryGetValues([May10.AddDays(1)], out var none).Should().BeFalse();
        none.Cast<object>().Should().BeEmpty();
    }

    [Fact]
    public void TryGetValues_DaysWithEmptyEntriesOnly_ReturnsFalse()
    {
        var events = new EventCollection { [May10] = new List<string>() };

        events.TryGetValues([May10], out ICollection values).Should().BeFalse();
        values.Cast<object>().Should().BeEmpty();
    }
}
