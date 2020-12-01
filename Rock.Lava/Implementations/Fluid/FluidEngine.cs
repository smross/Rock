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
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Fluid;
using Fluid.Values;
using Rock.Data;

namespace Rock.Lava.Fluid
{
    public class FluidEngine : LavaEngineBase
    {
        public override string EngineName
        {
            get
            {
                return "Fluid";
            }
        }

        public override LavaEngineTypeSpecifier EngineType
        {
            get
            {
                return LavaEngineTypeSpecifier.Fluid;
            }
        }

        public override ILavaContext NewContext()
        {
            var fluidContext = new global::Fluid.TemplateContext();

            return new FluidLavaContext( fluidContext );
        }

        /// <summary>
        /// Initializes Rock's Lava system (which uses DotLiquid)
        /// Doing this in startup will force the static Liquid class to get instantiated
        /// so that the standard filters are loaded prior to the custom RockFilter.
        /// This is to allow the custom 'Date' filter to replace the standard Date filter.
        /// </summary>
        public override void Initialize( ILavaFileSystem fileSystem, IList<Type> filterImplementationTypes = null )
        {
            // Re-register the basic Liquid filters implemented by Fluid using CamelCase rather than the default snakecase.
            HideSnakeCaseFilters();
            RegisterBaseFilters();

            // Set the default strategy for locating object properties to our custom implementation that adds
            // the ability to resolve properties of nested anonymous Types using Reflection.
            TemplateContext.GlobalMemberAccessStrategy = new LavaObjectMemberAccessStrategy();

            // Register custom filters last, so they can override built-in filters of the same name.
            if ( filterImplementationTypes != null )
            {
                foreach ( var filterImplementationType in filterImplementationTypes )
                {
                    RegisterLavaFiltersFromImplementingType( filterImplementationType );
                }
            }

            // Register all Types that implement ILavaDataObject as safe to render.
            RegisterSafeType( typeof( Rock.Lava.ILavaDataObject ) );

            // Set the file provider to resolve included file references.
            if ( fileSystem == null )
            {
                fileSystem = new LavaNullFileSystem();
            }

            TemplateContext.GlobalFileProvider = new FluidFileSystem( fileSystem );

        }
        /// <summary>
        /// This method hides the snake-case filters that are registered
        /// by default. Rock uses CamelCase filter names and to ensure that
        /// a mistype doesn't cause it to work anyway we hide these.
        /// </summary>
        private static void HideSnakeCaseFilters()
        {
            TemplateContext.GlobalFilters.AddFilter( "join", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "first", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "last", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "concat", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "map", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "reverse", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "size", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "sort", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "sort_natural", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "uniq", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "where", NoOp );

            TemplateContext.GlobalFilters.AddFilter( "default", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "date", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "format_date", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "raw", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "compact", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "url_encode", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "url_decode", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "strip_html", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "escape", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "escape_once", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "handle", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "handleize", NoOp );

            TemplateContext.GlobalFilters.AddFilter( "abs", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "at_least", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "at_most", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "ceil", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "divided_by", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "floor", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "minus", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "modulo", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "plus", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "round", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "times", NoOp );

            TemplateContext.GlobalFilters.AddFilter( "append", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "capitalize", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "downcase", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "lstrip", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "rstrip", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "newline_to_br", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "prepend", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "removefirst", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "remove", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "replacefirst", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "replace", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "slice", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "split", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "strip", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "strip_newlines", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "truncate", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "truncatewords", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "upcase", NoOp );
        }

        /// <summary>
        /// Registers all the base Fluid filters with the proper CamelCase.
        /// </summary>
        private static void RegisterBaseFilters()
        {
            TemplateContext.GlobalFilters.AddFilter( "Join", global::Fluid.Filters.ArrayFilters.Join );
            TemplateContext.GlobalFilters.AddFilter( "First", global::Fluid.Filters.ArrayFilters.First );
            TemplateContext.GlobalFilters.AddFilter( "Last", global::Fluid.Filters.ArrayFilters.Last );
            TemplateContext.GlobalFilters.AddAsyncFilter( "Map", global::Fluid.Filters.ArrayFilters.Map );
            TemplateContext.GlobalFilters.AddFilter( "Reverse", global::Fluid.Filters.ArrayFilters.Reverse );
            TemplateContext.GlobalFilters.AddAsyncFilter( "Size", global::Fluid.Filters.ArrayFilters.Size );
            TemplateContext.GlobalFilters.AddAsyncFilter( "Sort", global::Fluid.Filters.ArrayFilters.Sort );
            TemplateContext.GlobalFilters.AddFilter( "Uniq", global::Fluid.Filters.ArrayFilters.Uniq );
            TemplateContext.GlobalFilters.AddAsyncFilter( "Where", global::Fluid.Filters.ArrayFilters.Where );

            TemplateContext.GlobalFilters.AddFilter( "Default", global::Fluid.Filters.MiscFilters.Default );
            TemplateContext.GlobalFilters.AddFilter( "Date", global::Fluid.Filters.MiscFilters.Date );
            TemplateContext.GlobalFilters.AddFilter( "UnescapeDataString", global::Fluid.Filters.MiscFilters.UrlDecode );
            TemplateContext.GlobalFilters.AddFilter( "EscapeDataString", global::Fluid.Filters.MiscFilters.UrlEncode );
            TemplateContext.GlobalFilters.AddFilter( "StripHtml", global::Fluid.Filters.MiscFilters.StripHtml );
            TemplateContext.GlobalFilters.AddFilter( "Escape", global::Fluid.Filters.MiscFilters.Escape );

            TemplateContext.GlobalFilters.AddFilter( "AtLeast", global::Fluid.Filters.NumberFilters.AtLeast );
            TemplateContext.GlobalFilters.AddFilter( "AtMost", global::Fluid.Filters.NumberFilters.AtMost );
            TemplateContext.GlobalFilters.AddFilter( "Ceiling", global::Fluid.Filters.NumberFilters.Ceil );
            TemplateContext.GlobalFilters.AddFilter( "DividedBy", global::Fluid.Filters.NumberFilters.DividedBy );
            TemplateContext.GlobalFilters.AddFilter( "Floor", global::Fluid.Filters.NumberFilters.Floor );
            TemplateContext.GlobalFilters.AddFilter( "Minus", global::Fluid.Filters.NumberFilters.Minus );
            TemplateContext.GlobalFilters.AddFilter( "Modulo", global::Fluid.Filters.NumberFilters.Modulo );
            TemplateContext.GlobalFilters.AddFilter( "Plus", global::Fluid.Filters.NumberFilters.Plus );
            TemplateContext.GlobalFilters.AddFilter( "Times", global::Fluid.Filters.NumberFilters.Times );

            TemplateContext.GlobalFilters.AddFilter( "Append", global::Fluid.Filters.StringFilters.Append );
            TemplateContext.GlobalFilters.AddFilter( "Capitalize", global::Fluid.Filters.StringFilters.Capitalize );
            TemplateContext.GlobalFilters.AddFilter( "Downcase", global::Fluid.Filters.StringFilters.Downcase );
            TemplateContext.GlobalFilters.AddFilter( "NewlineToBr", global::Fluid.Filters.StringFilters.NewLineToBr );
            TemplateContext.GlobalFilters.AddFilter( "Prepend", global::Fluid.Filters.StringFilters.Prepend );
            TemplateContext.GlobalFilters.AddFilter( "RemoveFirst", global::Fluid.Filters.StringFilters.RemoveFirst );
            TemplateContext.GlobalFilters.AddFilter( "Remove", global::Fluid.Filters.StringFilters.Remove );
            TemplateContext.GlobalFilters.AddFilter( "ReplaceFirst", global::Fluid.Filters.StringFilters.ReplaceFirst );
            TemplateContext.GlobalFilters.AddFilter( "Replace", global::Fluid.Filters.StringFilters.Replace );
            TemplateContext.GlobalFilters.AddFilter( "Slice", global::Fluid.Filters.StringFilters.Slice );
            TemplateContext.GlobalFilters.AddFilter( "Split", global::Fluid.Filters.StringFilters.Split );
            TemplateContext.GlobalFilters.AddFilter( "StripNewlines", global::Fluid.Filters.StringFilters.StripNewLines );
            TemplateContext.GlobalFilters.AddFilter( "Truncate", global::Fluid.Filters.StringFilters.Truncate );
            TemplateContext.GlobalFilters.AddFilter( "Truncatewords", global::Fluid.Filters.StringFilters.TruncateWords );
            TemplateContext.GlobalFilters.AddFilter( "Upcase", global::Fluid.Filters.StringFilters.Upcase );
        }

        /// <summary>
        /// Registers a set of Liquid-style filter functions for use with the Fluid templating engine.
        /// The original filters are wrapped in a function with a Fluid-compatible signature so they can be called by Fluid.
        /// </summary>
        /// <param name="type">The type that contains the Liquid filter functions.</param>
        public static void RegisterLavaFiltersFromImplementingType( Type type )
        {
            // Get the filter methods ordered by name and parameter count.
            // Fluid only allows one registered method for each filter name, so use the overload with the most parameters.
            // This addresses the vast majority of use cases, but we could modify our Fluid filter function wrapper to
            // distinguish different method signatures if necessary.
            var lavaFilterMethods = type.GetMethods( System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public )
                .ToList()
                .OrderBy( x => x.Name )
                .ThenByDescending( x => x.GetParameters().Count() );

            string lastFilterName = null;

            foreach ( var lavaFilterMethod in lavaFilterMethods )
            {
                if ( lavaFilterMethod.Name == lastFilterName )
                {
                    continue;
                }

                lastFilterName = lavaFilterMethod.Name;

                var lavaFilterMethodParameters = lavaFilterMethod.GetParameters();

                if ( lavaFilterMethodParameters.Length == 0 )
                {
                    continue;
                }

                // The first argument passed to the Lava filter is either the Lava Context or the template input.
                var hasContextParameter = lavaFilterMethodParameters[0].ParameterType == typeof( ILavaContext );

                var firstParameterIndex = 1 + ( hasContextParameter ? 1 : 0 );

                // Define the Fluid-compatible filter function that will wrap the Lava filter method.
                FluidValue fluidFilterFunction( FluidValue input, FilterArguments arguments, TemplateContext context )
                {
                    var lavaFilterMethodArguments = new object[lavaFilterMethodParameters.Length];

                    for ( int i = 0; i < lavaFilterMethodParameters.Length; i++ )
                    {
                        FluidValue fluidFilterArgument = null;

                        // Get the value for the argument.
                        if ( i == 0 )
                        {
                            // If this is the first parameter, it may be a LavaContext or the input template.
                            if ( hasContextParameter )
                            {
                                lavaFilterMethodArguments[0] = new FluidLavaContext( context );
                                continue;
                            }
                            else
                            {
                                fluidFilterArgument = input;
                            }
                        }
                        else if ( i == 1 && hasContextParameter )
                        {
                            // If this is the second parameter, it must be the input template if the first parameter is a LavaContext.
                            fluidFilterArgument = input;
                        }
                        else if ( arguments.Count > ( i - firstParameterIndex ) )
                        {
                            // This parameter is a filter argument.
                            fluidFilterArgument = arguments.At( i - firstParameterIndex );
                        }

                        if ( fluidFilterArgument == null && lavaFilterMethodParameters[i].IsOptional )
                        {
                            lavaFilterMethodArguments[i] = lavaFilterMethodParameters[i].DefaultValue;
                        }
                        else
                        {
                            lavaFilterMethodArguments[i] = GetLavaParameterArgumentFromFluidValue( fluidFilterArgument, lavaFilterMethodParameters[i].ParameterType );
                        }
                    }

                    var result = lavaFilterMethod.Invoke( null, lavaFilterMethodArguments );

                    return FluidValue.Create( result );
                }

                TemplateContext.GlobalFilters.AddFilter( lavaFilterMethod.Name, fluidFilterFunction );
            }
        }

        private static object GetLavaParameterArgumentFromFluidValue( FluidValue fluidFilterArgument, Type argumentType )
        {
            object lavaArgument = null;

            if ( argumentType == typeof( string ) )
            {
                if ( fluidFilterArgument != null )
                {
                    lavaArgument = fluidFilterArgument.ToStringValue();
                }
            }
            else if ( argumentType == typeof( int ) )
            {
                if ( fluidFilterArgument == null )
                {
                    lavaArgument = 0;
                }
                else
                {
                    lavaArgument = (int)fluidFilterArgument.ToNumberValue();
                }
            }
            else if ( argumentType == typeof( bool ) )
            {
                if ( fluidFilterArgument == null )
                {
                    lavaArgument = false;
                }
                else
                {
                    lavaArgument = fluidFilterArgument.ToBooleanValue();
                }
            }
            else if ( argumentType == typeof( object ) )
            {
                if ( fluidFilterArgument != null )
                {
                    // Get the object value, ensuring that any Fluid wrapper that has been applied is removed.
                    lavaArgument = fluidFilterArgument.ToRealObjectValue();
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException( argumentType.Name, $"Parameter type '{argumentType.Name}' is not supported for legacy filters." );
            }

            return lavaArgument;
        }

        /// <summary>
        /// Performs a no-operation filter. Just return the input. This simulates using
        /// a filter that doesn't exist.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static FluidValue NoOp( FluidValue input, FilterArguments arguments, TemplateContext context )
        {
            return input;
        }

        public override void RegisterSafeType( Type type, string[] allowedMembers = null )
        {
            if ( allowedMembers != null
                 && allowedMembers.Length > 0 )
            {
                TemplateContext.GlobalMemberAccessStrategy.Register( type, allowedMembers );
            }
            else
            {
                TemplateContext.GlobalMemberAccessStrategy.Register( type );
            }
        }

        private LavaFluidTemplate CreateNewFluidTemplate( string inputTemplate )
        {
            IEnumerable<string> errors;
            LavaFluidTemplate template;

            var formattedInput = ReplaceTemplateShortcodes( inputTemplate );

            var isValidTemplate = LavaFluidTemplate.TryParse( formattedInput, out template, out errors );

            if ( !isValidTemplate )
            {
                throw new LavaException( "Create Lava Template failed.", errors );
            }

            return template;
        }

        internal static readonly Regex FullShortCodeToken = new Regex( @"{\[\s*(\w+)\s*([^\]}]*)?\]}", RegexOptions.Compiled );

        public static string ShortcodeNameSuffix = "_sc";

        private string ReplaceTemplateShortcodes( string inputTemplate )
        {
            /* The Lava shortcode syntax is not recognized as a document element by the Fluid parser, and at present there is no way to intercept or replace the Fluid parser.
             * As a workaround, pre-process the template to replace the Lava shortcode token "{[ ]}" with the Liquid tag token "{% %}" and add a prefix to avoid naming collisions with existing standard tags.
             * The shortcode can then be processed as a regular custom block by the Fluid templating engine.
             * As a future improvement, we could look at submitting a pull request to the Fluid project to add support for custom parsers.
             */
            var newBlockName = "{% $1<suffix> $2 %}".Replace( "<suffix>", ShortcodeNameSuffix );

            inputTemplate = FullShortCodeToken.Replace( inputTemplate, newBlockName );

            return inputTemplate;
        }

        public override bool TryRender( string inputTemplate, out string output, ILavaContext context )
        {
            try
            {
                var template = CreateNewFluidTemplate( inputTemplate );

                var templateContext = context as FluidLavaContext;

                if ( templateContext == null )
                {
                    throw new LavaException( "Invalid LavaContext type." );
                }

                /* The Fluid framework parses the input template into a set of executable statements that can be rendered.
                 * To remain independent of a specific framework, custom Lava tags and blocks parse the original source template text to extract
                 * the information necessary to render their output. For this reason, we need to store the source in the context so that it can be passed
                 * to the Lava custom components when they are rendered.
                 */
                templateContext.SetInternalValue( Constants.ContextKeys.SourceTemplateText, inputTemplate );

                output = template.Render( templateContext.FluidContext );
                
                return true;
            }
            catch ( Exception ex )
            {
                ProcessException( ex, out output );

                return false;
            }
        }

        public override void UnregisterShortcode( string name )
        {
            throw new NotImplementedException();
        }

        public override void RegisterTag( string name, Func<string, IRockLavaTag> factoryMethod )
        {
            if ( name == null )
            {
                throw new ArgumentException( "Name must be specified." );
            }

            name = name.Trim().ToLower();

            base.RegisterTag( name, factoryMethod );

            // Some Lava elements, such as shortcodes, are defined dynamically at runtime.
            // Therefore, we register the tag as a factory that can produce the requested element on demand.
            var lavaTag = factoryMethod( name );

            var fluidTag = new FluidTagProxy();

            FluidTagProxy.RegisterFactory( name, factoryMethod );

            // Register the proxy for the specified tag name.
            LavaFluidTemplate.Factory.RegisterTag<FluidTagProxy>( name );
        }

        public override void RegisterBlock( string name, Func<string, IRockLavaBlock> factoryMethod )
        {
            if ( name == null )
            {
                throw new ArgumentException( "Name must be specified." );
            }

            name = name.Trim().ToLower();

            base.RegisterBlock( name, factoryMethod );

            // Some Lava elements, such as shortcodes, are defined dynamically at runtime.
            // Therefore, we register the tag as a factory that can produce the requested element on demand.
            var lavaBlock = factoryMethod( name );

            //var fluidBlock = new FluidBlockProxy();

            FluidBlockProxy.RegisterFactory( name, factoryMethod );

            // Register the proxy for the specified tag name.
            LavaFluidTemplate.Factory.RegisterBlock<FluidBlockProxy>( name );
        }

        public override ILavaTemplate ParseTemplate( string inputTemplate )
        {
            var lavaTemplate = new FluidTemplateProxy( CreateNewFluidTemplate( inputTemplate ) );

            return lavaTemplate;
        }

        #region Obsolete/Not Required?

        public override bool AreEqualValue( object left, object right )
        {
            throw new NotImplementedException();
        }

        public override Type GetShortcodeType( string name )
        {
            throw new NotImplementedException();
        }

        #endregion

    }

    #region Member Access Strategy implementation

    /// <summary>
    /// An implementation of a MemberAccessStrategy for the Fluid framework
    /// The MemberAccessStrategy determines the way in which property values are retrieved from specific types of objects supported by Lava:
    /// anonymous types, types that implement ILavaDataObject, and types that are decorated with the LavaTypeAttribute.
    /// </summary>
    public class LavaObjectMemberAccessStrategy : IMemberAccessStrategy
    {
        private Dictionary<Type, Dictionary<string, IMemberAccessor>> _map;
        private readonly IMemberAccessStrategy _parent;

        private DynamicMemberAccessor _dynamicMemberAccessor = new DynamicMemberAccessor();
        private LavaDataSourceMemberAccessor _lavaDataSourceMemberAccessor = new LavaDataSourceMemberAccessor();

        public LavaObjectMemberAccessStrategy()
        {
            _map = new Dictionary<Type, Dictionary<string, IMemberAccessor>>();
        }

        public LavaObjectMemberAccessStrategy( IMemberAccessStrategy parent ) : this()
        {
            _parent = parent;
            MemberNameStrategy = _parent.MemberNameStrategy;
        }

        /// <summary>
        /// A flag indicating if an exception should be thrown when a member access is invalid.
        /// If set to false, an invalid access returns an empty value.
        /// </summary>
        public bool ThrowOnInvalidMemberAccess { get; set; } = true;

        public MemberNameStrategy MemberNameStrategy { get; set; } = MemberNameStrategies.Default;

        public bool IgnoreCasing { get; set; }

        public IMemberAccessor GetAccessor( Type type, string name )
        {
            IMemberAccessor accessor = null;

            /*
             * To access the members of an anonymous type, we need to use reflection.
             * The entire MemberAccessStrategy must be reimplemented rather than simply registering a new accessor via the Register() method,
             * because the type being accessed is not known in advance.
             * This check for an anonymous type is fairly naive, but it works correctly for the .Net and Mono frameworks.
             * Refer https://stackoverflow.com/questions/2483023/how-to-test-if-a-type-is-anonymous.
             */
            if ( type.Name.Contains( "AnonymousType" ) || type.Name.Contains( "AnonType" ) )
            {
                return _dynamicMemberAccessor;
            }

            if ( typeof( RockDynamic ).IsAssignableFrom( type ) )
            {
                return _dynamicMemberAccessor;
            }

            // Check for ILavaDataObject implementation.
            if ( typeof( ILavaDataObjectSource ).IsAssignableFrom( type )
                 || typeof( ILavaDataObject ).IsAssignableFrom( type ) )
            {
                return _lavaDataSourceMemberAccessor;
            }

            var isMapped = _map.ContainsKey( type );

            if ( !isMapped )
            {
                // Check for LavaTypeAttribute and if it exists, register a new member accessor for the decorated type.
                // Subsequent requests will use the registered map.
                var attr = (LavaTypeAttribute)type.GetCustomAttributes( typeof( LavaTypeAttribute ), false ).FirstOrDefault();

                if ( attr != null )
                {
                    RegisterLavaTypeProperties( type, attr );
                }
            }

            // Check for a specific property map for any Type in the inheritance chain.
            var mapType = type;

            while ( mapType != typeof( object ) )
            {
                // Look for specific property map
                if ( _map.TryGetValue( mapType, out var typeMap ) )
                {
                    if ( typeMap.TryGetValue( name, out accessor ) || typeMap.TryGetValue( "*", out accessor ) )
                    {
                        return accessor;
                    }
                }

                accessor = accessor ?? _parent?.GetAccessor( mapType, name );

                if ( accessor != null )
                {
                    return accessor;
                }

                mapType = mapType.GetTypeInfo().BaseType;
            }

            return null;
        }

        private void RegisterLavaTypeProperties( Type type, LavaTypeAttribute attr )
        {
            List<PropertyInfo> includedProperties;

            // Get the list of included properties, then remove the ignored properties.
            if ( attr.AllowedMembers == null || !attr.AllowedMembers.Any() )
            {
                // No included properties have been specified, so assume all are included.
                includedProperties = type.GetProperties().ToList();
            }
            else
            {
                includedProperties = type.GetProperties().Where( x => attr.AllowedMembers.Contains( x.Name, StringComparer.OrdinalIgnoreCase ) ).ToList();
            }

            var ignoredProperties = type.GetProperties().Where( x => x.GetCustomAttributes( typeof( LavaIgnoreAttribute ), false ).Any() ).ToList();

            foreach ( var includedProperty in includedProperties )
            {
                if ( ignoredProperties.Contains( includedProperty ) )
                {
                    continue;
                }

                var newAccessor = new LavaTypeMemberAccessor( includedProperty );

                Register( type, includedProperty.Name, newAccessor );
            }
        }

        public void Register( Type type, string name, IMemberAccessor getter )
        {
            if ( !_map.TryGetValue( type, out var typeMap ) )
            {
                typeMap = new Dictionary<string, IMemberAccessor>( IgnoreCasing
                    ? StringComparer.OrdinalIgnoreCase
                    : StringComparer.Ordinal );

                _map[type] = typeMap;
            }

            typeMap[name] = getter;
        }
    }

    /// <summary>
    /// A Fluid Engine Member Accessor that can retrieve the value of a LavaDataSource.
    /// </summary>
    public class LavaDataSourceMemberAccessor : IMemberAccessor
    {
        public object Get( object obj, string name, TemplateContext ctx )
        {
            ILavaDataObject lavaObject;

            if ( obj is Rock.Lava.ILavaDataObjectSource lavaSource )
            {
                lavaObject = lavaSource.GetLavaDataObject();
            }
            else
            {
                lavaObject = (ILavaDataObject)obj;
            }

            return lavaObject.GetValue( name );
        }
    }

    /// <summary>
    /// A Fluid Engine Member Accessor that can retrieve the value of a member of an anonymously-typed object.
    /// </summary>
    public class DynamicMemberAccessor : IMemberAccessor
    {
        public object Get( object obj, string name, TemplateContext ctx )
        {
            return GetPropertyPathValue( obj, name );
        }

        private object GetPropertyPathValue( object obj, string propertyPath )
        {
            if ( string.IsNullOrWhiteSpace( propertyPath ) )
            {
                return obj;
            }

            return Rock.Common.ExtensionMethods.GetPropertyValue( obj, propertyPath );
        }
    }

    /// <summary>
    /// A Fluid Engine Member Accessor that reads a specific property value of a class decorated with the LavaType attribute.
    /// </summary>
    public class LavaTypeMemberAccessor : IMemberAccessor
    {
        private PropertyInfo _info;

        public LavaTypeMemberAccessor( PropertyInfo info )
        {
            _info = info;
        }

        public object Get( object obj, string name, TemplateContext ctx )
        {
            return _info.GetValue( obj );
        }
    }

    #endregion

}
