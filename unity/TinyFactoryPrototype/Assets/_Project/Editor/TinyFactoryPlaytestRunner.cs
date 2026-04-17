using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TinyFactory.Economy;
using TinyFactory.Stations;
using TinyFactory.Workers;
using UnityEditor;
using UnityEngine;

namespace TinyFactory.Editor
{
    [InitializeOnLoad]
    public static class TinyFactoryPlaytestRunner
    {
        private const string ActiveKey = "TinyFactory.Playtest.Active";
        private const string StatusKey = "TinyFactory.Playtest.Status";
        private const string HookedKey = "TinyFactory.Playtest.Hooked";
        private const string FirstPickupKey = "TinyFactory.Playtest.FirstPickup";
        private const string FirstWorkerKey = "TinyFactory.Playtest.FirstWorker";
        private const string Level2Key = "TinyFactory.Playtest.Level2";
        private const string Level3Key = "TinyFactory.Playtest.Level3";
        private const string SecondBenchKey = "TinyFactory.Playtest.SecondBench";
        private const string PickupAfterSecondBenchKey = "TinyFactory.Playtest.PickupAfterSecondBench";
        private const string CompletedAtSecondBenchKey = "TinyFactory.Playtest.CompletedAtSecondBench";
        private const string ThroughputBoughtKey = "TinyFactory.Playtest.ThroughputBought";

        private const float SimulatedDurationSeconds = 300f;
        private const float TimeScale = 20f;

        static TinyFactoryPlaytestRunner()
        {
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
            EditorApplication.delayCall += ResumeIfNeeded;
        }

        public static string StartMilestone10Playtest()
        {
            if (SessionState.GetBool(ActiveKey, false))
            {
                return GetStatus();
            }

            ResetMetrics();
            SessionState.SetBool(ActiveKey, true);
            SessionState.SetString(StatusKey, "state=starting");

            if (EditorApplication.isPlaying)
            {
                HookUpdateIfNeeded();
            }
            else if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.isPlaying = true;
            }

            return GetStatus();
        }

        public static string GetStatus()
        {
            return SessionState.GetString(StatusKey, "state=idle");
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
                SessionState.SetString(StatusKey, BuildStatus("running"));
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
                SessionState.SetString(StatusKey, BuildStatus("running"));
                return;
            }

            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                UnityEngine.Time.timeScale = 1f;
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

            if (moneyManager == null
                || workerManager == null
                || facilityManager == null
                || pickupCounter == null
                || upgradeManager == null
                || productProgressionManager == null)
            {
                Finish("failed_missing_manager");
                return;
            }

            UnityEngine.Time.timeScale = TimeScale;

            float simTime = UnityEngine.Time.timeSinceLevelLoad;
            TrackMilestones(simTime, workerManager, facilityManager, pickupCounter, productProgressionManager);
            ApplyMilestone10Purchases(workerManager, facilityManager, upgradeManager, productProgressionManager);

            SessionState.SetString(StatusKey, BuildStatus("running"));

            if (simTime >= SimulatedDurationSeconds)
            {
                Finish("completed");
            }
        }

        private static void TrackMilestones(
            float simTime,
            WorkerManager workerManager,
            FacilityManager facilityManager,
            PickupCounter pickupCounter,
            ProductProgressionManager productProgressionManager)
        {
            if (SessionState.GetFloat(FirstPickupKey, -1f) < 0f && pickupCounter.CompletedOrderCount >= 1)
            {
                SessionState.SetFloat(FirstPickupKey, simTime);
            }

            if (SessionState.GetFloat(FirstWorkerKey, -1f) < 0f && workerManager.WorkerCount >= 2)
            {
                SessionState.SetFloat(FirstWorkerKey, simTime);
            }

            if (SessionState.GetFloat(Level2Key, -1f) < 0f && productProgressionManager.ProductLevel >= 2)
            {
                SessionState.SetFloat(Level2Key, simTime);
            }

            if (SessionState.GetFloat(Level3Key, -1f) < 0f && productProgressionManager.ProductLevel >= 3)
            {
                SessionState.SetFloat(Level3Key, simTime);
            }

            if (SessionState.GetFloat(SecondBenchKey, -1f) < 0f && facilityManager.ActiveAssemblyBenchCount >= 2)
            {
                SessionState.SetFloat(SecondBenchKey, simTime);
                SessionState.SetInt(CompletedAtSecondBenchKey, pickupCounter.CompletedOrderCount);
            }

            float secondBenchTime = SessionState.GetFloat(SecondBenchKey, -1f);
            if (secondBenchTime >= 0f
                && SessionState.GetFloat(PickupAfterSecondBenchKey, -1f) < 0f
                && pickupCounter.CompletedOrderCount > SessionState.GetInt(CompletedAtSecondBenchKey, -1))
            {
                SessionState.SetFloat(PickupAfterSecondBenchKey, simTime);
            }
        }

        private static void ApplyMilestone10Purchases(
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

            if (!SessionState.GetBool(ThroughputBoughtKey, false))
            {
                bool bought = workerManager.TryUpgradeWorkerThroughput();
                if (bought)
                {
                    SessionState.SetBool(ThroughputBoughtKey, true);
                }
            }
        }

        private static void Finish(string state)
        {
            SessionState.SetString(StatusKey, BuildStatus(state));
            SessionState.SetBool(ActiveKey, false);
            SessionState.SetBool(ThroughputBoughtKey, false);
            UnhookUpdate();
            UnityEngine.Time.timeScale = 1f;

            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }
        }

        private static string BuildStatus(string state)
        {
            MoneyManager moneyManager = Object.FindFirstObjectByType<MoneyManager>();
            WorkerManager workerManager = Object.FindFirstObjectByType<WorkerManager>();
            FacilityManager facilityManager = Object.FindFirstObjectByType<FacilityManager>();
            PickupCounter pickupCounter = Object.FindFirstObjectByType<PickupCounter>();
            ProductProgressionManager productProgressionManager = Object.FindFirstObjectByType<ProductProgressionManager>();
            float simTime = Application.isPlaying ? UnityEngine.Time.timeSinceLevelLoad : 0f;

            Dictionary<string, string> values = new Dictionary<string, string>
            {
                { "state", state },
                { "simTime", simTime.ToString("0.0", CultureInfo.InvariantCulture) },
                { "money", moneyManager != null ? moneyManager.CurrentMoney.ToString(CultureInfo.InvariantCulture) : "-1" },
                { "completed", pickupCounter != null ? pickupCounter.CompletedOrderCount.ToString(CultureInfo.InvariantCulture) : "-1" },
                { "workers", workerManager != null ? workerManager.WorkerCount.ToString(CultureInfo.InvariantCulture) : "-1" },
                { "productLevel", productProgressionManager != null ? productProgressionManager.ProductLevel.ToString(CultureInfo.InvariantCulture) : "-1" },
                {
                    "pickupValue",
                    productProgressionManager != null
                        ? productProgressionManager.CurrentPickupValue.ToString(CultureInfo.InvariantCulture)
                        : pickupCounter != null
                            ? pickupCounter.SaleValue.ToString(CultureInfo.InvariantCulture)
                            : "-1"
                },
                { "activeBenches", facilityManager != null ? facilityManager.ActiveAssemblyBenchCount.ToString(CultureInfo.InvariantCulture) : "-1" },
                { "firstPickup", FormatMetric(SessionState.GetFloat(FirstPickupKey, -1f)) },
                { "firstWorker", FormatMetric(SessionState.GetFloat(FirstWorkerKey, -1f)) },
                { "level2", FormatMetric(SessionState.GetFloat(Level2Key, -1f)) },
                { "level3", FormatMetric(SessionState.GetFloat(Level3Key, -1f)) },
                { "secondBench", FormatMetric(SessionState.GetFloat(SecondBenchKey, -1f)) },
                { "pickupAfterSecondBench", FormatMetric(SessionState.GetFloat(PickupAfterSecondBenchKey, -1f)) }
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

        private static string FormatMetric(float value)
        {
            return value >= 0f ? value.ToString("0.0", CultureInfo.InvariantCulture) : "-";
        }

        private static void ResetMetrics()
        {
            SessionState.SetBool(HookedKey, false);
            SessionState.SetBool(ThroughputBoughtKey, false);
            SessionState.SetFloat(FirstPickupKey, -1f);
            SessionState.SetFloat(FirstWorkerKey, -1f);
            SessionState.SetFloat(Level2Key, -1f);
            SessionState.SetFloat(Level3Key, -1f);
            SessionState.SetFloat(SecondBenchKey, -1f);
            SessionState.SetFloat(PickupAfterSecondBenchKey, -1f);
            SessionState.SetInt(CompletedAtSecondBenchKey, -1);
        }
    }
}
