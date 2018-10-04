using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace GTU_Processor
{
    public class CompGTLifespan : ThingComp
    {
        private int ticksRemaining;

        private int tickInterval;

        public CompProperties_GTLifespan Props
        {
            get
            {
                return (CompProperties_GTLifespan)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.ticksRemaining, "ticksRemaining", 0, false);
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
            if(this.ticksRemaining > 0)
            {
                this.ticksRemaining -= interval;
            }
            else
            {
                if (this.Props.killAtEnd)
                {
                    this.TryToKill();
                }
                else
                {
                    this.TryToEnd();
                }
                this.ResetTicksRemaining();
            }
        }

        private void TryToEnd()
        {
            this.ShowEndMessage();
            this.parent.Destroy(0);
        }

        private void TryToKill()
        {
            this.ShowEndMessage();
            this.parent.Kill(null, null);
        }

        private void ShowEndMessage()
        {
            if (this.Props.showMessageIfOwned)
            {
                if (this.Props.expiredMessage == null)
                {
                    Messages.Message(this.parent.Label + " has expired.".CapitalizeFirst(), this.parent, MessageTypeDefOf.NeutralEvent, false);
                }
                else
                {
                    Messages.Message(this.Props.expiredMessage, this.parent, MessageTypeDefOf.NeutralEvent, false);
                }
            }
        }

        private void ResetTicksRemaining()
        {
            this.ticksRemaining = this.Props.lifetimeRange.RandomInRange;
            this.tickInterval = this.ticksRemaining;
        }

        public override string CompInspectStringExtra()
        {
            string str = null;
            if (this.Props.writeTimeLeft)
            {
                if(this.ticksRemaining > 0)
                {
                    str = this.Props.endVerb + " in " + this.ticksRemaining.ToStringTicksToPeriod();
                }
                else
                {
                    str = "Expired";
                }
            }
            return str;
        }
    }
}
