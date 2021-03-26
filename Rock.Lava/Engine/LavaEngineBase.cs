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

namespace Rock.Lava
{
    /// <summary>
    /// Provides base functionality for an engine that can parse and render Lava Templates.
    /// </summary>
    public abstract class LavaEngineBase : ILavaEngine
    {
        private List<string> _defaultEnabledCommands = new List<string>();

        /// <summary>
        /// Initializes the Lava engine with the specified options.
        /// </summary>
        public void Initialize( LavaEngineConfigurationOptions options )
        {
            if ( options == null )
            {
                options = new LavaEngineConfigurationOptions();
            }

            // Connect the cache service to this Lava Engine instance.
            _cacheService = options.CacheService;

            if ( _cacheService != null )
            {
                _cacheService.LavaEngine = this;
            }

            _defaultEnabledCommands = options.DefaultEnabledCommands;

            if ( options.ExceptionHandlingStrategy != null )
            {
                this.ExceptionHandlingStrategy = options.ExceptionHandlingStrategy.Value;
            }

            OnSetConfiguration( options );
        }

        /// <summary>
        /// The set of Lava commands that are enabled by default when a new context is created.
        /// </summary>
        public List<string> DefaultEnabledCommands
        {
            get
            {
                return _defaultEnabledCommands;
            }
            set
            {
                _defaultEnabledCommands = value ?? new List<string>();
            }
        }

        /// <summary>
        /// Override this method to set configuration options for the specific Liquid framework engine implementation.
        /// </summary>
        /// <param name="options"></param>
        public abstract void OnSetConfiguration( LavaEngineConfigurationOptions options );

        /// <summary>
        /// Gets the descriptive name of the current Liquid engine that is providing template parsing and rendering functionality for the Lava library.
        /// </summary>
        public abstract string EngineName { get; }

        /// <summary>
        /// Create a new template context.
        /// </summary>
        /// <returns></returns>
        public ILavaRenderContext NewRenderContext()
        {
            var context = OnCreateRenderContext();

            if ( context == null )
            {
                throw new LavaException( "Failed to create a new render context." );
            }

            context.SetEnabledCommands( this.DefaultEnabledCommands );

            return context;
        }

        /// <summary>
        /// Create a new template context and add the specified merge fields.
        /// </summary>
        /// <param name="enabledCommands"></param>
        /// <returns></returns>
        public ILavaRenderContext NewRenderContext( IEnumerable<string> enabledCommands )
        {
            var context = OnCreateRenderContext();

            if ( context == null )
            {
                throw new LavaException( "Failed to create a new render context." );
            }

            enabledCommands = enabledCommands ?? this.DefaultEnabledCommands;

            context.SetEnabledCommands( enabledCommands );

            return context;
        }

        /// <summary>
        /// Create a new template context and add the specified merge fields.
        /// </summary>
        /// <param name="mergeFields"></param>
        /// <returns></returns>
        public ILavaRenderContext NewRenderContext( ILavaDataDictionary mergeFields, IEnumerable<string> enabledCommands = null )
        {
            var context = OnCreateRenderContext();

            if ( context == null )
            {
                throw new LavaException( "Failed to create a new render context." );
            }

            context.SetMergeFields( mergeFields );

            enabledCommands = enabledCommands ?? this.DefaultEnabledCommands;

            context.SetEnabledCommands( enabledCommands );

            return context;
        }

        /// <summary>
        /// Create a new template context and add the specified merge fields.
        /// </summary>
        /// <param name="mergeFields"></param>
        /// <returns></returns>
        public ILavaRenderContext NewRenderContext( IDictionary<string, object> mergeFields, IEnumerable<string> enabledCommands = null )
        {
            var context = OnCreateRenderContext();

            if ( context == null )
            {
                throw new LavaException( "Failed to create a new render context." );
            }

            context.SetMergeFields( mergeFields );

            enabledCommands = enabledCommands ?? this.DefaultEnabledCommands;

            context.SetEnabledCommands( enabledCommands );

            return context;
        }

        /// <summary>
        /// Create a new template context and add the specified merge fields.
        /// </summary>
        /// <param name="mergeFields"></param>
        /// <returns></returns>
        public ILavaRenderContext NewRenderContext( LavaDataDictionary mergeFields, IEnumerable<string> enabledCommands = null )
        {
            // This method exists as a convenience to disambiguate method calls using the LavaDataDictionary parameter, because
            //  it supports both the ILavaDataDictionary and IDictionary<string, object> interfaces.
            return NewRenderContext( (ILavaDataDictionary)mergeFields, enabledCommands );
        }

        /// <summary>
        /// Implement this method to provide a Liquid framework-specific instance of a new render context. 
        /// </summary>
        /// <returns></returns>
        protected abstract ILavaRenderContext OnCreateRenderContext();

        private ILavaTemplateCacheService _cacheService;

        /// <summary>
        /// Gets the current cache service for the Lava Engine.
        /// </summary>
        /// <returns>A reference to the current caching service, or null if caching is not configured.</returns>
        public ILavaTemplateCacheService TemplateCacheService
        {
            get
            {
                return _cacheService;
            }
        }

        /// <summary>
        /// Gets the type of third-party framework used to render and parse Lava/Liquid documents.
        /// </summary>
        public abstract LavaEngineTypeSpecifier EngineType { get; }

        /// <summary>
        /// Register a type that can be referenced in a template during the rendering process.
        /// </summary>
        /// <param name="type"></param>
        /// <remarks>
        /// The [LavaVisible] and [LavaHidden] custom attributes can be applied to determine the visibility of individual properties.
        /// If these attributes are not applied to any members of the type, all members are visible by default.
        /// </remarks>
        public void RegisterSafeType( Type type )
        {
            RegisterSafeType( type, null );
        }

        /// <summary>
        /// Register a type that can be referenced in a template during the rendering process.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="allowedMembers">
        /// The names of the properties that are visible to the Lava renderer.
        /// Specifying this parameter overrides the effect of any [LavaVisible] and [LavaHidden] custom attributes applied to the type.
        /// </param>
        public abstract void RegisterSafeType( Type type, IEnumerable<string> allowedMembers );

        /// <summary>
        /// Register a shortcode that is defined and implemented in code.
        /// </summary>
        /// <param name="shortcodeFactoryMethod"></param>
        public void RegisterStaticShortcode( Func<string, ILavaShortcode> shortcodeFactoryMethod )
        {
            var instance = shortcodeFactoryMethod( "default" );

            if ( instance == null )
            {
                throw new Exception( "Shortcode factory could not provide a valid instance for \"default\"." );
            }

            RegisterStaticShortcode( instance.SourceElementName, shortcodeFactoryMethod );
        }

        /// <summary>
        /// Register a shortcode that is defined and implemented in code.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="shortcodeFactoryMethod"></param>
        public void RegisterStaticShortcode( string name, Func<string, ILavaShortcode> shortcodeFactoryMethod )
        {
            var instance = shortcodeFactoryMethod( name );

            if ( instance == null )
            {
                throw new Exception( $"Shortcode factory could not provide a valid instance for \"{name}\" ." );
            }

            // Get a decorated name for the shortcode that will not collide with a regular tag name.
            var registrationKey = LavaUtilityHelper.GetLiquidElementNameFromShortcodeName( name );

            if ( instance.ElementType == LavaShortcodeTypeSpecifier.Inline )
            {
                var tagFactoryMethod = shortcodeFactoryMethod as Func<string, ILavaTag>;

                RegisterTag( registrationKey, tagFactoryMethod );
            }
            else
            {
                RegisterBlock( registrationKey, ( blockName ) =>
               {
                   // Get a shortcode instance using the provided shortcut factory.
                   var shortcode = shortcodeFactoryMethod( registrationKey );

                   // Return the shortcode instance as a RockLavaBlock
                   return shortcode as ILavaBlock;
               } );
                ;
            }
        }

        /// <summary>
        /// Register a shortcode that is defined in the data store.
        /// The definition of a dynamic shortcode can be changed at runtime.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="shortcodeFactoryMethod"></param>
        public void RegisterDynamicShortcode( string name, Func<string, DynamicShortcodeDefinition> shortcodeFactoryMethod )
        {
            // Create a default instance so we can retrieve the properties of the shortcode.
            var instance = shortcodeFactoryMethod( name );

            if ( instance == null )
            {
                throw new Exception( $"Shortcode factory could not provide a valid instance for \"{name}\" ." );
            }

            if ( instance.ElementType == LavaShortcodeTypeSpecifier.Inline )
            {
                // Create a new factory method that returns an initialized Shortcode Tag element.
                Func<string, ILavaTag> tagFactoryMethod = ( tagName ) =>
                {
                    var shortcodeInstance = GetShortcodeFromFactory<DynamicShortcodeTag>( tagName, shortcodeFactoryMethod );

                    return shortcodeInstance;
                };

                // Register the shortcode as a custom tag, but use a decorated registration name that will not collide with a regular element name.
                var registrationKey = LavaUtilityHelper.GetLiquidElementNameFromShortcodeName( name );

                RegisterTag( registrationKey, tagFactoryMethod );
            }
            else
            {
                // Create a new factory method that returns an initialized Shortcode Block element.
                Func<string, ILavaBlock> blockFactoryMethod = ( blockName ) =>
                {
                    // Call the factory method we have been passed to retrieve the definition of the shortcode.
                    // The definition may change at runtime, so we need to execute the factory method for each new shortcode instance.
                    var shortCodeName = LavaUtilityHelper.GetShortcodeNameFromLiquidElementName( blockName );

                    var shortcodeDefinition = shortcodeFactoryMethod( shortCodeName );

                    var shortcodeInstance = new DynamicShortcodeBlock( shortcodeDefinition );

                    return shortcodeInstance;
                };

                // Register the shortcode as a custom block, but use a decorated registration name that will not collide with a regular element name.
                var registrationKey = LavaUtilityHelper.GetLiquidElementNameFromShortcodeName( name );

                RegisterBlock( registrationKey, blockFactoryMethod );
            }
        }

        private T GetShortcodeFromFactory<T>( string shortcodeInternalName, Func<string, DynamicShortcodeDefinition> shortcodeFactoryMethod )
            where T : DynamicShortcode, new()
        {
            // Call the factory method we have been passed to retrieve the definition of the shortcode.
            // The definition may change at runtime, so we need to execute the factory method every time we create a new shortcode instance.
            var shortCodeName = LavaUtilityHelper.GetShortcodeNameFromLiquidElementName( shortcodeInternalName );

            var shortcodeDefinition = shortcodeFactoryMethod( shortCodeName );

            var shortcodeInstance = new T();

            shortcodeInstance.Initialize( shortcodeDefinition );

            return shortcodeInstance;
        }

        /// <summary>
        /// Render the provided template.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns>
        /// The rendered output of the template.
        /// If the template is invalid, returns an error message or an empty string according to the current ExceptionHandlingStrategy setting.
        /// </returns>
        public string RenderTemplate( string inputTemplate )
        {
            string output;
            List<Exception> errors;

            TryRenderTemplate( inputTemplate, mergeFields: null, output: out output, errors: out errors );

            return output;
        }

        /// <summary>
        /// Render the provided template in the specified context.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="context"></param>
        /// <returns>
        /// The rendered output of the template.
        /// If the template is invalid, returns an error message or an empty string according to the current ExceptionHandlingStrategy setting.
        /// </returns>
        public string RenderTemplate( string inputTemplate, ILavaRenderContext context )
        {
            string output;
            List<Exception> errors;

            TryRenderTemplate( inputTemplate, context, out output, out errors );

            return output;
        }

        /// <summary>
        /// Render the provided template in the specified context.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="context"></param>
        /// <returns>
        /// The rendered output of the template.
        /// If the template is invalid, returns an error message or an empty string according to the current ExceptionHandlingStrategy setting.
        /// </returns>
        public string RenderTemplate( string inputTemplate, ILavaDataDictionary mergeFields )
        {
            string output;
            List<Exception> errors;

            TryRenderTemplate( inputTemplate, mergeFields, out output, out errors );

            return output;
        }
        /// <summary>
        /// Try to render the provided template.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public bool TryRenderTemplate( string inputTemplate, out string output, out List<Exception> errors )
        {
            return TryRenderTemplate( inputTemplate, mergeFields: null, out output, out errors );
        }

        /// <summary>
        /// Try to render the provided template with the specified merge fields.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="mergeFields"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public bool TryRenderTemplate( string inputTemplate, ILavaDataDictionary mergeFields, out string output, out List<Exception> errors )
        {
            ILavaRenderContext context;

            if ( mergeFields != null )
            {
                context = NewRenderContext();

                context.SetMergeFields( mergeFields );
            }
            else
            {
                context = null;
            }

            return TryRenderTemplate( inputTemplate, context, out output, out errors );
        }

        /// <summary>
        /// Try to render the provided template in the specified context.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="context"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public bool TryRenderTemplate( string inputTemplate, ILavaRenderContext context, out string output, out List<Exception> errors )
        {
            ILavaTemplate template;

            try
            {
                if ( _cacheService != null )
                {
                    template = _cacheService.GetOrAddTemplate( inputTemplate );
                }
                else
                {
                    template = null;
                }

                if ( template == null )
                {
                    var isValid = TryParseTemplate( inputTemplate, out template );

                    if ( !isValid )
                    {
                        if ( template == null
                             || this.ExceptionHandlingStrategy == ExceptionHandlingStrategySpecifier.Throw )
                        {
                            throw new LavaException( "Lava Template render operation failed." );
                        }
                    }
                }

                if ( context == null )
                {
                    context = NewRenderContext();
                }

                var parameters = new LavaRenderParameters { Context = context };

                return OnTryRender( template, parameters, out output, out errors );
            }
            catch ( Exception ex )
            {
                ProcessException( ex );

                errors = new List<Exception> { ex };
                output = null;
                return false;
            }

        }

        /// <summary>
        /// Try to render the specified Lava template using the specified parameters.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="parameters"></param>
        /// <param name="output"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public bool TryRenderTemplate( ILavaTemplate inputTemplate, LavaRenderParameters parameters, out string output, out List<Exception> errors )
        {
            return OnTryRender( inputTemplate, parameters, out output, out errors );
        }

        /// <summary>
        /// Override this method to render the Lava template using the underlying rendering engine.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="parameters"></param>
        /// <param name="output"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        protected abstract bool OnTryRender( ILavaTemplate inputTemplate, LavaRenderParameters parameters, out string output, out List<Exception> errors );

        /// <summary>
        /// Compare two objects for equivalence according to the applicable Lava equality rules for the input object types.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>True if the two objects are considered equal.</returns>
        public abstract bool AreEqualValue( object left, object right );

        /// <summary>
        /// Attempt to parse and compile the specified Lava source text into a valid Lava template.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        public bool TryParseTemplate( string inputTemplate, out ILavaTemplate template )
        {
            try
            {
                template = OnParseTemplate( inputTemplate );

                return true;
            }
            catch ( Exception ex )
            {
                string message;

                ProcessException( ex, out message );

                if ( string.IsNullOrWhiteSpace( message ) )
                {
                    template = null;
                }
                else
                {
                    // If an error message is returned during the parsing process, create a new template containing the message.
                    if ( this.ExceptionHandlingStrategy == ExceptionHandlingStrategySpecifier.RenderToOutput )
                    {
                        template = OnParseTemplate( message );
                    }
                    else
                    {
                        template = null;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Override this method to implement parsing and compilation of Lava source text.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        protected abstract ILavaTemplate OnParseTemplate( string inputTemplate );

        /// <summary>
        /// Get the collection of registered Lava template elements.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, ILavaElementInfo> GetRegisteredElements()
        {
            var tags = new Dictionary<string, ILavaElementInfo>();

            foreach ( var tagWrapper in _lavaElements )
            {
                var info = new LavaTagInfo();

                info.Name = tagWrapper.Key;

                info.SystemTypeName = tagWrapper.Value.SystemTypeName;

                tags.Add( info.Name, info );
            }

            return tags;
        }

        #region Tags

        private Dictionary<string, ILavaElementInfo> _lavaElements = new Dictionary<string, ILavaElementInfo>( StringComparer.OrdinalIgnoreCase );

        /// <summary>
        /// Register a Lava Tag element.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryMethod"></param>
        public virtual void RegisterTag( string name, Func<string, ILavaTag> factoryMethod )
        {
            if ( string.IsNullOrWhiteSpace( name ) )
            {
                throw new ArgumentException( "Name must be specified." );
            }

            name = name.Trim().ToLower();

            var tagInstance = factoryMethod( name );

            var tagInfo = new LavaTagInfo();

            tagInfo.Name = name;
            tagInfo.FactoryMethod = factoryMethod;

            tagInfo.IsAvailable = ( tagInstance != null );

            if ( tagInstance != null )
            {
                tagInfo.SystemTypeName = tagInstance.GetType().FullName;
            }

            _lavaElements[name] = tagInfo;
        }

        /// <summary>
        /// Register a Lava Block element.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryMethod"></param>
        public virtual void RegisterBlock( string name, Func<string, ILavaBlock> factoryMethod )
        {
            if ( string.IsNullOrWhiteSpace( name ) )
            {
                throw new ArgumentException( "Name must be specified." );
            }

            name = name.Trim().ToLower();

            var blockInstance = factoryMethod( name );

            var blockInfo = new LavaBlockInfo();

            blockInfo.Name = name;
            blockInfo.FactoryMethod = factoryMethod;

            blockInfo.IsAvailable = ( blockInstance != null );

            if ( blockInstance != null )
            {
                blockInfo.SystemTypeName = blockInstance.GetType().FullName;
            }

            _lavaElements[name] = blockInfo;
        }

        #endregion

        #region Filters

        /// <summary>
        /// Register one or more filter functions that are implemented by the supplied Type.
        /// A filter must be defined as a public static function that returns a string.
        /// </summary>
        /// <param name="implementingType"></param>
        public void RegisterFilters( Type implementingType )
        {
            OnRegisterFilters( implementingType );
        }

        /// <summary>
        /// Override this method to register the filters defined by the provided Type with the underlying Liquid procesing framework.
        /// </summary>
        /// <param name="implementingType"></param>
        protected abstract void OnRegisterFilters( Type implementingType );

        #endregion

        protected void ProcessException( Exception ex )
        {
            string discardedOutput;

            ProcessException( ex, out discardedOutput );
        }

        protected void ProcessException( Exception ex, out string message )
        {
            if ( this.ExceptionHandlingStrategy == ExceptionHandlingStrategySpecifier.RenderToOutput )
            {
                message = $"Lava Error: {ex.Message}";
            }
            else if ( this.ExceptionHandlingStrategy == ExceptionHandlingStrategySpecifier.Ignore )
            {
                // We should probably log the message here rather than failing silently, but this preserves current behavior.
                message = null;
            }
            else
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gets or sets the strategy for handling exceptions encountered during the rendering process.
        /// </summary>
        public ExceptionHandlingStrategySpecifier ExceptionHandlingStrategy { get; set; } = ExceptionHandlingStrategySpecifier.RenderToOutput;

        /// <summary>
        /// Convert a Lava template to a Liquid-compatible template by replacing Lava-specific syntax and keywords.
        /// </summary>
        /// <param name="lavaTemplateText"></param>
        /// <returns></returns>
        public string ConvertToLiquid( string lavaTemplateText )
        {
            var converter = new LavaToLiquidTemplateConverter();

            return converter.ConvertToLiquid( lavaTemplateText );
        }

        /// <summary>
        /// Remove all entries from the template cache.
        /// </summary>
        public void ClearTemplateCache()
        {
            if ( _cacheService != null )
            {
                _cacheService.ClearCache();
            }
        }

        /// <summary>
        /// Remove the registration entry for the shortcode with the specified name.
        /// </summary>
        /// <param name="name"></param>
        public void UnregisterShortcode( string name )
        {
            var registrationKey = LavaUtilityHelper.GetLiquidElementNameFromShortcodeName( name );

            if ( _lavaElements.ContainsKey( registrationKey ) )
            {
                _lavaElements.Remove( registrationKey );
            }
        }

        /// <summary>
        /// Parse the input text into a compiled Lava template.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        public ILavaTemplate ParseTemplate( string inputTemplate )
        {
            ILavaTemplate template;

            var isValid = TryParseTemplate( inputTemplate, out template );

            if ( !isValid )
            {
                throw new LavaException( "ParseTemplate failed. The Lava template is invalid." );
            }

            return template;
        }
    }
}