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

//using System.Linq.Dynamic.Core;
//using System.Runtime.Caching;
using System.Text.RegularExpressions;

using DotLiquid;
using Rock;
using Rock.Common;
//using Rock.Utility;

namespace Rock.Lava
{
    /// <summary>
    /// A set of functions that can be used to perform filter and transformation operations on a text stream.
    /// </summary>
    /// <remarks>
    /// These filters are intended to be used in the context of a text templating engine, however their implementation should be engine-agnostic.
    /// This will allow the functions to be more easily wrapped and implemented elsewhere for specific templating engines.
    /// If these filters are found to have more general application, they should be moved to the Rock.Common library.
    ///
    /// Template filters must have the following properties:
    /// 1. The filter function must have a return type of string.
    /// 2. Input parameters should be of type string or object. Any parameter conversion should be performed in the function itself.
    /// 3. No optional parameters. Some Liquid templating frameworks do not handle these correctly, so use an explicit function overloads to define different parameter sets.
    /// </remarks>
    ///
    ///
    public class BlueBoxMoonLavaFilters //: IRockStartup
    {
        #region Private Fields

        //const string GlobalCacheKey = "com.blueboxmoon.LavaCache";

        #endregion

        #region Initialization

        /// <summary>
        /// All IRockStartup classes will be run in order by this value. If class does not depend on an order, return zero.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int StartupOrder { get { return 0; } }

        /// <summary>
        /// Method that will be run at Rock startup
        /// </summary>
        public virtual void OnStartup()
        {
            LavaEngine.CurrentEngine.RegisterFilters( GetType() );
            //Template.RegisterFilter( GetType() );
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Special use method to get the property value of some unknown object. Handles special
        /// cases like Liquid Drops and recursive property searches.
        /// </summary>
        /// <param name="obj">The object whose property value we want.</param>
        /// <param name="propertyPath">The path to the property.</param>
        /// <returns>The value at the given path or null.</returns>
        //private static object GetPropertyValue( object obj, string propertyPath )
        //{
        //    if ( string.IsNullOrWhiteSpace( propertyPath ) )
        //    {
        //        return obj;
        //    }

        //    var properties = propertyPath.Split( new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

        //    while ( properties.Any() && obj != null )
        //    {
        //        if ( obj is Drop )
        //        {
        //            obj = ( (Drop)obj ).InvokeDrop( properties.First() );
        //        }
        //        else if ( obj is IDictionary )
        //        {
        //            obj = ( (IDictionary)obj )[properties.First()];
        //        }
        //        else if ( obj is IDictionary<string, object> )
        //        {
        //            obj = ( (IDictionary<string, object>)obj )[properties.First()];
        //        }
        //        else
        //        {
        //            var property = obj.GetType().GetProperty( properties.First() );
        //            obj = property != null ? property.GetValue( obj ) : null;
        //        }

        //        properties = properties.Skip( 1 ).ToList();
        //    }

        //    return obj;
        //}

        ///// <summary>
        ///// Converts the given value to a dictionary.
        ///// </summary>
        ///// <param name="input">The value to be converted.</param>
        ///// <returns>An IDictionary&lt;string, object&gt; that represents the value.</returns>
        private static IDictionary<string, object> AsDictionary( object input )
        {
            IDictionary<string, object> dict = new Dictionary<string, object>();

            if ( input == null || ( input is string && string.IsNullOrEmpty( (string)input ) ) )
            {
                /* Intentionally left blank */
            }
            else if ( input is IDictionary )
            {
                foreach ( DictionaryEntry kvp in (IDictionary)input )
                {
                    dict.Add( kvp.Key.ToString(), kvp.Value );
                }
            }
            else if ( input is IDictionary<string, object> )
            {
                dict = (IDictionary<string, object>)input;
            }
            else
            {
                throw new Exception( "Cannot convert to dictionary type." );
            }

            return dict;
        }

        #endregion

        #region Arrays

/*
        /// <summary>
        /// Orders a collection of elements by the specified property (or properties)
        /// and returns a new collection in that order.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="property">The property or properties to order the collection by.</param>
        /// <returns>A new collection sorted in the requested order.</returns>
        /// <example>
        ///     {% assign members = 287635 | GroupById | Property:'Members' | OrderBy:'GroupRole.IsLeader desc,Person.FullNameReversed' %}
        ///    <ul>
        ///    {% for member in members %}
        ///        <li>{{ member.Person.FullName }} - {{ member.GroupRole.Name }}</li>
        ///    {% endfor %}
        ///    </ul>
        /// </example>
        public static IEnumerable OrderBy( object input, string property )
        {
            IEnumerable<object> e = input is IEnumerable<object> ? input as IEnumerable<object> : new List<object>( new[] { input } );

            if ( !e.Any() || string.IsNullOrWhiteSpace( property ) )
            {
                return e;
            }

            //
            // Create a list of order by objects for the field to order by
            // and the ascending/descending flag.
            //
            var orderBy =
               property.Split( ',' ).Select(
                  s => s.Split( new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries ) )
                  .Select( a => new { Property = a[0], Descending = a.Length >= 2 && "desc" == a[1].ToLower() } )
                  .ToList();

            IOrderedQueryable<object> qry;
            if ( orderBy[0].Descending )
            {
                qry = e.Cast<object>().AsQueryable().OrderByDescending( d => d.GetPropertyValue( orderBy[0].Property ) );
            }
            else
            {
                qry = e.Cast<object>().AsQueryable().OrderBy( d => d.GetPropertyValue( orderBy[0].Property ) );
            }

            //
            // For the rest use ThenBy and ThenByDescending.
            //
            for ( int i = 1; i < orderBy.Count; i++ )
            {
                if ( orderBy[i].Descending )
                {
                    qry = qry.ThenByDescending( d => d.GetPropertyValue( orderBy[i].Property ) );
                }
                else
                {
                    var prop = orderBy[i].Property;
                    qry = qry.ThenBy( d => d.GetPropertyValue( prop ) );
                }
            }

            return qry;
        }
*/

        /// <summary>
        /// Takes a collection and returns distinct values in that collection.
        /// </summary>
        /// <param name="input">A collection of objects.</param>
        /// <returns>A collection of objects with no repeating elements.</returns>
        /// <example>
        ///     {{ 'hello,test,one,hello,two,one,three' | Split:',' | Distinct | ToJSON }}    
        /// </example>
        public static IEnumerable Distinct( object input )
        {
            IEnumerable<object> e = input is IEnumerable<object> ? input as IEnumerable<object> : new List<object>( new[] { input } );

            if ( !e.Any() )
            {
                return e;
            }

            return e.ToList().Distinct();
        }

        /// <summary>
        /// Takes a collection and returns distinct values in that collection based on the property.
        /// </summary>
        /// <param name="input">A collection of objects.</param>
        /// <returns>A collection of objects with no repeating elements.</returns>
        /// <example><![CDATA[
        /// {% assign items = '[{"PersonId":1,"Title":"Mr"},{"PersonId":2,"Title","Mrs"},{"PersonId":1,"Title":"Dr"}]' | FromJSON %}
        /// {{ items | DistinctBy:'PersonId' | ToJSON }}
        /// 
        /// {% groupmember where:'PersonId == "1"' %}
        ///     {{ groupmemberItems | DistinctBy:'Person.FirstName' | Select:'Id' | ToJSON }}
        /// {% endgroupmember %}
        /// ]]></example>
        public static IEnumerable DistinctBy( object input, string property )
        {
            IEnumerable<object> e = input is IEnumerable<object> ? input as IEnumerable<object> : new List<object>( new[] { input } );

            if ( !e.Any() || string.IsNullOrWhiteSpace( property ) )
            {
                return e;
            }

            return e.GroupBy( d => d.GetPropertyValue( property ) ).Select( x => x.FirstOrDefault() );
            //return e.AsQueryable().DistinctBy( d => GetPropertyValue( d, property ) );
        }

        /// <summary>
        /// Takes a collection and groups it by the specified property tree.
        /// </summary>
        /// <param name="input">A collection of objects to be grouped.</param>
        /// <param name="property">The property to use when grouping the objects.</param>
        /// <returns>A dictionary of group keys and value collections.</returns>
        /// <example><![CDATA[
        /// {% assign members = 287635 | GroupById | Property:'Members' | GroupBy:'GroupRole.Name' %}
        /// <ul>
        /// {% for member in members %}
        ///     {% assign parts = member | PropertyToKeyValue %}
        ///     <li>{{ parts.Key }}</li>
        ///     <ul>
        ///         {% for m in parts.Value %}
        ///             <li>{{ m.Person.FullName }}</li>
        ///         {% endfor %}
        ///     </ul>
        /// {% endfor %}
        /// </ul>
        /// ]]></example>
        public static object GroupBy( object input, string property )
        {
            IEnumerable<object> e = input is IEnumerable<object> ? input as IEnumerable<object> : new List<object>( new[] { input } );

            if ( !e.Any() )
            {
                return new Dictionary<string, List<object>>();
            }

            if ( string.IsNullOrWhiteSpace( property ) )
            {
                throw new Exception( "Must provide a property to group by." );
            }

            return e.AsQueryable()
                .GroupBy( x => x.GetPropertyValue( property ) )
                .ToDictionary( g => g.Key != null ? g.Key.ToString() : string.Empty, g => (object)g.ToList() );
        }

        /// <summary>
        /// Takes an enumerable and returns a new enumerable with the object appended.
        /// </summary>
        /// <param name="input">The existing enumerable.</param>
        /// <param name="newObject">The new object to append.</param>
        /// <returns>A new enumerable that contains the old objects and the new one.</returns>
        /// <example><![CDATA[
        /// {% assign array = '' | AddToArray:'one' %}
        /// {% assign array = array | AddToArray:'two' | AddToArray:'three' %}
        /// {% assign array = array | RemoveFromArray:'one' %}
        /// {{ array | ToJSON }}
        /// ]]></example>
        public static IEnumerable AddToArray( object input, object newObject )
        {
            List<object> array = new List<object>();

            if ( input == null || ( input is string && string.IsNullOrEmpty( (string)input ) ) )
            {
                /* Intentionally left blank, start with empty array. */
            }
            else if ( input is IEnumerable )
            {
                foreach ( object item in input as IEnumerable )
                {
                    array.Add( item );
                }
            }
            else
            {
                array.Add( input );
            }

            array.Add( newObject );

            return array;
        }

        /// <summary>
        /// Takes an enumerable and returns a new enumerable with the specified object removed.
        /// </summary>
        /// <param name="input">The existing enumerable.</param>
        /// <param name="newObject">The new object to remove.</param>
        /// <returns>A new enumerable that contains the old objects without the specified object.</returns>
        /// <example><![CDATA[
        /// {% assign array = '' | AddToArray:'one' %}
        /// {% assign array = array | AddToArray:'two' | AddToArray:'three' %}
        /// {% assign array = array | RemoveFromArray:'one' %}
        /// {{ array | ToJSON }}
        /// ]]></example>
        public static IEnumerable RemoveFromArray( object input, object oldObject )
        {
            List<object> array = new List<object>();

            if ( input == null || ( input is string && string.IsNullOrEmpty( (string)input ) ) )
            {
                /* Intentionally left blank, start with empty array. */
            }
            else if ( input is IEnumerable )
            {
                foreach ( object item in input as IEnumerable )
                {
                    array.Add( item );
                }
            }
            else
            {
                array.Add( input );
            }

            array.Remove( oldObject );

            return array;
        }

        /// <summary>
        /// Takes an enumerable and returns the sum of all the values.
        /// </summary>
        /// <param name="input">The existing enumerable.</param>
        /// <returns>A sum of all values in the input.</returns>
        /// <example><![CDATA[
        /// Total: {{ '3,5,7' | Split:',' | Sum }}
        /// ]]></example>
        public static object Sum( object input )
        {
            IEnumerable<object> e = input is IEnumerable<object> ? input as IEnumerable<object> : new List<object>( new[] { input } );
            var array = e.ToList();

            if ( !e.Any() )
            {
                return 0;
            }

            bool isDouble = false;
            array.ForEach( a => isDouble = isDouble || a is double );

            bool isDecimal = false;
            array.ForEach( a => isDecimal = isDecimal || a is decimal );

            bool isInteger = false;
            array.ForEach( a => isInteger = isInteger || a is int );

            if ( isDouble )
            {
                return array.Select( a => Convert.ToDouble( a ) ).Sum();
            }
            else if ( isDecimal )
            {
                return array.Select( a => Convert.ToDecimal( a ) ).Sum();
            }
            else if ( isInteger )
            {
                return array.Select( a => Convert.ToInt32( a ) ).Sum();
            }
            else
            {
                var result = array.Select( a => a.ToString().AsDouble() ).Sum();

                return result == Math.Truncate( result ) ? Convert.ToInt32( result ) : result;
            }
        }

        #endregion

        #region Dictionaries

        /// <summary>
        /// Takes an existing (or empty) dictionary and adds a new key and value.
        /// </summary>
        /// <param name="input">The existing dictionary.</param>
        /// <param name="key">The key to use when adding the value.</param>
        /// <param name="value">The value ot use when adding the key.</param>
        /// <returns>A new dictionary that contains the old values and the new value.</returns>
        /// <example><![CDATA[
        /// {% assign dict = '' | AddToDictionary:'key1','value2' %}
        /// {% assign dict = array | AddToDictionary:'key2','value2' | AddToDictionary:'key3','value3' %}
        /// {% assign dict = array | RemoveFromDictionary:'key2' %}
        /// {{ dict | ToJSON }}
        /// {{ dict | AllKeysFromDictionary }}
        /// ]]></example>
        public static IDictionary<string, object> AddToDictionary( object input, object key, object value )
        {
            var dict = AsDictionary( input );

            dict.Add( key.ToString(), value );

            return dict;
        }

        /// <summary>
        /// Takes an existing (or empty) dictionary and removes a key and it's value.
        /// </summary>
        /// <param name="input">The existing dictionary.</param>
        /// <param name="key">The key to use when removing the value.</param>
        /// <returns>A new dictionary that contains the old values without the specified key.</returns>
        /// <example><![CDATA[
        /// {% assign dict = '' | AddToDictionary:'key1','value2' %}
        /// {% assign dict = array | AddToDictionary:'key2','value2' | AddToDictionary:'key3','value3' %}
        /// {% assign dict = array | RemoveFromDictionary:'key2' %}
        /// {{ dict | ToJSON }}
        /// {{ dict | AllKeysFromDictionary }}
        /// ]]></example>
        public static IDictionary<string, object> RemoveFromDictionary( object input, object key )
        {
            var dict = AsDictionary( input );

            dict.Remove( key.ToString() );

            return dict;
        }

        /// <summary>
        /// Returns an array of all keys in the dictionary.
        /// </summary>
        /// <param name="input">The existing dictionary.</param>
        /// <returns>An enumerable collection of keys.</returns>
        /// <example><![CDATA[
        /// {% assign dict = '' | AddToDictionary:'key1','value2' %}
        /// {% assign dict = array | AddToDictionary:'key2','value2' | AddToDictionary:'key3','value3' %}
        /// {% assign dict = array | RemoveFromDictionary:'key2' %}
        /// {{ dict | ToJSON }}
        /// {{ dict | AllKeysFromDictionary }}
        /// ]]></example>
        public static IEnumerable<string> AllKeysFromDictionary( object input )
        {
            var dict = AsDictionary( input );

            return dict.Keys.ToList();
        }

        #endregion

        #region Strings

        /// <summary>
        /// Run RegEx replacing on a string.
        /// </summary>
        /// <param name="input">The lava source to process.</param>
        /// <param name="pattern">The regex pattern to use when matching.</param>
        /// <param name="replacement">The string to use when doing replacement.</param>
        /// <param name="options">The regex options to modify the matching.</param>
        /// <example><![CDATA[
        /// {{ 'The Rock is awesome.' | RegExReplace:'the rock','Rock','i' }}
        /// {{ 'Hello Ted, how are you?' | RegExReplace:'[Hh]ello (\w+)','Greetings $1' }}
        /// ]]></example>
        public static object RegExReplace( object input, object pattern, object replacement, string options = null )
        {
            RegexOptions regexOptions = RegexOptions.None;
            var inputString = input.ToString();

            options = options ?? string.Empty;

            if ( options.Contains( 'm' ) )
            {
                regexOptions |= RegexOptions.Multiline;
            }

            if ( options.Contains( 'i' ) )
            {
                regexOptions |= RegexOptions.IgnoreCase;
            }

            return Regex.Replace( inputString, pattern.ToString(), replacement.ToString(), regexOptions );
        }

        /// <summary>
        /// Processes the Lava code in the source string.
        /// </summary>
        /// <param name="input">The lava source to process.</param>
        /// <example><![CDATA[
        /// {% capture lava %}{% raw %}{% assign test = "hello" %}{{ test }}{% endraw %}{% endcapture %}
        /// {{ lava | RunLava }}
        /// ]]></example>
        public static string RunLava( ILavaContext context, object input )
        {
            if ( input == null )
            {
                return null;
            }

            return LavaEngine.CurrentEngine.RenderTemplate( input.ToString(), context );
        }

        #endregion

        #region Misc

        /// <summary>
        /// Adds a header to the Http Response.
        /// </summary>
        /// <param name="input">The header to add, must be in form of "X-Header: value".</param>
        /// <example><![CDATA[
        /// {{ 'X-Header: my-value' | AddHttpHeader }}
        /// ]]></example>
        public static void AddHttpHeader( object input )
        {
            var response = System.Web.HttpContext.Current.Response;

            if ( !( input is string ) )
            {
                throw new Exception( "Cannot add non-string to response headers." );
            }

            var elements = ( (string)input ).Split( new char[] { ':' }, 2 );

            if ( elements.Length != 2 )
            {
                throw new Exception( "Header to add not in correct format." );
            }

            response.Headers.Add( elements[0].Trim(), elements[1].Trim() );
        }

/*
        /// <summary>
        /// Convert the given string or byte array into a base64 string.
        /// </summary>
        /// <param name="input">The string or byte array to be converted.</param>
        /// <example><![CDATA[
        /// {{ 'hello' | ToBase64 }}
        /// ]]></example>
        public static string ToBase64( object input )
        {
            if ( input is ICollection<byte> )
            {
                return Convert.ToBase64String( ( input as ICollection<byte> ).ToArray() );
            }
            else
            {
                return Convert.ToBase64String( System.Text.Encoding.UTF8.GetBytes( input.ToString() ) );
            }
        }
*/

        /// <summary>
        /// Convert the given string or byte array into a base64 string.
        /// </summary>
        /// <param name="input">The string or byte array to be converted.</param>
        /// <param name="asString">If true then the returned data is cast to a string.</param>
        /// <example><![CDATA[
        /// {{ 'aGVsbG8=' | FromBase64:true }}
        /// {{ 'aGVsbG8=' | FromBase64 | ComputeHash }}
        /// ]]></example>
        public static object FromBase64( object input, object asString = null )
        {
            var data = Convert.FromBase64String( input.ToString() );

            if ( asString != null && asString.ToString().AsBoolean() )
            {
                return System.Text.Encoding.UTF8.GetString( data );
            }

            return data;
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// Contains information about a matched group in a regular expression.
        /// </summary>
        public class RegexGroup : ILavaDataDictionarySource
        {
            public int Index { get; set; }

            public int Length { get; set; }

            public string Value { get; set; }

            public RegexGroup( Group group )
            {
                Index = group.Index;
                Length = group.Length;
                Value = group.Value;
            }

            public virtual ILavaDataDictionary GetLavaDataDictionary()
            {
                var dictionary = new LavaDataDictionary()
                {
                    { "Index", Index },
                    { "Length", Length },
                    { "Value", Value }
                };

                return dictionary;
            }
        }

        /// <summary>
        /// Contains information about a match in a regular expression.
        /// </summary>
        public class RegexMatch : RegexGroup
        {
            public List<RegexGroup> Groups { get; set; }

            public RegexMatch( Match match )
                : base( match )
            {
                Groups = new List<RegexGroup>();

                foreach ( Group g in match.Groups )
                {
                    Groups.Add( new RegexGroup( g ) );
                }
            }

            public override ILavaDataDictionary GetLavaDataDictionary()
            {
                var dictionary = base.GetLavaDataDictionary() as LavaDataDictionary;

                dictionary.Add( "Groups", Groups );

                return dictionary;
            }
        }

        #endregion
    }
}