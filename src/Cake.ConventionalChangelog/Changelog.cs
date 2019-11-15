using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

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

        public string Generate(ChangelogOptions options)
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

            return GetChangelogCommits(tag, options);
        }

        private string GetChangelogCommits(string tag, ChangelogOptions options)
        {
            string from = (!String.IsNullOrEmpty(tag)) ? tag : options.From;


            var git = new Git(options.WorkingDirectory);
            var commits = git.GetCommits(grep: options.Grep, from: from, to: options.To ?? "HEAD",
                parseNormalMessages: options.WriteNormalMessages,
                invertGrep: options.InvertGrep);

            return WriteLog(commits, options);
        }

        private string WriteLog(List<CommitMessage> commits, ChangelogOptions options)
        {
            Writer writer = new Writer();
            string changelog = writer.WriteLog(commits, new WriterOptions()
            {
                Version = options.Version,
                Subtitle = options.Subtitle,
                WriteOthers = options.WriteOthers
            });

            string filePath = Path.IsPathRooted(options.File) ? options.File :
                        Path.Combine(options.WorkingDirectory, options.File);

            string currentlog = "";
            if (File.Exists(filePath))
            {
                currentlog = File.ReadAllText(filePath, Encoding.UTF8);
            }

            //var matches = Regex.Match(currentlog, @"<a\s+name=\""(?<version>.+)""></a>.*");
            //if (matches.Success && options.Version.Equals(matches.Groups["version"].Value, StringComparison.InvariantCultureIgnoreCase))
            if (!options.AlwaysPrepends &&
                currentlog.StartsWith(string.Format(Writer.HEADER_TPL_PRE, options.Version)))
            {
                currentlog = RemoveLastVersion(currentlog, options.Version);
            }

            File.WriteAllText(filePath, changelog + "\n" + currentlog, Encoding.UTF8);

            return changelog;
        }

        private string RemoveLastVersion(string currentlog, string version)
        {
            var first = currentlog.IndexOf("<a name=\"", 10);
            return first <= 0 ? "" : currentlog.Substring(first, currentlog.Length - first);
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
        public bool AlwaysPrepends { get; set; }
        public bool WriteOthers { get; set; }
        public bool WriteNormalMessages { get; set; }
        public bool InvertGrep { get; set; }

        public ChangelogOptions()
        {
            To = "HEAD";
            File = "CHANGELOG.md";
            Subtitle = "";
            WorkingDirectory = ".";
            Grep = @"^feat|^fix|BREAKING";
            AlwaysPrepends = false;
            WriteOthers = true;
            WriteNormalMessages = false;
        }
    }
}