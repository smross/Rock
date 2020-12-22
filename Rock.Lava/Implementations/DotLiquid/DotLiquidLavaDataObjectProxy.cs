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

namespace Rock.Lava.DotLiquid
{
    /// <summary>
    /// An implementation of a Lava Data Object that can be used by the DotLiquid Templating Framework.
    /// </summary>
    internal class DotLiquidLavaDataObjectProxy : ILiquidizable, IIndexable, IValueTypeConvertible, ILavaDataObject
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
        /// Returns a representation of this object that can be safely used by the Liquid templating language.
        /// </summary>
        /// <returns></returns>
        public object ToLiquid()
        {
            return this;
        }

        #endregion

        /// <summary>
        /// Return a string representation of the object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // If we are wrapping an object instance, return the ToString() for the object.
            if ( _dataObject != null )
            {
                return _dataObject.ToString();
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
