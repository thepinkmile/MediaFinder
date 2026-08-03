using FluentAssertions;

using MediaFinder.Messages;
using MediaFinder.Views;

namespace MediaFinder.Tests;

//This attribute generates tests for MainWindowViewModel that
//asserts all constructor arguments are checked for null
[ConstructorTests(typeof(MainWindowViewModel))]
public partial class MainWindowViewModelTests
{
    [Fact]
    public void Receive_ShowProgressMessage_Populates_Progress_Template_Variables()
    {
        // Arrange
        AutoMocker mocker = new();
        MainWindowViewModel viewModel = mocker.CreateInstance<MainWindowViewModel>(true);
        viewModel.ProgressVisible.Should().BeFalse();

        var testMessage = new ShowProgressMessage(
            "Receive_ShowProgressMessage_Populates_Progress_Template_Variables",
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

    [Fact]
    public void Receive_UpdateProgressMessage_Populates_Progress_Template_Variables()
    {
        // Arrange
        AutoMocker mocker = new();
        MainWindowViewModel viewModel = mocker.CreateInstance<MainWindowViewModel>(true);
        viewModel.Receive(new ShowProgressMessage(
            "Receive_UpdateProgressMessage_Populates_Progress_Template_Variables",
            50,
            "Initial Message"
            ));
        viewModel.ProgressVisible.Should().BeTrue();
        viewModel.ProgressValue.Should().Be(50);
        viewModel.ProgressMessage.Should().Be("Initial Message");

        var testMessage = new UpdateProgressMessage(
            "Receive_UpdateProgressMessage_Populates_Progress_Template_Variables",
            -1,
            "Test Message");

        // Act
        viewModel.Receive(testMessage);

        // Assert
        viewModel.ProgressVisible.Should().BeTrue();
        viewModel.ProgressValue.Should().Be(-1);
        viewModel.ProgressMessage.Should().Be("Test Message");
    }

    [Fact]
    public void Receive_CancelProgressMessage_Populates_Progress_Template_Variables()
    {
        // Arrange
        AutoMocker mocker = new();
        MainWindowViewModel viewModel = mocker.CreateInstance<MainWindowViewModel>(true);
        viewModel.Receive(new ShowProgressMessage(
            "Receive_CancelProgressMessage_Populates_Progress_Template_Variables",
            50,
            "Initial Message"
            ));
        viewModel.ProgressVisible.Should().BeTrue();
        viewModel.ProgressValue.Should().Be(50);
        viewModel.ProgressMessage.Should().Be("Initial Message");

        var testMessage = new CancelProgressMessage(
            "Receive_CancelProgressMessage_Populates_Progress_Template_Variables");

        // Act
        viewModel.Receive(testMessage);

        // Assert
        viewModel.ProgressVisible.Should().BeTrue();
        viewModel.ProgressValue.Should().Be(0);
        viewModel.ProgressMessage.Should().Be("Cancelling...");
    }

    [Fact]
    public void Receive_CompleteProgressMessage_Populates_Progress_Template_Variables()
    {
        // Arrange
        AutoMocker mocker = new();
        MainWindowViewModel viewModel = mocker.CreateInstance<MainWindowViewModel>(true);
        viewModel.Receive(new ShowProgressMessage(
            "Receive_CompleteProgressMessage_Populates_Progress_Template_Variables",
            50,
            "Initial Message"
            ));
        viewModel.ProgressVisible.Should().BeTrue();
        viewModel.ProgressValue.Should().Be(50);
        viewModel.ProgressMessage.Should().Be("Initial Message");

        var testMessage = new CompleteProgressMessage(
            "Receive_CompleteProgressMessage_Populates_Progress_Template_Variables");

        // Act
        viewModel.Receive(testMessage);

        // Assert
        viewModel.ProgressVisible.Should().BeFalse();
        viewModel.ProgressValue.Should().Be(0);
        viewModel.ProgressMessage.Should().BeNull();
    }
}