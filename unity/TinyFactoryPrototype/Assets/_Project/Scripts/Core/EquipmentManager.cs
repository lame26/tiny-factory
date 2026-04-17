using System;
using System.Collections.Generic;
using TinyFactory.Economy;
using TinyFactory.Workers;
using UnityEngine;

namespace TinyFactory.Core
{
    public sealed class EquipmentManager : MonoBehaviour
    {
        public enum EquipmentSlot
        {
            Head,
            Body,
            Tool
        }

        private enum EquipmentStatType
        {
            MoveSpeedPercent,
            SaleValuePercent,
            AssemblySpeedPercent
        }

        [Serializable]
        private sealed class OwnedEquipmentEntry
        {
            public string InstanceId;
            public string DefinitionId;
            public int Level = 1;
        }

        private sealed class OwnedEquipmentGroup
        {
            public string DefinitionId;
            public int Level;
            public string PrimaryInstanceId;
            public string EquippedInstanceId;
            public int Count;
        }

        private readonly struct EquipmentDefinition
        {
            public EquipmentDefinition(
                string id,
                string displayName,
                EquipmentSlot slot,
                string rarityLabel,
                EquipmentStatType primaryStatType,
                float primaryStatValue,
                int maxLevel,
                EquipmentStatType secondaryStatType = EquipmentStatType.MoveSpeedPercent,
                float secondaryStatValue = 0f)
            {
                Id = id;
                DisplayName = displayName;
                Slot = slot;
                RarityLabel = rarityLabel;
                PrimaryStatType = primaryStatType;
                PrimaryStatValue = primaryStatValue;
                MaxLevel = maxLevel;
                SecondaryStatType = secondaryStatType;
                SecondaryStatValue = Mathf.Max(0f, secondaryStatValue);
            }

            public string Id { get; }
            public string DisplayName { get; }
            public EquipmentSlot Slot { get; }
            public string RarityLabel { get; }
            public EquipmentStatType PrimaryStatType { get; }
            public float PrimaryStatValue { get; }
            public int MaxLevel { get; }
            public EquipmentStatType SecondaryStatType { get; }
            public float SecondaryStatValue { get; }
            public bool HasSecondaryStat => SecondaryStatValue > 0f;
        }

        private static readonly EquipmentDefinition[] EquipmentDefinitions =
        {
            new EquipmentDefinition("head_safety_goggles", "Safety Goggles", EquipmentSlot.Head, "Common", EquipmentStatType.SaleValuePercent, 6f, 5),
            new EquipmentDefinition("head_scan_visor", "Scan Visor", EquipmentSlot.Head, "Uncommon", EquipmentStatType.SaleValuePercent, 10f, 10),
            new EquipmentDefinition("head_quality_headset", "Quality Headset", EquipmentSlot.Head, "Rare", EquipmentStatType.SaleValuePercent, 14f, 15, EquipmentStatType.AssemblySpeedPercent, 6f),
            new EquipmentDefinition("head_overseer_halo", "Overseer Halo", EquipmentSlot.Head, "Epic", EquipmentStatType.SaleValuePercent, 22f, 20, EquipmentStatType.AssemblySpeedPercent, 10f),
            new EquipmentDefinition("body_work_apron", "Work Apron", EquipmentSlot.Body, "Common", EquipmentStatType.MoveSpeedPercent, 8f, 5),
            new EquipmentDefinition("body_lift_harness", "Lift Harness", EquipmentSlot.Body, "Uncommon", EquipmentStatType.MoveSpeedPercent, 12f, 10),
            new EquipmentDefinition("body_servo_frame", "Servo Frame", EquipmentSlot.Body, "Rare", EquipmentStatType.MoveSpeedPercent, 18f, 15, EquipmentStatType.SaleValuePercent, 5f),
            new EquipmentDefinition("body_reactor_frame", "Reactor Frame", EquipmentSlot.Body, "Epic", EquipmentStatType.MoveSpeedPercent, 28f, 20, EquipmentStatType.SaleValuePercent, 10f),
            new EquipmentDefinition("tool_hand_driver", "Hand Driver", EquipmentSlot.Tool, "Common", EquipmentStatType.AssemblySpeedPercent, 10f, 5),
            new EquipmentDefinition("tool_smart_driver", "Smart Driver", EquipmentSlot.Tool, "Uncommon", EquipmentStatType.AssemblySpeedPercent, 16f, 10),
            new EquipmentDefinition("tool_precision_rig", "Precision Rig", EquipmentSlot.Tool, "Rare", EquipmentStatType.AssemblySpeedPercent, 22f, 15, EquipmentStatType.MoveSpeedPercent, 6f),
            new EquipmentDefinition("tool_quantum_driver", "Quantum Driver", EquipmentSlot.Tool, "Epic", EquipmentStatType.AssemblySpeedPercent, 34f, 20, EquipmentStatType.MoveSpeedPercent, 10f)
        };

        private static readonly string[] BasicBoxSequence =
        {
            "body_work_apron",
            "body_work_apron",
            "body_work_apron",
            "tool_hand_driver",
            "tool_hand_driver",
            "tool_hand_driver",
            "head_safety_goggles",
            "head_safety_goggles",
            "head_safety_goggles",
            "body_lift_harness",
            "body_lift_harness",
            "body_lift_harness",
            "tool_smart_driver",
            "tool_smart_driver",
            "tool_smart_driver",
            "head_scan_visor",
            "head_scan_visor",
            "head_scan_visor"
        };

        private static readonly string[] AdvancedBoxSequence =
        {
            "body_lift_harness",
            "body_lift_harness",
            "body_lift_harness",
            "tool_smart_driver",
            "tool_smart_driver",
            "tool_smart_driver",
            "head_scan_visor",
            "head_scan_visor",
            "head_scan_visor",
            "body_servo_frame",
            "body_servo_frame",
            "body_servo_frame",
            "tool_precision_rig",
            "tool_precision_rig",
            "tool_precision_rig",
            "head_quality_headset",
            "head_quality_headset",
            "head_quality_headset"
        };

        private static readonly string[] PremiumBoxSequence =
        {
            "body_servo_frame",
            "tool_precision_rig",
            "head_quality_headset",
            "body_servo_frame",
            "tool_precision_rig",
            "head_quality_headset",
            "body_servo_frame",
            "tool_precision_rig",
            "head_quality_headset"
        };

        private const float EpicSetMoveSpeedBonusPercent = 12f;
        private const float EpicSetSaleValueBonusPercent = 12f;
        private const float EpicSetAssemblySpeedBonusPercent = 12f;

        private static EquipmentManager s_runtimeInstance;

        [SerializeField] private StageGoalManager stageGoalManager;
        [SerializeField] private SupportBonusSlots supportBonusSlots;
        [SerializeField] private WorkerManager workerManager;
        [SerializeField] private string equippedHeadInstanceId;
        [SerializeField] private string equippedBodyInstanceId;
        [SerializeField] private string equippedToolInstanceId;
        [SerializeField] private int openedBasicBoxes;
        [SerializeField] private int openedAdvancedBoxes;
        [SerializeField] private int openedPremiumBoxes;
        [SerializeField] private int headBlueprintFragments;
        [SerializeField] private int bodyBlueprintFragments;
        [SerializeField] private int toolBlueprintFragments;
        [SerializeField] private int nextEquipmentInstanceSerial = 1;
        [SerializeField] private string lastMessage = "Worker gear ready";

        [SerializeField] private List<OwnedEquipmentEntry> ownedEquipment = new List<OwnedEquipmentEntry>();

        public int BasicBoxCount => stageGoalManager != null ? stageGoalManager.BasicBoxCount : 0;
        public int AdvancedBoxCount => stageGoalManager != null ? stageGoalManager.AdvancedBoxCount : 0;
        public int PremiumBoxCount => stageGoalManager != null ? stageGoalManager.PremiumBoxCount : 0;
        public int OwnedEquipmentCount => ownedEquipment.Count;
        public int OwnedEquipmentGroupCount => BuildOwnedEquipmentGroups().Count;
        public int CombineReadyGroupCount => GetCombineReadyGroupCount();
        public string LastMessage => string.IsNullOrWhiteSpace(lastMessage) ? "Worker gear ready" : lastMessage;
        public int OpenedBasicBoxCount => Mathf.Max(0, openedBasicBoxes);
        public int OpenedAdvancedBoxCount => Mathf.Max(0, openedAdvancedBoxes);
        public int OpenedPremiumBoxCount => Mathf.Max(0, openedPremiumBoxes);
        public int BlueprintReadySlotCount => GetBlueprintReadySlotCount();
        public int CraftedEpicSlotCount => GetCraftedEpicSlotCount();

        public static EquipmentManager GetOrCreate()
        {
            EquipmentManager existing = FindFirstObjectByType<EquipmentManager>();
            if (existing != null)
            {
                return existing;
            }

            if (s_runtimeInstance != null)
            {
                return s_runtimeInstance;
            }

            GameObject runtimeObject = new GameObject("EquipmentManager");
            s_runtimeInstance = runtimeObject.AddComponent<EquipmentManager>();
            return s_runtimeInstance;
        }

        private void Awake()
        {
            if (s_runtimeInstance == null)
            {
                s_runtimeInstance = this;
            }

            ResolveReferences();
            EnsureEquipmentState();
            ReapplyEquippedBonuses();
        }

        private void OnDestroy()
        {
            if (s_runtimeInstance == this)
            {
                s_runtimeInstance = null;
            }
        }

        public bool TryOpenBasicBox()
        {
            ResolveReferences();
            if (stageGoalManager == null || !stageGoalManager.TryConsumeBasicBox())
            {
                lastMessage = "No Basic Box available.";
                return false;
            }

            string definitionId = BasicBoxSequence[openedBasicBoxes % BasicBoxSequence.Length];
            openedBasicBoxes++;
            ownedEquipment.Add(new OwnedEquipmentEntry
            {
                InstanceId = CreateInstanceId(),
                DefinitionId = definitionId,
                Level = 1
            });

            EquipmentDefinition definition = GetDefinition(definitionId);
            lastMessage = "Opened Basic Box: " + definition.DisplayName + ".";
            return true;
        }

        public bool TryOpenAdvancedBox()
        {
            ResolveReferences();
            if (stageGoalManager == null || !stageGoalManager.TryConsumeAdvancedBox())
            {
                lastMessage = "No Advanced Box available.";
                return false;
            }

            string definitionId = AdvancedBoxSequence[openedAdvancedBoxes % AdvancedBoxSequence.Length];
            openedAdvancedBoxes++;
            ownedEquipment.Add(new OwnedEquipmentEntry
            {
                InstanceId = CreateInstanceId(),
                DefinitionId = definitionId,
                Level = 1
            });

            EquipmentDefinition definition = GetDefinition(definitionId);
            lastMessage = "Opened Advanced Box: " + definition.DisplayName + ".";
            return true;
        }

        public bool TryOpenPremiumBox()
        {
            ResolveReferences();
            if (stageGoalManager == null || !stageGoalManager.TryConsumePremiumBox())
            {
                lastMessage = "No Premium Box available.";
                return false;
            }

            string definitionId = PremiumBoxSequence[openedPremiumBoxes % PremiumBoxSequence.Length];
            openedPremiumBoxes++;
            ownedEquipment.Add(new OwnedEquipmentEntry
            {
                InstanceId = CreateInstanceId(),
                DefinitionId = definitionId,
                Level = 1
            });

            EquipmentDefinition definition = GetDefinition(definitionId);
            AddBlueprintFragment(definition.Slot, 1);
            lastMessage = "Opened Premium Box: "
                + definition.DisplayName
                + " / "
                + GetBlueprintSummary(definition.Slot)
                + ".";
            return true;
        }

        public bool TryEquipOwnedEquipmentGroup(int index)
        {
            if (!TryGetOwnedEquipmentGroup(index, out OwnedEquipmentGroup group))
            {
                lastMessage = "Equipment slot is empty.";
                return false;
            }

            EquipmentDefinition definition = GetDefinition(group.DefinitionId);
            string targetInstanceId = string.IsNullOrWhiteSpace(group.EquippedInstanceId) ? group.PrimaryInstanceId : group.EquippedInstanceId;
            switch (definition.Slot)
            {
                case EquipmentSlot.Head:
                    equippedHeadInstanceId = targetInstanceId;
                    break;
                case EquipmentSlot.Body:
                    equippedBodyInstanceId = targetInstanceId;
                    break;
                case EquipmentSlot.Tool:
                    equippedToolInstanceId = targetInstanceId;
                    break;
            }

            ReapplyEquippedBonuses();
            lastMessage = "Equipped " + definition.DisplayName + " Lv " + group.Level + " on " + definition.Slot + ".";
            return true;
        }

        public bool TryCombineOwnedEquipmentGroup(int index)
        {
            if (!TryGetOwnedEquipmentGroup(index, out OwnedEquipmentGroup group))
            {
                lastMessage = "Nothing to combine.";
                return false;
            }

            EquipmentDefinition definition = GetDefinition(group.DefinitionId);
            if (group.Level >= definition.MaxLevel)
            {
                lastMessage = definition.DisplayName + " is already at max level.";
                return false;
            }

            if (group.Count < 3)
            {
                lastMessage = "Need 3 copies of " + definition.DisplayName + " Lv " + group.Level + ".";
                return false;
            }

            bool reEquipCombinedResult = false;
            int removedCount = 0;
            for (int i = ownedEquipment.Count - 1; i >= 0 && removedCount < 3; i--)
            {
                OwnedEquipmentEntry entry = ownedEquipment[i];
                if (entry == null
                    || !string.Equals(entry.DefinitionId, group.DefinitionId, StringComparison.Ordinal)
                    || Mathf.Max(1, entry.Level) != group.Level)
                {
                    continue;
                }

                if (IsEquipped(entry.InstanceId))
                {
                    reEquipCombinedResult = true;
                    ClearEquippedInstance(entry.InstanceId);
                }

                ownedEquipment.RemoveAt(i);
                removedCount++;
            }

            OwnedEquipmentEntry upgradedEntry = new OwnedEquipmentEntry
            {
                InstanceId = CreateInstanceId(),
                DefinitionId = group.DefinitionId,
                Level = group.Level + 1
            };
            ownedEquipment.Add(upgradedEntry);

            if (reEquipCombinedResult)
            {
                EquipInstance(definition.Slot, upgradedEntry.InstanceId);
            }

            ReapplyEquippedBonuses();
            lastMessage = "Combined " + definition.DisplayName + " Lv " + group.Level + " x3 -> Lv " + upgradedEntry.Level + ".";
            return true;
        }

        public string GetSlotSummary(EquipmentSlot slot)
        {
            if (!TryGetEquippedDefinition(slot, out EquipmentDefinition definition))
            {
                return slot + ": Empty";
            }

            int level = GetEquippedLevel(slot);
            return slot + ": " + definition.DisplayName + " [" + definition.RarityLabel + "] Lv " + level + " / " + FormatDefinitionEffect(definition, level);
        }

        public string GetEquippedDefinitionId(EquipmentSlot slot)
        {
            return TryGetOwnedEquipmentByInstance(GetEquippedInstanceId(slot), out OwnedEquipmentEntry entry)
                ? entry.DefinitionId
                : string.Empty;
        }

        public int GetEquippedSlotLevel(EquipmentSlot slot)
        {
            return GetEquippedLevel(slot);
        }

        public string GetEquippedBonusSummary()
        {
            GetEquippedBonusTotals(out float moveSpeedBonusPercent, out float saleValueBonusPercent, out float assemblySpeedBonusPercent);
            return FormatBonusSummary(moveSpeedBonusPercent, saleValueBonusPercent, assemblySpeedBonusPercent);
        }

        public string GetEpicSetSummary()
        {
            if (HasFullEpicSetEquipped())
            {
                return "Epic Set Active / Move +"
                    + EpicSetMoveSpeedBonusPercent.ToString("0.#")
                    + "% / Pickup +"
                    + EpicSetSaleValueBonusPercent.ToString("0.#")
                    + "% / Assembly +"
                    + EpicSetAssemblySpeedBonusPercent.ToString("0.#")
                    + "%";
            }

            return "Epic Set Inactive / Equip Head, Body, Tool Epic";
        }

        public string GetBasicBoxNextSummary()
        {
            return GetBoxNextSummary(BasicBoxSequence, openedBasicBoxes, "Basic");
        }

        public string GetAdvancedBoxNextSummary()
        {
            return GetBoxNextSummary(AdvancedBoxSequence, openedAdvancedBoxes, "Advanced");
        }

        public string GetPremiumBoxNextSummary()
        {
            return GetBoxNextSummary(PremiumBoxSequence, openedPremiumBoxes, "Premium");
        }

        public string GetBlueprintProgressSummary()
        {
            return "Epic Prep / "
                + GetBlueprintSummary(EquipmentSlot.Head)
                + " / "
                + GetBlueprintSummary(EquipmentSlot.Body)
                + " / "
                + GetBlueprintSummary(EquipmentSlot.Tool);
        }

        public string GetBlueprintReadySummary()
        {
            return "Blueprint Ready "
                + BlueprintReadySlotCount
                + "/3 / Crafted Epic "
                + CraftedEpicSlotCount
                + "/3";
        }

        public string GetEpicCraftSummary(EquipmentSlot slot)
        {
            EquipmentDefinition definition = GetEpicDefinition(slot);
            if (HasOwnedEpicForSlot(slot))
            {
                int epicLevel = GetHighestOwnedLevel(definition.Id);
                return slot + " Epic: " + definition.DisplayName + " [" + definition.RarityLabel + "] Lv " + epicLevel;
            }

            return slot
                + " Epic: "
                + definition.DisplayName
                + " / Need "
                + GetBlueprintFragmentCount(slot)
                + "/3 "
                + slot
                + " BP";
        }

        public bool CanCraftEpic(EquipmentSlot slot)
        {
            return GetBlueprintFragmentCount(slot) >= 3;
        }

        public bool TryCraftEpic(EquipmentSlot slot)
        {
            if (!CanCraftEpic(slot))
            {
                lastMessage = "Need 3 " + slot + " blueprint fragments.";
                return false;
            }

            EquipmentDefinition epicDefinition = GetEpicDefinition(slot);
            SpendBlueprintFragments(slot, 3);

            OwnedEquipmentEntry craftedEntry = new OwnedEquipmentEntry
            {
                InstanceId = CreateInstanceId(),
                DefinitionId = epicDefinition.Id,
                Level = 1
            };
            ownedEquipment.Add(craftedEntry);

            ReapplyEquippedBonuses();
            lastMessage = "Crafted " + epicDefinition.DisplayName + ".";
            return true;
        }

        public void DebugOpenAllAvailableBoxes(bool includePremiumBoxes)
        {
            while (BasicBoxCount > 0)
            {
                if (!TryOpenBasicBox())
                {
                    break;
                }
            }

            while (AdvancedBoxCount > 0)
            {
                if (!TryOpenAdvancedBox())
                {
                    break;
                }
            }

            if (!includePremiumBoxes)
            {
                return;
            }

            while (PremiumBoxCount > 0)
            {
                if (!TryOpenPremiumBox())
                {
                    break;
                }
            }
        }

        public void DebugCombineAllPossible()
        {
            bool combinedAny;
            int safety = 0;
            do
            {
                combinedAny = false;
                List<OwnedEquipmentGroup> groups = BuildOwnedEquipmentGroups();
                for (int i = 0; i < groups.Count; i++)
                {
                    int liveIndex = FindOwnedEquipmentGroupIndex(groups[i].DefinitionId, groups[i].Level);
                    if (liveIndex < 0 || !CanCombineOwnedEquipmentGroup(liveIndex))
                    {
                        continue;
                    }

                    if (TryCombineOwnedEquipmentGroup(liveIndex))
                    {
                        combinedAny = true;
                        break;
                    }
                }

                safety++;
            }
            while (combinedAny && safety < 200);
        }

        public void DebugCraftAllReadyEpics()
        {
            CraftReadyEpic(EquipmentSlot.Head);
            CraftReadyEpic(EquipmentSlot.Body);
            CraftReadyEpic(EquipmentSlot.Tool);
        }

        public void DebugEquipBestLoadout()
        {
            EquipBestForSlot(EquipmentSlot.Head);
            EquipBestForSlot(EquipmentSlot.Body);
            EquipBestForSlot(EquipmentSlot.Tool);
            ReapplyEquippedBonuses();
            lastMessage = "Equipped best available loadout.";
        }

        public string GetOwnedEquipmentGroupSummary(int index)
        {
            if (!TryGetOwnedEquipmentGroup(index, out OwnedEquipmentGroup group))
            {
                return "Empty";
            }

            EquipmentDefinition definition = GetDefinition(group.DefinitionId);
            string equippedLabel = !string.IsNullOrWhiteSpace(group.EquippedInstanceId) ? " / Equipped" : string.Empty;
            string combineProgressLabel = group.Level >= definition.MaxLevel
                ? " / Max"
                : " / " + Mathf.Min(group.Count, 3) + "/3";
            return definition.Slot
                + " / "
                + definition.DisplayName
                + " ["
                + definition.RarityLabel
                + "]"
                + " Lv "
                + group.Level
                + " x"
                + group.Count
                + " / "
                + FormatDefinitionEffect(definition, group.Level)
                + combineProgressLabel
                + equippedLabel;
        }

        public string GetOwnedEquipmentGroupPreview(int index)
        {
            if (!TryGetOwnedEquipmentGroup(index, out OwnedEquipmentGroup group))
            {
                return "Next: None";
            }

            EquipmentDefinition definition = GetDefinition(group.DefinitionId);
            if (group.Level >= definition.MaxLevel)
            {
                return "Next: Max level reached";
            }

            return "Next Lv " + (group.Level + 1) + " / " + FormatDefinitionEffect(definition, group.Level + 1);
        }

        public bool CanEquipOwnedEquipmentGroup(int index)
        {
            return TryGetOwnedEquipmentGroup(index, out _);
        }

        public int FindOwnedEquipmentGroupIndex(string definitionId, int level = 1)
        {
            List<OwnedEquipmentGroup> groups = BuildOwnedEquipmentGroups();
            int sanitizedLevel = Mathf.Max(1, level);
            for (int i = 0; i < groups.Count; i++)
            {
                if (string.Equals(groups[i].DefinitionId, definitionId, StringComparison.Ordinal)
                    && groups[i].Level == sanitizedLevel)
                {
                    return i;
                }
            }

            return -1;
        }

        public int GetOwnedEquipmentGroupItemCount(string definitionId, int level = 1)
        {
            int groupIndex = FindOwnedEquipmentGroupIndex(definitionId, level);
            if (!TryGetOwnedEquipmentGroup(groupIndex, out OwnedEquipmentGroup group))
            {
                return 0;
            }

            return group.Count;
        }

        public bool CanCombineOwnedEquipmentGroup(int index)
        {
            if (!TryGetOwnedEquipmentGroup(index, out OwnedEquipmentGroup group))
            {
                return false;
            }

            EquipmentDefinition definition = GetDefinition(group.DefinitionId);
            return group.Count >= 3 && group.Level < definition.MaxLevel;
        }

        public string GetCombineButtonLabel(int index)
        {
            if (!TryGetOwnedEquipmentGroup(index, out OwnedEquipmentGroup group))
            {
                return "Combine";
            }

            EquipmentDefinition definition = GetDefinition(group.DefinitionId);
            if (group.Level >= definition.MaxLevel)
            {
                return "Max";
            }

            if (group.Count < 3)
            {
                return group.Count + "/3";
            }

            return "Lv " + (group.Level + 1);
        }

        private void ResolveReferences()
        {
            if (stageGoalManager == null)
            {
                stageGoalManager = StageGoalManager.GetOrCreate();
            }

            if (supportBonusSlots == null)
            {
                supportBonusSlots = FindFirstObjectByType<SupportBonusSlots>();
            }

            if (workerManager == null)
            {
                workerManager = FindFirstObjectByType<WorkerManager>();
            }
        }

        private void EnsureEquipmentState()
        {
            int maxSerial = 0;
            for (int i = 0; i < ownedEquipment.Count; i++)
            {
                OwnedEquipmentEntry entry = ownedEquipment[i];
                if (entry == null)
                {
                    continue;
                }

                entry.Level = Mathf.Max(1, entry.Level);
                if (TryParseInstanceSerial(entry.InstanceId, out int serial))
                {
                    maxSerial = Mathf.Max(maxSerial, serial);
                }
            }

            nextEquipmentInstanceSerial = Mathf.Max(Mathf.Max(nextEquipmentInstanceSerial, maxSerial + 1), 1);
        }

        private void ReapplyEquippedBonuses()
        {
            ResolveReferences();
            GetEquippedBonusTotals(out float moveSpeedBonusPercent, out float saleValueBonusPercent, out float assemblySpeedBonusPercent);

            if (supportBonusSlots != null)
            {
                supportBonusSlots.ApplyEquipmentBonuses(moveSpeedBonusPercent, saleValueBonusPercent, assemblySpeedBonusPercent);
            }

            if (workerManager != null)
            {
                workerManager.RefreshWorkerStats();
            }
        }

        private void AddDefinitionBonus(
            string instanceId,
            ref float moveSpeedBonusPercent,
            ref float saleValueBonusPercent,
            ref float assemblySpeedBonusPercent)
        {
            if (!TryGetOwnedEquipmentByInstance(instanceId, out OwnedEquipmentEntry entry))
            {
                return;
            }

            EquipmentDefinition definition = GetDefinition(entry.DefinitionId);
            AddScaledStatBonus(definition.PrimaryStatType, definition.PrimaryStatValue, entry.Level, ref moveSpeedBonusPercent, ref saleValueBonusPercent, ref assemblySpeedBonusPercent);
            if (definition.HasSecondaryStat)
            {
                AddScaledStatBonus(definition.SecondaryStatType, definition.SecondaryStatValue, entry.Level, ref moveSpeedBonusPercent, ref saleValueBonusPercent, ref assemblySpeedBonusPercent);
            }
        }

        private bool TryGetOwnedEquipment(int index, out OwnedEquipmentEntry entry)
        {
            if (index < 0 || index >= ownedEquipment.Count)
            {
                entry = null;
                return false;
            }

            entry = ownedEquipment[index];
            return entry != null;
        }

        private bool TryGetOwnedEquipmentByInstance(string instanceId, out OwnedEquipmentEntry entry)
        {
            entry = null;
            if (string.IsNullOrWhiteSpace(instanceId))
            {
                return false;
            }

            for (int i = 0; i < ownedEquipment.Count; i++)
            {
                if (ownedEquipment[i] != null && string.Equals(ownedEquipment[i].InstanceId, instanceId, StringComparison.Ordinal))
                {
                    entry = ownedEquipment[i];
                    return true;
                }
            }

            return false;
        }

        private bool TryGetOwnedEquipmentGroup(int index, out OwnedEquipmentGroup group)
        {
            List<OwnedEquipmentGroup> groups = BuildOwnedEquipmentGroups();
            if (index < 0 || index >= groups.Count)
            {
                group = null;
                return false;
            }

            group = groups[index];
            return group != null;
        }

        private bool TryGetEquippedDefinition(EquipmentSlot slot, out EquipmentDefinition definition)
        {
            definition = default;
            string instanceId = GetEquippedInstanceId(slot);
            if (!TryGetOwnedEquipmentByInstance(instanceId, out OwnedEquipmentEntry entry))
            {
                return false;
            }

            definition = GetDefinition(entry.DefinitionId);
            return true;
        }

        private int GetEquippedLevel(EquipmentSlot slot)
        {
            if (!TryGetOwnedEquipmentByInstance(GetEquippedInstanceId(slot), out OwnedEquipmentEntry entry))
            {
                return 1;
            }

            return Mathf.Max(1, entry.Level);
        }

        private string GetEquippedInstanceId(EquipmentSlot slot)
        {
            switch (slot)
            {
                case EquipmentSlot.Head:
                    return equippedHeadInstanceId;
                case EquipmentSlot.Body:
                    return equippedBodyInstanceId;
                default:
                    return equippedToolInstanceId;
            }
        }

        private bool IsEquipped(string instanceId)
        {
            return string.Equals(equippedHeadInstanceId, instanceId, StringComparison.Ordinal)
                || string.Equals(equippedBodyInstanceId, instanceId, StringComparison.Ordinal)
                || string.Equals(equippedToolInstanceId, instanceId, StringComparison.Ordinal);
        }

        private void ClearEquippedInstance(string instanceId)
        {
            if (string.Equals(equippedHeadInstanceId, instanceId, StringComparison.Ordinal))
            {
                equippedHeadInstanceId = string.Empty;
            }

            if (string.Equals(equippedBodyInstanceId, instanceId, StringComparison.Ordinal))
            {
                equippedBodyInstanceId = string.Empty;
            }

            if (string.Equals(equippedToolInstanceId, instanceId, StringComparison.Ordinal))
            {
                equippedToolInstanceId = string.Empty;
            }
        }

        private void EquipInstance(EquipmentSlot slot, string instanceId)
        {
            switch (slot)
            {
                case EquipmentSlot.Head:
                    equippedHeadInstanceId = instanceId;
                    break;
                case EquipmentSlot.Body:
                    equippedBodyInstanceId = instanceId;
                    break;
                default:
                    equippedToolInstanceId = instanceId;
                    break;
            }
        }

        private int GetCombineReadyGroupCount()
        {
            List<OwnedEquipmentGroup> groups = BuildOwnedEquipmentGroups();
            int readyGroupCount = 0;
            for (int i = 0; i < groups.Count; i++)
            {
                EquipmentDefinition definition = GetDefinition(groups[i].DefinitionId);
                if (groups[i].Count >= 3 && groups[i].Level < definition.MaxLevel)
                {
                    readyGroupCount++;
                }
            }

            return readyGroupCount;
        }

        private int GetBlueprintReadySlotCount()
        {
            int readyCount = 0;
            if (GetBlueprintFragmentCount(EquipmentSlot.Head) >= 3 || HasOwnedEpicForSlot(EquipmentSlot.Head))
            {
                readyCount++;
            }

            if (GetBlueprintFragmentCount(EquipmentSlot.Body) >= 3 || HasOwnedEpicForSlot(EquipmentSlot.Body))
            {
                readyCount++;
            }

            if (GetBlueprintFragmentCount(EquipmentSlot.Tool) >= 3 || HasOwnedEpicForSlot(EquipmentSlot.Tool))
            {
                readyCount++;
            }

            return readyCount;
        }

        private int GetCraftedEpicSlotCount()
        {
            int craftedCount = 0;
            if (HasOwnedEpicForSlot(EquipmentSlot.Head))
            {
                craftedCount++;
            }

            if (HasOwnedEpicForSlot(EquipmentSlot.Body))
            {
                craftedCount++;
            }

            if (HasOwnedEpicForSlot(EquipmentSlot.Tool))
            {
                craftedCount++;
            }

            return craftedCount;
        }

        private void EquipBestForSlot(EquipmentSlot slot)
        {
            List<OwnedEquipmentGroup> groups = BuildOwnedEquipmentGroups();
            int bestIndex = -1;
            OwnedEquipmentGroup bestGroup = null;

            for (int i = 0; i < groups.Count; i++)
            {
                EquipmentDefinition definition = GetDefinition(groups[i].DefinitionId);
                if (definition.Slot != slot)
                {
                    continue;
                }

                if (bestGroup == null || CompareOwnedEquipmentGroups(groups[i], bestGroup) < 0)
                {
                    bestGroup = groups[i];
                    bestIndex = FindOwnedEquipmentGroupIndex(groups[i].DefinitionId, groups[i].Level);
                }
            }

            if (bestIndex >= 0)
            {
                TryEquipOwnedEquipmentGroup(bestIndex);
            }
        }

        private void GetEquippedBonusTotals(
            out float moveSpeedBonusPercent,
            out float saleValueBonusPercent,
            out float assemblySpeedBonusPercent)
        {
            moveSpeedBonusPercent = 0f;
            saleValueBonusPercent = 0f;
            assemblySpeedBonusPercent = 0f;

            AddDefinitionBonus(equippedHeadInstanceId, ref moveSpeedBonusPercent, ref saleValueBonusPercent, ref assemblySpeedBonusPercent);
            AddDefinitionBonus(equippedBodyInstanceId, ref moveSpeedBonusPercent, ref saleValueBonusPercent, ref assemblySpeedBonusPercent);
            AddDefinitionBonus(equippedToolInstanceId, ref moveSpeedBonusPercent, ref saleValueBonusPercent, ref assemblySpeedBonusPercent);

            if (HasFullEpicSetEquipped())
            {
                moveSpeedBonusPercent += EpicSetMoveSpeedBonusPercent;
                saleValueBonusPercent += EpicSetSaleValueBonusPercent;
                assemblySpeedBonusPercent += EpicSetAssemblySpeedBonusPercent;
            }
        }

        private List<OwnedEquipmentGroup> BuildOwnedEquipmentGroups()
        {
            List<OwnedEquipmentGroup> groups = new List<OwnedEquipmentGroup>();
            Dictionary<string, OwnedEquipmentGroup> groupMap = new Dictionary<string, OwnedEquipmentGroup>();

            for (int i = 0; i < ownedEquipment.Count; i++)
            {
                OwnedEquipmentEntry entry = ownedEquipment[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.DefinitionId))
                {
                    continue;
                }

                int level = Mathf.Max(1, entry.Level);
                string key = entry.DefinitionId + "|" + level;
                if (!groupMap.TryGetValue(key, out OwnedEquipmentGroup group))
                {
                    group = new OwnedEquipmentGroup
                    {
                        DefinitionId = entry.DefinitionId,
                        Level = level,
                        PrimaryInstanceId = entry.InstanceId,
                        Count = 0
                    };
                    groupMap.Add(key, group);
                    groups.Add(group);
                }

                group.Count++;
                if (IsEquipped(entry.InstanceId))
                {
                    group.EquippedInstanceId = entry.InstanceId;
                }
            }

            groups.Sort(CompareOwnedEquipmentGroups);
            return groups;
        }

        private string CreateInstanceId()
        {
            string instanceId = "gear_" + nextEquipmentInstanceSerial.ToString("000");
            nextEquipmentInstanceSerial++;
            return instanceId;
        }

        private static bool TryParseInstanceSerial(string instanceId, out int serial)
        {
            serial = 0;
            if (string.IsNullOrWhiteSpace(instanceId) || instanceId.Length <= 5)
            {
                return false;
            }

            return int.TryParse(instanceId.Substring(5), out serial);
        }

        private static EquipmentDefinition GetDefinition(string definitionId)
        {
            for (int i = 0; i < EquipmentDefinitions.Length; i++)
            {
                if (string.Equals(EquipmentDefinitions[i].Id, definitionId, StringComparison.Ordinal))
                {
                    return EquipmentDefinitions[i];
                }
            }

            return EquipmentDefinitions[0];
        }

        private static void AddScaledStatBonus(
            EquipmentStatType statType,
            float baseStatValue,
            int level,
            ref float moveSpeedBonusPercent,
            ref float saleValueBonusPercent,
            ref float assemblySpeedBonusPercent)
        {
            float statValue = GetScaledStatValue(baseStatValue, level);
            switch (statType)
            {
                case EquipmentStatType.MoveSpeedPercent:
                    moveSpeedBonusPercent += statValue;
                    break;
                case EquipmentStatType.SaleValuePercent:
                    saleValueBonusPercent += statValue;
                    break;
                case EquipmentStatType.AssemblySpeedPercent:
                    assemblySpeedBonusPercent += statValue;
                    break;
            }
        }

        private static float GetScaledStatValue(float baseStatValue, int level)
        {
            int sanitizedLevel = Mathf.Max(1, level);
            return Mathf.Max(0f, baseStatValue) * (1f + (sanitizedLevel - 1) * 0.75f);
        }

        private static int CompareOwnedEquipmentGroups(OwnedEquipmentGroup left, OwnedEquipmentGroup right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            EquipmentDefinition leftDefinition = GetDefinition(left.DefinitionId);
            EquipmentDefinition rightDefinition = GetDefinition(right.DefinitionId);
            int slotCompare = GetSlotSortOrder(leftDefinition.Slot).CompareTo(GetSlotSortOrder(rightDefinition.Slot));
            if (slotCompare != 0)
            {
                return slotCompare;
            }

            bool leftEquipped = !string.IsNullOrWhiteSpace(left.EquippedInstanceId);
            bool rightEquipped = !string.IsNullOrWhiteSpace(right.EquippedInstanceId);
            if (leftEquipped != rightEquipped)
            {
                return leftEquipped ? -1 : 1;
            }

            int rarityCompare = GetRaritySortOrder(rightDefinition.RarityLabel).CompareTo(GetRaritySortOrder(leftDefinition.RarityLabel));
            if (rarityCompare != 0)
            {
                return rarityCompare;
            }

            int nameCompare = string.Compare(leftDefinition.DisplayName, rightDefinition.DisplayName, StringComparison.Ordinal);
            if (nameCompare != 0)
            {
                return nameCompare;
            }

            return left.Level.CompareTo(right.Level);
        }

        private static int GetSlotSortOrder(EquipmentSlot slot)
        {
            switch (slot)
            {
                case EquipmentSlot.Head:
                    return 0;
                case EquipmentSlot.Body:
                    return 1;
                default:
                    return 2;
            }
        }

        private static int GetRaritySortOrder(string rarityLabel)
        {
            switch (rarityLabel)
            {
                case "Epic":
                    return 3;
                case "Rare":
                    return 2;
                case "Uncommon":
                    return 1;
                case "Common":
                    return 0;
                default:
                    return -1;
            }
        }

        private static string FormatDefinitionEffect(EquipmentDefinition definition, int level)
        {
            string primaryLabel = FormatStatLabel(definition.PrimaryStatType, GetScaledStatValue(definition.PrimaryStatValue, level));
            if (!definition.HasSecondaryStat)
            {
                return primaryLabel;
            }

            string secondaryLabel = FormatStatLabel(definition.SecondaryStatType, GetScaledStatValue(definition.SecondaryStatValue, level));
            return primaryLabel + " / " + secondaryLabel;
        }

        private static string FormatStatLabel(EquipmentStatType statType, float statValue)
        {
            switch (statType)
            {
                case EquipmentStatType.MoveSpeedPercent:
                    return "Move +" + statValue.ToString("0.#") + "%";
                case EquipmentStatType.AssemblySpeedPercent:
                    return "Assembly +" + statValue.ToString("0.#") + "%";
                default:
                    return "Pickup +" + statValue.ToString("0.#") + "%";
            }
        }

        private static string FormatBonusSummary(float moveSpeedBonusPercent, float saleValueBonusPercent, float assemblySpeedBonusPercent)
        {
            return "Move +"
                + moveSpeedBonusPercent.ToString("0.#")
                + "% / Pickup +"
                + saleValueBonusPercent.ToString("0.#")
                + "% / Assembly +"
                + assemblySpeedBonusPercent.ToString("0.#")
                + "%";
        }

        private void AddBlueprintFragment(EquipmentSlot slot, int amount)
        {
            int sanitizedAmount = Mathf.Max(0, amount);
            switch (slot)
            {
                case EquipmentSlot.Head:
                    headBlueprintFragments += sanitizedAmount;
                    break;
                case EquipmentSlot.Body:
                    bodyBlueprintFragments += sanitizedAmount;
                    break;
                default:
                    toolBlueprintFragments += sanitizedAmount;
                    break;
            }
        }

        private void SpendBlueprintFragments(EquipmentSlot slot, int amount)
        {
            int sanitizedAmount = Mathf.Max(0, amount);
            switch (slot)
            {
                case EquipmentSlot.Head:
                    headBlueprintFragments = Mathf.Max(0, headBlueprintFragments - sanitizedAmount);
                    break;
                case EquipmentSlot.Body:
                    bodyBlueprintFragments = Mathf.Max(0, bodyBlueprintFragments - sanitizedAmount);
                    break;
                default:
                    toolBlueprintFragments = Mathf.Max(0, toolBlueprintFragments - sanitizedAmount);
                    break;
            }
        }

        private int GetBlueprintFragmentCount(EquipmentSlot slot)
        {
            switch (slot)
            {
                case EquipmentSlot.Head:
                    return Mathf.Max(0, headBlueprintFragments);
                case EquipmentSlot.Body:
                    return Mathf.Max(0, bodyBlueprintFragments);
                default:
                    return Mathf.Max(0, toolBlueprintFragments);
            }
        }

        private string GetBlueprintSummary(EquipmentSlot slot)
        {
            int count = GetBlueprintFragmentCount(slot);
            string slotLabel = slot + " BP";
            if (HasOwnedEpicForSlot(slot))
            {
                return slotLabel + " Crafted";
            }

            if (count >= 3)
            {
                return slotLabel + " Ready";
            }

            return slotLabel + " " + count + "/3";
        }

        private static string GetBoxNextSummary(string[] sequence, int openedCount, string boxLabel)
        {
            if (sequence == null || sequence.Length == 0)
            {
                return boxLabel + ": Empty";
            }

            int sequenceIndex = Mathf.Max(0, openedCount) % sequence.Length;
            EquipmentDefinition definition = GetDefinition(sequence[sequenceIndex]);
            int phaseLength = GetPhaseLength(sequence, sequenceIndex);
            int phaseIndex = GetPhaseProgressIndex(sequence, sequenceIndex);
            return boxLabel
                + " next: "
                + definition.RarityLabel
                + " "
                + definition.DisplayName
                + " ("
                + (phaseIndex + 1)
                + "/"
                + phaseLength
                + ")";
        }

        private static int GetPhaseLength(string[] sequence, int startIndex)
        {
            if (sequence == null || sequence.Length == 0)
            {
                return 0;
            }

            EquipmentDefinition definition = GetDefinition(sequence[startIndex]);
            int length = 0;
            for (int i = startIndex; i < sequence.Length; i++)
            {
                if (!string.Equals(GetDefinition(sequence[i]).RarityLabel, definition.RarityLabel, StringComparison.Ordinal))
                {
                    break;
                }

                length++;
            }

            if (length > 0)
            {
                return length;
            }

            for (int i = 0; i < sequence.Length; i++)
            {
                if (string.Equals(GetDefinition(sequence[i]).RarityLabel, definition.RarityLabel, StringComparison.Ordinal))
                {
                    length++;
                }
            }

            return Mathf.Max(1, length);
        }

        private static int GetPhaseProgressIndex(string[] sequence, int sequenceIndex)
        {
            if (sequence == null || sequence.Length == 0)
            {
                return 0;
            }

            EquipmentDefinition definition = GetDefinition(sequence[sequenceIndex]);
            int phaseStartIndex = sequenceIndex;
            while (phaseStartIndex > 0
                && string.Equals(GetDefinition(sequence[phaseStartIndex - 1]).RarityLabel, definition.RarityLabel, StringComparison.Ordinal))
            {
                phaseStartIndex--;
            }

            return Mathf.Max(0, sequenceIndex - phaseStartIndex);
        }

        private EquipmentDefinition GetEpicDefinition(EquipmentSlot slot)
        {
            switch (slot)
            {
                case EquipmentSlot.Head:
                    return GetDefinition("head_overseer_halo");
                case EquipmentSlot.Body:
                    return GetDefinition("body_reactor_frame");
                default:
                    return GetDefinition("tool_quantum_driver");
            }
        }

        private bool HasFullEpicSetEquipped()
        {
            return IsEpicEquipped(EquipmentSlot.Head)
                && IsEpicEquipped(EquipmentSlot.Body)
                && IsEpicEquipped(EquipmentSlot.Tool);
        }

        private bool IsEpicEquipped(EquipmentSlot slot)
        {
            if (!TryGetEquippedDefinition(slot, out EquipmentDefinition definition))
            {
                return false;
            }

            return string.Equals(definition.RarityLabel, "Epic", StringComparison.Ordinal);
        }

        private bool HasOwnedEpicForSlot(EquipmentSlot slot)
        {
            EquipmentDefinition epicDefinition = GetEpicDefinition(slot);
            return GetHighestOwnedLevel(epicDefinition.Id) > 0;
        }

        private int GetHighestOwnedLevel(string definitionId)
        {
            int highestLevel = 0;
            for (int i = 0; i < ownedEquipment.Count; i++)
            {
                OwnedEquipmentEntry entry = ownedEquipment[i];
                if (entry == null || !string.Equals(entry.DefinitionId, definitionId, StringComparison.Ordinal))
                {
                    continue;
                }

                highestLevel = Mathf.Max(highestLevel, Mathf.Max(1, entry.Level));
            }

            return highestLevel;
        }

        private void CraftReadyEpic(EquipmentSlot slot)
        {
            while (CanCraftEpic(slot))
            {
                if (!TryCraftEpic(slot))
                {
                    break;
                }
            }
        }
    }
}
