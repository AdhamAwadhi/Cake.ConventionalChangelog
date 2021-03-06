﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cake.ConventionalChangelog
{
    public class Git
    {
        private static readonly string COMMIT_PATTERN = @"^(\w*)(\(([\w\$\.\-\* ]*)\))?\: (.*)$";
        private static readonly int MAX_SUBJECT_LENGTH = 80;
        private static readonly string BREAK_MSG = "**{0}:** due to {1}, {2}";

        private string _repoDir = ".";

        public Git() { }
        public Git(string RepositoryDir)
        {
            _repoDir = RepositoryDir;
        }

        private string GitCommand(string Command)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("git.exe");

            startInfo.Arguments = Command;
            startInfo.CreateNoWindow = true;
            //startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.WorkingDirectory = _repoDir;
            startInfo.UseShellExecute = false;

            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new GitException(String.Format("Error running git commit: {0}", process.StandardError.ReadToEnd()));
            }

            return output;
        }

        public string LatestTag()
        {
            // Get commit hash for latest tag
            string hash;
            try
            {
                hash = GitCommand(@"rev-list --tags --max-count=1").Trim();
            }
            catch (GitException)
            {
                return GetFirstCommit();
            }

            try
            {
                string cmd = String.Format("describe --tags {0}", hash);

                var ret = GitCommand(cmd).Trim();
                return ret;
            }
            catch (GitException)
            {
                return GetFirstCommit();
            }
        }

        public string GetFirstCommit()
        {
            string ret;
            try
            {
                ret = GitCommand("log --format=\"%H\" --pretty=oneline --reverse").Trim();
            }
            catch (GitException ex)
            {
                if (ex.Message.ToLower().Contains("bad default revision 'head'")
                    || ex.Message.ToLower().Contains("does not have any commits yet"))
                {
                    throw new GitException("No commits found", ex);
                }
                else
                {
                    throw ex;
                }
            }

            if (String.IsNullOrEmpty(ret))
            {
                throw new GitException("No commits found");
            }
            else
            {
                return "";
            }
        }

        public List<CommitMessage> GetCommits(string grep = @"^feat|^fix|BREAKING", string format = @"%H%n%s%n%b%n==END==",
            string from = "", string to = "HEAD",
            bool parseNormalMessages = false,
            bool invertGrep = false)
        {
            string cmd = String.Format(@"log --grep=""{0}"" -E --format={1} {2} {3}",
                grep,
                format,
                !String.IsNullOrEmpty(from) ? '"' + from + ".." + to + '"' : "",
                invertGrep ? "--invert-grep" : ""
            );

            var ret = GitCommand(cmd);

            var lines = Regex.Split(ret, @"\n==END==\n").ToList();

            List<CommitMessage> commits = new List<CommitMessage>();

            foreach (var line in lines)
            {
                var commit = ParseRawCommit(line, parseNormalMessages);
                if (commit != null)
                {
                    commits.Add(commit);
                }
            }

            return commits;
        }

        public CommitMessage ParseRawCommit(string raw, bool parseNormalMessages)
        {
            if (String.IsNullOrEmpty(raw)) return null;

            var lines = raw.Split('\n').ToList();
            var msg = new CommitMessage();

            msg.Hash = lines.First(); lines.RemoveAt(0);
            msg.Subject = lines.First(); lines.RemoveAt(0);

            Regex closesRE = new Regex(@"\s*(?:Clos(?:es|ed)|Fix(?:es|ed)|Resolv(?:es|ed))\s#(?<issue>\d+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);

            msg.Closes = closesRE.Matches(msg.Subject)
                            .Cast<Match>()
                            .Select(x => x.Groups["issue"].Value)
                            .ToList();

            // Remove closes from subject
            msg.Subject = closesRE.Replace(msg.Subject, "");

            var lineRE = new Regex(@"(?:Clos(?:es|ed)|Fix(?:es|ed)|Resolv(?:es|ed))\s(?<issues>(?:#\d+(?:\,\s)?)+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            string issueRE = @"\d+";
            foreach (var line in lines)
            {
                lineRE.Matches(line)
                 .Cast<Match>()
                 .Select(x => x.Groups["issues"].Value)
                 .ToList()
                 .ForEach(x =>
                 {
                     x.Split(',').Select(i => i.Trim())
                         .ToList()
                         .ForEach(i =>
                         {
                             var issue = Regex.Match(i, issueRE);
                             if (!String.IsNullOrEmpty(issue.Value)) msg.Closes.Add(issue.Value);
                         });
                 });
            }

            // Get the commit message info
            msg.Body = String.Join("\n", lines);

            var match = (new Regex(COMMIT_PATTERN)).Match(msg.Subject);
            if (!match.Success || match.Groups[1] == null || match.Groups[4] == null)
            {
                return parseNormalMessages ? msg : null;
            }

            var subject = match.Groups[4].Value;

            if (subject.Length > MAX_SUBJECT_LENGTH)
            {
                subject = subject.Substring(0, MAX_SUBJECT_LENGTH);
            }

            msg.Type = match.Groups[1].Value;
            msg.Component = match.Groups[3].Value;
            msg.Subject = subject;

            // Get the breaking changes
            var breaksRE = new Regex(@"BREAKING CHANGE:\s(?<break>[\s\S]*)");

            var breakmatch = breaksRE.Match(raw).Groups["break"].Value;
            if (!String.IsNullOrEmpty(breakmatch))
            {
                msg.Breaks.Add(String.Format(BREAK_MSG, msg.Component, msg.Hash.Substring(0, 8), breakmatch.Trim()));
            }

            //breaksRE.Matches(raw)
            //    .Cast<Match>()
            //    .Select(x => x.Groups["break"].Value)
            //    .ToList()
            //    .ForEach(x =>
            //    {
            //        if (!String.IsNullOrEmpty(x)) msg.Breaks.Add(x)
            //    });

            return msg;
        }
    }

    //public class Commit {
    //    public string type { get; set; }
    //    public string component { get; set; }
    //    public string subject { get; set; }

    //    public Commit() { }

    //    public Commit(string type, string component, string subject)
    //    {
    //        this.type = type;
    //        this.component = component;
    //        this.subject = subject;
    //    }
    //}

    public class CommitMessage
    {
        public string Hash { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Component { get; set; }
        public string Type { get; set; }
        public List<string> Closes { get; set; }
        public List<string> Breaks { get; set; }

        public CommitMessage()
        {
            Closes = new List<string>();
            Breaks = new List<string>();
        }

        public CommitMessage(string hash, string subject)
        {
            this.Hash = hash;
            this.Subject = subject;
            Closes = new List<string>();
            Breaks = new List<string>();
        }
    }
}
