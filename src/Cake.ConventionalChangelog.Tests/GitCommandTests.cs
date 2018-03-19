using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Cake.ConventionalChangelog;
using LibGit2Sharp;
using System.IO;
using Cake.Core.IO;
using Cake.Testing;
using Cake.Core;

namespace Tests
{
    [TestFixture]
    public class GitCommandTests
    {
        IFileSystem fileSystem;
        ICakeEnvironment environment;

        private Git git;
        private Repository repo;

        [SetUp]
        public void Setup()
        {
            environment = FakeEnvironment.CreateWindowsEnvironment();
            fileSystem = new FakeFileSystem(environment);
            repo = Util.InitTestRepo();

            var dir = ((DirectoryPath)Util.TEST_REPO_DIR).MakeAbsolute(environment).FullPath;
            git = new Git(dir);
        }

        [TearDown]
        public void TearDown()
        {
            Util.CleanupRepos();
        }

        [Test]
        public void BadCommandThrows()
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject obj = new Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject(git);

            GitException ex = Assert.Throws<GitException>(() =>
            {
                obj.Invoke("GitCommand", new object[] { "boogers" });
            });

            Assert.True(ex.Message.ToLower().Contains("not a git command"));
        }
    }
}
