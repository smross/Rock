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
using DotLiquid;

namespace Rock.Lava.DotLiquid
{
    /// <summary>
    /// An implementation of a Lava Data Object that can be used by the DotLiquid Templating Framework.
    /// </summary>
    internal class DotLiquidLavaDataObjectProxy : ILiquidizable, IIndexable, IValueTypeConvertible, IDictionary<string, object>, ILavaDataObjectSource // ILavaDataObject
    {
        private ILavaDataObject _dataObject = null;

        #region Constructors

        public DotLiquidLavaDataObjectProxy( ILavaDataObject dataObject )
        {
            _dataObject = dataObject;
        }

        #endregion

        #region IIndexable implementation

        /// <summary>
        /// Returns the data value associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[object key]
        {
            get
            {
                return GetValue( key );
            }
        }

        #region IValueTypeConvertible implementation

        public object ConvertToValueType()
        {
            if ( _dataObject == null )
            {
                return null;
            }

            return _dataObject;
        }

        #endregion

        #region ILavaDataObject implementation

        /// <summary>
        /// Returns a flag indicating if this data object contains a value associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey( object key )
        {
            if ( _dataObject == null )
            {
                return false;
            }

            return _dataObject.ContainsKey( key );
        }

        #endregion

        /// <summary>
        ///if this 
        /// Gets a list of the keys defined by this data object.
        /// </summary>
        public List<string> AvailableKeys
        {
            get
            {
                if ( _dataObject == null )
                {
                    return new List<string>();
                }

                return _dataObject.AvailableKeys;
            }
        }

        /// <summary>
        /// Returns the data value associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetValue( object key )
        {
            if ( _dataObject == null )
            {
                return null;
            }

            return _dataObject.GetValue( key );
        }

        #endregion

        #region ILiquidizable implementation

        /// <summary>
        /// Returns a representation of this object that can be accessed by the DotLiquid framework.
        /// </summary>
        /// <returns></returns>
        public object ToLiquid()
        {
            if ( _dataObject == null )
            {
                return null;
            }

            if ( _dataObject is IDictionary<string, object> dictionary )
            {
                return dictionary;
            }

            if ( _dataObject is LavaDataObject rockDynamic )
            {
                // Return the RockDynamic object as a dictionary of values.
                return rockDynamic; //.AsDictionary();
            }

            return new DropProxy( _dataObject, this.AvailableKeys.ToArray(), (x) => { return _dataObject; } );

            if ( _dataObject is ILavaDataObject dataObject )
            {
                // Return the RockDynamic object as a dictionary of values.
                return dataObject;
                // this; // rockDynamic.AsDictionary();
            }



            return _dataObject;

            //return this;
        }

        public Dictionary<string, object> AsDictionary()
        {
            var dictionary = new Dictionary<string, object>();

            foreach ( var key in this.AvailableKeys )
            {
                dictionary.Add( key, GetValue( key ) );
            }

            return dictionary;
        }

        #endregion

        /// <summary>
        /// Return a string representation of the object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // Return the ToString() for the proxied object.
            if ( _dataObject != null )
            {
                return _dataObject.ToString();
            }
            else
            {
                return null;
            }
        }

        #region IDictionary<string, object> implementation

        public bool ContainsKey( string key )
        {
            return _dataObject.ContainsKey( key );
        }

        public void Add( string key, object value )
        {
            throw new NotImplementedException( "Lava Data Object is read-only." );
        }

        public bool Remove( string key )
        {
            throw new NotImplementedException( "Lava Data Object is read-only." );
        }

        public bool TryGetValue( string key, out object value )
        {
            throw new NotImplementedException();
        }

        public void Add( KeyValuePair<string, object> item )
        {
            throw new NotImplementedException( "Lava Data Object is read-only." );
        }

        public void Clear()
        {
            throw new NotImplementedException( "Lava Data Object is read-only." );
        }

        public bool Contains( KeyValuePair<string, object> item )
        {
            throw new NotImplementedException();
        }

        public void CopyTo( KeyValuePair<string, object>[] array, int arrayIndex )
        {
            throw new NotImplementedException();
        }

        public bool Remove( KeyValuePair<string, object> item )
        {
            throw new NotImplementedException( "Lava Data Object is read-only." );
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            var dictionary = new Dictionary<string, object>();

            foreach ( var key in this.AvailableKeys )
            {
                dictionary.Add( key, GetValue( key ) );
            }

            return dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        ICollection<string> IDictionary<string, object>.Keys
        {
            get
            {
                return AvailableKeys;
            }
        }

        ICollection<object> IDictionary<string, object>.Values => throw new NotImplementedException();

        int ICollection<KeyValuePair<string, object>>.Count
        {
            get
            {
                return _dataObject.AvailableKeys.Count;
            }
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly
        {
            get
            {
                return true;
            }

        }

        object IDictionary<string, object>.this[string key]
        {
            get
            {
                return this[key];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /*
        public class LavaDictionaryEnumerator : IEnumerator<KeyValuePair<string, object>>
        {
            public FibonacciEnumerator()
            {
                Reset();
            }

            KeyValuePair<string, object> Last { get; set; }
            public KeyValuePair<string, object> Current { get; private set; }
            object IEnumerator.Current { get { return Current; } }

            public void Dispose()
        {
        //
        }

            public void Reset()
            {
                Current = -1;
            }

            public bool MoveNext()
            {
                if ( Current == -1 )
                {
                    //State after first call to MoveNext() before the first element is read
                    //Fibonacci is defined to start with 0.
                    Current = 0;
                }
                else if ( Current == 0 )
                {
                    //2nd element in the Fibonacci series is defined to be 1.
                    Current = 1;
                }
                else
                {
                    //Fibonacci infinite series algorithm.
                    KeyValuePair<string, object> next = Current + Last;
                    Last = Current;
                    Current = next;
                }
                //infinite series. there is always another.
                return true;
            }
        }
*/

        #endregion

        #region ILavaDataObjectSource
        public ILavaDataObject GetLavaDataObject()
        {
            return _dataObject;
        }

        #endregion
    }
}
