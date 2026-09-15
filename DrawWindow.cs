using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace UtilityUsage
{
    public class MainTabWindow_Power : MainTabWindow
    {
        public MainTabWindow_Power()
        {
            // Window settings
            this.forcePause = false;
            this.draggable = true;
            this.doCloseX = true;
            this.preventCameraMotion = false;
        }

        private int lastScreenHeight;

        public override Vector2 InitialSize
        {
            get
            {
                lastScreenHeight = Screen.height;
                return new Vector2(400f, GetWindowHeight());
            }
        }

        public override void WindowUpdate()
        {
            base.WindowUpdate();

            if (Screen.height != lastScreenHeight)
            {
                lastScreenHeight = Screen.height;
                windowRect.height = GetWindowHeight();
            }
        }

        private float GetWindowHeight()
        {
            if (Screen.height <= 800)
                return 700f;

            if (Screen.height <= 960)
                return 800f;

            return 900f;
        }

        private Dictionary<string, bool> expandedEntries = new Dictionary<string, bool>();
        private float previousTotalY;
        private Vector2 scrollPosition;

        private static string FormatPower(float power)
        {
            return $"{Mathf.Round(power / 10f) * 10f:F0} W";
        }

        private static string FormatConsolidatedEntry(string label, int count)
        {
            return $"{label} x{count}";
        }

        public override void DoWindowContents(Rect inRect)
        {
            float curY = inRect.y;

            PowerList production;
            PowerList consumption;
            PowerList offline;
            PowerList battery;

            PowerManager.GetCurrentState(out production, out consumption, out offline, out battery);

            Verse.Text.Font = GameFont.Medium;
            GUI.color = Color.white;
            Widgets.DrawBox(new Rect(inRect.x, curY, inRect.width, 115f), thickness: 1);
            Widgets.Label(new Rect(inRect.x + 10f, curY, inRect.width, 30f), "Power Grid");
            curY += 40f;

            // TOTALS
            Verse.Text.Font = GameFont.Small;
            Widgets.Label(new Rect(inRect.x + 10f, curY, inRect.width, 24f), $"Total Power Production: {FormatPower(production.Total)}");
            curY += 25f;

            Widgets.Label(new Rect(inRect.x + 10f, curY, inRect.width, 24f), $"Total Power Consumption: {FormatPower(consumption.Total)}");
            curY += 25f;

            float netUsage = production.Total + consumption.Total;

            GUI.color = netUsage >= 0f ? Color.green : Color.red;
            Widgets.Label(new Rect(inRect.x + 10f, curY, inRect.width, 24f), $"Net Power Usage: {FormatPower(netUsage)}");
            GUI.color = Color.white;
            curY += 30f;

            // Beginning the scrollable area
            Rect contentRect = new Rect(inRect.x, curY, inRect.width, inRect.height - curY);

            curY = 0f;
            float contentWidth = inRect.width - 20f;

            Rect viewRect = new Rect(0f, 0f, inRect.width -20f, previousTotalY);

            Widgets.BeginScrollView(contentRect, ref scrollPosition, viewRect);



            // STORED POWER
            Verse.Text.Font = GameFont.Medium;
            GUI.color = Color.white;
            Widgets.Label(new Rect(0f, curY, inRect.width, 30f), "Stored Power");
            curY += 40f;

            Verse.Text.Font = GameFont.Small;
            foreach (ConsolidatedEntry consolidatedEntry in battery.ConsolidatedEntries)
            {
                // Add key to our Dictionary if it doesn't exist.
                if (!expandedEntries.ContainsKey(consolidatedEntry.ExpansionKey))
                {
                    expandedEntries.Add(consolidatedEntry.ExpansionKey, false);
                }

                // Rect for the bounding box and for the text itself.
                Rect rowRect = new Rect(0f, curY, contentWidth, 24f);

                if (ButtonTextSubtleRow(rowRect, FormatConsolidatedEntry(consolidatedEntry.Label, consolidatedEntry.Count), FormatPower(consolidatedEntry.Power) + "d"))
                {
                    // On click simply flip it's current state.
                    expandedEntries[consolidatedEntry.ExpansionKey] = !expandedEntries[consolidatedEntry.ExpansionKey];
                }

                // If player has expanded the drop down.
                if (expandedEntries[consolidatedEntry.ExpansionKey])
                {

                    foreach (PowerEntry entry in battery.Entries)
                    {
                        Thing? thing = entry.Component?.parent ?? entry.BatteryComponent?.parent;

                        // Draw the actual drop down boxes.
                        if (thing != null && "batteries_" + thing.def.defName == consolidatedEntry.ExpansionKey)
                        {
                            curY += 25f;

                            Rect dropDownRect = new Rect(0f, curY, contentWidth, 24f);

                            if (ButtonTextSubtleDropDown(dropDownRect, $"{thing.LabelCap}", FormatPower(entry.Power) + "d"))
                            {
                                CameraJumper.TryJumpAndSelect(thing);
                            }
                        }
                    }
                }
                curY += 25f;
            }
            curY += 20f;



            // POWER PRODUCTION
            Verse.Text.Font = GameFont.Medium;
            GUI.color = Color.white;
            Widgets.Label(new Rect(0f, curY, inRect.width, 30f), "Power Production");
            curY += 40f;

            Verse.Text.Font = GameFont.Small;
            foreach (ConsolidatedEntry consolidatedEntry in production.ConsolidatedEntries)
            {

                // Add key to our Dictionary if it doesn't exist.
                if (!expandedEntries.ContainsKey(consolidatedEntry.ExpansionKey))
                {
                    expandedEntries.Add(consolidatedEntry.ExpansionKey, false);
                }

                // Rect for the bounding box and for the text itself.
                Rect rowRect = new Rect(0f, curY, contentWidth, 24f);

                if (ButtonTextSubtleRow(rowRect, FormatConsolidatedEntry(consolidatedEntry.Label, consolidatedEntry.Count), FormatPower(consolidatedEntry.Power)))
                {
                    // On click simply flip it's current state.
                    expandedEntries[consolidatedEntry.ExpansionKey] = !expandedEntries[consolidatedEntry.ExpansionKey];
                }

                // If player has expanded the drop down.
                if (expandedEntries[consolidatedEntry.ExpansionKey])
                {

                    foreach (PowerEntry entry in production.Entries)
                    {
                        Thing? thing = entry.Component?.parent ?? entry.BatteryComponent?.parent;

                        // Draw the actual drop down boxes.
                        if (thing != null && "production_" + thing.def.defName == consolidatedEntry.ExpansionKey)
                        {
                            curY += 25f;

                            Rect dropDownRect = new Rect(0f, curY, contentWidth, 24f);

                            if (ButtonTextSubtleDropDown(dropDownRect, $"{thing.LabelCap}", FormatPower(entry.Power)))
                            {
                                CameraJumper.TryJumpAndSelect(thing);
                            }
                        }
                    }
                }
                curY += 25f;
            }
            curY += 20f;



            // POWER CONSUMPTION
            Verse.Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, curY, inRect.width, 30f), "Power Consumption");
            curY += 40f;

            Verse.Text.Font = GameFont.Small;
            foreach (ConsolidatedEntry consolidatedEntry in consumption.ConsolidatedEntries)
            {

                // Add key to our Dictionary if it doesn't exist.
                if (!expandedEntries.ContainsKey(consolidatedEntry.ExpansionKey))
                {
                    expandedEntries.Add(consolidatedEntry.ExpansionKey, false);
                }

                // Rect for the bounding box and for the text itself.
                Rect rowRect = new Rect(0f, curY, contentWidth, 24f);

                if (ButtonTextSubtleRow(rowRect, FormatConsolidatedEntry(consolidatedEntry.Label, consolidatedEntry.Count), FormatPower(consolidatedEntry.Power)))
                {
                    // On click simply flip it's current state.
                    expandedEntries[consolidatedEntry.ExpansionKey] = !expandedEntries[consolidatedEntry.ExpansionKey];
                }

                // If player has expanded the drop down.
                if (expandedEntries[consolidatedEntry.ExpansionKey])
                {

                    foreach (PowerEntry entry in consumption.Entries)
                    {
                        Thing? thing = entry.Component?.parent ?? entry.BatteryComponent?.parent;

                        // Draw the actual drop down boxes.
                        if (thing != null && "consumption_" + thing.def.defName == consolidatedEntry.ExpansionKey)
                        {
                            curY += 25f;

                            Rect dropDownRect = new Rect(0f, curY, contentWidth, 24f);

                            if (ButtonTextSubtleDropDown(dropDownRect, $"{thing.LabelCap}", FormatPower(entry.Power)))
                            {
                                CameraJumper.TryJumpAndSelect(thing);
                            }
                        }
                    }
                }
                curY += 25f;
            }
            curY += 20f;



            // OFFLINE
            Verse.Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, curY, inRect.width, 30f), "OFFLINE DEVICES");
            curY += 40f;

            Verse.Text.Font = GameFont.Small;
            foreach (ConsolidatedEntry consolidatedEntry in offline.ConsolidatedEntries)
            {

                // Add key to our Dictionary if it doesn't exist.
                if (!expandedEntries.ContainsKey(consolidatedEntry.ExpansionKey))
                {
                    expandedEntries.Add(consolidatedEntry.ExpansionKey, false);
                }

                // Rect for the bounding box and for the text itself.
                Rect rowRect = new Rect(0f, curY, contentWidth, 24f);

                if (ButtonTextSubtleRow(rowRect, FormatConsolidatedEntry(consolidatedEntry.Label, consolidatedEntry.Count), "0 W"))
                {
                    // On click simply flip it's current state.
                    expandedEntries[consolidatedEntry.ExpansionKey] = !expandedEntries[consolidatedEntry.ExpansionKey];
                }

                // If player has expanded the drop down.
                if (expandedEntries[consolidatedEntry.ExpansionKey])
                {

                    foreach (PowerEntry entry in offline.Entries)
                    {
                        Thing? thing = entry.Component?.parent ?? entry.BatteryComponent?.parent;

                        // Draw the actual drop down boxes.
                        if (thing != null && "offline_" + thing.def.defName == consolidatedEntry.ExpansionKey)
                        {
                            curY += 25f;

                            Rect dropDownRect = new Rect(0f, curY, contentWidth, 24f);

                            if (ButtonTextSubtleDropDown(dropDownRect, $"{thing.LabelCap}", "0 W"))
                            {
                                CameraJumper.TryJumpAndSelect(thing);
                            }
                        }
                    }
                }
                curY += 25f;
            }
            curY += 20f;

            previousTotalY = curY;

            Widgets.EndScrollView();
        }

        private static bool ButtonTextSubtleRow(Rect rect, string label, string powerText)
        {
            bool mouseOver = Mouse.IsOver(rect);
            bool mouseDown = mouseOver && Input.GetMouseButton(0);
            string arrowText = "⇓";

            // Arrow
            Rect arrowBox = new Rect(rect.x + rect.width - 21f, rect.y, 20f, rect.height);
            Rect arrowRect = new Rect(rect.x + rect.width - 25f, rect.y, 30f, rect.height);

            if (mouseOver) GUI.color = GenUI.MouseoverColor;

            if (mouseDown)
            {
                arrowRect.y += 2f;
                arrowBox.y += 2f;
            }

            Widgets.DrawAtlas(rect, Widgets.ButtonSubtleAtlas);

            GUI.color = Color.white;

            // Label
            Rect labelRect = new Rect(rect.x + 10f, rect.y, rect.width * 0.5f, rect.height);

            // Power
            Rect powerRect = new Rect(rect.x + rect.width - 110f, rect.y, 80f, rect.height);

            Text.Font = GameFont.Small;
            Text.WordWrap = false;

            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = Color.white;
            Widgets.Label(labelRect, label);

            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(powerRect, powerText);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.DrawBox(arrowBox, 2);
            Widgets.Label(arrowRect, arrowText);
            GUI.color = Color.white;

            Text.Anchor = TextAnchor.UpperLeft;
            Text.WordWrap = true;
            GUI.color = Color.white;

            return Widgets.ButtonInvisible(rect, true);
        }

        private static bool ButtonTextSubtleDropDown(Rect rect, string label, string powerText)
        {
            bool mouseOver = Mouse.IsOver(rect);
            bool mouseDown = mouseOver && Input.GetMouseButton(0);

            if (mouseOver) GUI.color = GenUI.SubtleMouseoverColor;

            Widgets.DrawAtlas(rect, Widgets.ButtonSubtleAtlas);

            GUI.color = Color.white;

            // Label
            Rect labelRect = new Rect(rect.x + 10f, rect.y, rect.width * 0.5f, rect.height);

            // Power
            Rect powerRect = new Rect(rect.x + rect.width - 110f, rect.y, 80f, rect.height);

            Text.Font = GameFont.Small;
            Text.WordWrap = false;

            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = Color.white;
            Widgets.Label(labelRect, label);

            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(powerRect, powerText);

            Text.Anchor = TextAnchor.UpperLeft;
            Text.WordWrap = true;
            GUI.color = Color.white;

            return Widgets.ButtonInvisible(rect, true);
        }
    }
}
