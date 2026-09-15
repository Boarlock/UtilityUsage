using RimWorld;
using System.Collections.Generic;
using Verse;

namespace UtilityUsage
{

    public static class PowerManager
    {

        public static void GetCurrentState(out PowerList production, out PowerList consumption, out PowerList offline, out PowerList batteries)
        {
            production = new PowerList();
            consumption = new PowerList();
            offline = new PowerList();
            batteries = new PowerList();

            if (Find.CurrentMap?.powerNetManager?.AllNetsListForReading != null)
            {
                foreach (PowerNet network in Find.CurrentMap.powerNetManager.AllNetsListForReading)
                {
                    if (network == null)
                        continue;

                    foreach (CompPowerTrader component in network.powerComps)
                    {
                        if (component == null)
                            continue;

                        float flow = component.PowerOutput;

                        if (flow > 0f)
                        {
                            production.Add(component, flow);
                        }
                        else if (flow < 0f)
                        {
                            consumption.Add(component, flow);
                        }
                        else // Flow is exactly 0.
                        {
                            offline.Add(component, 0f);
                        }
                    }

                    foreach (CompPowerBattery battery in network.batteryComps)
                    {
                        if (battery == null)
                            continue;

                        if (battery.StoredEnergy <= 0)
                        {
                            offline.Add(battery, 0f);
                        }
                        else
                        {
                            batteries.Add(battery, battery.StoredEnergy);
                        }
                    }
                }
                production.Consolidate("production");
                consumption.Consolidate("consumption");
                batteries.Consolidate("batteries");
                offline.Consolidate("offline");
            }
        }
    }

    // Class that holds a List of <PowerEntry> that track each component, and that components power.
    // List<ConsolidatedPowerEntry> holds entries used for the power summary screen.
    public class PowerList
    {
        public List<PowerEntry> Entries { get; } = new List<PowerEntry>();
        public List<ConsolidatedEntry> ConsolidatedEntries { get; } = new List<ConsolidatedEntry>();
        public float Total { get; private set; }

        public void Add(CompPowerTrader component, float power)
        {
            Entries.Add(new PowerEntry(component, null, power));
            Total += component.PowerOutput;
        }

        public void Add(CompPowerBattery batteryComponent, float power)
        {
            Entries.Add(new PowerEntry(null, batteryComponent, power));
        }

        public void Consolidate(string caller)
        {
            ConsolidatedEntries.Clear();

            foreach (PowerEntry entry in Entries)
            {

                if (entry.Thing == null)
                    continue;

                string label = entry.Thing.LabelCap;
                string defName = entry.Thing.def.defName;

                string expansionKey = $"{caller}_{defName}";

                ConsolidatedEntry? existingEntry = null;

                foreach (ConsolidatedEntry consolidated in ConsolidatedEntries)
                {
                    if (consolidated.ExpansionKey == expansionKey)
                    {
                        existingEntry = consolidated;
                        break;
                    }
                }

                if (existingEntry != null)
                {
                    existingEntry.Count += 1;
                    existingEntry.Power += entry.Power;
                }
                else
                {
                    ConsolidatedEntries.Add(new ConsolidatedEntry(label, expansionKey, 1, entry.Power));
                }
            }
        }
    }

    public class PowerEntry
    {
        public CompPowerTrader? Component { get; }
        public CompPowerBattery? BatteryComponent { get; }
        public float Power { get; }

        public PowerEntry(CompPowerTrader? component, CompPowerBattery? batteryComponent, float power)
        {
            Component = component;
            BatteryComponent = batteryComponent;
            Power = power;
        }

        public Thing? Thing
        {
            get
            {
                if (Component != null)
                    return Component.parent;

                if (BatteryComponent != null)
                    return BatteryComponent.parent;

                return null;
            }
        }
    }

    public class ConsolidatedEntry
    {
        public string Label { get; }
        public string ExpansionKey { get; }
        public int Count { get; set; }
        public float Power { get; set; }

        public ConsolidatedEntry(string label, string expansionKey, int count, float power)
        {
            Label = label;
            ExpansionKey = expansionKey;
            Count = count;
            Power = power;
        }
    }
}
