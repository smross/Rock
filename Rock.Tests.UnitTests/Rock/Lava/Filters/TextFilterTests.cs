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
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Lava;
using Rock.Tests.Shared;

namespace Rock.Tests.UnitTests.Lava
{
    [TestClass]
    public class TextFilterTests : LavaUnitTestBase
    {
        private const string _TestTextParagraph = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.Odio eu feugiat pretium nibh.Semper risus in hendrerit gravida.Enim diam vulputate ut pharetra. Massa tincidunt nunc pulvinar sapien et ligula ullamcorper. Morbi tristique senectus et netus et malesuada.Praesent semper feugiat nibh sed pulvinar proin gravida hendrerit.Ultrices in iaculis nunc sed.Tortor id aliquet lectus proin nibh nisl condimentum id.Vel pretium lectus quam id leo in vitae.In mollis nunc sed id.";

        /// <summary>
        /// For complex objects, the filter should return the .NET ToString() result for the object.
        /// </summary>
        /// <remarks>
        /// This filter has an identical implementation to "ToString", so we only need to test that it is accessible.
        /// </remarks>
        [TestMethod]
        public void AsString_AnonymousObjectWithToStringMethodOverride_ReturnsToStringForObject()
        {
            var person = TestHelper.GetTestPersonAlishaMarble();

            var mergeValues = new LavaDataDictionary { { "CurrentPerson", person } };

            TestHelper.AssertTemplateOutput( "Alisha Marble", "{{ CurrentPerson | AsString }}", mergeValues );
        }

        #region Filter Tests: Humanize

        /// <summary>
        /// A lower-case string should be formatted with the first letter of each word capitalized.
        /// </summary>
        [TestMethod]
        public void Humanize_CamelCase_ProducesSeparatedWords()
        {
            TestHelper.AssertTemplateOutput( "Camel case", "{{ 'camelCase' | Humanize }}" );
        }

        /// <summary>
        /// A lower-case string should be formatted with the first letter of each word capitalized.
        /// </summary>
        [TestMethod]
        public void Humanize_Underscore_ProducesSeparatedWords()
        {
            TestHelper.AssertTemplateOutput( "underscore a point", "{{ 'underscore_a_point' | Humanize }}" );
        }

        /// <summary>
        /// A lower-case string should be formatted with the first letter of each word capitalized.
        /// </summary>
        [TestMethod]
        public void Humanize_DashSeparator_ProducesSeparatedWords()
        {
            TestHelper.AssertTemplateOutput( "css classes", "{{ 'css-classes' | Humanize }}" );
        }

        #endregion

        /// <summary>
        /// The input sentence should be formatted with only the first letter of the sentence capitalized.
        /// </summary>
        [TestMethod]
        public void SentenceCase_LowerCaseString_ProducesSentenceCase()
        {
            TestHelper.AssertTemplateOutput( "Good to great", "{{ 'good to great' | SentenceCase }}" );
        }

        /// <summary>
        /// A lower-case string should be formatted with the first letter of each word capitalized.
        /// </summary>
        [TestMethod]
        public void TitleCase_MixedCaseString_ProducesTitleCase()
        {
            TestHelper.AssertTemplateOutput( "Job Posting For Groundskeeper", "{{ 'Job posting for groundskeeper' | TitleCase }}" );
        }

        /// <summary>
        /// A lower-case string should be formatted with the first letter of each word capitalized and all whitespace removed.
        /// </summary>
        [TestMethod]
        public void ToPascal_LowerCaseString_ProducesPascalCase()
        {
            TestHelper.AssertTemplateOutput( "CommunityParticipant", "{{ 'community participant' | ToPascal }}" );
        }

        /// <summary>
        /// A numeric input should return a text string.
        /// </summary>
        [TestMethod]
        public void ToString_NumericInput_ProducesText()
        {
            TestHelper.AssertTemplateOutput( "1234567.89", "{{ 1234567.89 | ToString }}" );
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void ObfuscateEmail_ReadableEmailAddress_IsObfuscated()
        {
            TestHelper.AssertTemplateOutput( "txxxxx@rocksolidchurchdemo.com", "{{ 'ted@rocksolidchurchdemo.com' | ObfuscateEmail }}" );
        }

        #region Filter Tests: Pluralize

        /// <summary>
        /// Providing a singular noun as input produces a collective noun as output.
        /// </summary>
        [TestMethod]
        public void Pluralize_SingularTerm_ProducesPluralizedTerm()
        {
            TestHelper.AssertTemplateOutput( "geese", "{{ 'goose' | Pluralize }}" );
        }

        /// <summary>
        /// Providing a collective noun as input produces unchanged output.
        /// </summary>
        [TestMethod]
        public void Pluralize_PluralTerm_ProducesUnchangedOutput()
        {
            TestHelper.AssertTemplateOutput( "requests", "{{ 'requests' | Pluralize }}" );
        }

        #endregion

        #region Filter Tests: PluralizeForQuantity

        /// <summary>
        /// Providing an input argument for quantity of 3 produces a collective noun as output.
        /// </summary>
        [TestMethod]
        public void PluralizeForQuantity_QuantityOf3_ProducesPluralizedTerm()
        {
            TestHelper.AssertTemplateOutput( "Leaders", "{{ 'Leader' | PluralizeForQuantity:3 }}" );
        }

        /// <summary>
        /// Providing an input argument for quantity of 1 produces a singular noun as output.
        /// </summary>
        [TestMethod]
        public void PluralizeForQuantity_QuantityOf1_ProducesSingularTerm()
        {
            TestHelper.AssertTemplateOutput( "Leader", "{{ 'Leader' | PluralizeForQuantity:1 }}" );
        }

        #endregion

        /// <summary>
        /// A noun ending with the letter "S" as input should produce a possessive form having a trailing apostrophe.
        /// </summary>
        [TestMethod]
        public void Possessive_NameEndingWithS_ProducesPossessiveFormWithTrailingApostrophe()
        {
            TestHelper.AssertTemplateOutput( "Charles’", "{{ 'Charles' | Possessive }}" );
        }

        /// <summary>
        /// A noun ending with the letter "S" as input should produce a possessive form having a trailing apostrophe.
        /// </summary>
        [TestMethod]
        public void Possessive_NameNotEndingWithS_ProducesPossessiveForm()
        {
            TestHelper.AssertTemplateOutput( "Ted’s", "{{ 'Ted' | Possessive }}" );
        }

        #region Filter Tests: Read Time

        private static readonly string[] _timeSpanOutputFormats = new string[] { "m' mins'", "m' min'", "s' secs'", "s' sec'" };

        /// <summary>
        /// The read time for a known piece of text using the default settings should return a number of minutes.
        /// </summary>
        [TestMethod]
        public void ReadTime_CalculateReadTimeForAverageReader_ProducesTimeInSeconds()
        {
            // Create a large document.
            var documentText = GetRepeatString( _TestTextParagraph, 5 );

            var template = "{{ '<content>' | ReadTime }}"
                .Replace( "<content>", documentText );

            TestHelper.ExecuteTestAction( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine.EngineType, template );

                Assert.That.False( string.IsNullOrWhiteSpace( output ) );

                var readTime = TimeSpan.ParseExact( output, _timeSpanOutputFormats, CultureInfo.CurrentCulture );

                Assert.That.AreProximate( 120, readTime.TotalSeconds, 10 );
            } );
        }

        /// <summary>
        /// The average read time for a known piece of text should return a specific number.
        /// </summary>
        [TestMethod]
        public void ReadTime_CalculateReadTimeForSlowReader_ProducesTimeInMinutes()
        {
            // Create a large document.
            var documentText = GetRepeatString( _TestTextParagraph, 5 );

            var template = "{{ '<content>' | ReadTime:5,30 }}"
                .Replace( "<content>", documentText );

            TestHelper.ExecuteTestAction( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine.EngineType, template );

                Assert.That.False( string.IsNullOrWhiteSpace( output ) );

                var readTime = TimeSpan.ParseExact( output, _timeSpanOutputFormats, CultureInfo.CurrentCulture );

                Assert.That.AreProximate( 360, readTime.TotalSeconds, 10 );
            } );
        }

        /// <summary>
        /// The average read time for a known piece of text should return a specific number.
        /// </summary>
        [TestMethod]
        public void ReadTime_CalculateReadTimeForFastReader_ProducesTimeInSeconds()
        {
            // Create a large document.
            var documentText = GetRepeatString( _TestTextParagraph, 5 );

            var template = "{{ '<content>' | ReadTime:500,6 }}"
                .Replace( "<content>", documentText );

            TestHelper.ExecuteTestAction( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine.EngineType, template );
                var readTime = TimeSpan.ParseExact( output, _timeSpanOutputFormats, CultureInfo.CurrentCulture );

                Assert.That.AreProximate( 50, readTime.TotalSeconds, 10 );
            } );
        }

        /// <summary>
        /// Create a string by concatenating the input string to itself a specified number of times.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private string GetRepeatString( string value, int count )
        {
            string output = string.Empty;

            for ( int i = 0; i < count; i++ )
            {
                output += value;
            }

            return output;
        }

        #endregion

        #region Filter Tests: Regular Expressions

        /// <summary>
        /// Various email address formats can be matched using a regular expression.
        /// </summary>
        [DataTestMethod]
        [DataRow( "ted@rocksolidchurchdemo.com", true )]
        [DataRow( "has_underscore@rocksolidchurchdemo.com", true )]
        [DataRow( "has.dot@rocksolidchurchdemo.com", true )]
        [DataRow( "no_at_symbol-rocksolidchurchdemo.com", false )]
        [DataRow( "extra_at_symbol@@rocksolidchurchdemo.com", false )]
        [DataRow( "has.no.domain.separator@rocksolidchurchdemo", false )]
        public void RegExMatch_EmailAddressValidationSucceeds( string input, bool isMatch )
        {
            // This regular expression is the same one used in Rock for email validation.
            var template = @"{{ '<input>' | RegExMatch:'\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*' }}"
                           .Replace( "<input>", input );

            TestHelper.AssertTemplateOutput( isMatch.ToString().ToLower(), template );
        }

        /// <summary>
        /// Regular expression match is found.
        /// </summary>
        [TestMethod]
        public void RegExMatchValue_FindsFirstMatchOnly()
        {
            // [2020-12-22] DJL - This test passes using DotLiquid - modified "\\" to "\".
            // May need to verify if Fluid automatically escapes the filter parameter input?
            TestHelper.AssertTemplateOutput( "12345", @"{{ 'group 12345' | RegExMatchValue:'\d+' }}" );
            TestHelper.AssertTemplateOutput( "Saturday", @"{{ 'Services on Saturday and Sunday' | RegExMatchValue:'\b\w+day\b' }}" );
        }

        /// <summary>
        /// Multiple Regular expression matches are found.
        /// </summary>
        [TestMethod]
        public void RegExMatchValues_FindsAllMatches()
        {
            var template = @"
{% assign days = 'Services on Saturday and Sunday and now also on Monday!' | RegExMatchValues:'\b\w+day\b' %}
{% for day in days %}{{ day }},{% endfor %}
";
            template = template.Replace( "\n", string.Empty ).Replace( "\r", string.Empty );

            TestHelper.AssertTemplateOutput( "Saturday,Sunday,Monday,", template );
        }

        [TestMethod]
        public void RegExReplace_WithIgnoreCaseOption_ReplacesAllCases()
        {
            var template = @"
{{ 'Testing: one, two, One, two, ONE, two...' | RegExReplace:'one','ONE','i' }}
";

            TestHelper.AssertTemplateOutput( "Testing: ONE, two, ONE, two, ONE, two...", template );
        }

        [TestMethod]
        public void RegExReplace_WithCaptureGroup_EmitsExpectedOutput()
        {
            var template = @"
{{ 'Hello Ted, how are you?' | RegExReplace:'[Hh]ello (\w+)','Greetings $1' }}
";

            TestHelper.AssertTemplateOutput( "Greetings Ted, how are you?", template );
        }

        #endregion

        /// <summary>
        /// Last instance of search string should be replaced with replacement string.
        /// </summary>
        [DataTestMethod]
        [DataRow( "Blue, Blue, Red, Red", "Blue, Blue, Red, Green" )]
        [DataRow( "Red, Red, Blue, Blue", "Red, Green, Blue, Blue" )]
        [DataRow( "Red, Blue, Blue, Blue", "Green, Blue, Blue, Blue" )]
        [DataRow( "Blue, Blue, Blue, Blue", "Blue, Blue, Blue, Blue" )]
        [DataRow( "Red", "Green" )]
        public void ReplaceLast_LastInstanceOfRedIsReplacedWithGreen( string input, string expected )
        {
            var template = "{{ '<input>' | ReplaceLast:'Red','Green' }}"
                           .Replace( "<input>", input );

            TestHelper.AssertTemplateOutput( expected, template );
        }

        #region Filter Tests: Right

        /// <summary>
        /// The rightmost part of the input sentence should be returned in the output.
        /// </summary>
        [TestMethod]
        public void Right_LessThanStringLength_ProducesSubstring()
        {
            TestHelper.AssertTemplateOutput( "cker", "{{ 'Decker' | Right:4 }}" );
        }

        /// <summary>
        /// If the requested number of characters exceeds the string length, the entire string should be returned.
        /// </summary>
        [TestMethod]
        public void Right_GreaterThanStringLength_ProducesEntireString()
        {
            TestHelper.AssertTemplateOutput( "Decker", "{{ 'Decker' | Right:10 }}" );
        }

        /// <summary>
        /// If the input is an empty string, the output is also an empty string.
        /// </summary>
        [TestMethod]
        public void Right_EmptyString_ProducesEmptyString()
        {
            TestHelper.AssertTemplateOutput( "", "{{ '' | Right:10 }}" );
        }

        #endregion

        #region Filter Tests: Singularize

        /// <summary>
        /// Providing a singular noun as input produces unchanged output.
        /// </summary>
        [TestMethod]
        public void Singularize_SingularTerm_ProducesUnchangedOutput()
        {
            TestHelper.AssertTemplateOutput( "goose", "{{ 'goose' | Singularize }}" );
        }

        /// <summary>
        /// Providing a collective noun as input produces a singular noun as output.
        /// </summary>
        [TestMethod]
        public void Singularize_PluralTerm_ProducesSingularTerm()
        {
            TestHelper.AssertTemplateOutput( "goose", "{{ 'geese' | Singularize }}" );
        }

        #endregion

        /// <summary>
        /// Leading and trailing whitespace should be removed, while internal whitespace is preserved.
        /// </summary>
        [DataTestMethod]
        [DataRow( "Ted Decker    ", "Ted Decker" )]
        [DataRow( "   Ted Decker", "Ted Decker" )]
        [DataRow( "   Ted Decker    ","Ted Decker")]
        [DataRow( "   ", "" )]
        [DataRow( "", "" )]
        public void Trim_LeadingAndTrailingWhitespaceIsRemoved( string input, string expected )
        {
            var template = "{{ '" + input + "' | Trim }}";

            TestHelper.AssertTemplateOutput( expected, template );
        }

        /// <summary>
        /// Url parts queries return the correct segment of the Url.
        /// </summary>
        [TestMethod]
        public void Url_SimplePartQuery_ReturnCorrectPart()
        {
            var url = "https://www.rockrms.com/WorkflowEntry/35?PersonId=2";

            VerifyUrlPart( url, "host", "", "www.rockrms.com" );
            VerifyUrlPart( url, "port", "", "443" );            
            VerifyUrlPart( url, "scheme", "", "https" );
            VerifyUrlPart( url, "protocol", "", "https" );
            VerifyUrlPart( url, "localpath", "", "/WorkflowEntry/35" );
            VerifyUrlPart( url, "pathandquery", "", "/WorkflowEntry/35?PersonId=2" );
            VerifyUrlPart( url, "queryparameter", "", "" );
            VerifyUrlPart( url, "queryparameter", "PersonId", "2" );
            VerifyUrlPart( url, "url", "", "https://www.rockrms.com/WorkflowEntry/35?PersonId=2" );
            VerifyUrlPart( url, "invalid_part", "", "" );
        }

        /// <summary>
        /// Url Segments query returns a collection of all segments of the Url.
        /// </summary>        
        [TestMethod]
        public void Url_SegmentsQuery_ReturnCorrectParts()
        {
            var url = "https://www.rockrms.com/WorkflowEntry/35?PersonId=2";

            var template = "{{ '<url>' | Url:'segments' | Join:'|' }}";
            
            template = template.Replace( "<url>", url );

            TestHelper.AssertTemplateOutput( "/|WorkflowEntry/|35", template );
        }

        private void VerifyUrlPart( string url, string part, string key, string expected )
        {
            var template = "{{ '<url>' | Url:<options> }}";
            
            var options = $"'{part}'";
            
            if ( key != null )
            {
                options += $",'{key}'";
            }

            template = template.Replace( "<url>", url );
            template = template.Replace( "<options>", options );

            TestHelper.AssertTemplateOutput( expected, template );
        }

        /// <summary>
        /// Success text is appended when input text contains value.
        /// </summary>
        //[TestMethod]
        //public void Url_InputTextContainsValue_SuccessTextIsAppended()
        //{
        //    _Helper.AssertTemplateOutput( "Ted, are you interested in baptism?", "{{ 'Ted' | WithFallback:', are', 'Are' }} you interested in baptism?" );
        //}

        /// <summary>
        /// Success text is appended when input text contains value.
        /// </summary>
        [TestMethod]
        public void WithFallback_InputTextContainsValue_SuccessTextIsAppended()
        {
            TestHelper.AssertTemplateOutput( "Ted, are you interested in baptism?", "{{ 'Ted' | WithFallback:', are', 'Are', 'append' }} you interested in baptism?" );
        }

        /// <summary>
        /// Fallback text is appended when input text is empty.
        /// </summary>
        [TestMethod]
        public void WithFallback_InputTextIsEmpty_FallbackTextIsAppended()
        {
            TestHelper.AssertTemplateOutput( "Are you interested in baptism?", "{{ '' | WithFallback:', are', 'Are' }} you interested in baptism?" );
        }
        /// <summary>
        /// Success text is prepended when input text contains value.
        /// </summary>
        [TestMethod]
        public void WithFallback_InputTextContainsValue_SuccessTextIsPrepended()
        {
            TestHelper.AssertTemplateOutput( "Welcome back Ted!", "Welcome{{ 'Ted' | WithFallback:' back ', ' stranger', 'prepend' }}!" );
        }

        /// <summary>
        /// Fallback text is prepended when input text is empty.
        /// </summary>
        [TestMethod]
        public void WithFallback_InputTextIsEmpty_FallbackTextIsPrepended()
        {
            TestHelper.AssertTemplateOutput( "Welcome stranger!", "Welcome{{ '' | WithFallback:' back ', ' stranger', 'prepend' }}!" );
        }

    }
}
