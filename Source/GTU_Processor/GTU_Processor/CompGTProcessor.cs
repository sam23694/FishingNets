using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using UnityEngine;

namespace GTU_Processor
{
    public class CompGTProcessor : ThingComp
    {
        private int ticksRemaining;

        private int tickInterval;

        private int ingredientStored;

        private const string ActiveOffGraphicSuffix = "_ActiveOff";

        private const string ActiveOnGraphicSuffix = "_ActiveOn";

        public CompProperties_GTProcessor Props
        {
            get
            {
                return (CompProperties_GTProcessor)this.props;
            }
        }

        private bool PowerOn
        {
            get
            {
                CompPowerTrader comp = this.parent.GetComp<CompPowerTrader>();
                return comp != null && comp.PowerOn;
            }
        }

        private bool Empty
        {
            get
            {
                return this.ingredientStored <= 0;
            }
        }

        public int IngredientRequired
        {
            get
            {
                return this.Props.ingredientCount - this.ingredientStored;
            }
        }

        public bool Full
        {
            get
            {
                return this.IngredientRequired <= 0;
            }
        }

        public bool Completed
        {
            get
            {
                return this.Full && this.ticksRemaining <= 0;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.ticksRemaining, "ticksRemaining", 0, false);
            Scribe_Values.Look<int>(ref this.ingredientStored, "ingredientStored", 0, false);
            Scribe_Values.Look<int>(ref this.tickInterval, "tickInterval", 0, false);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (!respawningAfterLoad)
            {
                this.ResetTicksRemaining();
            }
        }
        
        public override void CompTickRare()
        {
            this.TickInterval(250);
        }

        private void TickInterval(int interval)
        {
            if (this.Props.requiresPower && !this.PowerOn)
            {
                return;
            }
            if (this.Full)
            {
                if(this.ticksRemaining > 0)
                {
                    this.ticksRemaining -= interval;
                }
                if (this.Props.spawnOnFloor && this.AttemptSpawn())
                {
                    this.ResetTicksRemaining();
                }
            }
        }

        private bool AttemptSpawn()
        {
            if (this.ticksRemaining <= 0)
            {
                if (this.Props.spawnMaxAdjacent >= 0)
                {
                    int num = 0;
                    for (int i = 0; i < 9; i++)
                    {
                        IntVec3 c = this.parent.Position + GenAdj.AdjacentCellsAndInside[i];
                        if (c.InBounds(this.parent.Map))
                        {
                            List<Thing> thingList = c.GetThingList(this.parent.Map);
                            for (int j = 0; j < thingList.Count; j++)
                            {
                                if (thingList[j].def == this.Props.thingResult)
                                {
                                    num += thingList[j].stackCount;
                                    if (num >= this.Props.spawnMaxAdjacent)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
                IntVec3 center;
                if (this.TryFindSpawnCell(out center))
                {
                    Thing thing = ThingMaker.MakeThing(this.Props.thingResult, null);
                    thing.stackCount = this.Props.resultCount;
                    Thing t;
                    GenPlace.TryPlaceThing(thing, center, this.parent.Map, ThingPlaceMode.Direct, out t, null, null);
                    if (this.Props.spawnForbidden)
                    {
                        t.SetForbidden(true, true);
                    }
                    if(this.Props.showMessageIfOwned && this.parent.Faction == Faction.OfPlayer)
                    {
                        Messages.Message("MessageCompSpawnerSpawnedItem".Translate(new object[]
                        {
                            this.Props.thingResult.LabelCap
                        }).CapitalizeFirst(), thing, MessageTypeDefOf.PositiveEvent, true);
                    }
                    return true;
                }
            }
            return false;
        }

        private bool TryFindSpawnCell(out IntVec3 result)
        {
            foreach (IntVec3 current in GenAdj.CellsAdjacent8Way(this.parent).InRandomOrder(null))
            {
                if (current.Walkable(this.parent.Map))
                {
                    Building edifice = current.GetEdifice(this.parent.Map);
                    if(edifice == null || !this.Props.thingResult.IsEdifice())
                    {
                        Building_Door building_door = edifice as Building_Door;
                        if(building_door == null || building_door.FreePassage)
                        {
                            if(this.parent.def.passability == Traversability.Impassable || GenSight.LineOfSight(this.parent.Position, current, this.parent.Map, false, null, 0, 0))
                            {
                                bool flag = false;
                                List<Thing> thingList = current.GetThingList(this.parent.Map);
                                for(int i = 0; i < thingList.Count; i++)
                                {
                                    Thing thing = thingList[i];
                                    if(thing.def.category == ThingCategory.Item && (thing.def != this.Props.thingResult || thing.stackCount > this.Props.thingResult.stackLimit - this.Props.resultCount))
                                    {
                                        flag = true;
                                        break;
                                    }
                                }
                                if (!flag)
                                {
                                    result = current;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            result = IntVec3.Invalid;
            return false;
        }
        
        public Thing TakeOutResult()
        {
            if (!this.Completed)
            {
                Log.Warning("Tried to remove processor result before it completed.", false);
                return null;
            }
            Thing thing = ThingMaker.MakeThing(this.Props.thingResult, null);
            thing.stackCount = this.Props.resultCount;
            this.ResetTicksRemaining();
            return thing;
        }

        public void AddIngredient(int count)
        {
            if (this.Full)
            {
                Log.Warning("Tried to add ingredients to CompProperties_GTProcessor when it is full.", false);
            }
            int num = Mathf.Min(count, this.IngredientRequired);
            if (num <= 0) return;
            this.ingredientStored += num;
        }

        public void AddIngredient(Thing ingredient)
        {
            if(ingredient.def == this.Props.thingIngredient)
            {
                int num = Mathf.Min(ingredient.stackCount, this.IngredientRequired);
                if (num > 0)
                {
                    this.AddIngredient(num);
                    ingredient.SplitOff(num).Destroy(DestroyMode.Vanish);
                }
            }
            
        }

        private void ResetTicksRemaining()
        {
            this.ingredientStored = 0;
            this.ticksRemaining = this.Props.durationIntervalRange.RandomInRange;
            this.tickInterval = this.ticksRemaining;
        }

        public override string CompInspectStringExtra()
        {
            string str = null;
            if (!this.Full && this.Props.writeRequiredIngredients)
            {
                str = this.ingredientStored.ToString() + "/" + this.Props.ingredientCount.ToString() + " " + this.Props.thingIngredient.label;
            }
            else if (this.Full && !this.Completed && this.Props.writeTimeLeftToProcess)
            {
                float progInt = 0f;
                string workVerb = " " + this.Props.workVerb + " ";
                if (this.tickInterval != 0)
                {
                    progInt = 1f - (float)this.ticksRemaining / this.tickInterval;
                }
                str = progInt.ToStringPercent() + workVerb + this.ticksRemaining.ToStringTicksToPeriod();
            }
            else if (this.Completed && !this.Props.spawnOnFloor && this.Props.writeTimeLeftToProcess)
            {
                string workVerbPast = " " + this.Props.workVerbPast;
                str = 1f.ToStringPercent() + workVerbPast;
            }
            return str;
        }

        [DebuggerHidden]
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Progress 1 hour",
                    action = delegate
                    {
                        this.ticksRemaining -= 2500;
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Progress 1 day",
                    action = delegate
                    {
                        this.ticksRemaining -= 60000;
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Complete processing",
                    action = delegate
                    {
                        this.ticksRemaining = 0;
                    }
                };
            }
        }
    }
}
