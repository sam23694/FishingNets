using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace GTU_Processor
{
    public class WorkGiver_EmptyProcessor : WorkGiver_Scanner
    {

        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);
            }
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            ThingWithComps processor = t as ThingWithComps;
            CompGTProcessor comp = processor.GetComp<CompGTProcessor>();
            if(comp == null || !comp.Completed || comp.Props.spawnOnFloor)
            {
                return false;
            }
            if (!t.IsForbidden(pawn))
            {
                LocalTargetInfo target = t;
                if(pawn.CanReserve(target, 1, -1, null, forced))
                {
                    return !t.IsBurning();
                }
            }
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return new Job(JobDefOf.EmptyGTProcessor, t);
        }
    }
}