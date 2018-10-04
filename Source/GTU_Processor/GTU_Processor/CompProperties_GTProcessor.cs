using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace GTU_Processor
{
    public class CompProperties_GTProcessor : CompProperties
    {
        public ThingDef thingIngredient;

        public ThingDef thingResult;

        public int ingredientCount = -1;

        public int resultCount = 1;

        public bool spawnOnFloor;

        public int spawnMaxAdjacent = -1;

        public bool spawnForbidden;

        public bool requiresPower;

        public bool writeRequiredIngredients;

        public bool writeTimeLeftToProcess;

        public bool showMessageIfOwned;

        public string workVerb = "Processing";

        public string workVerbPast = "Processed";

        public IntRange durationIntervalRange = new IntRange(100, 100);

        public CompProperties_GTProcessor()
        {
            this.compClass = typeof(CompGTProcessor);
        }
    }
}
