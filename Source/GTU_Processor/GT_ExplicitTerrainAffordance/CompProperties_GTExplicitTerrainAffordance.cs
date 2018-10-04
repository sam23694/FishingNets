using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace GT_ExplicitTerrainAffordance
{
    class CompProperties_GTExplicitTerrainAffordance : CompProperties
    {
        public List<TerrainDef> requiredTerrains = null;

        public List<TerrainAffordanceDef> requiredAffordances = null;

        public CompProperties_GTExplicitTerrainAffordance()
        {
            this.compClass = typeof(CompGTExplicitTerrainAffordance);
        }
    }
}
