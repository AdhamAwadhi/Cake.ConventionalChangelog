using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cake.ConventionalChangelog;
using NUnit.Framework;
using Cake.Core.IO;
using Cake.Testing;
using Cake.Core;

namespace Tests
{
    [TestFixture]
    public class EmptyRepoTests
    {
        IFileSystem fileSystem;
        ICakeEnvironment environment;

        private Git git;

        [SetUp]
        public void Setup()
        {
            environment = FakeEnvironment.CreateWindowsEnvironment();
            fileSystem = new FakeFileSystem(environment);

            Util.InitEmptyRepo();
            var dir = ((DirectoryPath)Util.EMPTY_REPO_DIR).MakeAbsolute(environment).FullPath;
            git = new Git(dir);
        }

        [TearDown]
        public void TearDown()
        {
            Util.CleanupRepos();
        }

        [Test]
        public void GetFirstCommit_NoCommitsThrowsError()
        {
            GitException ex = Assert.Throws<GitException>(() =>
            {
                git.GetFirstCommit();
            });

            Assert.True(ex.Message.ToLower().Contains("no commits found"));
        }

        [Test]
        public void LatestTag_WithNoCommitsThrows()
        {
            Assert.Throws<GitException>(() =>
            {
                git.LatestTag();
            });
        }
    }
}
