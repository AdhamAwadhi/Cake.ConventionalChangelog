using Cake.Core.IO;
using Cake.Core.Tooling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cake.ConventionalChangelog
{
    /// <summary>
    /// Changelog settings used by <see cref="Changelog"/>.
    /// </summary>
    public class ChangeLogSettings
    {
        /// <summary>
        ///     Gets or sets the version.
        /// </summary>
        /// <value>
        ///     Version to use in change log file.
        /// </value>
        public string Version { get; set; }

        /// <summary>
        /// First boundary of revision range, Show only commits in the specified revision range.
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Last boundary of revision range, Show only commits in the specified revision range.
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// File path which will be generated 
        /// </summary>
        public FilePath File { get; set; }

        /// <summary>
        ///  Gets or sets the working directory 
        /// </summary>
        public DirectoryPath WorkingDirectory { get; set; }

        /// <summary>
        /// Header subtitle
        /// </summary>
        public string Subtitle { get; set; }

        /// <summary>
        /// Limit the commits output to ones with log message that matches the specified pattern (regular expression)
        /// </summary>
        public string Grep { get; set; }

        /// <summary>
        /// Always prepends change log and don't replace current version
        /// </summary>
        public bool AlwaysPrepends { get; set; }

        /// <summary>
        /// Write others section
        /// </summary>
        public bool WriteOthers { get; set; }

        /// <summary>
        /// Write normal messages that is not matched COMMIT_PATTERN = @"^(\w*)(\(([\w\$\.\-\* ]*)\))?\: (.*)$"
        /// </summary>
        public bool WriteNormalMessages { get; set; }

        /// <summary>
        /// Limit the commits output to ones with log message that do not match the pattern specified with <see cref="Grep"/>.
        /// </summary>
        public bool InvertGrep { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChangeLogSettings"/> class.
        /// </summary>
        public ChangeLogSettings()
        {
            To = "HEAD";
            File = "CHANGELOG.md";
            Subtitle = "";
            WorkingDirectory = ".";
            AlwaysPrepends = false;
            WriteOthers = true;
            Grep = @"^feat|^fix|BREAKING" + (WriteOthers ? "" : "");
            InvertGrep = false;
        }
    }
}