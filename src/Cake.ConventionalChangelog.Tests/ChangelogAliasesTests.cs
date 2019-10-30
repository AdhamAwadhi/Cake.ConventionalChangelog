using Cake.ConventionalChangelog;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cake.ConventionalChangelog.Tests
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
            result.ShouldBeOfType<ArgumentNullException>().ParamName.ShouldBe("context");
        }
    }
}
