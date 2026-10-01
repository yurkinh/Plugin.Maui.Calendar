using FluentAssertions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Plugin.Maui.Calendar.Tests.TestHelpers;
using Xunit;
using CalendarControl = Plugin.Maui.Calendar.Controls.Calendar;

namespace Plugin.Maui.Calendar.Tests.Controls.Calendar;

/// <summary>
/// Verifies showing and hiding the calendar section (the header and the days) with
/// <c>CalendarSectionShown</c>, <c>ShowHideCalendarCommand</c> and swipe up, with and without the
/// animation that slides it.
/// </summary>
[Collection(MauiControlsCollection.Name)]
public class CalendarSectionTests
{
    const string animationName = "calendarSectionAnimationId";
    const double sectionHeight = 280;

    static VerticalStackLayout Section(CalendarControl calendar) => calendar.Named<VerticalStackLayout>("calendarContainer");

    /// <summary>Lays the section out, as the platform does when it is first shown.</summary>
    static void LayOut(CalendarControl calendar) => Section(calendar).Frame = new Rect(0, 0, 320, sectionHeight);

    static void ShouldBeShown(CalendarControl calendar)
    {
        var section = Section(calendar);
        section.HeightRequest.Should().Be(-1, "a shown section takes its natural height");
        section.TranslationY.Should().Be(0);
        section.Opacity.Should().Be(1);
    }

    static void ShouldBeHidden(CalendarControl calendar, double translation)
    {
        var section = Section(calendar);
        section.HeightRequest.Should().Be(0);
        section.TranslationY.Should().Be(translation);
        section.Opacity.Should().Be(0);
    }

    [Fact]
    public void WithoutHandler_HidesAndShowsRightAway()
    {
        var calendar = new TestCalendar();

        calendar.CalendarSectionShown = false;
        ShouldBeHidden(calendar, translation: 0);

        calendar.CalendarSectionShown = true;
        ShouldBeShown(calendar);
    }

    [Fact]
    public void HiddenBeforeItWasEverLaidOut_CanBeShownAgain()
    {
        // CalendarSectionShown="False" in XAML: the section was never laid out, so its height is unknown.
        // It used to be animated back to a height of 0, so it could never be shown again.
        var calendar = new TestCalendar { CalendarSectionShown = false };
        calendar.AttachHandler(FakeHandlers.WithAnimations(new FakeTicker()));

        calendar.CalendarSectionShown = true;

        ShouldBeShown(calendar);
    }

    [Fact]
    public void WithHandler_AnimationsOffInTheSystem_FinishesRightAway()
    {
        var calendar = new TestCalendar();
        calendar.AttachHandler(FakeHandlers.WithAnimations(new FakeTicker { SystemEnabled = false }));
        LayOut(calendar);

        calendar.CalendarSectionShown = false;
        ShouldBeHidden(calendar, translation: -sectionHeight);

        calendar.CalendarSectionShown = true;
        ShouldBeShown(calendar);
    }

    [Fact]
    public void WithHandler_AnimatesTheSectionOutAndIn()
    {
        var calendar = new TestCalendar();
        calendar.AttachHandler(FakeHandlers.WithAnimations(new FakeTicker { SystemEnabled = true }));
        LayOut(calendar);

        calendar.CalendarSectionShown = false;

        // The first frame is drawn right away: the section still has its full height.
        calendar.AnimationIsRunning(animationName).Should().BeTrue();
        Section(calendar).HeightRequest.Should().Be(sectionHeight);

        calendar.AbortAnimation(animationName);
        ShouldBeHidden(calendar, translation: -sectionHeight);

        calendar.CalendarSectionShown = true;
        calendar.AnimationIsRunning(animationName).Should().BeTrue();
        Section(calendar).HeightRequest.Should().Be(0);

        calendar.AbortAnimation(animationName);
        ShouldBeShown(calendar);
    }

    [Fact]
    public void ChangedWhileAnimating_TheLastValueIsAnimatedWhenTheAnimationFinishes()
    {
        var ticker = new FakeTicker { SystemEnabled = true };
        var calendar = new TestCalendar();
        calendar.AttachHandler(FakeHandlers.WithAnimations(ticker));
        LayOut(calendar);

        calendar.CalendarSectionShown = false;
        calendar.CalendarSectionShown = true;

        // Hiding finishes, then the section is animated back because CalendarSectionShown is true now.
        ticker.FinishFrame();
        calendar.AnimationIsRunning(animationName).Should().BeTrue();
        Section(calendar).HeightRequest.Should().Be(0);

        ticker.FinishFrame();
        calendar.AnimationIsRunning(animationName).Should().BeFalse();
        ShouldBeShown(calendar);
    }

    [Fact]
    public void ChangedWhileAnimating_AbortingTheAnimationAppliesTheLastValue()
    {
        var calendar = new TestCalendar();
        calendar.AttachHandler(FakeHandlers.WithAnimations(new FakeTicker { SystemEnabled = true }));
        LayOut(calendar);

        calendar.CalendarSectionShown = false;
        calendar.CalendarSectionShown = true;

        // Aborting also aborts the animation started when the first one finished.
        calendar.AbortAnimation(animationName);

        calendar.AnimationIsRunning(animationName).Should().BeFalse();
        ShouldBeShown(calendar);
    }

    [Fact]
    public void AnimatesAgainAfterTheHandlerWasRemovedAndAttachedAgain()
    {
        // Removing the handler disposes the calendar. The animations used to be disposed with it,
        // which clears their step callback, so afterwards the section never moved again.
        var calendar = new TestCalendar();
        calendar.AttachHandler(FakeHandlers.WithAnimations(new FakeTicker()));
        LayOut(calendar);
        calendar.CalendarSectionShown = false;
        calendar.CalendarSectionShown = true;

        calendar.Handler = null;
        calendar.AttachHandler(FakeHandlers.WithAnimations(new FakeTicker()));
        calendar.CalendarSectionShown = false;

        ShouldBeHidden(calendar, translation: -sectionHeight);
    }

    [Fact]
    public void SectionHeight_IsNotUpdatedWhileAnimating()
    {
        var calendar = new TestCalendar();
        calendar.AttachHandler(FakeHandlers.WithAnimations(new FakeTicker { SystemEnabled = true }));
        LayOut(calendar);

        calendar.CalendarSectionShown = false;
        // A layout pass during the animation (the section is shrinking) must not become the new height.
        Section(calendar).Frame = new Rect(0, 0, 320, 100);
        calendar.AbortAnimation(animationName);

        ShouldBeHidden(calendar, translation: -sectionHeight);
    }

    [Fact]
    public void SectionHeight_IsNotUpdatedWhileTheSectionHasNoHeight()
    {
        var calendar = new TestCalendar();
        calendar.AttachHandler(FakeHandlers.WithAnimations(new FakeTicker()));
        LayOut(calendar);
        Section(calendar).Frame = new Rect(0, 0, 320, 0);

        calendar.CalendarSectionShown = false;

        ShouldBeHidden(calendar, translation: -sectionHeight);
    }

    [Fact]
    public void ShowHideCalendarCommand_TogglesTheSection()
    {
        var calendar = new TestCalendar();

        calendar.ShowHideCalendarCommand.Execute(null);
        calendar.CalendarSectionShown.Should().BeFalse();

        calendar.ShowHideCalendarCommand.Execute(null);
        calendar.CalendarSectionShown.Should().BeTrue();
    }
}
