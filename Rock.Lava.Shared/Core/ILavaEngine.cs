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

namespace Rock.Lava
{
    public enum LavaEngineTypeSpecifier
    {
        // DotLiquid is an open-source implementation of the Liquid templating language. [https://github.com/dotliquid/dotliquid]
        DotLiquid = 1,
        // Fluid is an open-source implementation of the Liquid templating language. [https://github.com/sebastienros/fluid]
        Fluid = 2
    }

    /// <summary>
    /// Represents the Lava Engine that is responsible for compiling and rendering templates.
    /// </summary>
    public interface ILavaEngine
    {
        /// <summary>
        /// Gets or sets a flag to determine if compiled Lava templates can be cached and reused.
        /// </summary>
        //bool TemplateCachingIsEnabled { get; set; }

        /// <summary>
        /// Remove all items from the template cache.
        /// </summary>
        void ClearTemplateCache();

        ILavaTemplateCacheService TemplateCacheService { get; }

        /// <summary>
        /// Set configuration options for the Lava engine.
        /// </summary>
        /// <param name="options"></param>
        void Initialize( LavaEngineConfigurationOptions options = null );

        /// <summary>
        /// The descriptive name of the templating framework on which Lava is currently operating.
        /// </summary>
        string EngineName { get; }

        /// <summary>
        /// The Liquid template framework used to parse and render Lava templates.
        /// </summary>
        LavaEngineTypeSpecifier EngineType { get; }

        /// <summary>
        /// Get a new context instance containing the specified merge values.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        ILavaContext NewContext( IDictionary<string, object> values = null );

        /// <summary>
        /// Register a Lava Tag elemennt.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryMethod"></param>
        void RegisterTag( string name, Func<string, IRockLavaTag> factoryMethod );

        /// <summary>
        /// Register a Lava Block element.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryMethod"></param>
        void RegisterBlock( string name, Func<string, IRockLavaBlock> factoryMethod );

        /// <summary>
        /// Registers a shortcode with a factory method that provides the definition of the shortcode on demand.
        /// A dynamic shortcode is defined in the active Rock database, and its associated template can be modified at runtime.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryMethod"></param>
        void RegisterDynamicShortcode( string name, Func<string, DynamicShortcodeDefinition> factoryMethod );

        /// <summary>
        /// Registers a static shortcode with a factory method that provides the shortcode definition.
        /// A static shortcode is defined by a code component and its associated template cannot be modified at runtime.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryMethod"></param>
        void RegisterStaticShortcode( string name, Func<string, IRockShortcode> factoryMethod );

        /// <summary>
        /// Registers a shortcode with a factory method that provides the name and definition of the shortcode dynamically.
        /// </summary>
        /// <param name="factoryMethod"></param>
        //void RegisterStaticShortcode( Func<string, IRockShortcode> factoryMethod );

        /// <summary>
        /// Deregister a shortcode.
        /// </summary>
        /// <param name="name"></param>
        void UnregisterShortcode( string name );

        /// <summary>
        /// Gets the collection of all registered Lava document elements.
        /// </summary>
        /// <returns></returns>
        Dictionary<string, ILavaElementInfo> GetRegisteredElements();

        /// <summary>
        /// Try to render the provided template.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        bool TryRender( string inputTemplate, out string output );

        /// <summary>
        /// Try to render the provided template with the specified merge fields.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="output"></param>
        /// <param name="mergeValues"></param>
        /// <returns></returns>
        bool TryRender( string inputTemplate, out string output, LavaDictionary mergeValues );

        /// <summary>
        /// Try to render the provided template in the specified context.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="output"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        bool TryRender( string inputTemplate, out string output, ILavaContext context );

        /// <summary>
        /// Try to render a compiled Lava template using the specified parameters.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="parameters"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        bool TryRender( ILavaTemplate inputTemplate, LavaRenderParameters parameters, out string output );

        /// <summary>
        /// Register a type that can be referenced in a template during the rendering process.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="allowedMembers"></param>
        void RegisterSafeType( Type type, string[] allowedMembers = null );

        /// <summary>
        /// Try to parse the provided template.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        bool TryParseTemplate( string inputTemplate, out ILavaTemplate template );

        /// <summary>
        /// Parse the provided template.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns>A compiled template object.</returns>
        ILavaTemplate ParseTemplate( string inputTemplate );

        /// <summary>
        /// Compare two objects for equivalence according to the applicable Lava equality rules for the input object types.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>True if the two objects are considered equal.</returns>
        bool AreEqualValue( object left, object right );

        /// <summary>
        /// Gets or sets the strategy for handling exceptions encountered during the rendering process.
        /// </summary>
        ExceptionHandlingStrategySpecifier ExceptionHandlingStrategy { get; set; }
    }

    /// <summary>
    /// Specifies a strategy for handling exceptions encountered during the template rendering process.
    /// </summary>
    public enum ExceptionHandlingStrategySpecifier
    {
        Throw = 0,
        RenderToOutput = 1,
        Ignore = 2

    }
}
