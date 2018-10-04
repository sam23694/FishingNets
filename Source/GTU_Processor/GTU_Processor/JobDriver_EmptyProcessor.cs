using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace GTU_Processor
{
    public class JobDriver_EmptyProcessor : JobDriver
    {
        private const TargetIndex ProcessorInd = TargetIndex.A;

        private const TargetIndex ResultInd = TargetIndex.B;

        private const TargetIndex StorageCellInd = TargetIndex.C;

        protected ThingWithComps Processor
        {
            get
            {
                return (ThingWithComps)this.job.GetTarget(TargetIndex.A).Thing;
            }
        }

        protected Thing Result
        {
            get
            {
                return this.job.GetTarget(TargetIndex.B).Thing;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.Processor, this.job, 1, -1, null, errorOnFailed);
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(200, TargetIndex.None).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).FailOn(() => !this.Processor.GetComp<CompGTProcessor>().Completed).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            yield return new Toil
            {
                initAction = delegate
                {
                    Thing thing = this.Processor.GetComp<CompGTProcessor>().TakeOutResult();
                    GenPlace.TryPlaceThing(thing, this.pawn.Position, this.Map, ThingPlaceMode.Near, null, null);
                    StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(thing);
                    IntVec3 c;
                    if(StoreUtility.TryFindBestBetterStoreCellFor(thing, this.pawn, this.Map, currentPriority, this.pawn.Faction, out c, true))
                    {
                        this.job.SetTarget(TargetIndex.C, c);
                        this.job.SetTarget(TargetIndex.B, thing);
                        this.job.count = thing.stackCount;
                    }
                    else
                    {
                        this.EndJobWith(JobCondition.Incompletable);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
            yield return Toils_Reserve.Reserve(TargetIndex.C, 1, -1, null);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, false, false);
            Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
            yield return carryToCell;
            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, carryToCell, true);
        }
    }
}
