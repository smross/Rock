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
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Irony.Parsing;

namespace Rock.Lava.Fluid
{
    /// <summary>
    /// A wrapper for a Lava Tag that enables it to be rendered by the Fluid templating engine.
    /// </summary>
    /// <remarks>
    /// This implementation allows a set of factory methods to be registered, and subsequently used to 
    /// generate instances of Fluid Tag elements dynamically at runtime.
    /// The FluidTagProxy wraps a LavaTag that is executed internally to render the element content.
    /// This approach allows the LavaTag to be more easily adapted for use with alternative Liquid templating engines.
    /// </remarks>
    internal class FluidTagProxy : global::Fluid.Tags.ITag, ILiquidFrameworkRenderer
    {
        #region Static factory methods

        private static Dictionary<string, Func<string, IRockLavaTag>> _factoryMethods = new Dictionary<string, Func<string, IRockLavaTag>>( StringComparer.OrdinalIgnoreCase );

        public static void RegisterFactory( string name, Func<string, IRockLavaTag> factoryMethod )
        {
            if ( string.IsNullOrWhiteSpace( name ) )
            {
                throw new ArgumentException( "Name must be specified." );
            }

            name = name.Trim().ToLower();

            _factoryMethods[name] = factoryMethod;
        }

        #endregion

        private IRockLavaTag _lavaTag = null;

        public string SourceElementName
        {
            get
            {
                return _lavaTag.SourceElementName;
            }
        }

        #region Fluid ITag Implementation

        public Statement Parse( ParseTreeNode node, ParserContext context )
        {
            // Get the tag instance.
            var tagName = node.Term.Name;

            var factoryMethod = _factoryMethods[tagName];

            _lavaTag = factoryMethod( tagName );

            // When this element is rendered, write the content to the output stream.
            return new DelegateStatement( WriteToAsync );
        }

        /// <summary>
        /// Retrieve the syntax rules for the argument markup in this element tag.
        /// The syntax is defined by a set of Irony.NET grammar rules that Fluid uses to parse the tag.
        /// </summary>
        /// <param name="grammar"></param>
        /// <returns></returns>
        public BnfTerm GetSyntax( FluidGrammar grammar )
        {
            // Lava syntax uses whitespace as a separator between arguments, which Fluid/Irony does not support.
            // Therefore we return a syntax for this element that captures the entire argument list as a single token
            // and we will then parse the arguments list ourselves.
            //var filterArguments = new NonTerminal( "filterArguments" );

            var lavaArgumentList = new FreeTextLiteral( "lavaElementAttributesMarkup", FreeTextOptions.AllowEmpty | FreeTextOptions.AllowEof, "%}" );

            // Return a syntax that allows an empty arguments list, a comma-delimited list per the standard Fluid implementation,
            // or a whitespace-delimited list to support Lava syntax.
            return grammar.Empty | grammar.FilterArguments.Rule | lavaArgumentList;
        }

        //public BnfTerm GetSyntax( FluidGrammar grammar )
        //{
        //    // The grammar for an inline tag element allows for zero or more arguments.
        //    // TODO: Modify the grammar rule to make the argument separator (comma) optional.
        //    return grammar.Empty | grammar.FilterArguments.Rule;
        //}

        public ValueTask<Completion> WriteToAsync( TextWriter writer, TextEncoder encoder, TemplateContext context )
        {
            var lavaContext = new FluidLavaContext( context );

            var tag = _lavaTag as ILiquidFrameworkRenderer;

            if ( tag == null )
            {
                throw new Exception( "Tag proxy cannot be rendered." );
            }

            // Call the renderer implemented by the wrapped Lava block.
            tag.Render( this, lavaContext, writer );

            return new ValueTask<Completion>( Completion.Normal );
        }

        #endregion

        #region IRockLavaTag implementation

        void ILiquidFrameworkRenderer.Render( ILiquidFrameworkRenderer baseRenderer, ILavaContext context, TextWriter result )
        {
            var fluidContext = ( (FluidLavaContext)context ).FluidContext;

            this.WriteToAsync( result, HtmlEncoder.Default, fluidContext );
        }

        public void OnStartup()
        {
            throw new NotImplementedException( "The OnStartup method is not a valid operation for the DotLiquidTagProxy." );
        }

        void ILiquidFrameworkRenderer.Parse( ILiquidFrameworkRenderer baseRenderer, List<string> tokens, out List<object> nodes )
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
