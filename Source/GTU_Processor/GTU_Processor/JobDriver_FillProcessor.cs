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
    public class JobDriver_FillProcessor : JobDriver
    {
        private const TargetIndex ProcessorInd = TargetIndex.A;

        private const TargetIndex IngredientInd = TargetIndex.B;

        protected ThingWithComps Processor
        {
            get
            {
                return (ThingWithComps)this.job.GetTarget(TargetIndex.A).Thing;
            }
        }

        protected Thing Ingredient
        {
            get
            {
                return this.job.GetTarget(TargetIndex.B).Thing;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.Processor, this.job, 1, -1, null, errorOnFailed) && this.pawn.Reserve(this.Ingredient, this.job, 1, -1, null, errorOnFailed);
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            base.AddEndCondition(() => (!this.Processor.GetComp<CompGTProcessor>().Full) ? JobCondition.Ongoing : JobCondition.Succeeded);
            yield return Toils_General.DoAtomic(delegate
            {
                this.job.count = this.Processor.GetComp<CompGTProcessor>().IngredientRequired;
            });
            Toil reserveIngredient = Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
            yield return reserveIngredient;
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true, false).FailOnDestroyedNullOrForbidden(TargetIndex.B);
            yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveIngredient, TargetIndex.B, TargetIndex.None, true, null);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(200, TargetIndex.None).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            yield return new Toil
            {
                initAction = delegate
                {
                    this.Processor.GetComp<CompGTProcessor>().AddIngredient(this.Ingredient);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}
