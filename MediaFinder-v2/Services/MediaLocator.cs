using System.IO;
using System.Runtime.CompilerServices;

using MediaFinder_v2.DataAccessLayer.Models;

using SevenZipExtractor;

namespace MediaFinder_v2.Services
{
    public class MediaLocator
    {
        private static readonly string[] ImageExtensions = new[] { ".bmp", ".jpg", ".jpeg", ".jfif", ".png", ".tif", ".tiff", ".gif", ".svg" };
        private static readonly string[] VideoExtensions = new[] { "webm", "mkv", "flv", "vob", "ogv", "ogg", "rrc", "gifv", "mng", "mov", "avi", "qt", "wmv", "yuv", "rm", "asf", "amv", "mp4", "m4p", "m4v", "mpg", "mp2", "mpeg", "mpe", "mpv", "m4v", "svi", "3gp", "3g2", "mxf", "roq", "nsv", "flv", "f4v", "f4p", "f4a", "f4b", "mod" };

        public static async IAsyncEnumerable<FileDetails> Search(IEnumerable<string> directoryPaths,
            string pattern = "*",
            bool recursive = false,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var dir in directoryPaths.Where(Directory.Exists))
            {
                await foreach(var file in Search(dir, pattern, recursive, cancellationToken))
                {
                    yield return file;
                }
            }
        }

        public static async IAsyncEnumerable<FileDetails> Search(string directoryPath,
            string pattern = "*",
            bool recursive = false,
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
                    FileType = IsArchive(file) ? MultiMediaType.Archive
                        : ImageExtensions.Contains(fileInfo.Extension) ? MultiMediaType.Image
                        : VideoExtensions.Contains(fileInfo.Extension) ? MultiMediaType.Video
                        : MultiMediaType.Unknown,
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

        private static bool IsArchive(string filename)
        {
            bool result = false;
            try
            {
                using var fileStream = File.Open(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                using var archive = new ArchiveFile(fileStream);
                result = true;
            }
            catch
            {
                // do nothing
            }
            return result;
        }
    }
}
