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
    public class DotLiquidTemplateProxy : LavaTemplateBase
    {
        #region Constructors

        // A parsed DotLiquid template.
        private Template _dotLiquidTemplate;

        private Hash _Context = new Hash();

        public DotLiquidTemplateProxy( Template template )
        {
            _dotLiquidTemplate = template;            
        }

        #endregion

        //public override ILavaEngine LavaEngine
        //{
        //    get
        //    {
        //        return global::Rock.Lava.LavaEngine.Instance;
        //    }
        //}

        //[Obsolete]
        //public override void SetContextValue( string key, object value )
        //{
        //    var newValue = ConvertExpandoObjectsToDictionaries( value );

        //    _Context.AddOrReplace( key, newValue );
        //}

        /// <summary>
        /// Try to render the template using the supplied context variables.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        protected override bool OnTryRender( LavaRenderParameters parameters, out string output, out IList<Exception> errors )
        {
            var values = new Dictionary<string, object>();

            if ( parameters.LocalVariables != null )
            {
                values.AddOrReplace( parameters.LocalVariables );
            }
            if ( parameters.Registers != null )
            {
                values.AddOrReplace( parameters.Registers );
            }
            if ( parameters.InstanceAssigns != null )
            {
                values.AddOrReplace( parameters.InstanceAssigns );
            }

            // The DotLiquid rendering engine ignores the LocalVariables and Registers parameters when using thread-safe Templates.
            // Rock requires all templates to be thread-safe, so we need to supply a Liquid context here rather than using the LocalVariables and Registers parameters.
            var dotLiquidRenderParameters = new RenderParameters();

            dotLiquidRenderParameters.Context = new Context();

            dotLiquidRenderParameters.Context.Merge( Hash.FromDictionary( values ) );

            //dotLiquidRenderParameters.LocalVariables = Hash.FromDictionary( parameters.LocalVariables );
            //dotLiquidRenderParameters.Registers = Hash.FromDictionary( parameters.Registers );

            // Store the EnabledCommands setting for the context and the template in the DotLiquid Registers collection.
            // Registers are internal variables that are not exposed in the template during the rendering process.
            var enabledCommands = new List<string>();

            if ( this.EnabledCommands != null )
            {
                enabledCommands.AddRange( this.EnabledCommands );
            }

            if ( parameters.EnabledCommands != null )
            {
                enabledCommands.AddRange( parameters.EnabledCommands );
            }

            dotLiquidRenderParameters.Registers = new Hash();

            dotLiquidRenderParameters.Context.Registers["EnabledCommands"] = enabledCommands.Distinct().JoinStrings( "," );

            output = _dotLiquidTemplate.Render( dotLiquidRenderParameters );

            errors = _dotLiquidTemplate.Errors;

            return !errors.Any();
        }

        /// <summary>
        /// This method is specifically used to convert an <see cref="ExpandoObject"/> with a Tree structure to a <see cref="Dictionary{string, object}"/>.
        /// </summary>
        /// <param name="expando">The <see cref="ExpandoObject"/> to convert</param>
        /// <returns>The fully converted <see cref="ExpandoObject"/></returns>
        //private object ConvertExpandoObjectsToDictionaries( object input )
        //{
        //    // Replace ExpandoObject collection.
        //    var expandoCollection = input as IEnumerable<ExpandoObject>;

        //    if ( expandoCollection != null )
        //    {
        //        // Replace with a list of Dictionary<string, object>
        //        var newList = new List<Dictionary<string, object>>();
                
        //        foreach (var expandoElement in expandoCollection)
        //        {
        //            var newDictionary = ConvertExpandoObjectsToDictionaries( expandoElement ) as Dictionary<string, object>;

        //            newList.Add( newDictionary );
        //        }

        //        return newList;
        //    }

        //    // Replace ExpandoObject.
        //    var expando = input as ExpandoObject;

        //    if ( expando != null )
        //    {
        //        // Replace with a list of Dictionary<string, object>
        //        var newValue = new Dictionary<string, object>( expando );

        //        return ConvertExpandoObjectsToDictionaries( newValue );
        //    }
            
        //    var dictionary = input as IDictionary<string, object>;

        //    if ( dictionary != null )
        //    {
        //        var replacementItems = new Dictionary<string, object>();

        //        foreach ( var kvp in dictionary )
        //        {
        //            var newValue = ConvertExpandoObjectsToDictionaries( kvp.Value );

        //            if ( newValue != kvp.Value )
        //            {
        //                replacementItems.Add( kvp.Key, newValue );
        //            }
        //        }

        //        if ( replacementItems.Any() )
        //        {
        //            foreach (var kvp in replacementItems )
        //            {
        //                dictionary.AddOrReplace( kvp.Key, kvp.Value );
        //            }
        //        }

        //        return dictionary;
        //    }

        //    return input;
        //}

        /// <summary>
        /// This method takes an <see cref="IDictionary{string, object}"/> and recursively converts it to a <see cref="Dictionary{string, object}"/>. 
        /// The idea is that every <see cref="IDictionary{string, object}"/> in the tree will be of type <see cref="Dictionary{string, object}"/> instead of some other implementation like <see cref="ExpandoObject"/>.
        /// </summary>
        /// <param name="value">The <see cref="IDictionary{string, object}"/> to convert</param>
        /// <returns>The fully converted <see cref="Dictionary{string, object}"/></returns>
        //private Dictionary<string, object> RecursivelyConvertIDictToDict( IDictionary<string, object> value )
        //{
        //    var newDictionary = value.ToDictionary(
        //        keySelector => keySelector.Key,
        //        elementSelector =>
        //        {
        //            // if it's another IDict just go through it recursively
        //            if ( elementSelector.Value is IDictionary<string, object> dict )
        //            {
        //                return RecursivelyConvertIDictToDict( dict );
        //            }

        //            // if it's an IEnumerable check each element
        //            if ( elementSelector.Value is IEnumerable<object> list )
        //            {
        //                // go through all objects in the list
        //                // if the object is an IDict -> convert it
        //                // if not keep it as is
        //                return list
        //                            .Select( o => o is IDictionary<string, object>
        //                         ? RecursivelyConvertIDictToDict( ( IDictionary<string, object> ) o )
        //                         : o
        //                            );
        //            }

        //            // neither an IDict nor an IEnumerable -> it's fine to just return the value it has
        //            return elementSelector.Value;
        //        }
        //    );

        //    return newDictionary;
        //}

        public void Dispose()
        {
            //
        }
    }
}
