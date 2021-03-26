﻿// <copyright>
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Lava
{
    public class LavaIntegrationTestHelper
    {
        private static LavaIntegrationTestHelper _instance = null;
        public static LavaIntegrationTestHelper CurrentInstance
        {
            get
            {
                if ( _instance == null )
                {
                    throw new Exception( "Helper not configured. Call the Initialize() method to initialize this helper before use." );
                }

                return _instance;
            }

        }
        public static bool FluidEngineIsEnabled { get; set; }
        public static bool DotLiquidEngineIsEnabled { get; set; }
        public static bool RockLiquidEngineIsEnabled { get; set; }

        private static ILavaEngine _rockliquidEngine = null;
        private static ILavaEngine _dotliquidEngine = null;
        private static ILavaEngine _fluidEngine = null;

        public static void Initialize( bool testRockLiquidEngine, bool testDotLiquidEngine, bool testFluidEngine )
        {
            RockLiquidEngineIsEnabled = testRockLiquidEngine;
            DotLiquidEngineIsEnabled = testDotLiquidEngine;
            FluidEngineIsEnabled = testFluidEngine;

            var engineOptions = new LavaEngineConfigurationOptions();

            engineOptions.FileSystem = new MockFileProvider();
            engineOptions.ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.RenderToOutput;

            if ( RockLiquidEngineIsEnabled )
            {
                // Initialize the Rock variant of the DotLiquid Engine
                _rockliquidEngine = global::Rock.Lava.LavaEngine.NewEngineInstance( LavaEngineTypeSpecifier.RockLiquid, engineOptions );

                RegisterFilters( _rockliquidEngine );
                RegisterTags( _rockliquidEngine );
                RegisterBlocks( _rockliquidEngine );

                RegisterStaticShortcodes( _rockliquidEngine );
                RegisterDynamicShortcodes( _rockliquidEngine );
            }

            if ( DotLiquidEngineIsEnabled )
            {
                // Initialize the DotLiquid Engine
                //engineOptions.CacheService = new WebsiteLavaTemplateCache();

                _dotliquidEngine = global::Rock.Lava.LavaEngine.NewEngineInstance( LavaEngineTypeSpecifier.DotLiquid, engineOptions );

                RegisterFilters( _dotliquidEngine );
                RegisterTags( _dotliquidEngine );
                RegisterBlocks( _dotliquidEngine );

                RegisterStaticShortcodes( _dotliquidEngine );
                RegisterDynamicShortcodes( _dotliquidEngine );
            }

            if ( FluidEngineIsEnabled )
            {
                // Initialize Fluid Engine
                _fluidEngine = global::Rock.Lava.LavaEngine.NewEngineInstance( LavaEngineTypeSpecifier.Fluid, engineOptions );

                // Register the common Rock.Lava filters first, then overwrite with the web-based RockFilters as needed.
                _fluidEngine.RegisterFilters( typeof( global::Rock.Lava.Filters.TemplateFilters ) );
                _fluidEngine.RegisterFilters( typeof( global::Rock.Lava.LavaFilters ) );

                RegisterFilters( _fluidEngine );
                RegisterTags( _fluidEngine );
                RegisterBlocks( _fluidEngine );

                RegisterStaticShortcodes( _fluidEngine );
                RegisterDynamicShortcodes( _fluidEngine );
            }

            _instance = new LavaIntegrationTestHelper();
        }

        private ILavaEngine GetEngineInstance( LavaEngineTypeSpecifier engineType )
        {
            ILavaEngine engine = null;

            if ( engineType == LavaEngineTypeSpecifier.DotLiquid )
            {
                engine = _dotliquidEngine;
            }
            else if ( engineType == LavaEngineTypeSpecifier.Fluid )
            {
                engine = _fluidEngine;
            }
            else if ( engineType == LavaEngineTypeSpecifier.RockLiquid )
            {
                engine = _rockliquidEngine;
            }

            // Set the global instance of the engine to ensure that it is available to Lava components.
            LavaEngine.CurrentEngine = engine;

            return engine;
            
        }

        //public static LavaIntegrationTestHelper New( LavaEngineTypeSpecifier? engineType = null )
        //{
        //    engineType = engineType ?? LavaEngineTypeSpecifier.DotLiquid;

        //    var engineOptions = new LavaEngineConfigurationOptions();

        //    engineOptions.FileSystem = new MockFileProvider();

        //    if ( engineType != LavaEngineTypeSpecifier.RockLiquid )
        //    {
        //        engineOptions.CacheService = new WebsiteLavaTemplateCache();
        //    }

        //    global::Rock.Lava.LavaEngine.Initialize( engineType, engineOptions );

        //    var engine = global::Rock.Lava.LavaEngine.CurrentEngine;

        //    engine.ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.RenderToOutput;

        //    RegisterFilters( engine );
        //    RegisterTags( engine );
        //    RegisterBlocks( engine );

        //    RegisterStaticShortcodes( engine );
        //    RegisterDynamicShortcodes( engine );

        //    var helper = new LavaIntegrationTestHelper();

        //    return helper;
        //}

        private static void RegisterFilters( ILavaEngine engine )
        {
            if ( engine.EngineType == LavaEngineTypeSpecifier.RockLiquid )
            {
                engine.RegisterFilters( typeof( global::Rock.Lava.Filters.TemplateFilters ) );
                engine.RegisterFilters( typeof( Rock.Lava.RockFilters ) );
            }
            else
            {
                engine.RegisterFilters( typeof( Rock.Lava.LavaFilters ) );
                engine.RegisterFilters( typeof( global::Rock.Lava.Filters.TemplateFilters ) );
            }
        }

        private static void RegisterTags( ILavaEngine engine )
        {
            // Get all tags and call OnStartup methods
            if ( engine.EngineType == LavaEngineTypeSpecifier.RockLiquid )
            {
                // Find all tag elements that implement IRockStartup.
                var elementTypes = Rock.Reflection.FindTypes( typeof( DotLiquid.Tag ) ).Select( a => a.Value ).ToList();

                foreach ( var elementType in elementTypes )
                {
                    var instance = Activator.CreateInstance( elementType ) as IRockStartup;

                    if ( instance == null )
                    {
                        continue;
                    }

                    try
                    {
                        // RockLiquid blocks register themselves with the DotLiquid framework during their startup process.
                        instance.OnStartup();
                    }
                    catch ( Exception ex )
                    {
                        var lavaException = new Exception( string.Format( "Lava component initialization failure. Startup failed for Lava Tag \"{0}\".", elementType.FullName ), ex );

                        ExceptionLogService.LogException( lavaException, null );
                    }
                }
            }

            if ( engine.EngineType == LavaEngineTypeSpecifier.RockLiquid )
            {
                return;
            }

            // Get all Lava tags and call the OnStartup method.
            try
            {
                List<Type> elementTypes;

                elementTypes = Rock.Reflection.FindTypes( typeof( ILavaTag ) ).Select( a => a.Value ).ToList();

                foreach ( var elementType in elementTypes )
                {
                    var instance = Activator.CreateInstance( elementType ) as ILavaTag;

                    var name = instance.SourceElementName;

                    if ( string.IsNullOrWhiteSpace( name ) )
                    {
                        name = elementType.Name;
                    }

                    engine.RegisterTag( name, ( shortcodeName ) =>
                    {
                        var shortcode = Activator.CreateInstance( elementType ) as ILavaTag;

                        return shortcode;
                    } );

                    try
                    {
                        instance.OnStartup( engine );
                    }
                    catch ( Exception ex )
                    {
                        var lavaException = new Exception( string.Format( "Lava component initialization failure. Startup failed for Lava Tag \"{0}\".", elementType.FullName ), ex );

                        ExceptionLogService.LogException( lavaException, null );
                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
            }
        }

        private static void RegisterBlocks( ILavaEngine engine )
        {
            // Get all blocks and call OnStartup methods
            if ( engine.EngineType == LavaEngineTypeSpecifier.RockLiquid )
            {
                // Find all tag elements that implement IRockStartup.
                var elementTypes = Rock.Reflection.FindTypes( typeof( DotLiquid.Block ) ).Select( a => a.Value ).ToList();

                foreach ( var elementType in elementTypes )
                {
                    var instance = Activator.CreateInstance( elementType ) as IRockStartup;

                    if ( instance == null )
                    {
                        continue;
                    }

                    try
                    {
                        // RockLiquid blocks register themselves with the DotLiquid framework during their startup process.
                        instance.OnStartup();
                    }
                    catch ( Exception ex )
                    {
                        var lavaException = new Exception( string.Format( "Lava component initialization failure. Startup failed for Lava Tag \"{0}\".", elementType.FullName ), ex );

                        ExceptionLogService.LogException( lavaException, null );
                    }
                }
            }
            else
            {
                try
                {
                    var elementTypes = Rock.Reflection.FindTypes( typeof( ILavaBlock ) ).Select( a => a.Value ).ToList();

                    foreach ( var elementType in elementTypes )
                    {
                        var instance = Activator.CreateInstance( elementType ) as ILavaBlock;

                        var name = instance.SourceElementName;

                        if ( string.IsNullOrWhiteSpace( name ) )
                        {
                            name = elementType.Name;
                        }

                        engine.RegisterBlock( name, ( shortcodeName ) =>
                        {
                            var shortcode = Activator.CreateInstance( elementType ) as ILavaBlock;

                            return shortcode;
                        } );

                        try
                        {
                            instance.OnStartup( engine );
                        }
                        catch ( Exception ex )
                        {
                            var lavaException = new Exception( string.Format( "Lava component initialization failure. Startup failed for Lava Block \"{0}\".", elementType.FullName ), ex );

                            ExceptionLogService.LogException( lavaException, null );
                        }
                    }
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex, null );
                }
            }
        }

        private static void RegisterStaticShortcodes( ILavaEngine engine )
        {
            // Get all shortcodes and call OnStartup methods
            try
            {
                var shortcodeTypes = Rock.Reflection.FindTypes( typeof( ILavaShortcode ) ).Select( a => a.Value ).ToList();

                foreach ( var shortcodeType in shortcodeTypes )
                {
                    // Create an instance of the shortcode to get the registration name.
                    var instance = Activator.CreateInstance( shortcodeType ) as ILavaShortcode;

                    var name = instance.SourceElementName;

                    if ( string.IsNullOrWhiteSpace( name ) )
                    {
                        name = shortcodeType.Name;
                    }

                    engine.RegisterStaticShortcode( name, ( shortcodeName ) =>
                    {
                        var shortcode = Activator.CreateInstance( shortcodeType ) as ILavaShortcode;

                        return shortcode;
                    } );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
            }
        }

        private static void RegisterDynamicShortcodes( ILavaEngine engine )
        {
            // Register dynamic shortcodes with a factory method to ensure that the latest definition is retrieved from the global cache each time the shortcode is used.
            Func<string, DynamicShortcodeDefinition> shortCodeFactory = ( shortcodeName ) =>
            {
                var shortcodeDefinition = LavaShortcodeCache.All().Where( c => c.TagName == shortcodeName ).FirstOrDefault();

                if ( shortcodeDefinition == null )
                {
                    return null;
                }

                var newShortcode = new DynamicShortcodeDefinition();

                newShortcode.Name = shortcodeDefinition.Name;
                newShortcode.TemplateMarkup = shortcodeDefinition.Markup;

                var parameters = RockSerializableDictionary.FromUriEncodedString( shortcodeDefinition.Parameters );

                newShortcode.Parameters = new Dictionary<string, string>( parameters.Dictionary );

                newShortcode.EnabledLavaCommands = shortcodeDefinition.EnabledLavaCommands.SplitDelimitedValues( ",", StringSplitOptions.RemoveEmptyEntries ).ToList();

                if ( shortcodeDefinition.TagType == TagType.Block )
                {
                    newShortcode.ElementType = LavaShortcodeTypeSpecifier.Block;
                }
                else
                {
                    newShortcode.ElementType = LavaShortcodeTypeSpecifier.Inline;
                }

                return newShortcode;
            };

            var shortCodes = LavaShortcodeCache.All();

            foreach ( var shortcode in shortCodes )
            {
                engine.RegisterDynamicShortcode( shortcode.TagName, shortCodeFactory );
            }
        }

        /// <summary>
        /// Process the specified input template and return the result.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        public string GetTemplateOutput( LavaEngineTypeSpecifier engineType, string inputTemplate, LavaDataDictionary mergeFields = null )
        {
            inputTemplate = inputTemplate ?? string.Empty;

            var engine = GetEngineInstance( engineType );

            var outputString = engine.RenderTemplate( inputTemplate.Trim(), mergeFields );

            return outputString;
        }

        /// <summary>
        /// Process the specified input template and return the result.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        public string GetTemplateOutput( LavaEngineTypeSpecifier engineType, string inputTemplate, ILavaRenderContext context )
        {
            inputTemplate = inputTemplate ?? string.Empty;

            var engine = GetEngineInstance( engineType );

            var outputString = engine.RenderTemplate( inputTemplate.Trim(), context );

            return outputString;
        }

        /// <summary>
        /// Process the specified input template and return the result.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        public string GetTemplateOutput( LavaEngineTypeSpecifier engineType, string inputTemplate, LavaTestRenderOptions options )
        {
            inputTemplate = inputTemplate ?? string.Empty;

            var engine = GetEngineInstance( engineType );

            var context = engine.NewRenderContext();

            if ( options != null )
            {
                context.SetEnabledCommands( options.EnabledCommands, options.EnabledCommandsDelimiter );
                context.SetMergeFields( options.MergeFields );
            }

            var outputString = engine.RenderTemplate( inputTemplate.Trim(), context );

            return outputString;
        }

        /// <summary>
        /// For each of the currently enabled Lava Engines, process the specified action.
        /// </summary>
        /// <param name="testMethod"></param>
        public void AssertAction( Action<ILavaEngine> testMethod )
        {
            var engines = GetActiveTestEngines();

            foreach ( var engine in engines )
            {
                LavaEngine.CurrentEngine = engine;

                Debug.Print( $"\n**\n** Lava Render Test: {engine.EngineType}\n**\n" );

                try
                {
                    testMethod( engine );
                }
                catch (Exception ex)
                {
                    // Write the error to debug output.
                    Debug.Print( $"\n** ERROR:\n{ex.ToString()}" );

                    throw ex;
                }
            }
        }

        private List<ILavaEngine> _activeEngines = null;

        private List<ILavaEngine> GetActiveTestEngines()
        {
            if ( _activeEngines == null )
            {
                _activeEngines = new List<ILavaEngine>();

                if ( DotLiquidEngineIsEnabled )
                {
                    _activeEngines.Add( _dotliquidEngine );
                }

                if ( FluidEngineIsEnabled )
                {
                    _activeEngines.Add( _fluidEngine );
                }

                if ( RockLiquidEngineIsEnabled )
                {
                    _activeEngines.Add( _rockliquidEngine );
                }
            }

            return _activeEngines;

        }

        //private void WriteTestHeader( ILavaEngine engine, string inputString )
        //{
        //    Debug.Print( $"\n**\n** Lava Template Render: {engine.EngineType}\n** Input:\n**\n{inputString}" );
        //    //Debug.Print( $"\n**\n** Template Output ({engine.EngineType}):\n**\n{outputString}" );

        //}

        /// <summary
        /// <summary>
        /// Process the specified input template and verify against the expected output.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        //public void AssertTemplateOutput( string expectedOutput, string inputTemplate, ILavaDataDictionary mergeFields )
        //{
        //    AssertTemplateOutput( expectedOutput, inputTemplate, mergeFields );
        //}

        /// <summary>
        /// Process the specified input template and verify against the expected output.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        //public void AssertTemplateOutput( string expectedOutput, string inputTemplate, LavaTestRenderOptions options )
        //{
        //    AssertTemplateOutput( expectedOutput, inputTemplate, options );
        //}

        /// <summary>
        /// Process the specified input template and verify against the expected output.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutput( string expectedOutput, string inputTemplate, LavaTestRenderOptions options = null )
        {
            AssertAction( ( engine ) =>
            {
                AssertTemplateOutput( engine.EngineType, expectedOutput, inputTemplate, options );
            } );
        }

        /// <summary>
        /// Process the specified input template and verify against the expected output.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutput( LavaEngineTypeSpecifier engineType, string expectedOutput, string inputTemplate, LavaTestRenderOptions options = null )
        {
            var engine = GetEngineInstance( engineType );

            var context = engine.NewRenderContext();

            options = options ?? new LavaTestRenderOptions();

            context.SetEnabledCommands( options.EnabledCommands, options.EnabledCommandsDelimiter );
            context.SetMergeFields( options.MergeFields );

            var outputString = GetTemplateOutput( engineType, inputTemplate, context );

            Assert.IsNotNull( outputString, "Template failed to render." );

            DebugWriteRenderResult( engineType, inputTemplate, outputString );

            if ( options.IgnoreWhiteSpace )
            {
                outputString = Regex.Replace( outputString, @"\s*", string.Empty );
                expectedOutput = Regex.Replace( expectedOutput, @"\s*", string.Empty );
            }

            Assert.That.Equal( expectedOutput, outputString );
        }

        /// <summary>
        /// Verify that the specified template is invalid.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        public void AssertTemplateIsInvalid( string inputTemplate, LavaDataDictionary mergeFields = null )
        {
            AssertAction( ( engine ) =>
            {
                AssertTemplateIsInvalid( engine.EngineType, inputTemplate, mergeFields );
            } );
        }

        /// <summary>
        /// Verify that the specified template is invalid.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        public void AssertTemplateIsInvalid( LavaEngineTypeSpecifier engineType, string inputTemplate, LavaDataDictionary mergeFields = null )
        {
            string outputString;
            List<Exception> errors;

            inputTemplate = inputTemplate ?? string.Empty;

            var engine = GetEngineInstance( engineType );

            var isValid = engine.TryRenderTemplate( inputTemplate.Trim(), mergeFields, out outputString, out errors );

            Assert.That.IsFalse( isValid, "Invalid template expected." );
        }

        /// <summary>
        /// Write a rendered template to debug, with some additional configuration details.
        /// </summary>
        /// <param name="outputString"></param>
        public void DebugWriteRenderResult( LavaEngineTypeSpecifier engineType, string inputString, string outputString )
        {
            var engine = GetEngineInstance( engineType );

            Debug.Print( $"\n** Input:\n{inputString}" );
            Debug.Print( $"\n** Output:\n{outputString}" );
        }

        /// <summary>
        /// Process the specified input template and verify against the expected output regular expression.
        /// </summary>
        /// <param name="expectedOutputRegex"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutputRegex( string expectedOutputRegex, string inputTemplate, LavaDataDictionary mergeValues = null )
        {
            AssertAction( ( engine ) =>
            {
                var outputString = GetTemplateOutput( engine.EngineType, inputTemplate, mergeValues );

                Assert.IsNotNull( outputString, "Template failed to render." );

                var regex = new Regex( expectedOutputRegex );

                DebugWriteRenderResult( engine.EngineType, inputTemplate, outputString );
                StringAssert.Matches( outputString, regex );
            } );
        }


        /// <summary>
        /// Process the specified input template and verify against the expected output regular expression.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutputWithWildcard( string expectedOutput, string inputTemplate, LavaTestRenderOptions options = null )
        {
            AssertAction( ( engine ) =>
            {
                AssertTemplateOutputWithWildcard( engine.EngineType, expectedOutput, inputTemplate, options );
            } );
        }

        /// <summary>
        /// Process the specified input template and verify against the expected output regular expression.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutputWithWildcard( LavaEngineTypeSpecifier engineType, string expectedOutput, string inputTemplate, LavaTestRenderOptions options = null )
        {
            var engine = GetEngineInstance( engineType );

            options = options ?? new LavaTestRenderOptions();

            var lavaContext = engine.NewRenderContext( options.MergeFields );

            if ( options.EnabledCommands != null )
            {
                lavaContext.SetEnabledCommands( options.EnabledCommands.SplitDelimitedValues( options.EnabledCommandsDelimiter ) );
            }

            var outputString = GetTemplateOutput( engineType, inputTemplate, lavaContext );

            Assert.IsNotNull( outputString, "Template contains no output." );

            // Replace the wildcards with a non-Regex symbol.
            expectedOutput = expectedOutput.Replace( options.WildcardPlaceholder, "<<<wildCard>>>" );

            if ( options.IgnoreWhiteSpace )
            {
                outputString = Regex.Replace( outputString, @"\s*", string.Empty );
                expectedOutput = Regex.Replace( expectedOutput, @"\s*", string.Empty );
            }

            expectedOutput = Regex.Escape( expectedOutput );
            expectedOutput = expectedOutput.Replace( "/", @"\/" );

            expectedOutput = expectedOutput.Replace( "<<<wildCard>>>", "(.*)" );

            var regex = new Regex( expectedOutput );

            DebugWriteRenderResult( engineType, inputTemplate, outputString );
            StringAssert.Matches( outputString, regex );
        }

        /// <summary>
        /// Process the specified input template and verify against the expected output regular expression.
        /// </summary>
        /// <param name="expectedOutputRegex"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutputRegex( string expectedOutputRegex, string inputTemplate, ILavaRenderContext context, bool ignoreWhiteSpace = true )
        {
            AssertAction( ( engine ) =>
            {
                var outputString = GetTemplateOutput( engine.EngineType, inputTemplate, context );

                // If ignoring whitespace, replace any whitespace in the expected output regex with a greedy whitespace match.
                if ( ignoreWhiteSpace )
                {
                    expectedOutputRegex = Regex.Replace( expectedOutputRegex, @"\s+", @"\s*" );
                }

                var regex = new Regex( expectedOutputRegex );

                DebugWriteRenderResult( engine.EngineType, inputTemplate, outputString );
                StringAssert.Matches( outputString, regex );
            } );
        }

        /// <summary>
        /// Process the specified input template and verify the output against an expected DateTime result.
        /// </summary>
        /// <param name="expectedDateTime"></param>
        /// <param name="inputTemplate"></param>
        /// <param name="maximumDelta"></param>
        public void AssertTemplateOutputDate( DateTime? expectedDateTime, string inputTemplate, TimeSpan? maximumDelta = null )
        {
            AssertAction( ( engine ) =>
            {
                var outputString = GetTemplateOutput( engine.EngineType, inputTemplate );

                DebugWriteRenderResult( engine.EngineType, inputTemplate, outputString );

                DateTime outputDate;

                var isValidDate = DateTime.TryParse( outputString, out outputDate );

                Assert.That.True( isValidDate, $"Template Output does not represent a valid DateTime. [Output=\"{ outputString }\"]" );

                if ( maximumDelta != null )
                {
                    Assert.That.AreProximate( expectedDateTime, outputDate, maximumDelta.Value );
                }
                else
                {
                    Assert.That.AreEqual( expectedDateTime, outputDate );
                }
            } );
        }

        /// <summary>
        /// Resolve the specified template to a date and verify that it is equivalent to the expected date.
        /// </summary>
        /// <param name="expectedDateString"></param>
        /// <param name="inputTemplate"></param>
        /// <param name="maximumDelta"></param>
        public void AssertTemplateOutputDate( string expectedDateString, string inputTemplate, TimeSpan? maximumDelta = null )
        {
            bool isValid;
            DateTime expectedDate;

            isValid = DateTime.TryParse( expectedDateString, out expectedDate );

            Assert.That.True( isValid, "Expected Date String input is not a valid date." );

            AssertTemplateOutputDate( expectedDate, inputTemplate, maximumDelta );
        }

        #region Test Data

        /// <summary>
        /// Return an initialized Person object for test subject Ted Decker.
        /// </summary>
        /// <returns></returns>
        public TestPerson GetTestPersonTedDecker()
        {
            var campus = new TestCampus { Name = "North Campus", Id = 1 };
            var person = new TestPerson { FirstName = "Edward", NickName = "Ted", LastName = "Decker", Campus = campus, Id = 1, Guid = "8FEDC6EE-8630-41ED-9FC5-C7157FD1EAA4" };

            return person;
        }

        /// <summary>
        /// Return an initialized Person object for test subject Alisha Marble.
        /// </summary>
        /// <returns></returns>
        public TestPerson GetTestPersonAlishaMarble()
        {
            var campus = new TestCampus { Name = "South Campus", Id = 2 };
            var person = new TestPerson { FirstName = "Alisha", NickName = "Alisha", LastName = "Marble", Campus = campus, Id = 2 };

            return person;
        }

        /// <summary>
        /// Return a collection of initialized Person objects for the Decker family.
        /// </summary>
        /// <returns></returns>
        public List<TestPerson> GetTestPersonCollectionForDecker()
        {
            var personList = new List<TestPerson>();

            personList.Add( GetTestPersonTedDecker() );
            personList.Add( new TestPerson { FirstName = "Cindy", LastName = "Decker", Id = 2 } );
            personList.Add( new TestPerson { FirstName = "Noah", LastName = "Decker", Id = 3 } );
            personList.Add( new TestPerson { FirstName = "Alex", LastName = "Decker", Id = 4 } );

            return personList;
        }

        #endregion

        #region Test Classes

        /// <summary>
        /// A representation of a Person used for testing purposes.
        /// </summary>
        public class TestPerson : LavaDataObject
        {
            public int Id { get; set; }
            public string Guid { get; set; }
            public string NickName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public TestCampus Campus { get; set; }

            public override string ToString()
            {
                return $"{NickName} {LastName}";
            }
        }

        /// <summary>
        /// A representation of a Campus used for testing purposes.
        /// </summary>
        public class TestCampus : LavaDataObject
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        #endregion

    }

    public class LavaTestRenderOptions
    {
        public IDictionary<string, object> MergeFields = null;
        public string EnabledCommands = null;
        public string EnabledCommandsDelimiter = ",";
        public bool IgnoreWhiteSpace = true;

        public string WildcardPlaceholder = "*";


    }
}
