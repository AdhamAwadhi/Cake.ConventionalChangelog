using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cake.ConventionalChangelog
{
    /// <summary>
    /// Contains functionality related to running git Changelog.
    /// </summary>
    [CakeAliasCategory("Changelog")]
    public static class ChangelogAliases
    {
        /// <summary>
        /// Generate changelog file.
        /// </summary>
        /// <example>
        /// <code>
        /// #tool "nuget:?package=Cake.ConventionalChangelog"
        /// 
        /// Task("Generate-Changelog")
        ///  .Does(() => {
        ///    GitChangelog("v0.1");
        /// });
        /// </code>
        /// </example>
        /// <param name="context">The context.</param>
        /// <param name="version">Current version to generate changelog for.</param>
        [CakeMethodAlias]
        public static void GitChangelog(this ICakeContext context, string version)
        {
            GitChangelog(context, new ChangeLogSettings() { Version = version });
        }

        /// <summary>
        /// Generate changelog file.
        /// </summary>
        /// <example>
        /// <code>
        /// #tool "nuget:?package=Cake.ConventionalChangelog"
        /// 
        /// Task("Generate-Changelog")
        ///  .Does(() => {
        ///     var settings = new ChangeLogSettings() {
        ///         Version = "v0.1",
        ///         File = "changelog.md",
        ///         Subtitle = "my subtitle",
        ///         Grep = ".*",
        ///         From = "MY_TAG"       
        ///     };
        ///     
        ///    GitChangelog(settings);
        /// });
        /// </code>
        /// </example>
        /// <param name="context">The context.</param>
        /// <param name="settings">changelog settings.</param>
        /// <returns>Generated change log </returns>
        [CakeMethodAlias]
        public static string GitChangelog(this ICakeContext context, ChangeLogSettings settings)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (settings.WorkingDirectory.IsRelative)
                settings.WorkingDirectory = settings.WorkingDirectory.MakeAbsolute(context.Environment);
            if (settings.File.IsRelative)
                settings.File = settings.File.MakeAbsolute(settings.WorkingDirectory);


            var options = new ChangelogOptions
            {
                Version = settings.Version,
                From = settings.From,
                To = settings.To,
                Grep = settings.Grep,
                Subtitle = settings.Subtitle,
                WorkingDirectory = settings.WorkingDirectory.FullPath,
                File = settings.File.FullPath,
                AlwaysPrepends = settings.AlwaysPrepends,
                WriteOthers = settings.WriteOthers,
                WriteNormalMessages = settings.WriteNormalMessages
            };

            var gen = new Changelog();
            return gen.Generate(options);
        }
    }
}
