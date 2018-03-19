using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;
using NUnit.Framework;

namespace Cake.ConventionalChangelog.Tests
{
    public class Util
    {
        public static string CurrentDirectory { get { return TestContext.CurrentContext.TestDirectory; } }

        public static string TEST_REPO_DIR { get { return "test_repo"; } }

        public static string EMPTY_REPO_DIR { get { return "empty_repo"; } }

        public static void InitUser()
        {

        }

        public static void InitAll()
        {
            CleanupRepos();

            InitTestRepo();
            InitEmptyRepo();
        }

        public static Repository InitTestRepo()
        {
            Repository repo = CleanupAndInitRepo(TEST_REPO_DIR);

            repo.Config.Set("user.name", "test", ConfigurationLevel.Local);
            repo.Config.Set("user.email", "blah@blah.bl", ConfigurationLevel.Local);

            return repo;
        }

        public static void TagTestRepo()
        {
            Repository repo = new Repository(TEST_REPO_DIR);

            repo.Tags.Add("v0.0.0", repo.Head.Tip);
        }

        public static void CleanupRepos()
        {
            (new[] { TEST_REPO_DIR, EMPTY_REPO_DIR }).Select(x => GetFullPath(x))
                .ToList()
                .ForEach(x => CleanupRepo(x));
        }

        public static void CleanupRepo(string path)
        {
            if (Directory.Exists(path))
            {
                var directory = new DirectoryInfo(path) { Attributes = FileAttributes.Normal };

                foreach (var info in directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
                {
                    info.Attributes = FileAttributes.Normal;
                }

                try
                {
                    directory.Delete(true);
                }
                catch (Exception) { }
            }
        }

        public static Repository InitEmptyRepo()
        {
            Repository repo = CleanupAndInitRepo(EMPTY_REPO_DIR);

            repo.Config.Set("user.name", "test", ConfigurationLevel.Local);
            repo.Config.Set("user.email", "blah@blah.bl", ConfigurationLevel.Local);

            return repo;
        }

        private static Repository CleanupAndInitRepo(string path)
        {
            path = GetFullPath(path);

            if (Directory.Exists(path))
            {
                CleanupRepo(path);
            }

            Directory.CreateDirectory(path);
            Repository.Init(path);

            return new Repository(path);
        }

        public static string GetFullPath(params string[] paths)
        {
            return Path.Combine(new[] { CurrentDirectory }.Concat(paths).ToArray());
        }
    }
}
