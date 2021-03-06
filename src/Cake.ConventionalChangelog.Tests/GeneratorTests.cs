﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Cake.ConventionalChangelog;
using LibGit2Sharp;
using System.IO;
using System.Text.RegularExpressions;

namespace Cake.ConventionalChangelog.Tests
{
    [TestFixture]
    public class GeneratorTests
    {
        Signature author;
        Signature committer;
        Repository repo;
        string readmePath;

        private string TestRepoChangelogPath
        {
            get
            {
                return Util.GetFullPath(Util.TEST_REPO_DIR, "CHANGELOG.md");
            }
        }

        [SetUp]
        public void Setup()
        {
            author = Util.InitSignature();
            committer = author;

            repo = Util.InitTestRepo();
            readmePath = Util.GetFullPath(Util.TEST_REPO_DIR, "README.md");
        }

        [TearDown]
        public void Cleanup()
        {
            Util.CleanupRepos();

            // NOTE: this happens with the above command as the directory is removed
            // Cleanup changelog from local dir
            //fileSystem.File.Delete(TestRepoChangelogPath);
        }

        [Test]
        public void FullLineByLineTest()
        {
            // Set up the repo
            File.AppendAllText(readmePath, "\nThis is for a fix commit");
            repo.Index.Add("README.md");
            repo.Commit("feat(Foo): Adding foo feature\n\nFixes #123, #245\nFixed #8000\n\nBREAKING CHANGE: Breaks Mr. Guy!", author, committer);

            // Set up the repo
            File.AppendAllText(readmePath, "\nThis is for a fix commit");
            repo.Index.Add("README.md");
            repo.Commit("fix(Bar): Fixed something in Bar\n\nFixes #200\n\nBREAKING CHANGE: I broke it", author, committer);

            File.AppendAllText(readmePath, "\nThis is for another commit, which should not show up");
            repo.Index.Add("README.md");
            repo.Commit("chore(Bar): Did a a chore", author, committer);

            File.AppendAllText(readmePath, "\nThis is the final commit which should go with the first one");
            repo.Index.Add("README.md");
            repo.Commit("feat(Foo): Extended Foo", author, committer);

            var changelog = new Changelog();

            changelog.Generate(new ChangelogOptions()
            {
                Version = "1.0.1",
                WorkingDirectory = Util.GetFullPath(Util.TEST_REPO_DIR)
            });

            var text = File.ReadAllText(TestRepoChangelogPath);

            var lines = text.Split('\n');

            /* 
                0  <a name="1.0.1"></a> 
                1  ### 1.0.1 (2019-11-15) 
                2   
                3   
                4  #### Bug Fixes 
                5   
                6  * **Bar:** Fixed something in Bar ((7059c009), closes (#200)) 
                7   
                8   
                9  #### Features 
               10   
               11  * **Foo:** 
               12    * Extended Foo ((0e4414b9)) 
               13    * Adding foo feature ((0b3ede82), closes (#123), (#245), (#8000)) 
               14   
               15   
               16  #### Breaking Changes 
               17   
               18  * **Bar:** due to 7059c009, I broke it ((7059c009)) 
               19  * **Foo:** due to 0b3ede82, Breaks Mr. Guy! ((0b3ede82)) 
               20   
               21   
            */

            Assert.True(lines[0].Contains("1.0.1"));
            Assert.True(lines[1].StartsWith("### 1.0.1"));
            Assert.True(lines[4].StartsWith("#### Bug Fixes"));
            Assert.True(lines[6].StartsWith("* **Bar:** Fixed something in Bar"));
            Assert.True(lines[6].EndsWith("closes (#200))"));
            Assert.True(lines[9].StartsWith("#### Features"));
            Assert.True(lines[11].StartsWith("* **Foo:**"));
            Assert.True(lines[12].StartsWith("  * Extended Foo"));
            Assert.True(Regex.Match(lines[12], @"\(\w{8}\)").Success);
            Assert.True(lines[13].StartsWith("  * Adding foo feature"));
            Assert.True(lines[16].StartsWith("#### Breaking Changes"));
            Assert.True(lines[18].StartsWith("* **Bar:** due to"));
            Assert.True(lines[18].Contains("I broke it"));
            Assert.True(lines[19].StartsWith("* **Foo:** due to"));
            Assert.True(lines[19].Contains("Breaks Mr. Guy!"));

            // TODO: Add tests for breaking changes once their formatting is fixed
        }

        [Test]
        public void FullLineByLineGrepFullTest()
        {
            // Set up the repo
            File.AppendAllText(readmePath, "\nThis is for a fix commit");
            repo.Index.Add("README.md");
            repo.Commit("feat(Foo): Adding foo feature\n\nFixes #123, #245\nFixed #8000\n\nBREAKING CHANGE: Breaks Mr. Guy!", author, committer);

            // Set up the repo
            File.AppendAllText(readmePath, "\nThis is for a fix commit");
            repo.Index.Add("README.md");
            repo.Commit("fix(Bar): Fixed something in Bar\n\nFixes #200\n\nBREAKING CHANGE: I broke it", author, committer);

            File.AppendAllText(readmePath, "\nThis is for another commit, which should not show up if Grep does not contain");
            repo.Index.Add("README.md");
            repo.Commit("chore(Bar): Did a a chore\n\nmessage under chore", author, committer);

            File.AppendAllText(readmePath, "\nThis is for normal commit, with normal message");
            repo.Index.Add("README.md");
            repo.Commit("normal message which is Conventional", author, committer);

            File.AppendAllText(readmePath, "\nThis is the final commit which should go with the first one");
            repo.Index.Add("README.md");
            repo.Commit("feat(Foo): Extended Foo", author, committer);

            var changelog = new Changelog();

            changelog.Generate(new ChangelogOptions()
            {
                Version = "1.0.1",
                WorkingDirectory = Util.GetFullPath(Util.TEST_REPO_DIR),
                Grep = ".*"
            });

            var text = File.ReadAllText(TestRepoChangelogPath);

            var lines = text.Split('\n');

            /* 
                0  <a name="1.0.1"></a> 
                1  ### 1.0.1 (2019-11-15) 
                2   
                3   
                4  #### Bug Fixes 
                5   
                6  * **Bar:** Fixed something in Bar ((8cea0660), closes (#200)) 
                7   
                8   
                9  #### Features 
               10   
               11  * **Foo:** 
               12    * Extended Foo ((d7f45532)) 
               13    * Adding foo feature ((182a091a), closes (#123), (#245), (#8000)) 
               14   
               15   
               16  #### Breaking Changes 
               17   
               18  * **Bar:** due to 8cea0660, I broke it ((8cea0660)) 
               19  * **Foo:** due to 182a091a, Breaks Mr. Guy! ((182a091a)) 
               20   
               21   
               22  #### Others 
               23   
               24  * **Bar:** Did a a chore ((a6ab8da4)) 
               25   
*/

            Assert.True(lines[0].Contains("1.0.1"));
            Assert.True(lines[1].StartsWith("### 1.0.1"));
            Assert.True(lines[4].StartsWith("#### Bug Fixes"));
            Assert.True(lines[6].StartsWith("* **Bar:** Fixed something in Bar"));
            Assert.True(lines[6].EndsWith("closes (#200))"));
            Assert.True(lines[9].StartsWith("#### Features"));
            Assert.True(lines[11].StartsWith("* **Foo:**"));
            Assert.True(lines[12].StartsWith("  * Extended Foo"));
            Assert.True(Regex.Match(lines[12], @"\(\w{8}\)").Success);
            Assert.True(lines[13].StartsWith("  * Adding foo feature"));
            Assert.True(lines[16].StartsWith("#### Breaking Changes"));
            Assert.True(lines[18].StartsWith("* **Bar:** due to"));
            Assert.True(lines[18].Contains("I broke it"));
            Assert.True(lines[19].StartsWith("* **Foo:** due to"));
            Assert.True(lines[19].Contains("Breaks Mr. Guy!"));
            Assert.True(lines[24].StartsWith("* **Bar:** Did a a chore"));

            // TODO: Add tests for breaking changes once their formatting is fixed
        }

        [Test]
        public void FullLineByLineGrepFullTestWithNormalMessages()
        {
            // Set up the repo
            File.AppendAllText(readmePath, "\nThis is for a fix commit");
            repo.Index.Add("README.md");
            repo.Commit("feat(Foo): Adding foo feature\n\nFixes #123, #245\nFixed #8000\n\nBREAKING CHANGE: Breaks Mr. Guy!", author, committer);

            // Set up the repo
            File.AppendAllText(readmePath, "\nThis is for a fix commit");
            repo.Index.Add("README.md");
            repo.Commit("fix(Bar): Fixed something in Bar\n\nFixes #200\n\nBREAKING CHANGE: I broke it", author, committer);

            File.AppendAllText(readmePath, "\nThis is for another commit, which should not show up if Grep does not contain");
            repo.Index.Add("README.md");
            repo.Commit("chore(Bar): Did a a chore\n\nmessage under chore", author, committer);

            File.AppendAllText(readmePath, "\nThis is for normal commit, with normal message");
            repo.Index.Add("README.md");
            repo.Commit("normal message which is Conventional", author, committer);

            File.AppendAllText(readmePath, "\nThis is the final commit which should go with the first one");
            repo.Index.Add("README.md");
            repo.Commit("feat(Foo): Extended Foo", author, committer);

            var changelog = new Changelog();

            changelog.Generate(new ChangelogOptions()
            {
                Version = "1.0.1",
                WorkingDirectory = Util.GetFullPath(Util.TEST_REPO_DIR),
                Grep = ".*",
                WriteNormalMessages = true
            });

            var text = File.ReadAllText(TestRepoChangelogPath);

            var lines = text.Split('\n');

            /* 
                0  <a name="1.0.1"></a> 
                1  ### 1.0.1 (2019-11-15) 
                2   
                3   
                4  #### Bug Fixes 
                5   
                6  * **Bar:** Fixed something in Bar ((ebc009c0), closes (#200)) 
                7   
                8   
                9  #### Features 
               10   
               11  * **Foo:** 
               12    * Extended Foo ((e12344df)) 
               13    * Adding foo feature ((b454a440), closes (#123), (#245), (#8000)) 
               14   
               15   
               16  #### Breaking Changes 
               17   
               18  * **Bar:** due to ebc009c0, I broke it ((ebc009c0)) 
               19  * **Foo:** due to b454a440, Breaks Mr. Guy! ((b454a440)) 
               20   
               21   
               22  #### Others 
               23   
               24  * normal message which is Conventional ((8bc93dfa)) 
               25  * **Bar:** Did a a chore ((6cea2e9b))                 
            */

            Assert.True(lines[0].Contains("1.0.1"));
            Assert.True(lines[1].StartsWith("### 1.0.1"));
            Assert.True(lines[4].StartsWith("#### Bug Fixes"));
            Assert.True(lines[6].StartsWith("* **Bar:** Fixed something in Bar"));
            Assert.True(lines[6].EndsWith("closes (#200))"));
            Assert.True(lines[9].StartsWith("#### Features"));
            Assert.True(lines[11].StartsWith("* **Foo:**"));
            Assert.True(lines[12].StartsWith("  * Extended Foo"));
            Assert.True(Regex.Match(lines[12], @"\(\w{8}\)").Success);
            Assert.True(lines[13].StartsWith("  * Adding foo feature"));
            Assert.True(lines[16].StartsWith("#### Breaking Changes"));
            Assert.True(lines[18].StartsWith("* **Bar:** due to"));
            Assert.True(lines[18].Contains("I broke it"));
            Assert.True(lines[19].StartsWith("* **Foo:** due to"));
            Assert.True(lines[19].Contains("Breaks Mr. Guy!"));
            Assert.True(lines[24].StartsWith("* normal message which is Conventional"));
            Assert.True(lines[25].StartsWith("* **Bar:** Did a a chore"));

            // TODO: Add tests for breaking changes once their formatting is fixed
        }

        [Test]
        public void FullLineByLineGrepFullTestWithTag()
        {
            // Set up the repo
            File.AppendAllText(readmePath, "\nThis is for a fix commit");
            repo.Index.Add("README.md");
            repo.Commit("feat(Foo): Adding foo feature\n\nFixes #123, #245\nFixed #8000\n\nBREAKING CHANGE: Breaks Mr. Guy!", author, committer);

            // Set up the repo
            File.AppendAllText(readmePath, "\nThis is for a fix commit");
            repo.Index.Add("README.md");
            repo.Commit("fix(Bar): Fixed something in Bar\n\nFixes #200\n\nBREAKING CHANGE: I broke it", author, committer);

            File.AppendAllText(readmePath, "\nThis is for another commit, which should not show up if Grep does not contain");
            repo.Index.Add("README.md");
            repo.Commit("chore(Bar): Did a a chore\n\nmessage under chore", author, committer);

            repo.Tags.Add("v1.0.0", repo.Head.Tip);

            File.AppendAllText(readmePath, "\nThis is for normal commit, with normal message");
            repo.Index.Add("README.md");
            repo.Commit("normal message which is Conventional", author, committer);

            File.AppendAllText(readmePath, "\nThis is the final commit which should go with the first one");
            repo.Index.Add("README.md");
            repo.Commit("feat(Foo): Extended Foo", author, committer);

            var changelog = new Changelog();

            changelog.Generate(new ChangelogOptions()
            {
                Version = "1.0.1",
                WorkingDirectory = Util.GetFullPath(Util.TEST_REPO_DIR),
                Grep = ".*"
            });

            var text = File.ReadAllText(TestRepoChangelogPath);

            var lines = text.Split('\n');

            /* 
                0  <a name="1.0.1"></a>
                1  ### 1.0.1 (2019-11-15)
                2  
                3  
                4  #### Features
                5  
                6  * **Foo:** Extended Foo ((be8d7e54))
                7  
                8  
                9  
            */

            Assert.True(lines[0].Contains("1.0.1"));
            Assert.True(lines[1].StartsWith("### 1.0.1"));
            Assert.True(lines[4].StartsWith("#### Features"));
            Assert.True(lines[6].StartsWith("* **Foo:** Extended Foo"));
            Assert.True(Regex.Match(lines[6], @"\(\w{8}\)").Success);

            // TODO: Add tests for breaking changes once their formatting is fixed
        }

        [Ignore("Cannot pass only the version number when testing as the repo directory under test mode is not the current directory")]
        [Test]
        public void PassingOnlyVersionStringWorks()
        {
            var changelog = new Changelog();

            changelog.Generate("1.0.1");

            var text = File.ReadAllText("CHANGELOG.md");

            Assert.False(String.IsNullOrEmpty(text));
            Assert.True(text.Contains("1.0.1"));

            // Cleanup changelog from local dir
            File.Delete("CHANGELOG.md");
        }

        [Test]
        public void MustPassVersionParam()
        {
            var ex = Assert.Throws<Exception>(() =>
            {
                var changelog = new Changelog();
                changelog.Generate("");
            });

            Assert.AreEqual("No version specified", ex.Message);
        }

        [Test]
        public void BasiChangelogConstructorWorks()
        {
            Assert.DoesNotThrow(() =>
            {
                var c = new Changelog();
            });
        }

        [Test]
        public void GeneratorOnEmptyRepoFails()
        {
            Util.InitEmptyRepo();

            var changelog = new Changelog();

            GitException ex = Assert.Throws<GitException>(() =>
            {
                changelog.Generate(new ChangelogOptions()
                {
                    Version = "1.0.0",
                    WorkingDirectory = Util.GetFullPath(Util.EMPTY_REPO_DIR)
                });
            });

            StringAssert.Contains("Failed to read git tags", ex.Message);
            StringAssert.Contains("No commits found", ex.Message);
        }

        [Test]
        public void NonFixOrFeatTypeIsNotCaptured()
        {
            // Set up the repo
            File.AppendAllText(readmePath, "\nThis is for a fix commit");
            repo.Index.Add("README.md");
            repo.Commit("feat(Foo): Foo feature", author, committer);

            File.AppendAllText(readmePath, "\nThis is for a fix commit");
            repo.Index.Add("README.md");
            repo.Commit("chore(Foo): Foo chore", author, committer);

            var changelog = new Changelog();
            changelog.Generate(new ChangelogOptions()
            {
                Version = "1.0.1",
                WorkingDirectory = Util.GetFullPath(Util.TEST_REPO_DIR)
            });

            var text = File.ReadAllText(TestRepoChangelogPath);

            Assert.True(text.Contains("Foo feature"));
            Assert.False(text.Contains("Foo chore"));
        }

        // Make sure the changelog generator doesn't remove previous content and only prepends
        [Test]
        public void PrependsToChangelog()
        {
            // Set up the repo
            File.AppendAllText(readmePath, "\nThis is for a fix commit");
            repo.Index.Add("README.md");
            repo.Commit("feat(Foo): Foo feature", author, committer);

            var changelog = new Changelog();

            File.WriteAllText(TestRepoChangelogPath, "This is previous stuff");

            changelog.Generate(new ChangelogOptions()
            {
                Version = "1.0.1",
                WorkingDirectory = Util.GetFullPath(Util.TEST_REPO_DIR)
            });

            var text = File.ReadAllText(TestRepoChangelogPath);

            Assert.True(Regex.Match(text, @"1.0.1[\s\S]+?This is previous stuff", RegexOptions.IgnoreCase | RegexOptions.Multiline).Success);
        }

        // Make sure the changelog generator replace current version changes on multiple calls
        //[Test]
        [TestCase(true, 5)]
        [TestCase(false, 3)]
        public void AlteredCurrentVersionChangelog(bool prepends, int length)
        {
            // Set up the repo
            File.AppendAllText(readmePath, "\nThis is for a fix commit");
            repo.Index.Add("README.md");
            repo.Commit("feat(Foo): Foo feature", author, committer);

            var changelog = new Changelog();

            File.WriteAllText(TestRepoChangelogPath, "This is previous stuff");
            var options = new ChangelogOptions()
            {
                Version = "1.0.0",
                WorkingDirectory = Util.GetFullPath(Util.TEST_REPO_DIR),
                AlwaysPrepends = prepends
            };

            changelog.Generate(options);

            File.AppendAllText(readmePath, "\nsecond commit");
            repo.Index.Add("README.md");
            repo.Commit("fix(Foo): second commit", author, committer);

            options.Version = "1.0.1";
            changelog.Generate(options);

            File.AppendAllText(readmePath, "\n3rd commit");
            repo.Index.Add("README.md");
            repo.Commit("fix(Foo): 3rd commit commit", author, committer);

            options.Version = "1.0.1";
            changelog.Generate(options);

            var text = File.ReadAllText(TestRepoChangelogPath);

            var r = text.Split(new[] { "1.0.1" }, StringSplitOptions.None);
            Assert.AreEqual(r.Length, length);
        }
    }
}
