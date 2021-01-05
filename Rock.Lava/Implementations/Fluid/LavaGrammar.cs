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
using Fluid;
using Irony.Parsing;

namespace Rock.Lava.Fluid
{
    /// <summary>
    /// A wrapper for a Lava Block that can be rendered by the Fluid templating engine.
    /// </summary>
    /// <remarks>


    public class LavaGrammar : FluidGrammar
    {
        public override void OnGrammarDataConstructed( LanguageData language )
        {
            base.OnGrammarDataConstructed( language );

            Elsif.Rule |= ToTerm( "elseif" ) + Expression;

            FilterArguments.Rule |= MakeListRule( FilterArguments, ToTerm(" "), FilterArgument );
        }
    }
}
