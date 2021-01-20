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
using DotLiquid;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

using Rock.Common;

namespace Rock.Lava.DotLiquid
{
    /// <summary>
    /// A Lava wrapper for the DotLiquid implementation of a Liquid Template.
    /// </summary>
    [Obsolete("Not needed")]
    public class DotLiquidTemplateProxy : LavaTemplateBase
    {
        #region Constructors

        // A parsed DotLiquid template.
        private Template _dotLiquidTemplate;

        public DotLiquidTemplateProxy( Template template )
        {
            _dotLiquidTemplate = template;            
        }

        #endregion

        public Template DotLiquidTemplate
        {
            get
            {
                return _dotLiquidTemplate;
            }
        }

        /// <summary>
        /// Try to render the template using the supplied context variables.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        //protected override bool OnTryRender( LavaRenderParameters parameters, out string output, out IList<Exception> errors )
        //{
        //    //var values = new Dictionary<string, object>();

        //    //if ( parameters.LocalVariables != null )
        //    //{
        //    //    values.AddOrReplace( parameters.LocalVariables );
        //    //}
        //    //if ( parameters.Registers != null )
        //    //{
        //    //    values.AddOrReplace( parameters.Registers );
        //    //}
        //    //if ( parameters.InstanceAssigns != null )
        //    //{
        //    //    values.AddOrReplace( parameters.InstanceAssigns );
        //    //}

        //    //var context = new Context();

        //    //context.Merge( Hash.FromDictionary( values ) );

        //    // The DotLiquid rendering engine ignores the LocalVariables and Registers parameters when using thread-safe Templates.
        //    // Rock requires all templates to be thread-safe, so we need to supply a DotLiquid context here rather than using the LocalVariables and Registers parameters.
        //    var dotLiquidRenderParameters = new RenderParameters();

        //    var dotliquidContext = parameters.LavaContext as DotLiquidLavaContext;

        //    dotLiquidRenderParameters.Context = dotliquidContext.DotLiquidContext;

        //    if ( parameters.ShouldEncodeStringsAsXml )
        //    {
        //        dotLiquidRenderParameters.ValueTypeTransformers = new Dictionary<Type, Func<object, object>>();
        //        dotLiquidRenderParameters.ValueTypeTransformers[typeof( string )] = EncodeStringTransformer;
        //    }

        //    return TryRenderInternal( dotLiquidRenderParameters, out output, out errors );
        //}


        //if ( encodeStrings )
        //{
        //    // if encodeStrings = true, we want any string values to be XML Encoded                    
        //    renderParameters.ValueTypeTransformers = new Dictionary<Type, Func<object, object>>();
        //    renderParameters.ValueTypeTransformers[typeof( string )] = EncodeStringTransformer;
        //}

        //private bool TryRenderInternal( RenderParameters dotLiquidRenderParameters, out string output, out IList<Exception> errors )
        //{
        //    // Add the enabled commands for this template to those that are allowed in the current context.
        //    if ( this.EnabledCommands != null )
        //    {
        //        var enabledCommands = dotLiquidRenderParameters.Context.Registers["EnabledCommands"].ToStringSafe().SplitDelimitedValues( "," );

        //        enabledCommands.Concat( this.EnabledCommands );

        //        dotLiquidRenderParameters.Context.Registers["EnabledCommands"] = enabledCommands.Distinct().JoinStrings( "," );
        //    }

        //    //dotLiquidRenderParameters.Registers = new Hash();

        //    output = _dotLiquidTemplate.Render( dotLiquidRenderParameters );

        //    errors = _dotLiquidTemplate.Errors;

        //    return !errors.Any();
        //}

        public void Dispose()
        {
            //
        }
    }
}
