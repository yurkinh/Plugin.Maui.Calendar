using FluentAssertions;
using Microsoft.Maui.Controls;
using Plugin.Maui.Calendar.Tests.TestHelpers;
using Xunit;
using CalendarExtensions = Plugin.Maui.Calendar.Extensions;

namespace Plugin.Maui.Calendar.Tests.Extensions;

/// <summary>
/// Verifies the internal DataTemplate helpers used for day cells, headers and footers: a
/// <see cref="DataTemplateSelector"/> receives the item and the real container, and the template
/// itself is never modified (it is usually a shared resource).
/// </summary>
[Collection(MauiControlsCollection.Name)]
public class DataTemplateExtensionsTests
{
    sealed class Selector(DataTemplate? choice) : DataTemplateSelector
    {
        public object? Item { get; private set; }

        public BindableObject? Container { get; private set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            Item = item;
            Container = container;
            return choice!;
        }
    }

    [Fact]
    public void SelectDataTemplate_PlainTemplate_ReturnsTheTemplateItself()
    {
        var template = new DataTemplate(() => new Label());

        CalendarExtensions.SelectDataTemplate(template, new object(), new ContentView())
            .Should().BeSameAs(template);
    }

    [Fact]
    public void SelectDataTemplate_Selector_PassesItemAndContainerAndReturnsItsChoice()
    {
        var choice = new DataTemplate(() => new Label());
        var selector = new Selector(choice);
        var item = new object();
        var container = new ContentView();

        CalendarExtensions.SelectDataTemplate(selector, item, container).Should().BeSameAs(choice);

        selector.Item.Should().BeSameAs(item);
        selector.Container.Should().BeSameAs(container);
    }

    [Fact]
    public void SelectDataTemplate_SelectorChoosingNothing_ReturnsNull()
    {
        CalendarExtensions.SelectDataTemplate(new Selector(null), new object(), new ContentView())
            .Should().BeNull();
    }

    [Fact]
    public void CreateContent_DoesNotModifyTemplateNorSetBindingContext()
    {
        var template = new DataTemplate(() => new Label());
        var item = new object();

        var content = CalendarExtensions.CreateContent(template, item, new ContentView());

        content.Should().BeOfType<Label>()
            .Which.BindingContext.Should().BeNull("the content inherits the item from its container instead");
        template.Values.Should().BeEmpty("a value set on a shared template leaks into every view it creates later");
        template.Bindings.Should().BeEmpty();
    }

    [Fact]
    public void CreateContent_Selector_CreatesContentOfTheChosenTemplateWithoutModifyingIt()
    {
        var choice = new DataTemplate(() => new BoxView());

        var content = CalendarExtensions.CreateContent(new Selector(choice), new object(), new ContentView());

        content.Should().BeOfType<BoxView>();
        choice.Values.Should().BeEmpty();
    }

    [Fact]
    public void CreateContent_SelectorChoosingNothing_ReturnsNull()
    {
        CalendarExtensions.CreateContent(new Selector(null), new object(), new ContentView())
            .Should().BeNull();
    }
}
