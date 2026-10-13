using FluentAssertions;
using Microsoft.Maui.Controls;
using Plugin.Maui.Calendar.Controls;
using Plugin.Maui.Calendar.Models;
using Plugin.Maui.Calendar.Tests.TestHelpers;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Controls;

/// <summary>
/// Verifies a <see cref="DayView"/> cell on its own: when it creates its content, which content a
/// template or selector gives it, and what a tap does when the cell is not inside a calendar.
/// </summary>
[Collection(MauiControlsCollection.Name)]
public class DayViewTests
{
    static readonly DateTime May10 = new(2025, 5, 10);

    static DayModel VisibleDay() => new() { Date = May10, IsThisMonth = true, AllowDeselect = true };

    [Fact]
    public void EnsureContent_CreatesTheContentOnlyOnce()
    {
        var template = new TemplateSpy(() => new Label());
        var dayView = new DayView(template.Template) { BindingContext = VisibleDay() };

        dayView.EnsureContent();
        dayView.EnsureContent();

        template.CreatedCount.Should().Be(1);
    }

    [Fact]
    public void Handler_CreatesTheContentWhenAttachedAndKeepsItWhenRemoved()
    {
        var dayView = new DayView { BindingContext = VisibleDay() };

        dayView.Handler = new FakeViewHandler();
        dayView.IsContentCreated.Should().BeTrue();

        dayView.Handler = null;
        dayView.ShowsBuiltInCell().Should().BeTrue();
    }

    [Fact]
    public void SetDayViewTemplate_SameTemplate_KeepsTheContent()
    {
        var template = new TemplateSpy(() => new Label());
        var dayView = new DayView(template.Template) { BindingContext = VisibleDay() };
        dayView.EnsureContent();
        var content = dayView.TemplateContent;

        dayView.SetDayViewTemplate(template.Template);

        dayView.TemplateContent.Should().BeSameAs(content);
        template.CreatedCount.Should().Be(1);
    }

    [Fact]
    public void Selector_WithoutADay_ShowsTheBuiltInCell()
    {
        var selector = new RecordingTemplateSelector(_ => CalendarTestExtensions.DayLabelTemplate());
        var dayView = new DayView(selector);

        dayView.EnsureContent();

        dayView.ShowsBuiltInCell().Should().BeTrue();
        selector.Calls.Should().BeEmpty("a selector is only given a day");
    }

    [Fact]
    public void Template_CreatingNothing_ThrowsAnExceptionThatSaysSo()
    {
        var dayView = new DayView(new DataTemplate(() => null!)) { BindingContext = VisibleDay() };

        var createContent = dayView.EnsureContent;

        createContent.Should().Throw<InvalidOperationException>().WithMessage("*created null*");
    }

    [Fact]
    public void Tap_OnACellOutsideACalendar_ChangesNoCalendar()
    {
        var dayView = new DayView { BindingContext = VisibleDay() };
        dayView.EnsureContent();
        var calendar = new TestCalendar { ShownDate = May10 };

        dayView.Tap();

        calendar.SelectedDate.Should().BeNull("only the calendar that holds a cell handles its taps");
    }

    [Fact]
    public void Tap_OnADisabledOrHiddenDay_DoesNothing()
    {
        var disabled = VisibleDay();
        disabled.IsDisabled = true;
        var hidden = VisibleDay();
        hidden.IsThisMonth = false;
        var taps = new List<DateTime>();
        foreach (var day in new[] { disabled, hidden })
        {
            day.DayTappedCommand = new Command<DateTime>(taps.Add);
            var dayView = new DayView { BindingContext = day };
            dayView.EnsureContent();
            dayView.Tap();
            day.IsSelected.Should().BeFalse();
        }

        taps.Should().BeEmpty();
    }

    [Fact]
    public void Tap_RunsTheDayTappedCommandWithTheDate()
    {
        var tapped = new List<DateTime>();
        var day = VisibleDay();
        day.DayTappedCommand = new Command<DateTime>(tapped.Add);
        var dayView = new DayView { BindingContext = day };
        dayView.EnsureContent();

        dayView.Tap();

        tapped.Should().Equal(May10);
        day.IsSelected.Should().BeTrue();
    }
}
