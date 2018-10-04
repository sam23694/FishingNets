using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using RimWorld;

namespace GT_ExplicitTerrainAffordance
{
    public class PlaceWorker_ExplicitAffordance : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            CompProperties_GTExplicitTerrainAffordance comp = ((ThingDef)checkingDef).GetCompProperties<CompProperties_GTExplicitTerrainAffordance>();
            if(comp.requiredAffordances != null)
            {
                for (int i = 0; i < comp.requiredAffordances.Count; i++)
                {
                    if (map.terrainGrid.TerrainAt(loc).affordances.Contains(comp.requiredAffordances[i]))
                    {
                        return true;
                    }
                }
            }
            if (comp.requiredTerrains != null)
            {
                for (int i = 0; i < comp.requiredTerrains.Count; i++)
                {
                    if (map.terrainGrid.TerrainAt(loc) == comp.requiredTerrains[i])
                    {
                        return true;
                    }
                }
            }
            return "TerrainCannotSupport".Translate();
        }
    }
}
