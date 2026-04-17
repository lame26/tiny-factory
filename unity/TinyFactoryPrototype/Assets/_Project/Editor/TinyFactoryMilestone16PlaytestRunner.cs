using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using TinyFactory.Core;
using TinyFactory.Economy;
using TinyFactory.Stations;
using TinyFactory.Workers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TinyFactory.Editor
{
    [InitializeOnLoad]
    public static class TinyFactoryMilestone16PlaytestRunner
    {
        private const string ActiveKey = "TinyFactory.M16.Active";
        private const string StatusKey = "TinyFactory.M16.Status";
        private const string HookedKey = "TinyFactory.M16.Hooked";
        private const string BaseSpeedKey = "TinyFactory.M16.BaseSpeed";
        private const string Lv1SpeedKey = "TinyFactory.M16.Lv1Speed";
        private const string Lv2SpeedKey = "TinyFactory.M16.Lv2Speed";
        private const string CombineTimeKey = "TinyFactory.M16.CombineTime";
        private const string StageClearCountKey = "TinyFactory.M16.StageClearCount";
        private const string LastLoggedStateKey = "TinyFactory.M16.LastLoggedState";
        private const string LastObservedCompletedOrdersKey = "TinyFactory.M16.LastObservedCompletedOrders";
        private const string BaseAssemblyKey = "TinyFactory.M16.BaseAssembly";
        private const string ToolLv1AssemblyKey = "TinyFactory.M16.ToolLv1Assembly";
        private const string ToolLv2AssemblyKey = "TinyFactory.M16.ToolLv2Assembly";
        private const string BaseRushPayoutKey = "TinyFactory.M16.BaseRushPayout";
        private const string HeadLv1RushPayoutKey = "TinyFactory.M16.HeadLv1RushPayout";
        private const string HeadLv2RushPayoutKey = "TinyFactory.M16.HeadLv2RushPayout";
        private const string HeadLv1ObservedPayoutKey = "TinyFactory.M16.HeadLv1ObservedPayout";
        private const string HeadLv2ObservedPayoutKey = "TinyFactory.M16.HeadLv2ObservedPayout";
        private const string HeadLv1ObservedLabelKey = "TinyFactory.M16.HeadLv1ObservedLabel";
        private const string HeadLv2ObservedLabelKey = "TinyFactory.M16.HeadLv2ObservedLabel";

        private const float SimulatedDurationSeconds = 660f;
        private const float TimeScale = 30f;
        private const float AssemblyToleranceSeconds = 0.08f;
        private const string TargetScenePath = "Assets/_Project/Scenes/Prototype_01_Workshop.unity";
        private const string BodyDefinitionId = "body_work_apron";
        private const string ToolDefinitionId = "tool_hand_driver";
        private const string HeadDefinitionId = "head_safety_goggles";
        private const string TriggerFileName = "tinyfactory_m16_playtest.trigger";
        private const string StatusFileName = "tinyfactory_m16_playtest.status";

        private static readonly Dictionary<int, int> BenchCompletionCache = new Dictionary<int, int>();

        static TinyFactoryMilestone16PlaytestRunner()
        {
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
            EditorApplication.update += CheckForTrigger;
            EditorApplication.delayCall += ResumeIfNeeded;
        }

        private static void CheckForTrigger()
        {
            if (SessionState.GetBool(ActiveKey, false))
            {
                return;
            }

            string triggerPath = GetTriggerPath();
            if (!File.Exists(triggerPath))
            {
                return;
            }

            File.Delete(triggerPath);
            StartPlaytest();
        }

        private static void StartPlaytest()
        {
            ResetMetrics();
            SessionState.SetBool(ActiveKey, true);
            SessionState.SetString(StatusKey, "state=starting");
            WriteStatus("starting");

            if (!EditorApplication.isPlaying)
            {
                EditorSceneManager.OpenScene(TargetScenePath);
            }

            if (EditorApplication.isPlaying)
            {
                HookUpdateIfNeeded();
            }
            else if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.isPlaying = true;
            }
        }

        private static void ResumeIfNeeded()
        {
            if (!SessionState.GetBool(ActiveKey, false))
            {
                return;
            }

            if (EditorApplication.isPlaying)
            {
                HookUpdateIfNeeded();
                WriteStatus("running");
                return;
            }

            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.isPlaying = true;
            }
        }

        private static void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(ActiveKey, false))
            {
                return;
            }

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                HookUpdateIfNeeded();
                WriteStatus("running");
                return;
            }

            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                Time.timeScale = 1f;
                SessionState.SetBool(HookedKey, false);
            }
        }

        private static void HookUpdateIfNeeded()
        {
            if (SessionState.GetBool(HookedKey, false))
            {
                return;
            }

            EditorApplication.update += UpdatePlaytest;
            SessionState.SetBool(HookedKey, true);
        }

        private static void UnhookUpdate()
        {
            EditorApplication.update -= UpdatePlaytest;
            SessionState.SetBool(HookedKey, false);
        }

        private static void UpdatePlaytest()
        {
            if (!SessionState.GetBool(ActiveKey, false))
            {
                UnhookUpdate();
                return;
            }

            if (!EditorApplication.isPlaying)
            {
                return;
            }

            MoneyManager moneyManager = Object.FindFirstObjectByType<MoneyManager>();
            WorkerManager workerManager = Object.FindFirstObjectByType<WorkerManager>();
            FacilityManager facilityManager = Object.FindFirstObjectByType<FacilityManager>();
            PickupCounter pickupCounter = Object.FindFirstObjectByType<PickupCounter>();
            UpgradeManager upgradeManager = Object.FindFirstObjectByType<UpgradeManager>();
            ProductProgressionManager productProgressionManager = ProductProgressionManager.GetOrCreate();
            StageGoalManager stageGoalManager = StageGoalManager.GetOrCreate();
            EquipmentManager equipmentManager = EquipmentManager.GetOrCreate();
            SupportBonusSlots supportBonusSlots = Object.FindFirstObjectByType<SupportBonusSlots>();

            if (moneyManager == null
                || workerManager == null
                || facilityManager == null
                || pickupCounter == null
                || upgradeManager == null
                || productProgressionManager == null
                || stageGoalManager == null
                || equipmentManager == null
                || supportBonusSlots == null)
            {
                Finish("failed_missing_manager");
                return;
            }

            Time.timeScale = TimeScale;

            TrackBaseSpeed();
            TrackStageClearProgress(stageGoalManager);
            ApplyPurchases(workerManager, facilityManager, upgradeManager, productProgressionManager);
            ApplyEquipmentFlow(equipmentManager);
            ObserveRuntimeMetrics(pickupCounter, stageGoalManager, equipmentManager, supportBonusSlots);

            if (HasCompletedVerification(stageGoalManager))
            {
                Finish("completed");
                return;
            }

            if (Time.timeSinceLevelLoad >= SimulatedDurationSeconds)
            {
                Finish(HasReachedLateVerificationState(stageGoalManager) ? "completed_timeout" : "timeout");
                return;
            }

            WriteStatus("running");
        }

        private static void TrackBaseSpeed()
        {
            if (SessionState.GetFloat(BaseSpeedKey, -1f) >= 0f)
            {
                return;
            }

            WorkerController firstWorker = GetFirstWorker();
            if (firstWorker == null)
            {
                return;
            }

            SessionState.SetFloat(BaseSpeedKey, firstWorker.MoveSpeed);
        }

        private static void TrackStageClearProgress(StageGoalManager stageGoalManager)
        {
            int clearCount = 0;
            if (stageGoalManager.HasCompletedAllStages)
            {
                clearCount = 3;
            }
            else
            {
                clearCount = Mathf.Clamp(stageGoalManager.CurrentStageNumber - 1, 0, 2);
            }

            SessionState.SetInt(StageClearCountKey, clearCount);
        }

        private static void ApplyPurchases(
            WorkerManager workerManager,
            FacilityManager facilityManager,
            UpgradeManager upgradeManager,
            ProductProgressionManager productProgressionManager)
        {
            if (workerManager.WorkerCount < 2)
            {
                workerManager.TryHireWorker();
                return;
            }

            if (productProgressionManager.ProductLevel < 2)
            {
                upgradeManager.TryLevelUpProduct();
                return;
            }

            if (productProgressionManager.ProductLevel < 3)
            {
                upgradeManager.TryLevelUpProduct();
                return;
            }

            if (facilityManager.CanBuildAssemblyBench)
            {
                facilityManager.TryBuildAssemblyBench();
                return;
            }

            if (facilityManager.CanBuildDispatchRack)
            {
                facilityManager.TryBuildDispatchRack();
                return;
            }

            if (workerManager.WorkerCount < 3)
            {
                workerManager.TryHireWorker();
            }
        }

        private static void ApplyEquipmentFlow(EquipmentManager equipmentManager)
        {
            while (equipmentManager.BasicBoxCount > 0)
            {
                equipmentManager.TryOpenBasicBox();
            }

            ApplyBodyFlow(equipmentManager);
            ApplyToolFlow(equipmentManager);
            ApplyHeadFlow(equipmentManager);
        }

        private static void ApplyBodyFlow(EquipmentManager equipmentManager)
        {
            int lv1GroupIndex = equipmentManager.FindOwnedEquipmentGroupIndex(BodyDefinitionId, 1);
            if (lv1GroupIndex >= 0 && SessionState.GetFloat(Lv1SpeedKey, -1f) < 0f)
            {
                equipmentManager.TryEquipOwnedEquipmentGroup(lv1GroupIndex);
                WorkerController firstWorker = GetFirstWorker();
                if (firstWorker != null)
                {
                    SessionState.SetFloat(Lv1SpeedKey, firstWorker.MoveSpeed);
                }
            }

            if (lv1GroupIndex >= 0 && equipmentManager.CanCombineOwnedEquipmentGroup(lv1GroupIndex))
            {
                if (equipmentManager.TryCombineOwnedEquipmentGroup(lv1GroupIndex))
                {
                    SessionState.SetFloat(CombineTimeKey, Time.timeSinceLevelLoad);
                }
            }

            int lv2GroupIndex = equipmentManager.FindOwnedEquipmentGroupIndex(BodyDefinitionId, 2);
            if (lv2GroupIndex >= 0 && SessionState.GetFloat(Lv2SpeedKey, -1f) < 0f)
            {
                equipmentManager.TryEquipOwnedEquipmentGroup(lv2GroupIndex);
                WorkerController firstWorker = GetFirstWorker();
                if (firstWorker != null)
                {
                    SessionState.SetFloat(Lv2SpeedKey, firstWorker.MoveSpeed);
                }
            }
        }

        private static void ApplyToolFlow(EquipmentManager equipmentManager)
        {
            int lv1GroupIndex = equipmentManager.FindOwnedEquipmentGroupIndex(ToolDefinitionId, 1);
            if (lv1GroupIndex >= 0 && equipmentManager.GetEquippedDefinitionId(EquipmentManager.EquipmentSlot.Tool) != ToolDefinitionId)
            {
                equipmentManager.TryEquipOwnedEquipmentGroup(lv1GroupIndex);
            }

            if (lv1GroupIndex >= 0 && equipmentManager.CanCombineOwnedEquipmentGroup(lv1GroupIndex))
            {
                equipmentManager.TryCombineOwnedEquipmentGroup(lv1GroupIndex);
            }

            int lv2GroupIndex = equipmentManager.FindOwnedEquipmentGroupIndex(ToolDefinitionId, 2);
            if (lv2GroupIndex >= 0 && equipmentManager.GetEquippedSlotLevel(EquipmentManager.EquipmentSlot.Tool) < 2)
            {
                equipmentManager.TryEquipOwnedEquipmentGroup(lv2GroupIndex);
            }
        }

        private static void ApplyHeadFlow(EquipmentManager equipmentManager)
        {
            int lv1GroupIndex = equipmentManager.FindOwnedEquipmentGroupIndex(HeadDefinitionId, 1);
            if (lv1GroupIndex >= 0 && equipmentManager.GetEquippedDefinitionId(EquipmentManager.EquipmentSlot.Head) != HeadDefinitionId)
            {
                equipmentManager.TryEquipOwnedEquipmentGroup(lv1GroupIndex);
            }

            if (lv1GroupIndex >= 0 && equipmentManager.CanCombineOwnedEquipmentGroup(lv1GroupIndex))
            {
                equipmentManager.TryCombineOwnedEquipmentGroup(lv1GroupIndex);
            }

            int lv2GroupIndex = equipmentManager.FindOwnedEquipmentGroupIndex(HeadDefinitionId, 2);
            if (lv2GroupIndex >= 0 && equipmentManager.GetEquippedSlotLevel(EquipmentManager.EquipmentSlot.Head) < 2)
            {
                equipmentManager.TryEquipOwnedEquipmentGroup(lv2GroupIndex);
            }
        }

        private static void ObserveRuntimeMetrics(
            PickupCounter pickupCounter,
            StageGoalManager stageGoalManager,
            EquipmentManager equipmentManager,
            SupportBonusSlots supportBonusSlots)
        {
            ObservePickupMetrics(pickupCounter, stageGoalManager, equipmentManager);
            ObserveAssemblyMetrics(equipmentManager, supportBonusSlots);
        }

        private static void ObservePickupMetrics(
            PickupCounter pickupCounter,
            StageGoalManager stageGoalManager,
            EquipmentManager equipmentManager)
        {
            int completedOrders = pickupCounter.CompletedOrderCount;
            int lastObservedCompletedOrders = SessionState.GetInt(LastObservedCompletedOrdersKey, 0);
            if (completedOrders <= lastObservedCompletedOrders)
            {
                return;
            }

            SessionState.SetInt(LastObservedCompletedOrdersKey, completedOrders);
            if (!string.Equals(pickupCounter.LastCompletedOrderLabel, "Rush", System.StringComparison.Ordinal))
            {
                return;
            }

            int payout = pickupCounter.LastShipmentPayout;
            if (payout <= 0)
            {
                return;
            }

            int headLevel = string.Equals(
                equipmentManager.GetEquippedDefinitionId(EquipmentManager.EquipmentSlot.Head),
                HeadDefinitionId,
                System.StringComparison.Ordinal)
                ? equipmentManager.GetEquippedSlotLevel(EquipmentManager.EquipmentSlot.Head)
                : 0;

            if (headLevel <= 0)
            {
                if (SessionState.GetFloat(BaseRushPayoutKey, -1f) < 0f
                    && stageGoalManager.HasCompletedAllStages)
                {
                    SessionState.SetFloat(BaseRushPayoutKey, payout);
                }

                return;
            }

            if (headLevel == 1 && SessionState.GetFloat(HeadLv1RushPayoutKey, -1f) < 0f)
            {
                if (SessionState.GetFloat(HeadLv1ObservedPayoutKey, -1f) < 0f)
                {
                    SessionState.SetFloat(HeadLv1ObservedPayoutKey, payout);
                    SessionState.SetString(HeadLv1ObservedLabelKey, pickupCounter.LastCompletedOrderLabel);
                }

                if (string.Equals(pickupCounter.LastCompletedOrderLabel, "Rush", System.StringComparison.Ordinal))
                {
                    SessionState.SetFloat(HeadLv1RushPayoutKey, payout);
                }

                return;
            }

            if (headLevel >= 2 && SessionState.GetFloat(HeadLv2RushPayoutKey, -1f) < 0f)
            {
                if (SessionState.GetFloat(HeadLv2ObservedPayoutKey, -1f) < 0f)
                {
                    SessionState.SetFloat(HeadLv2ObservedPayoutKey, payout);
                    SessionState.SetString(HeadLv2ObservedLabelKey, pickupCounter.LastCompletedOrderLabel);
                }

                if (string.Equals(pickupCounter.LastCompletedOrderLabel, "Rush", System.StringComparison.Ordinal))
                {
                    SessionState.SetFloat(HeadLv2RushPayoutKey, payout);
                }
            }
        }

        private static void ObserveAssemblyMetrics(EquipmentManager equipmentManager, SupportBonusSlots supportBonusSlots)
        {
            AssemblyBench[] benches = Object.FindObjectsByType<AssemblyBench>(FindObjectsSortMode.None);
            for (int i = 0; i < benches.Length; i++)
            {
                AssemblyBench bench = benches[i];
                if (bench == null)
                {
                    continue;
                }

                int benchId = bench.GetInstanceID();
                int completedCount = bench.CompletedAssemblyCount;
                BenchCompletionCache.TryGetValue(benchId, out int lastCompletedCount);
                if (completedCount <= lastCompletedCount)
                {
                    continue;
                }

                BenchCompletionCache[benchId] = completedCount;
                float duration = bench.LastCompletedAssemblySeconds;
                if (duration <= 0f)
                {
                    continue;
                }

                bool toolEquipped = string.Equals(
                    equipmentManager.GetEquippedDefinitionId(EquipmentManager.EquipmentSlot.Tool),
                    ToolDefinitionId,
                    System.StringComparison.Ordinal);
                int toolLevel = toolEquipped ? equipmentManager.GetEquippedSlotLevel(EquipmentManager.EquipmentSlot.Tool) : 0;

                if (toolLevel <= 0)
                {
                    if (SessionState.GetFloat(BaseAssemblyKey, -1f) < 0f)
                    {
                        SessionState.SetFloat(BaseAssemblyKey, duration);
                    }

                    continue;
                }

                float expectedDuration = bench.AssemblySeconds / Mathf.Max(0.1f, supportBonusSlots.EquipmentAssemblySpeedMultiplier);
                if (Mathf.Abs(duration - expectedDuration) > AssemblyToleranceSeconds)
                {
                    continue;
                }

                if (toolLevel == 1 && SessionState.GetFloat(ToolLv1AssemblyKey, -1f) < 0f)
                {
                    SessionState.SetFloat(ToolLv1AssemblyKey, duration);
                    continue;
                }

                if (toolLevel >= 2 && SessionState.GetFloat(ToolLv2AssemblyKey, -1f) < 0f)
                {
                    SessionState.SetFloat(ToolLv2AssemblyKey, duration);
                }
            }
        }

        private static WorkerController GetFirstWorker()
        {
            WorkerController[] workers = Object.FindObjectsByType<WorkerController>(FindObjectsSortMode.None);
            if (workers == null || workers.Length == 0)
            {
                return null;
            }

            System.Array.Sort(workers, (left, right) => string.Compare(left.name, right.name, System.StringComparison.Ordinal));
            return workers[0];
        }

        private static bool HasCompletedVerification(StageGoalManager stageGoalManager)
        {
            return stageGoalManager.HasCompletedAllStages
                && !stageGoalManager.HasActiveProductionGoal
                && SessionState.GetFloat(Lv2SpeedKey, -1f) > 0f
                && SessionState.GetFloat(ToolLv2AssemblyKey, -1f) > 0f
                && SessionState.GetFloat(HeadLv2RushPayoutKey, -1f) > 0f;
        }

        private static bool HasReachedLateVerificationState(StageGoalManager stageGoalManager)
        {
            return stageGoalManager.HasCompletedAllStages
                && stageGoalManager.ProductionRewardCount >= 4
                && SessionState.GetFloat(Lv2SpeedKey, -1f) > 0f
                && SessionState.GetFloat(ToolLv1AssemblyKey, -1f) > 0f
                && SessionState.GetFloat(HeadLv1RushPayoutKey, -1f) > 0f;
        }

        private static void Finish(string state)
        {
            SessionState.SetString(StatusKey, BuildStatus(state));
            SessionState.SetBool(ActiveKey, false);
            WriteStatus(state);
            UnhookUpdate();
            Time.timeScale = 1f;

            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }

            if (Application.isBatchMode)
            {
                int exitCode = state == "completed" || state == "completed_timeout" ? 0 : 1;
                EditorApplication.Exit(exitCode);
            }
        }

        private static void WriteStatus(string state)
        {
            string status = BuildStatus(state);
            SessionState.SetString(StatusKey, status);

            string statusPath = GetStatusPath();
            Directory.CreateDirectory(Path.GetDirectoryName(statusPath) ?? string.Empty);
            File.WriteAllText(statusPath, status);
            if (!string.Equals(SessionState.GetString(LastLoggedStateKey, string.Empty), state, System.StringComparison.Ordinal))
            {
                SessionState.SetString(LastLoggedStateKey, state);
                Debug.Log("[TinyFactory][M16] " + state);
            }
        }

        private static string BuildStatus(string state)
        {
            MoneyManager moneyManager = Object.FindFirstObjectByType<MoneyManager>();
            WorkerManager workerManager = Object.FindFirstObjectByType<WorkerManager>();
            FacilityManager facilityManager = Object.FindFirstObjectByType<FacilityManager>();
            PickupCounter pickupCounter = Object.FindFirstObjectByType<PickupCounter>();
            ProductProgressionManager productProgressionManager = Object.FindFirstObjectByType<ProductProgressionManager>();
            StageGoalManager stageGoalManager = Object.FindFirstObjectByType<StageGoalManager>();
            EquipmentManager equipmentManager = Object.FindFirstObjectByType<EquipmentManager>();

            Dictionary<string, string> values = new Dictionary<string, string>
            {
                { "state", state },
                { "simTime", FormatFloat(Application.isPlaying ? Time.timeSinceLevelLoad : 0f) },
                { "money", moneyManager != null ? moneyManager.CurrentMoney.ToString(CultureInfo.InvariantCulture) : "-1" },
                { "completed", pickupCounter != null ? pickupCounter.CompletedOrderCount.ToString(CultureInfo.InvariantCulture) : "-1" },
                { "workers", workerManager != null ? workerManager.WorkerCount.ToString(CultureInfo.InvariantCulture) : "-1" },
                { "productLevel", productProgressionManager != null ? productProgressionManager.ProductLevel.ToString(CultureInfo.InvariantCulture) : "-1" },
                { "activeBenches", facilityManager != null ? facilityManager.ActiveAssemblyBenchCount.ToString(CultureInfo.InvariantCulture) : "-1" },
                { "dispatchRack", facilityManager != null && facilityManager.HasDispatchRack ? "1" : "0" },
                { "stageLabel", stageGoalManager != null ? stageGoalManager.CurrentStageLabel : "missing" },
                { "stageClearCount", SessionState.GetInt(StageClearCountKey, 0).ToString(CultureInfo.InvariantCulture) },
                { "basicBoxes", stageGoalManager != null ? stageGoalManager.BasicBoxCount.ToString(CultureInfo.InvariantCulture) : "-1" },
                { "productionRewardCount", stageGoalManager != null ? stageGoalManager.ProductionRewardCount.ToString(CultureInfo.InvariantCulture) : "-1" },
                { "productionGoal", stageGoalManager != null ? stageGoalManager.GetProductionProgressText() : "missing" },
                { "ownedItems", equipmentManager != null ? equipmentManager.OwnedEquipmentCount.ToString(CultureInfo.InvariantCulture) : "-1" },
                { "bodyLv1Copies", equipmentManager != null ? equipmentManager.GetOwnedEquipmentGroupItemCount(BodyDefinitionId, 1).ToString(CultureInfo.InvariantCulture) : "-1" },
                { "bodyLv2Copies", equipmentManager != null ? equipmentManager.GetOwnedEquipmentGroupItemCount(BodyDefinitionId, 2).ToString(CultureInfo.InvariantCulture) : "-1" },
                { "toolLv1Copies", equipmentManager != null ? equipmentManager.GetOwnedEquipmentGroupItemCount(ToolDefinitionId, 1).ToString(CultureInfo.InvariantCulture) : "-1" },
                { "toolLv2Copies", equipmentManager != null ? equipmentManager.GetOwnedEquipmentGroupItemCount(ToolDefinitionId, 2).ToString(CultureInfo.InvariantCulture) : "-1" },
                { "headLv1Copies", equipmentManager != null ? equipmentManager.GetOwnedEquipmentGroupItemCount(HeadDefinitionId, 1).ToString(CultureInfo.InvariantCulture) : "-1" },
                { "headLv2Copies", equipmentManager != null ? equipmentManager.GetOwnedEquipmentGroupItemCount(HeadDefinitionId, 2).ToString(CultureInfo.InvariantCulture) : "-1" },
                { "baseSpeed", FormatFloat(SessionState.GetFloat(BaseSpeedKey, -1f)) },
                { "bodyLv1Speed", FormatFloat(SessionState.GetFloat(Lv1SpeedKey, -1f)) },
                { "bodyLv2Speed", FormatFloat(SessionState.GetFloat(Lv2SpeedKey, -1f)) },
                { "combineTime", FormatFloat(SessionState.GetFloat(CombineTimeKey, -1f)) },
                { "baseAssembly", FormatFloat(SessionState.GetFloat(BaseAssemblyKey, -1f)) },
                { "toolLv1Assembly", FormatFloat(SessionState.GetFloat(ToolLv1AssemblyKey, -1f)) },
                { "toolLv2Assembly", FormatFloat(SessionState.GetFloat(ToolLv2AssemblyKey, -1f)) },
                { "baseRushPayout", FormatFloat(SessionState.GetFloat(BaseRushPayoutKey, -1f)) },
                { "headLv1ObservedPayout", FormatFloat(SessionState.GetFloat(HeadLv1ObservedPayoutKey, -1f)) },
                { "headLv1ObservedLabel", SessionState.GetString(HeadLv1ObservedLabelKey, "-") },
                { "headLv1RushPayout", FormatFloat(SessionState.GetFloat(HeadLv1RushPayoutKey, -1f)) },
                { "headLv2ObservedPayout", FormatFloat(SessionState.GetFloat(HeadLv2ObservedPayoutKey, -1f)) },
                { "headLv2ObservedLabel", SessionState.GetString(HeadLv2ObservedLabelKey, "-") },
                { "headLv2RushPayout", FormatFloat(SessionState.GetFloat(HeadLv2RushPayoutKey, -1f)) },
                { "lastOrderLabel", pickupCounter != null ? pickupCounter.LastCompletedOrderLabel : "missing" },
                { "lastShipmentPayout", pickupCounter != null ? pickupCounter.LastShipmentPayout.ToString(CultureInfo.InvariantCulture) : "-1" },
                { "equipmentMessage", equipmentManager != null ? equipmentManager.LastMessage : "missing" },
                { "headSlot", equipmentManager != null ? equipmentManager.GetSlotSummary(EquipmentManager.EquipmentSlot.Head) : "missing" },
                { "bodySlot", equipmentManager != null ? equipmentManager.GetSlotSummary(EquipmentManager.EquipmentSlot.Body) : "missing" },
                { "toolSlot", equipmentManager != null ? equipmentManager.GetSlotSummary(EquipmentManager.EquipmentSlot.Tool) : "missing" },
                { "equippedBonus", equipmentManager != null ? equipmentManager.GetEquippedBonusSummary() : "missing" },
                { "bodyLv2Preview", equipmentManager != null ? GetGroupPreview(equipmentManager, BodyDefinitionId, 2) : "missing" },
                { "toolLv2Preview", equipmentManager != null ? GetGroupPreview(equipmentManager, ToolDefinitionId, 2) : "missing" },
                { "headLv2Preview", equipmentManager != null ? GetGroupPreview(equipmentManager, HeadDefinitionId, 2) : "missing" }
            };

            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, string> entry in values)
            {
                builder.Append(entry.Key);
                builder.Append('=');
                builder.AppendLine(entry.Value);
            }

            return builder.ToString().TrimEnd();
        }

        private static string GetGroupPreview(EquipmentManager equipmentManager, string definitionId, int level)
        {
            int groupIndex = equipmentManager.FindOwnedEquipmentGroupIndex(definitionId, level);
            return groupIndex >= 0 ? equipmentManager.GetOwnedEquipmentGroupPreview(groupIndex) : "missing";
        }

        private static string GetTriggerPath()
        {
            return Path.Combine(GetProjectLogsDirectory(), TriggerFileName);
        }

        private static string GetStatusPath()
        {
            return Path.Combine(GetProjectLogsDirectory(), StatusFileName);
        }

        private static string GetProjectLogsDirectory()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs"));
        }

        private static string FormatFloat(float value)
        {
            return value >= 0f ? value.ToString("0.000", CultureInfo.InvariantCulture) : "-";
        }

        private static void ResetMetrics()
        {
            BenchCompletionCache.Clear();
            SessionState.SetBool(HookedKey, false);
            SessionState.SetFloat(BaseSpeedKey, -1f);
            SessionState.SetFloat(Lv1SpeedKey, -1f);
            SessionState.SetFloat(Lv2SpeedKey, -1f);
            SessionState.SetFloat(CombineTimeKey, -1f);
            SessionState.SetInt(StageClearCountKey, 0);
            SessionState.SetString(LastLoggedStateKey, string.Empty);
            SessionState.SetInt(LastObservedCompletedOrdersKey, 0);
            SessionState.SetFloat(BaseAssemblyKey, -1f);
            SessionState.SetFloat(ToolLv1AssemblyKey, -1f);
            SessionState.SetFloat(ToolLv2AssemblyKey, -1f);
            SessionState.SetFloat(BaseRushPayoutKey, -1f);
            SessionState.SetFloat(HeadLv1RushPayoutKey, -1f);
            SessionState.SetFloat(HeadLv2RushPayoutKey, -1f);
            SessionState.SetFloat(HeadLv1ObservedPayoutKey, -1f);
            SessionState.SetFloat(HeadLv2ObservedPayoutKey, -1f);
            SessionState.SetString(HeadLv1ObservedLabelKey, string.Empty);
            SessionState.SetString(HeadLv2ObservedLabelKey, string.Empty);
        }
    }
}
