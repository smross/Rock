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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotLiquid;

using Rock.Common;
using Rock.Data;

namespace Rock.Lava.DotLiquid
{

    public class DotLiquidLavaContext : LavaContextBase
    {
        private Context _context;

        public DotLiquidLavaContext( Context context )
        {
            _context = context;
        }

        public Context DotLiquidContext
        {
            get
            {
                return _context;
            }
        }

        public override void SetEnabledCommands( IEnumerable<string> commands )
        {
            if ( commands == null )
            {
                _context.Registers["EnabledCommands"] = string.Empty;
            }
            else
            {
                _context.Registers["EnabledCommands"] = commands.JoinStrings( "," );
            }
        }

        public override List<string> GetEnabledCommands()
        {
            // The set of enabled Lava Commands is stored in the DotLiquid Registers collection.
            if ( _context.Registers?.ContainsKey( "EnabledCommands" ) == true )
            {
                return _context.Registers["EnabledCommands"].ToString().Split( ",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries ).ToList();
            }

            return new List<string>();
        }

        /// <summary>
        /// Gets the dictionary of values that are active in the local scope.
        /// Values are defined by the outermost container first, and overridden by values defined in a contained scope.
        /// </summary>
        /// <returns></returns>
        public override LavaDataDictionary GetMergeFields()
        {
            var fields = new LavaDataDictionary();

            // First, get all of the variables defined in the local lava context.
            // In DotLiquid, the innermost scope is the first element in the collection.
            foreach ( var scope in _context.Scopes )
            {
                foreach ( var item in scope )
                {
                    fields.AddOrIgnore( item.Key, item.Value );
                }
            }

            // Second, add any variables defined by the block or container in which the template is being resolved.
            foreach ( var environment in _context.Environments )
            {
                foreach ( var item in environment )
                {
                    fields.AddOrIgnore( item.Key, item.Value );
                }
            }

            return fields;
        }

        /// <summary>
        /// Get the value of a field that is accessible for merging into a template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public override object GetMergeFieldValue( string key, object defaultValue = null )
        {
            if ( !_context.HasKey(key) )
            {
                return defaultValue;
            }

            return _context[key];
        }

        public override void SetMergeFieldValue( string key, object value, LavaContextRelativeScopeSpecifier scope = LavaContextRelativeScopeSpecifier.Current )
        {
            int scopeIndex;

            // Scopes are ordered with the current level first.
            if ( scope == LavaContextRelativeScopeSpecifier.Root )
            {
                scopeIndex = _context.Scopes.Count - 1;
            }
            else if ( scope == LavaContextRelativeScopeSpecifier.Parent )
            {
                scopeIndex = 1;
            }
            else
            {
                scopeIndex = 0;
            }

            var fieldValue = GetDotLiquidCompatibleValue( value );

            // Set the variable in the specified scope.
            _context.Scopes[scopeIndex][key] = fieldValue;
        }

        public void ClearValues()
        {
            _context.ClearInstanceAssigns();
        }

        private object GetDotLiquidCompatibleValue( object value )
        {
            // Primitive values do not require any special processing.
            if ( value == null
                 || value is string
                 || value is IEnumerable
                 || value is decimal
                 || value is DateTime
                 || value is DateTimeOffset
                 || value is TimeSpan
                 || value is Guid
                 || value is Enum
                 || value is KeyValuePair<string, object>
                 )
            {
                return value;
            }

            var valueType = value.GetType();

            if ( valueType.IsPrimitive )
            {
                return value;
            }

            // For complex types, check if a specific transformer has been defined for the type.
            var safeTypeTransformer = Template.GetSafeTypeTransformer( valueType );

            if ( safeTypeTransformer != null )
            {
                return safeTypeTransformer( value );
            }

            // Check if the type is decorated with the LavaType attribute.
            var attr = (LavaTypeAttribute)valueType.GetCustomAttributes( typeof( LavaTypeAttribute ), false ).FirstOrDefault();

            if ( attr != null )
            {
                var allowedProperties = GetLavaTypeAllowedProperties( valueType, attr );

                return new DropProxy( value, allowedProperties );
            }

            return value;
        }

        private string[] GetLavaTypeAllowedProperties( Type type, LavaTypeAttribute attr )
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

            var allowedProperties = includedProperties.Except( ignoredProperties ).Select( x => x.Name ).ToArray();

            return allowedProperties;
        }

        public override object GetInternalFieldValue( string key, object defaultValue = null )
        {
            if ( _context.Registers.ContainsKey( key ) )
            {
                return _context.Registers[key];
            }

            return defaultValue;
        }

        public override LavaDataDictionary GetInternalFields()
        {
            var values = new LavaDataDictionary();

            foreach ( var item in _context.Registers )
            {
                values.AddOrReplace( item.Key, item.Value );
            }

            return values;
        }

        public override void SetInternalFieldValue( string key, object value )
        {
            _context.Registers[key] = value;
        }

        public override void EnterChildScope()
        {
            // Push a new scope onto the stack.
            var newScope = new Hash();

            _context.Push( newScope );
        }

        public override void ExitChildScope()
        {
            _context.Pop();
        }
    }
}