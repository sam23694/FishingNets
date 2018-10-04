using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace GTU_Processor
{
    public class WorkGiver_FillProcessor : WorkGiver_Scanner
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
            if(comp == null || comp.Full)
            {
                return false;
            }
            if (!t.IsForbidden(pawn))
            {
                LocalTargetInfo target = t;
                if(pawn.CanReserve(target, 1, -1, null, forced))
                {
                    if(pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
                    {
                        return false;
                    }
                    if(this.FindIngredient(pawn, processor) == null)
                    {
                        JobFailReason.Is("Could not find ingredient", null);
                        return false;
                    }
                    return !t.IsBurning();
                }
            }

            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            ThingWithComps processor = t as ThingWithComps;
            Thing t2 = this.FindIngredient(pawn, processor);
            return new Job(JobDefOf.FillGTProcessor, t, t2);
        }

        private Thing FindIngredient(Pawn pawn, ThingWithComps processor)
        {
            CompGTProcessor comp = processor.GetComp<CompGTProcessor>();
            Predicate<Thing> predicate = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false);
            IntVec3 position = pawn.Position;
            Map map = pawn.Map;
            ThingRequest thingReq = ThingRequest.ForDef(ThingDefOf.WoodLog);//comp.Props.thingIngredient);
            PathEndMode peMode = PathEndMode.ClosestTouch;
            TraverseParms traverseParms = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
            Predicate<Thing> validator = predicate;
            return GenClosest.ClosestThingReachable(position, map, thingReq, peMode, traverseParms, 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
        }
    }
}
