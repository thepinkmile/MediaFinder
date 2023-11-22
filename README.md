# Media Finder
This is an application to help locate and export any media files.
It can be used to migrate your existing content to another machine.
This tool can be useful if your source machine has files stored in "unusual" places (e.g. %APPDATA%, C:\Windows\xxx, C:\SomeDirectory).

## Features
- Discovers all files located in directories specified in the SearchSettings
- Discovers files within known archives (e.g. *.zip, *.tar.gz, ...)
- Allows for deep inspection of files
  - To detect correct media type regardless of file extension
- Enables basic initial filtering based on minimum width/height
  - [USE-CASE] - I don't want any application Icons to be exported
  - [USE-CASE] - I only want files created by my phone/camera (has known dimensions)
- Once discovered, allows for quick filtering and reviewing of discovered content

## Attributions
This project was created using Kevin Bost's DotnetTemplates tool [Keboo DotnetTemplates - GitHub](https://github.com/Keboo/DotnetTemplates).
```cli
dotnet new keboo.wpf
```

# ToDos
- Separate DAL model types from use in UI/App code
- Add Model type mappers
- Check ProgressOverlay functionality
- Add Tests
- Fix bug where navigation sometimes doesn't automatically move to next page
- Check issues with DataGrid virtualization in Drawer content with scroll bars
- Simplify logic for discovery services so type is created at start and just updated
  - Fix issue where MediaType is not discovered correctly (Video trumps Image)
  - Enables use of actual type instead of converting to and from strings (current Dictionary implementation)
- Fix namespaces (due to moving code into separate assemblies)
- Reintroduce details of archives located and extracted
- Add parent mapping of files that came from an archive to the archive file
- Add Discovery state tracking
- Add Discovery state persistence
- Add Discovery state loading
  - Load saved state
  - Verify discovered files still exist
  - Verify hashes of file as the same
  - If different, allow re-discovery of changed items
  - Allow discovery of new items
- Add feature to compare discovery runs (diff style)
