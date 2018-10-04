using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace GTU_Processor
{
    public class CompProperties_GTLifespan : CompProperties
    {
        public bool killAtEnd;

        public IntRange lifetimeRange = new IntRange(100, 100);

        public bool showMessageIfOwned;

        public string expiredMessage = null;

        public bool writeTimeLeft;

        public string endVerb = "Expires";

        public CompProperties_GTLifespan()
        {
            this.compClass = typeof(CompGTLifespan);
        }
    }
}
