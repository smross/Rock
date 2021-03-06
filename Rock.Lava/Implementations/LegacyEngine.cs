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
using DotLiquid;
using DotLiquid.NamingConventions;

namespace Rock.Lava.Legacy
{
    /// <summary>
    /// This represents the original Lava implementation for Rock, based on a custom fork of the DotLiquid framework.
    /// This engine provides no functionality, it is simply a placeholder to inform the Lava library
    /// that the Rock Legacy Lava framework is active.
    /// </summary>
    public class LegacyEngine : ILavaEngine
    {
        public string EngineName
        {
            get
            {
                return "DotLiquid (Legacy)";
            }
        }

        public LavaEngineTypeSpecifier EngineType
        {
            get
            {
                return LavaEngineTypeSpecifier.Legacy;
            }
        }
        public ILavaTemplateCacheService TemplateCacheService
        {
            get
            {
                HandleFeatureNotImplemented();

                return null;
            }
        }

        public ExceptionHandlingStrategySpecifier ExceptionHandlingStrategy
        {
            get
            {
                HandleFeatureNotImplemented();

                return ExceptionHandlingStrategySpecifier.Throw;
            }
            set
            {
                HandleFeatureNotImplemented();
            }
        }


        public bool AreEqualValue( object left, object right )
        {
            HandleFeatureNotImplemented();

            return false;
        }

        public void ClearTemplateCache()
        {
            HandleFeatureNotImplemented();
        }

        public Dictionary<string, ILavaElementInfo> GetRegisteredElements()
        {
            HandleFeatureNotImplemented();

            return null;
        }

        public void Initialize( LavaEngineConfigurationOptions options = null )
        {
            // DotLiquid uses a RubyDateFormat by default,
            // but since we aren't using Ruby, we want to disable that
            Liquid.UseRubyDateFormat = false;

            /* 2020-05-20 MDP (actually this comment was here a long time ago)
                NOTE: This means that all the built in template filters,
                and the RockFilters, will use CSharpNamingConvention.

                For example the dotliquid documentation says to do this for formatting dates: 
                {{ some_date_value | date:"MMM dd, yyyy" }}

                However, if CSharpNamingConvention is enabled, it needs to be: 
                {{ some_date_value | Date:"MMM dd, yyyy" }}
            */

            Template.NamingConvention = new CSharpNamingConvention();

            //Template.FileSystem = new LavaFileSystem();
            Template.RegisterSafeType( typeof( Enum ), o => o.ToString() );
            Template.RegisterSafeType( typeof( DBNull ), o => null );
        }

        public ILavaRenderContext NewRenderContext( IDictionary<string, object> values = null, IEnumerable<string> enabledCommands = null )
        {
            HandleFeatureNotImplemented();

            return null;
        }

        public ILavaTemplate ParseTemplate( string inputTemplate )
        {
            HandleFeatureNotImplemented();

            return null;
        }

        public void RegisterBlock( string name, Func<string, ILavaBlock> factoryMethod )
        {
            HandleFeatureNotImplemented();
        }

        public void RegisterDynamicShortcode( string name, Func<string, DynamicShortcodeDefinition> factoryMethod )
        {
            HandleFeatureNotImplemented();
        }

        public void RegisterFilters( Type implementingType )
        {
            HandleFeatureNotImplemented();
        }

        public void RegisterSafeType( Type type )
        {
            HandleFeatureNotImplemented();
        }

        public void RegisterSafeType( Type type, IEnumerable<string> allowedMembers )
        {
            HandleFeatureNotImplemented();
        }

        public void RegisterStaticShortcode( string name, Func<string, ILavaShortcode> factoryMethod )
        {
            HandleFeatureNotImplemented();
        }

        public void RegisterTag( string name, Func<string, ILavaTag> factoryMethod )
        {
            HandleFeatureNotImplemented();
        }

        public string RenderTemplate( string inputTemplate )
        {
            HandleFeatureNotImplemented();

            return null;
        }

        public string RenderTemplate( string inputTemplate, ILavaRenderContext context )
        {
            HandleFeatureNotImplemented();

            return null;
        }

        public string RenderTemplate( string inputTemplate, LavaDataDictionary mergeFields )
        {
            HandleFeatureNotImplemented();

            return null;
        }

        public bool TryParseTemplate( string inputTemplate, out ILavaTemplate template )
        {
            HandleFeatureNotImplemented();

            template = null;
            return false;
        }

        public bool TryRenderTemplate( string inputTemplate, out string output, out List<Exception> errors )
        {
            HandleFeatureNotImplemented();

            output = null;
            errors = null;
            return false;
        }

        public bool TryRenderTemplate( string inputTemplate, LavaDataDictionary mergeFields, out string output, out List<Exception> errors )
        {
            HandleFeatureNotImplemented();

            output = null;
            errors = null;
            return false;
        }

        public bool TryRenderTemplate( string inputTemplate, ILavaRenderContext context, out string output, out List<Exception> errors )
        {
            HandleFeatureNotImplemented();

            output = null;
            errors = null;
            return false;
        }

        public bool TryRenderTemplate( ILavaTemplate inputTemplate, LavaRenderParameters parameters, out string output, out List<Exception> errors )
        {
            HandleFeatureNotImplemented();

            output = null;
            errors = null;
            return false;
        }

        public void UnregisterShortcode( string name )
        {
            HandleFeatureNotImplemented();
        }

        private void HandleFeatureNotImplemented()
        {
            //throw new NotImplementedException( "This feature is not implemented for the Legacy Lava Engine." );
        }
    }


}
