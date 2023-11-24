using FluentAssertions;

using MediaFinder.Messages;
using MediaFinder.Views;
using MediaFinder.Views.Discovery;

namespace MediaFinder.Tests;

//This attribute generates tests for MainWindowViewModel that
//asserts all constructor arguments are checked for null
[ConstructorTests(typeof(MainWindowViewModel))]
public partial class MainWindowViewModelTests
{
    // TODO: re-create tests

    [Fact]
    public void Receive_ShowProgressMessage_Populates_Progress_Template_Variables()
    {
        // Arrange
        AutoMocker mocker = new();
        MainWindowViewModel viewModel = mocker.CreateInstance<MainWindowViewModel>(true);
        viewModel.ProgressVisible.Should().BeFalse();

        var testMessage = new ShowProgressMessage(
            "TestToken",
            50,
            "Test Message"
            );

        // Act
        viewModel.Receive(testMessage);

        // Assert
        viewModel.ProgressVisible.Should().BeTrue();
        viewModel.ProgressValue.Should().Be(50);
        viewModel.ProgressMessage.Should().Be("Test Message");
    }
}