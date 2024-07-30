using LibGit2Sharp;

namespace GitFileAuthorFinder
{
    class Program
    {
        static void Main(string[] args)
        {
            string repoPath = "C:\\Work\\Studio";
            string folderPath = "C:\\Work\\Studio\\Qa\\E2E";

            try
            {
                using (var repo = new Repository(repoPath))
                {
                    var filter = new CommitFilter { SortBy = CommitSortStrategies.Topological | CommitSortStrategies.Reverse };

                    var files = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories);

                    using (StreamWriter writer = new StreamWriter("FileAuthors.txt"))
                    {
                        foreach (var file in files.Where(f => f.Contains("\\Tests\\") && f.EndsWith(".cs")))
                        {
                            string relativePath = Path.GetRelativePath(repoPath, file).Replace('\\', '/');

                            foreach (var commit in repo.Commits.QueryBy(filter))
                            {
                                if (FileExistsInCommit(commit.Tree, relativePath))
                                {
                                    writer.WriteLine($"{relativePath.Split('/').Last()} - {commit.Author.Name}");
                                    Console.WriteLine($"{relativePath.Split('/').Last()} - {commit.Author.Name}");
                                    break;
                                }
                            }
                        }
                    }

                    Console.WriteLine("Output written to FileAuthors.txt");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static bool FileExistsInCommit(Tree tree, string relativePath)
        {
            var segments = relativePath.Split('/');
            TreeEntry entry = null;

            foreach (var segment in segments)
            {
                entry = tree[segment];
                if (entry == null || entry.TargetType != TreeEntryTargetType.Tree && entry.TargetType != TreeEntryTargetType.Blob)
                {
                    return false;
                }

                if (entry.TargetType == TreeEntryTargetType.Tree)
                {
                    tree = (Tree)entry.Target;
                }
            }

            return entry != null && entry.TargetType == TreeEntryTargetType.Blob;
        }
    }
}