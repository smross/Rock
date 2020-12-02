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

using System.Collections.Generic;
using Fluid;

namespace Rock.Lava.Fluid
{
    //public class LavaFluidParser : DefaultFluidParser
    //{
    //    public LavaFluidParser( LanguageData languageData, Dictionary<string, ITag> tags, Dictionary<string, ITag> blocks )
    //        : base( languageData, tags, blocks )
    //    {
    //        // refer https://github.com/sebastienros/fluid/blob/464c60768ec520a7c3d8b709ecb7037df36d543b/Fluid/DefaultFluidParser.cs
    //    }
    //}

    public class LavaFluidTemplate : BaseFluidTemplate<LavaFluidTemplate>
    {
        /// <summary>
        /// The text of the Lava source template.
        /// </summary>
        public string SourceDocument { get; set; }

        public List<FluidParsedTemplateElement> Elements { get; set; }

        static LavaFluidTemplate()
        {
            //Factory.RegisterTag<QuoteTag>( "quote" );
            //Factory.RegisterBlock<RepeatBlock>( "repeat" );
            //var parserFactory = new LavaFluidParserFactory();

            //parserFactory.
            //SetParserFactory( parserFactory );
        }
    }



    

}
