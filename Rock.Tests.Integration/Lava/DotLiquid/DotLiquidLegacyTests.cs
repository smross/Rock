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
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Lava
{
    public static class TestDotLiquidFilter
    {
        public static string GetVariableType( object input )
        {
            if ( input == null )
            {
                return "null";
            }
            else
            {
                var inputType = input.GetType();

                // If this is an Entity Framework proxy, get the proxied type.
                inputType = System.Data.Entity.Core.Objects.ObjectContext.GetObjectType( inputType );

                return inputType.Name;
            }
        }
    }


    [TestClass]
    public class DotLiquidLegacyTests : LavaIntegrationTestBase
    {
        #region Filter Tests: GetVariableType

        /// <summary>
        /// Registering a filter directly in the DotLiquid framework and bypassing the Lava Engine
        /// correctly unwraps Lava library types when the filter is called.
        /// </summary>
        [TestMethod]
        public void DotLiquid_FilterRegisteredToDotLiquidFramework_ProducesCorrectResultInLavaLibrary()
        {
            DotLiquid.Template.RegisterFilter( typeof( TestDotLiquidFilter ) );

            var rockContext = new RockContext();

            var contentChannelItem = new ContentChannelItemService( rockContext )
                .Queryable()
                .FirstOrDefault( x => x.ContentChannel.Name == "External Website Ads" && x.Title == "SAMPLE: Easter" );

            Assert.That.IsNotNull( contentChannelItem, "Required test data not found." );

            var values = new LavaDataDictionary { { "Item", contentChannelItem } };

            var inputTemplate = @"
{% assign image = Item | Attribute:'Image','Object' %}
Type: {{ image | GetVariableType }}<br/>
";

            var expectedOutput = @"BinaryFile";

            TestHelper.AssertTemplateOutputWithWildcard( expectedOutput, inputTemplate, values, wildCard: "{moreBase64Data}" );
        }

        #endregion
    }
}
