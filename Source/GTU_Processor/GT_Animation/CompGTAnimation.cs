using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace GT_Animation
{
    class CompGTAnimation : ThingComp
    {
        public CompProperties_GTAnimation Props
        {
            get
            {
                return (CompProperties_GTAnimation)this.props;
            }
        }
    }
}
