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
using System.IO;
using System.Text.Encodings.Web;

namespace Rock.Lava
{
    /// <summary>
    /// A component that can parse and render a Lava document element using a Liquid templating framework.
    /// </summary>
    internal interface ILiquidFrameworkElementRenderer
    {
        void Render( ILiquidFrameworkElementRenderer baseRenderer, ILavaContext context, TextWriter writer, TextEncoder encoder );

        [Obsolete]
        void Parse( ILiquidFrameworkElementRenderer baseRenderer, List<string> tokens, out List<object> nodes );
    }
}
