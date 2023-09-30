using MediaFinder_v2.Views.SearchSettings;

namespace MediaFinder_v2;

//This attribute generates tests for MainWindowViewModel that
//asserts all constructor arguments are checked for null
[ConstructorTests(typeof(SearchSettingsViewModel))]
public partial class SearchSettingsViewModelTests
{
    [Fact]
    public void AddSearchSettings_PersistsDataCorrectly()
    {
        //Arrange
        AutoMocker mocker = new();
        var viewModel = mocker.CreateInstance<SearchSettingsViewModel>();

        //Act
        viewModel.AddSearchSettingCommand.Execute(null);

        //Assert
        // TODO: add something here
    }
}
