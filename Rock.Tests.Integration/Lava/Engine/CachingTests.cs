using System.Linq;
using DotLiquid;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Lava
{
    [TestClass]
    public class LavaCachingTests : LavaIntegrationTestBase
    {
        #region Caching

        /// <summary>
        /// Verify that templates with varying amounts of whitespace are correctly cached and return the expected output.
        /// </summary>
        [TestMethod]
        public void TemplateCaching_WhitespaceTemplatesWithDifferentLengths_AreCachedIndependently()
        {
            // Process an initial whitespace template - this will be cached.
            var input0 = string.Empty;
            AssertTemplateOutput( input0, input0 );

            // Process a whitespace template of a different length - this should be cached separately from the first template.
            // If not, the caching mechanism may cause whitespace to be rendered incorrectly.
            var input1 = new string( ' ', 100 );
            AssertTemplateOutput( input1, input1 );
        }

        /// <summary>
        /// Verify that templates with varying amounts of whitespace are correctly cached and return the expected output.
        /// </summary>
        [TestMethod]
        public void ShortcodeCaching_ModifiedShortcode_ReturnsCorrectVersionAfterModification()
        {
            //LavaShortcode lavaShortcode;

            var rockContext = new RockContext();
            var lavaShortCodeService = new LavaShortcodeService( rockContext );

            // Create a new Shortcode.
            var shortcodeGuid1 = TestGuids.Shortcodes.ShortcodeTest1.AsGuid();

            var lavaShortcode = lavaShortCodeService.Queryable().FirstOrDefault( x => x.Guid == shortcodeGuid1 );

            if ( lavaShortcode == null )
            {
                lavaShortcode = new LavaShortcode();

                lavaShortCodeService.Add( lavaShortcode );
            }

            lavaShortcode.Guid = shortcodeGuid1;
            lavaShortcode.TagName = "TestShortcode1";
            lavaShortcode.Name = "Test Shortcode 1";
            lavaShortcode.IsActive = true;
            lavaShortcode.Description = "Test shortcode";
            //lavaShortcode.Documentation = htmlDocumentation.Text;
            lavaShortcode.TagType = TagType.Inline;

            lavaShortcode.Markup = "Hello!";
            //lavaShortcode.Parameters = kvlParameters.Value;
            //lavaShortcode.EnabledLavaCommands = String.Join( ",", lcpLavaCommands.SelectedLavaCommands );

            rockContext.SaveChanges();

            LavaEngine.CurrentEngine.RegisterDynamicShortcode( "TestShortcode1",
                ( shortcodeName ) => WebsiteLavaShortcodeProvider.GetShortcodeDefinition( shortcodeName ) );

            // Resolve a template using the new shortcode and verify the result.
            //string templateOutput;

            //LavaEngine.CurrentEngine.TryRender( "{[ testshortcode1 ]}", out templateOutput );

            TestHelper.AssertTemplateOutput( "Hello!", "{[ testshortcode1 ]}" );

            lavaShortcode.Markup = "Goodbye!";

            rockContext.SaveChanges();

            ?? FlushCache here ?;

            TestHelper.AssertTemplateOutput( "Goodbye!", "{[ testshortcode1 ]}" );
            //LavaEngine.CurrentEngine.TryRender( "{[ testshortcode1 ]}", out templateOutput );

            //AssertTemplateOutput( "Goodbye!", templateOutput );
        }

        private void AssertTemplateOutput( string expectedOutput, string templateContent )
        {
            // Get access to the private GetTemplate method so we can use the internal Lava template caching mechanism.
            var GetTemplateMethodsInfo = typeof( ExtensionMethods ).GetMethod( "GetTemplate", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic );

            var template = GetTemplateMethodsInfo.Invoke( null, new object[] { templateContent } ) as ILavaTemplate;

            var output = template.Render();

            // Verify that the rendered template matches the expected output.
            Assert.AreEqual( expectedOutput, output, "Template Output does not match expected output." );
        }
        #endregion
    }
}
