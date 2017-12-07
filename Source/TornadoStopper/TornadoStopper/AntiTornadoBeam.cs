using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using System.Reflection;
using Verse.Sound;

namespace TornadoStopper
{
    public class AntiTornadoBeam : OrbitalStrike
    {
        public const int strikeDuration = 60;
        public override void StartStrike()
        {
            base.StartStrike();
            Mote mote = (Mote)ThingMaker.MakeThing(TornadoStopperLocalDefOf.Mote_AntiTornadoBeam, null);
            mote.exactPosition = Position.ToVector3Shifted();
            mote.Scale = 90f;
            mote.rotationRate = 1.2f;
            GenSpawn.Spawn(mote, Position, Map);
        }
    }
}
