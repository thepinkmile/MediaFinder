using MediaFinder_v2.Helpers;

namespace MediaFinder_v2.Messages
{
    public class AnalyseRequest : ReactiveBackgroundWorkerContextBase
    {
        private AnalyseRequest(
            object progressToken,
            ICollection<string> files,
            ICollection<string> originalPaths,
            string workingDirectory,
            bool performDeepAnalysis = false)
            : base(progressToken)
        {
            Files = files;
            OriginalPaths = originalPaths;
            WorkingDirectory = workingDirectory;
            PerformDeepAnalysis = performDeepAnalysis;
        }

        public ICollection<string> Files { get; }

        public ICollection<string> OriginalPaths { get; }

        public string WorkingDirectory { get; }

        public bool PerformDeepAnalysis { get; }

        public static AnalyseRequest Create(object progressToken, ICollection<string> files,
            ICollection<string> originalPaths, string workingDirectory, bool performDeepAnalysis = false)
            => new(progressToken, files, originalPaths, workingDirectory, performDeepAnalysis);
    }
}
