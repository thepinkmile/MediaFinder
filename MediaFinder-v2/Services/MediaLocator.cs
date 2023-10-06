using System.Collections.Immutable;
using System.IO;
using System.Runtime.CompilerServices;

using MediaFinder_v2.DataAccessLayer.Models;

namespace MediaFinder_v2.Services
{
    public class MediaLocator
    {
        private readonly IImmutableList<IMediaDetector> _mediaDetectors;


        public MediaLocator(IEnumerable<IMediaDetector> mediaDetectors)
        {
            _mediaDetectors = mediaDetectors.ToImmutableList();
        }

        public async IAsyncEnumerable<FileDetails> Search(IEnumerable<string> directoryPaths,
            string pattern = "*",
            bool recursive = false,
            bool performDeepAnalysis = false,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var dir in directoryPaths.Where(Directory.Exists))
            {
                await foreach(var file in Search(dir, pattern, recursive, performDeepAnalysis, cancellationToken))
                {
                    yield return file;
                }
            }
        }

        public async IAsyncEnumerable<FileDetails> Search(string directoryPath,
            string pattern = "*",
            bool recursive = false,
            bool performDeepAnalysis = false,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var file in EnumerateFiles(directoryPath, pattern, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly, cancellationToken))
            {
                var fileInfo = new FileInfo(file);

                yield return new FileDetails
                {
                    ParentPath = fileInfo.DirectoryName!,
                    FileName = fileInfo.Name,
                    FileSize = fileInfo.Length,
                    FileType = _mediaDetectors.FirstOrDefault(md => md.IsPositiveDetection(file, performDeepAnalysis))?.MediaType
                        ?? MultiMediaType.Unknown,
                    Created = fileInfo.CreationTimeUtc
                };
            }
        }

        private static async IAsyncEnumerable<string> EnumerateFiles(
            string path,
            string pattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var files = await Task.Factory.StartNew((state) => state is not EnumerateDirectoryState edsState
                    ? Array.Empty<string>()
                    : Directory.EnumerateFiles(edsState.Path, edsState.Pattern, edsState.SearchOption),
                    EnumerateDirectoryState.Create(path, pattern, searchOption));
            foreach(var file in files)
            {
                await Task.Yield();
                cancellationToken.ThrowIfCancellationRequested();

                yield return file;
            }
        }
    }
}
