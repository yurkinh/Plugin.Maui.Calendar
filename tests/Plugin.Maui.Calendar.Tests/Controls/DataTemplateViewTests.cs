using FluentAssertions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Plugin.Maui.Calendar.Controls;
using Plugin.Maui.Calendar.Tests.TestHelpers;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Controls;

/// <summary>
/// Verifies <see cref="DataTemplateView"/>, which hosts the calendar's header and footer
/// templates: its content is bound to its own binding context by inheritance, a selector gets the
/// view as its container, and the template is never modified.
/// </summary>
[Collection(MauiControlsCollection.Name)]
public class DataTemplateViewTests
{
    [Fact]
    public void ItemTemplate_ContentInheritsBindingContextAndTemplateIsNotModified()
    {
        var template = new DataTemplate(() => new Label());
        var item = new object();

        var view = new DataTemplateView { ItemTemplate = template, BindingContext = item };

        view.Content.Should().BeOfType<Label>().Which.BindingContext.Should().BeSameAs(item);
        template.Values.Should().BeEmpty();
        ((Label)template.CreateContent()).BindingContext.Should().BeNull();
    }

    [Fact]
    public void ItemTemplate_Selector_ReceivesTheViewAsContainer()
    {
        var choice = new DataTemplate(() => new Label());
        var selector = new RecordingSelector(choice);

        var view = new DataTemplateView { ItemTemplate = selector, BindingContext = new object() };

        selector.Containers.Should().NotBeEmpty().And.OnlyContain(container => ReferenceEquals(container, view));
        view.Content.Should().BeOfType<Label>();
    }

    [Fact]
    public void Calendar_CustomHeaderTemplate_IsBoundToTheCalendar()
    {
        var header = new DataTemplate(() => new Label());
        var calendar = new TestCalendar();

        calendar.HeaderSectionTemplate = header;

        var headerView = calendar.GetVisualTreeDescendants().OfType<DataTemplateView>()
            .Single(view => ReferenceEquals(view.ItemTemplate, header));
        headerView.Content.Should().BeOfType<Label>().Which.BindingContext.Should().BeSameAs(calendar);
        header.Values.Should().BeEmpty();
    }

    sealed class RecordingSelector(DataTemplate choice) : DataTemplateSelector
    {
        public List<BindableObject> Containers { get; } = [];

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            Containers.Add(container);
            return choice;
        }
    }
}
