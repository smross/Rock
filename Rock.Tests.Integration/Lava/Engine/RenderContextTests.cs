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

namespace Rock.Tests.Integration.Lava
{
    /// <summary>
    /// Test the scoping of variables in a Lava context using various container configurations
    /// </summary>
    [TestClass]
    public class RenderContextTests : LavaIntegrationTestBase
    {
        // TODO: This is the observed behavior, but is it correct?
        [TestMethod]
        public void RenderContext_BinaryFileObjectRoundtrip_IsSuccessful()
        {
            var input = @"
{% execute type:'class' %}
    using Rock;
    using Rock.Data;
    using Rock.Model;
    
    public class MyScript 
    {
        public string Execute() {
            using(RockContext rockContext = new RockContext()){
                var person = new PersonService(rockContext).Get({{ CurrentPerson | Property: 'Id' }});
                
                return person.FullName;
            }
        }
    }
{% endexecute %}
";
            var expectedOutput = @"Admin Admin"; // NOT 'Ted Decker'

            var context = TestHelper.LavaEngine.NewRenderContext();

            context.SetEnabledCommands( "execute" );

            context.SetMergeField( "CurrentPerson", TestHelper.GetTestPersonTedDecker() );

            TestHelper.AssertTemplateOutput( expectedOutput, input, context );
        }
    }
}
