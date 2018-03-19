using Cake.ConventionalChangelog;
using NUnit.Framework;
using Should;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestFixture]
    public class ChangelogAliasesTests
    {
        [Test]
        public void Should_Throw_If_Context_Is_Null()
        {        
            // When
            var result = Assert.Catch(() => ChangelogAliases.GitChangelog(null, "v1"));

            // Then
            result.ShouldBeType<ArgumentNullException>().ParamName.ShouldEqual("context");
        }
    }
}
