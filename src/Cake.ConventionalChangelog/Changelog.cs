using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Cake.Core.IO;
using Cake.Core;
using Cake.Core.Annotations;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

namespace Cake.ConventionalChangelog
{
    public class Changelog
    {
        private readonly IFileSystem _fileSystem;
        private readonly ICakeEnvironment _environment;

        /// <summary>
        /// Initializes a new instance of the <see cref="Changelog"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="environment">The environment.</param>
        public Changelog(IFileSystem fileSystem, ICakeEnvironment environment)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException("fileSystem");
            }
            if (environment == null)
            {
                throw new ArgumentNullException("environment");
            }
            _fileSystem = fileSystem;
            _environment = environment;
        }

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

            if (options.WorkingDirectory.IsRelative)
            {
                options.WorkingDirectory = options.WorkingDirectory.MakeAbsolute(_environment);
            }

            if (options.File.IsRelative)
            {
                options.File = options.File.MakeAbsolute(options.WorkingDirectory);
            }

            var git = new Git(options.WorkingDirectory.FullPath);

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


            var git = new Git(options.WorkingDirectory.FullPath);
            var commits = git.GetCommits(grep: options.Grep, from: from, to: options.To ?? "HEAD");

            WriteLog(commits, options);
        }

        private void WriteLog(List<CommitMessage> commits, ChangelogOptions options)
        {
            Writer writer = new Writer();
            string changelog = writer.WriteLog(commits, new WriterOptions()
            {
                Version = options.Version,
                Subtitle = options.Subtitle
            });

            string filePath = options.File.FullPath;// fileSystem.Path.Combine(options.WorkingDirectory, options.File);
            var file = _fileSystem.GetFile(options.File);

            var fullPath = file.Path.FullPath;
            string currentlog = "";
            if (File.Exists(fullPath))
            {
                currentlog = File.ReadAllText(fullPath, Encoding.UTF8);
            }

            File.WriteAllText(fullPath, changelog + "\n" + currentlog, Encoding.UTF8);
            //using (var ws = new StreamWriter(file.OpenWrite(), Encoding.UTF8))
            //    ws.WriteLine(string.Join("\n", new[] { changelog }.Concat(currentlog).ToArray()));
        }
    }

}
