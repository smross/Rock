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

namespace Rock.Lava
{
    /// <summary>
    /// A component that is capable of providing a compiled Lava template object from a Lava source document.
    /// The strategy for compiling, caching and retrieving the object is determined by the provider.
    /// </summary>
    public interface ILavaTemplateCacheService
    {
        /// <summary>
        /// Gets an item from cache or if it does not exist, executes the supplied itemFactory function to create the item and add it to the cache.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="itemFactory">The item factory.</param>
        /// <param name="template"></param>
        /// <returns></returns>
        bool TryGetTemplate( string content, Func<ILavaTemplate> itemFactory, out ILavaTemplate template );

        /// <summary>
        /// Remove all entries from the cache.
        /// </summary>
        void ClearCache();
    }
}