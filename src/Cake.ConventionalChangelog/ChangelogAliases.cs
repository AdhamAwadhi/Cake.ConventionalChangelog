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
            GitChangelog(context, new ChangelogOptions() { Version = version });
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
        ///     var options = new ChangelogOptions() {
        ///         Version = "v0.1",
        ///         File = "changelog.md",
        ///         Subtitle = "my subtitle",
        ///         Grep = ".*",
        ///         From = "MY_TAG"       
        ///     };
        ///     
        ///    GitChangelog(options);
        /// });
        /// </code>
        /// </example>
        /// <param name="context">The context.</param>
        /// <param name="options">changelog options.</param>
        [CakeMethodAlias]
        public static void GitChangelog(this ICakeContext context, ChangelogOptions options)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            var gen = new Changelog(context.FileSystem, context.Environment);
            gen.Generate(options);
        }
    }
}
