// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Lava;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Lava
{
    /// <summary>
    /// Test the parsing of a Lava document to a Liquid-compatible syntax.
    /// </summary>
    [TestClass]
    public class ParsingTests : LavaIntegrationTestBase
    {
        [TestMethod]
        public void Parsing_LavaTemplateWithElseIfKeyword_EmitsCorrectOutput()
        {
            var input = @"
{% assign speed = 50 %}
{% if speed > 70 -%}
Fast
{% elseif speed > 30 -%}
Moderate
{% else -%}
Slow
{% endif -%}
";
            var expectedOutput = @"Moderate";

            _helper.AssertTemplateOutput( expectedOutput, input, context:null, ignoreWhiteSpace:true );
        }

        [TestMethod]
        public void LavaToLiquidConverter_LavaTemplateWithElseIfKeyword_IsReplacedWithElsif()
        {
            var input = @"
{% assign speed = 50 %}
{% if speed > 70 -%}
Fast
{% elseif speed > 30 -%}
Moderate
{% else -%}
Slow
{% endif -%}
";
            var expectedOutput = @"
{% assign speed = 50 %}
{% if speed > 70 -%}
Fast
{% elsif speed > 30 -%}
Moderate
{% else -%}
Slow
{% endif -%}
";

            var converter = new LavaToLiquidTemplateConverter();

            var output = converter.ReplaceElseIfKeyword( input );

            Assert.That.AreEqual( expectedOutput, output );
        }
    }
}
