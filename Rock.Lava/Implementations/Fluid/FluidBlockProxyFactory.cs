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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Irony.Parsing;

namespace Rock.Lava.Fluid
{
    /// <summary>
    /// A wrapper for a Lava Block that enables it to be rendered by the Fluid templating engine.
    /// </summary>
    /// <remarks>
    /// This implementation allows a set of factory methods to be registered, and subsequently used to 
    /// generate instances of Fluid Block elements dynamically at runtime.
    /// The FluidBlockProxy wraps a LavaBlock that is executed internally to render the element content.
    /// This approach allows the LavaBlock to be more easily adapted for use with alternative Liquid templating engines.
    /// </remarks>
    internal class FluidBlockProxyFactory : global::Fluid.Tags.ITag, ILiquidFrameworkElementRenderer
    {
        #region Static factory methods

        private static Dictionary<string, Func<string, IRockLavaBlock>> _factoryMethods = new Dictionary<string, Func<string, IRockLavaBlock>>( StringComparer.OrdinalIgnoreCase );

        public static void RegisterFactory( string name, Func<string, IRockLavaBlock> factoryMethod )
        {
            if ( string.IsNullOrWhiteSpace( name ) )
            {
                throw new ArgumentException( "Name must be specified." );
            }

            name = name.Trim().ToLower();

            _factoryMethods[name] = factoryMethod;
        }

        #endregion

        private IRockLavaBlock _lavaBlock = null;

        public string SourceElementName
        {
            get
            {
                return _lavaBlock.SourceElementName;
            }
        }

        #region ITag Implementation

        /// <summary>
        /// Retrieve the syntax rules for the argument markup in this element tag.
        /// The syntax is defined by a set of Irony.NET grammar rules that Fluid uses to parse the tag.
        /// </summary>
        /// <param name="grammar"></param>
        /// <returns></returns>
        public BnfTerm GetSyntax( FluidGrammar grammar )
        {
            var blockEndTag = "%}";
            
            // Lava syntax uses whitespace as a separator between arguments, which Fluid/Irony does not support.
            // Therefore we return a syntax for this element that captures the entire argument list as a single token
            // and we will then parse the arguments list later in the process.
            var lavaArgumentList = new FreeTextLiteral( "lavaElementAttributesMarkup", FreeTextOptions.AllowEmpty | FreeTextOptions.AllowEof, blockEndTag );

            // Return a syntax that allows an empty arguments list, a comma-delimited list per the standard Fluid implementation,
            // or a freetext string to support the whitespace-delimited argument list used by Lava syntax.
            return grammar.Empty | grammar.FilterArguments.Rule | lavaArgumentList;
        }

        public Statement Parse( ParseTreeNode node, ParserContext context )
        {
            /* The Fluid framework parses the block into Liquid tokens using an adapted Irony.Net grammar.
             * Lava uses some syntax that is not recognized by Liquid, so we need to do some parsing of our own.
             * Also, some Lava blocks are designed to parse the raw source text of the block.
             * To maintain compatibility with that design, our parse process involves these steps:
             * 1. Extract the whitespace-delimited argument list from the open tag and parse it here.
             * 2. Extract text tokens from the source document, to allow Lava blocks to parse the raw source text.
             * The Lava parsing process is deferred to the Fluid rendering phase, because we need access to the template context that holds the template source document.
             *
             * Here, we simply add a Fluid statement to call a method to parse and render the block.
             * 
             * The source text will be passed to the Lava block during the rendering phase so it can be parsed and rendered at the same time.
            */
            //_rootNode = node;

            var blockName = node.Term.Name;
            
            var factoryMethod = _factoryMethods[blockName];

            _lavaBlock = factoryMethod( blockName );

            var argsNode = context.CurrentBlock.Tag.ChildNodes[0].ChildNodes[0];

            var elementAttributesMarkup = argsNode.FindTokenAndGetText().Trim();

            // These are the statements that are contained in the body of the block, not including the opening and closing tags.
            _statements = context.CurrentBlock.Statements;

            // TODO: Extract identifying info/position from here?
            var thisTag = context.CurrentBlock.Tag;

            var nodeStartPosition = node.Span.Location.Position;

            var renderBlockDelegate = new DelegateStatement( ( writer, encoder, ctx ) => WriteToAsync( writer, encoder, ctx, elementAttributesMarkup, _statements, node ) );

            return renderBlockDelegate; 
        }

        // TODO: Tidy up

        /*

        /// <summary>
        /// Parses the specified tokens.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        private string GetBlockMarkup( string tagName, string templateMarkup, int tagStartIndex )
        {
            // Get the block markup. The list of tokens contains all of the lava from the start tag to
            // the end of the template. This will pull out just the internals of the block.

            // We must take into consideration nested tags of the same type

            // This is similar logic to the Shortcodes, but the tag regex are different. Attempted to refactor to a reusable helper, but it needs
            // access to a lot of the internals of the command.
            //var _blockMarkup = new StringBuilder();

            //var endTagFound = false;

            var startTag = $@"{{\%\s*{ tagName }\s*(.*?)\%}}";
            var endTag = $@"{{\%\s*end{ tagName }\s*\%}}";

            var openTags = 0;

            Regex regExStart = new Regex( startTag );
            Regex regExEnd = new Regex( endTag );

            int currentIndex = tagStartIndex;
            int lastIndex = templateMarkup.Length - 1;
            int endTagIndex = -1;
            int blockStartIndex = 0;
            int blockEndIndex = 0;

            while ( currentIndex < lastIndex )
            {
                Match startTagMatch = regExStart.Match( templateMarkup, currentIndex );

                if ( startTagMatch.Success )
                {
                    if ( startTagMatch.Index > lastIndex )
                    {
                        continue;
                    }

                    if ( openTags == 0 )
                    {
                        blockStartIndex = startTagMatch.Index + startTagMatch.Length;
                    }

                    openTags++;

                    currentIndex += startTagMatch.Length;
                }
                else
                {
                    Match endTagMatch = regExEnd.Match( templateMarkup, currentIndex );

                    if ( endTagMatch.Success )
                    {
                        if ( endTagMatch.Index > lastIndex )
                        {
                            continue;
                        }
                       
                        if ( openTags > 0 )
                        {
                            openTags--; // decrement the child tag counter
                        }
                        else
                        {
                            blockEndIndex = endTagMatch.Index;
                            endTagIndex = endTagMatch.Index;

                            break;
                        }

                        currentIndex += endTagMatch.Length;
                    }
                    else
                    {
                        currentIndex++;
                    }
                }
            }

            string blockMarkup;

            if ( endTagIndex > blockStartIndex )
            {
                blockMarkup = templateMarkup.Substring( blockStartIndex, blockEndIndex - blockStartIndex );
            }
            else
            {
                throw new LavaException( "Block syntax error. Missing end tag." );
            }

            return blockMarkup;
        }

        */

        private List<Statement> _statements = null;

        private ValueTask<Completion> WriteToAsync( TextWriter writer, TextEncoder encoder, TemplateContext context, string elementAttributesMarkup, List<Statement> statements, ParseTreeNode node )
        {
            var lavaContext = new FluidLavaContext( context );
            
            var sourceElements = lavaContext.GetInternalFieldValue( Constants.ContextKeys.SourceTemplateElements ) as List<FluidParsedTemplateElement> ?? new List<FluidParsedTemplateElement>();

            // Get the tokens associated with this block
            var startPosition = node.Span.Location.Position;
            var endPosition = node.Span.EndPosition;

            var tokens = new List<string>();
            bool addElements = false;

            var lastStatement = statements.LastOrDefault();

                for ( int i = 0; i < sourceElements.Count; i++ )
                {
                    var element = sourceElements[i];

                    if ( element.StartIndex < startPosition && element.EndIndex >= endPosition )
                    {
                        addElements = true;
                    }

                    if ( addElements )
                    {
                        tokens.Add( element.Node );
                    }

                    if ( lastStatement == null
                         || element.Statement == lastStatement )
                    {
                        // Add the element for the closing tag, which does not have an associated statement.
                        tokens.Add( sourceElements[i + 1].Node );

                        break;
                    }
            }

            var elementRenderer = _lavaBlock as ILiquidFrameworkElementRenderer;

            if ( elementRenderer == null )
            {
                throw new Exception( "Block proxy cannot be rendered." );
            }

            var tagName = node.Term.Name;

            _lavaBlock.OnInitialize( tagName, elementAttributesMarkup, tokens );

            _lavaBlock.OnParsed( tokens );

            elementRenderer.Render( this, lavaContext, writer, encoder );

            return new ValueTask<Completion>( Completion.Normal );
        }

        #endregion

        private async ValueTask<Completion> WriteToDefaultAsync( TextWriter writer, TextEncoder encoder, TemplateContext context, List<Statement> statements )
        {
            Completion completion;

            foreach ( var statement in statements )
            {
                completion = await statement.WriteToAsync( writer, encoder, context );

                if ( completion != Completion.Normal )
                {
                    // Stop processing the block statements
                    return completion;
                }
            }

            return Completion.Normal;
        }

        #region ILiquidFrameworkRenderer implementation

        void ILiquidFrameworkElementRenderer.Render( ILiquidFrameworkElementRenderer baseRenderer, ILavaContext context, TextWriter writer, TextEncoder encoder )
        {
            var fluidContext = ( (FluidLavaContext)context ).FluidContext;

            var result = WriteToDefaultAsync( writer, HtmlEncoder.Default, fluidContext, _statements );
        }

        void ILiquidFrameworkElementRenderer.Parse( ILiquidFrameworkElementRenderer baseRenderer, List<string> tokens, out List<object> nodes )
        {
            // TODO: May need to rework this?
            // Make sure we are getting a list of text-based tokens here.
            // We should be returning a list of Fluid statements as nodes.

            //base.Parse( tokens );

            //nodes = base.NodeList;

            nodes = null;
        }

        #endregion
    }
}
