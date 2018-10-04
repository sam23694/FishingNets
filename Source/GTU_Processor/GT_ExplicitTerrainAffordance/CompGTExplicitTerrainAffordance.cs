using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace GT_ExplicitTerrainAffordance
{
    class CompGTExplicitTerrainAffordance : ThingComp
    {
        public CompProperties_GTExplicitTerrainAffordance Props
        {
            get
            {
                return (CompProperties_GTExplicitTerrainAffordance)this.props;
            }
        }
    }
}
