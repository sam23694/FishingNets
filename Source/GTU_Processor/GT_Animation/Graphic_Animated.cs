using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace GT_Animation
{
    public class Graphic_Animated : Graphic_Collection
    {
        private int currentFrame = 0;

        private int ticksPerFrame = 15;

        private int ticksPrev = 0;

        private bool randomized = false;

        private bool initFromComps = false;

        public override Material MatSingle
        {
            get
            {
                return this.subGraphics[0].MatSingle;
            }
        }

        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            if(newColorTwo != Color.white)
            {
                Log.Error("Cannot use Graphic_Animated.GetColoredVersion with a non-white colorTwo.", false);
            }
            return GraphicDatabase.Get<Graphic_Animated>(this.path, newShader, this.drawSize, newColor, Color.white, this.data);
        }

        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            if(thing == null)
            {
                return this.MatSingle;
            }
            return this.MatSingleFor(thing);
        }

        public override Material MatSingleFor(Thing thing)
        {
            if(thing == null)
            {
                return this.MatSingle;
            }
            return this.SubGraphicFor(thing).MatSingle;
        }

        public Graphic SubGraphicFor(Thing thing)
        {
            if(thing == null)
            {
                return this.subGraphics[0];
            }
            return this.subGraphics[thing.thingIDNumber % this.subGraphics.Length];
        }

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            if (thingDef == null)
            {
                Log.Error("Graphic_Animated with null thingDef", false);
                return;
            }
            if (this.subGraphics == null)
            {
                Log.Error("Graphic_Animated has no subgraphics", false);
                return;
            }
            Graphic graphic;
            if (thing == null)
            {
                graphic = this.SubGraphicFor(thing);
            }
            else
            {
                if (thingDef.HasComp(typeof(CompGTAnimation)) && !this.initFromComps)
                {
                    CompGTAnimation comp = thing.TryGetComp<CompGTAnimation>();
                    this.ticksPerFrame = comp.Props.frameSpeed;
                    this.randomized = comp.Props.randomized;
                    this.initFromComps = true;
                }
                int ticksCurrent = Find.TickManager.TicksGame;
                if (ticksCurrent >= this.ticksPrev + this.ticksPerFrame)
                {
                    this.ticksPrev = ticksCurrent;
                    if (this.randomized)
                    {
                        this.currentFrame = Rand.Range(0, this.subGraphics.Length - 1);
                    }
                    else
                    {
                        this.currentFrame++;
                    }
                }
                if (this.currentFrame >= this.subGraphics.Length)
                {
                    this.currentFrame = 0;
                }
                graphic = this.subGraphics[this.currentFrame];
            }
            graphic.DrawWorker(loc, rot, thingDef, thing, extraRotation);
        }

        public override string ToString()
        {
            return string.Concat(new object[]
            {
                "Animated(subgraphic[0]=",
                this.subGraphics[0].ToString(),
                ", count=",
                this.subGraphics.Length,
                ")"
            });
        }
    }
}
