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

namespace Rock.Lava
{
    /// <summary>
    /// Configuration options used to initialize the Lava Engine.
    /// </summary>
    public class LavaEngineConfigurationOptions
    {
        /// <summary>
        /// Gets or sets the component that provides environment-specific caching for the Lava Engine.
        /// </summary>
        public ILavaTemplateCacheService CacheService { get; set; }

        /// <summary>
        /// Gets or sets the component that provides environment-specific file access for the Lava Engine.
        /// The file system is required to locate aand load templates reference by the {% include %} command.
        /// </summary>
        public ILavaFileSystem FileSystem { get; set; }

        /// <summary>
        /// Gets or sets the collection of Types that define custom Lava filter functions.
        /// By convention, filters are declared as public static methods of a class Type.
        /// </summary>
        public IEnumerable<Type> FilterImplementationTypes { get; set; }
    }
}
