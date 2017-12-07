using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TornadoStopper
{
    [StaticConstructorOnStartup]
    public class Building_WeatherDisruptor : Building
    {
        public static readonly Texture2D ActivateButtonTex = ContentFinder<Texture2D>.Get("Things/WeatherDisruptor");
        public static readonly FieldInfo TicksLeftToDisappearField = typeof(Tornado).GetField("ticksLeftToDisappear", BindingFlags.NonPublic | BindingFlags.Instance);
        int rareTicksUntilActivate = 0;
        public bool PowerOn
        {
            get
            {
                return GetComp<CompPowerTrader>()?.PowerOn ?? true;
            }
        }
        public bool HasPowerComp => GetComp<CompPowerTrader>() != null;
        public override void PostMake()
        {
            base.PostMake();
            rareTicksUntilActivate = GenDate.TicksPerQuadrum / GenTicks.TickRareInterval;
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos()) yield return g;
            yield return new Command_Action()
            {
                defaultLabel = "ActivateWeatherDisruptor".Translate(),
                defaultDesc = "ActivateWeatherDisruptorDesc".Translate(),
                icon = ActivateButtonTex,
                action = Activate
            };
            if (Prefs.DevMode)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "DEBUG: Instant recharge",
                    action = () => rareTicksUntilActivate = 0
                };
            }
        }
        public override void TickRare()
        {
            base.TickRare();
            CompPowerTrader powerComp = GetComp<CompPowerTrader>();
            if (rareTicksUntilActivate > 0 && (powerComp == null || powerComp.PowerOn))
            {
                rareTicksUntilActivate--;
                if (powerComp != null)
                {
                    if (rareTicksUntilActivate == 0)
                    {
                        powerComp.PowerOutput = powerComp.Props.basePowerConsumption * 0.01f;
                    }
                }
            }
        }
        public void Activate()
        {
            if (rareTicksUntilActivate > 0)
            {
                Messages.Message("WeatherDisruptor_StilCharging".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }
            IEnumerable<Tornado> tornadoSources = Map.listerThings.ThingsInGroup(ThingRequestGroup.WindSource).OfType<Tornado>();
            if (!tornadoSources.Any())
            {
                Messages.Message("WeatherDisruptor_NoTornadosOnMap".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }
            Tornado selected = tornadoSources.First();
            CreateBeam(selected.Position);
            TicksLeftToDisappearField.SetValue(selected, 1);
            rareTicksUntilActivate = GenDate.TicksPerQuadrum / GenTicks.TickRareInterval;
        }
        void CreateBeam(IntVec3 cell)
        {
            AntiTornadoBeam antiTornadoBeam = (AntiTornadoBeam)GenSpawn.Spawn(TornadoStopperLocalDefOf.AntiTornadoBeam, cell, Map);
            antiTornadoBeam.duration = AntiTornadoBeam.strikeDuration;
            antiTornadoBeam.StartStrike();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref rareTicksUntilActivate, "rareTicksUntilActivate");
        }
        public override string GetInspectString()
        {
            var baseInspectString = base.GetInspectString();
            StringBuilder stringBuilder = new StringBuilder();
            if (!baseInspectString.NullOrEmpty())
            {
                stringBuilder.AppendLine(baseInspectString);
            }
            if (PowerOn)
            {
                if (rareTicksUntilActivate == 0)
                {
                    stringBuilder.Append("WeatherDisruptorFullyCharged".Translate());
                }
                else
                {
                    stringBuilder.Append("WeatherDisruptorRechargesIn".Translate((rareTicksUntilActivate * GenTicks.TickRareInterval).ToStringTicksToPeriod()));
                }
            }
            else
            {
                stringBuilder.Append("WeatherDisruptorNoPower".Translate());
            }
            return stringBuilder.ToString();
        }
    }
}
