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
using Rock.Lava.DotLiquid;
using Rock.Lava.Blocks;
using System.IO;
using Rock.Lava.Fluid;

namespace Rock.Lava
{

    /// <summary>
    /// Provides access to core functions for the Rock Lava Engine.
    /// </summary>
    public static partial class LavaEngine
    {
        public static string ShortcodeNameSuffix = "_sc";

        private static ILavaEngine _instance = null;
        private static LavaEngineTypeSpecifier _liquidFramework = LavaEngineTypeSpecifier.DotLiquid;

        public static LavaEngineTypeSpecifier LiquidFramework
        {
            get
            {
                return _liquidFramework;
            }
            set
            {
                if ( _liquidFramework != value )
                {
                    _liquidFramework = value;

                    // Reset the existing instance so it can be re-created on next access.
                    _instance = null;
                }
            }
        }

        //public static void InitializeDotLiquidFramework( ILavaFileSystem fileSystem = null, IList<Type> filterImplementationTypes = null )
        //{
        //    _liquidFramework = LavaEngineTypeSpecifier.DotLiquid;

        //    var liquidEngine = new DotLiquidEngine();

        //    liquidEngine.Initialize( fileSystem, filterImplementationTypes );

        //    _instance = liquidEngine;
        //}

        public static void Initialize( LavaEngineTypeSpecifier? engineType, ILavaFileSystem fileSystem = null, IList<Type> filterImplementationTypes = null )
        {
            _liquidFramework = engineType ?? LavaEngineTypeSpecifier.DotLiquid;

            ILavaEngine engine;

            if ( _liquidFramework == LavaEngineTypeSpecifier.Fluid )
            {
                engine = new FluidEngine();

                fileSystem = new FluidFileSystem( fileSystem );
            }
            else
            {
                engine = new DotLiquidEngine();

                fileSystem = new DotLiquidFileSystem( fileSystem );
            }

            engine.Initialize( fileSystem, filterImplementationTypes );

            _instance = engine;
        }

        public static ILavaEngine Instance
        {
            get
            {
                if ( _instance == null )
                {
                    // Initialize a default instance.
                    Initialize( _liquidFramework );
                }

                return _instance;
            }
        }
    }
}