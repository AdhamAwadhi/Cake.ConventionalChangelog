using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Cake.ConventionalChangelog
{
    public class Changelog
    {
        public Changelog() { }        

        public void Generate(string Version)
        {
            Generate(new ChangelogOptions()
            {
                Version = Version
            });
        }
        public void Generate(ChangelogOptions options)
        {
            if (String.IsNullOrEmpty(options.Version))
            {
                throw new Exception("No version specified");
            }

            var git = new Git(options.WorkingDirectory);

            // Get the latest tag or commit
            string tag;
            try
            {
                tag = git.LatestTag();
            }
            catch (GitException ex)
            {
                throw new GitException("Failed to read git tags: " + ex.Message, ex);
            }

            GetChangelogCommits(tag, options);
        }

        private void GetChangelogCommits(string tag, ChangelogOptions options)
        {
            string from = (!String.IsNullOrEmpty(tag)) ? tag : options.From;


            var git = new Git(options.WorkingDirectory);
            var commits = git.GetCommits(grep: options.Grep, from: from, to: options.To ?? "HEAD");

            WriteLog(commits, options);
        }

        private void WriteLog(List<CommitMessage> commits, ChangelogOptions options)
        {
            Writer writer = new Writer();
            string changelog = writer.WriteLog(commits, new WriterOptions()
            {
                Version = options.Version
            });

            string filePath = options.File;

            string currentlog = "";
            if (File.Exists(filePath))
            {
                currentlog = File.ReadAllText(filePath, Encoding.UTF8);
            }

            File.WriteAllText(filePath, changelog + "\n" + currentlog, Encoding.UTF8);
        }
    }

    public class ChangelogOptions
    {
        public string Version { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string File { get; set; }
        public string WorkingDirectory { get; set; }
        public string Subtitle { get; set; }
        public string Grep { get; set; }

        public ChangelogOptions()
        {
            To = "HEAD";
            File = "CHANGELOG.md";
            Subtitle = "";
            WorkingDirectory = ".";
            Grep = @"^feat|^fix|BREAKING";
        }
    }
}