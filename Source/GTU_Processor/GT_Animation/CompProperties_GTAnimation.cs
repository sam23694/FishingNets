using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace GT_Animation
{
    class CompProperties_GTAnimation : CompProperties
    {
        public bool randomized = false;

        public int frameSpeed = 15;

        public CompProperties_GTAnimation()
        {
            this.compClass = typeof(CompGTAnimation);
        }
    }
}
