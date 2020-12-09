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
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Irony.Parsing;
using Microsoft.Extensions.Primitives;
using Rock.Lava.Blocks;

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
    internal class FluidBlockProxy : ITagEx, ILiquidFrameworkElementRenderer
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

        //private ParseTreeNode _rootNode = null;

        public Statement Parse( ParseTreeNode node, ParserContext context )
        {
            throw new NotImplementedException();
        }

        public Statement Parse( ParseTreeNode node, LavaFluidParserContext context )
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

            var nodeStartPosition = context.CurrentBlock.AdditionalData.StartPosition;

            var blockInnerText = context.CurrentBlock.AdditionalData.InnerText;

            string nodeGuid; //= (string)node.Tag; //.ToString();

            if ( node.Tag == null )
            {
                nodeGuid = Guid.NewGuid().ToString();
                node.Tag = nodeGuid;
            }
            else
            {
                nodeGuid = (string)node.Tag;
            }
            //node.Tag = nodeGuid;

            //var sourceElements = lavaContext.GetInternalValue( Constants.ContextKeys.SourceTemplateElements ) as List<FluidParsedTemplateElement> ?? new List<FluidParsedTemplateElement>();

            //var tokens = this.GetBlockTokens( sourceElements, blockTagGuid, statements.LastOrDefault() );


            //node.Tag = context.CurrentBlock..Tag. "Position= node.Span.Location.Position
            var renderBlockDelegate = new DelegateStatement( ( writer, encoder, ctx ) => WriteToAsync( writer, encoder, ctx, elementAttributesMarkup, _statements, blockInnerText, node, nodeGuid, nodeStartPosition ) );

            return renderBlockDelegate; 
        }

        private List<Statement> _statements = null;

        private List<string> GetBlockTokens( List<FluidParsedTemplateElement> sourceElements, string blockTagGuid, Statement lastStatement )
        {
            //var sourceTemplate = lavaContext.GetInternalValue( Constants.ContextKeys.SourceTemplateText ) as string ?? string.Empty;
            //var sourceElements = lavaContext.GetInternalValue( Constants.ContextKeys.SourceTemplateElements ) as List<FluidParsedTemplateElement> ?? new List<FluidParsedTemplateElement>();

            // Initialize the DotLiquid block.
            //var tokens = new List<string>();

            // Get the tokens associated with this block
            var startPosition = 0; // node.Span.Location.Position;
            var endPosition = 0; // node.Span.EndPosition;

            var tokens = new List<string>();
            bool addElements = false;

            var testElement = sourceElements.FirstOrDefault( x => x.ElementId != null && x.ElementId.StartsWith( blockTagGuid.Substring( 0, 6 ), StringComparison.OrdinalIgnoreCase ) );
            var firstElement = sourceElements.FirstOrDefault( x => x.ElementId != null && x.ElementId == blockTagGuid );

            if ( firstElement != null )
            {
                startPosition = firstElement.StartIndex;
            }
            else
            {
                int i = 0;
            }

            //var firstStatement = statements.FirstOrDefault();

            //if ( firstStatement != null )
            //{
            //    var firstElement = sourceElements.FirstOrDefault( x => x.Statement == firstStatement );

            //    if ( firstElement != null )
            //    {
            //        //startPosition = firstElement.StartIndex;
            //    }
            //}

            //if ( elementAttributesMarkup == "url:'~/Scripts/Chartjs/Chart.min.js' id:'chartjs'" )
            //{
            //    int i = 0;
            //}

            //var lastStatement = statements.LastOrDefault();

            if ( lastStatement != null )
            {
                // Find the end of the node associated with the last statement in the block.
                //var last = 
            }

            //if ( firstStatement != null
            //   && lastStatement != null )
            //{
            for ( int i = 0; i < sourceElements.Count; i++ )
            {
                var element = sourceElements[i];

                if ( element.StartIndex < startPosition && element.EndIndex >= endPosition )
                {
                    addElements = true;

                    // Add the element for the opening tag, which does not have an associated statement.
                    //tokens.Add( sourceElements[i - 1].Node );
                }

                if ( addElements )
                {
                    tokens.Add( element.Node );
                }

                if ( lastStatement == null
                     || element.Statement == lastStatement )
                {
                    endPosition = element.EndIndex;

                    // Add the element for the closing tag, which does not have an associated statement.
                    tokens.Add( sourceElements[i + 1].Node );

                    break;
                }
                //}
            }

            return tokens;
        }



        private ValueTask<Completion> WriteToAsync( TextWriter writer, TextEncoder encoder, TemplateContext context, string elementAttributesMarkup, List<Statement> statements, string blockInnerText, ParseTreeNode node, string blockTagGuid, int nodeStartPosition )
        {
            var lavaContext = new FluidLavaContext( context );

            //var sourceElements = lavaContext.GetInternalValue( Constants.ContextKeys.SourceTemplateElements ) as List<FluidParsedTemplateElement> ?? new List<FluidParsedTemplateElement>();

            //var tokens = this.GetBlockTokens( sourceElements, blockTagGuid, statements.LastOrDefault() );

            List<object> nodes;

            /*
            var startTag = new Regex( $@"{{\%\s*", RegexOptions.RightToLeft );

            var openingTagMatch = startTag.Match( sourceTemplate, _rootNode.Span.Location.Position );

            if ( !openingTagMatch.Success )
            {
                throw new LavaException( "Opening tag not found." );
            }
            */

            var elementRenderer = _lavaBlock as ILiquidFrameworkElementRenderer;

            if ( elementRenderer == null )
            {
                throw new Exception( "Block proxy cannot be rendered." );
            }

            //var blockText = GetBlockMarkup( _lavaBlock.InternalElementName, sourceTemplate, openingTagMatch.Index );



            //xyzyy: Need to get the tokens and pass them in here.
            //_lavaBlock.OnParse( tokens, out nodes );

            //_lavaBlock.SetSourceMarkup( blockText );
            //var blockBase = _lavaBlock as RockLavaBlockBase;

            //if ( blockBase != null )
            //{
            //    blockBase.SourceText = blockText;
            //}

            var tokens = new List<string> { blockInnerText };

            var tagName = node.Term.Name; // _lavaBlock.InternalElementName

            _lavaBlock.OnInitialize( tagName, elementAttributesMarkup, tokens );

            _lavaBlock.OnParse( tokens, out nodes );

            elementRenderer.Render( this, lavaContext, writer );

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

        void ILiquidFrameworkElementRenderer.Render( ILiquidFrameworkElementRenderer baseRenderer, ILavaContext context, TextWriter writer )
        {
            var fluidContext = ( (FluidLavaContext)context ).FluidContext;

            var result = WriteToDefaultAsync( writer, HtmlEncoder.Default, fluidContext, _statements );
        }

        public void OnStartup()
        {
            throw new NotImplementedException( "The OnStartup method is not a valid operation for the DotLiquidBlockProxy." );
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

                public List<string> GetTokenTreeFromParseTree( ParseTreeNode node, int index = 0, int level = 0 )
                {
                    //for ( var levelIndex = 0; levelIndex < level; levelIndex++ )
                    //{
                    //    Console.Write( "\t" );
                    //}
                    var nodes = new List<string>();

                    if ( node.IsPunctuationOrEmptyTransient() )
                    {
                        nodes.Add( node.ToString() );
                    }
                    else if ( node.Term != null )
                    {
                        nodes.Add( node.Term.Name );
                    }
                    else if ( node.Token != null )
                    {
                        nodes.Add( node.Token.Text );
                    }
                    else
                    {
                        nodes.Add( node.ToString() );
                    }


                    var childIndex = 0;

                    foreach ( var child in node.ChildNodes )
                    {
                        var childNodes = GetTokenTreeFromParseTree( child, childIndex, level + 1 );

                        nodes.AddRange( childNodes );

                        childIndex++;
                    }

                    return nodes;
                }

                /// <summary>
                /// Parser extension methods
                /// </summary>
                //public static class ParserExt
                //{
                    /// <summary>
                    /// Converts parser nodes tree to flat collection
                    /// </summary>
                    /// <param name="item"></param>
                    /// <param name="childSelector"></param>
                    /// <returns></returns>
                    private IEnumerable<ParseTreeNode> Traverse( ParseTreeNode item, Func<ParseTreeNode, IEnumerable<ParseTreeNode>> childSelector )
                    {
                        var stack = new Stack<ParseTreeNode>();
                        stack.Push( item );
                        while ( stack.Any() )
                        {
                            var next = stack.Pop();
                            yield return next;

                            var childs = childSelector( next ).ToList();
                            for ( var childId = childs.Count - 1; childId >= 0; childId-- )
                            {
                                stack.Push( childs[childId] );
                            }
                        }
                    }
                //}
        */

    }
}
