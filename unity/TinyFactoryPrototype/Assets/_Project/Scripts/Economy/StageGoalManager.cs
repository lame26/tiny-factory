using System;
using TinyFactory.Stations;
using TinyFactory.Workers;
using UnityEngine;

namespace TinyFactory.Economy
{
    public sealed class StageGoalManager : MonoBehaviour
    {
        private struct StageGoalDefinition
        {
            public StageGoalDefinition(
                string label,
                int requiredCompletedOrders,
                int requiredWorkers,
                int requiredAssemblyBenches,
                bool requireDispatchRack,
                int basicBoxReward,
                int advancedBoxReward = 0,
                int premiumBoxReward = 0)
            {
                Label = label;
                RequiredCompletedOrders = requiredCompletedOrders;
                RequiredWorkers = requiredWorkers;
                RequiredAssemblyBenches = requiredAssemblyBenches;
                RequireDispatchRack = requireDispatchRack;
                BasicBoxReward = basicBoxReward;
                AdvancedBoxReward = advancedBoxReward;
                PremiumBoxReward = premiumBoxReward;
            }

            public string Label { get; }
            public int RequiredCompletedOrders { get; }
            public int RequiredWorkers { get; }
            public int RequiredAssemblyBenches { get; }
            public bool RequireDispatchRack { get; }
            public int BasicBoxReward { get; }
            public int AdvancedBoxReward { get; }
            public int PremiumBoxReward { get; }
        }

        private struct ProductionGoalDefinition
        {
            public ProductionGoalDefinition(string label, int requiredCompletedOrders, int basicBoxReward, int advancedBoxReward = 0, int premiumBoxReward = 0)
            {
                Label = label;
                RequiredCompletedOrders = requiredCompletedOrders;
                BasicBoxReward = basicBoxReward;
                AdvancedBoxReward = advancedBoxReward;
                PremiumBoxReward = premiumBoxReward;
            }

            public string Label { get; }
            public int RequiredCompletedOrders { get; }
            public int BasicBoxReward { get; }
            public int AdvancedBoxReward { get; }
            public int PremiumBoxReward { get; }
        }

        private static readonly StageGoalDefinition[] StageGoals =
        {
            new StageGoalDefinition("Workshop 1", 30, 2, 2, false, 1),
            new StageGoalDefinition("Workshop 2", 36, 2, 2, true, 1),
            new StageGoalDefinition("Workshop 3", 45, 3, 2, true, 1)
        };

        private static readonly ProductionGoalDefinition[] ProductionGoals =
        {
            new ProductionGoalDefinition("Output 48", 48, 1),
            new ProductionGoalDefinition("Output 52", 52, 1),
            new ProductionGoalDefinition("Output 56", 56, 1),
            new ProductionGoalDefinition("Output 60", 60, 1),
            new ProductionGoalDefinition("Output 64", 64, 1),
            new ProductionGoalDefinition("Output 68", 68, 1),
            new ProductionGoalDefinition("Output 72", 72, 0, 1),
            new ProductionGoalDefinition("Output 76", 76, 0, 1),
            new ProductionGoalDefinition("Output 80", 80, 0, 1),
            new ProductionGoalDefinition("Output 84", 84, 0, 1),
            new ProductionGoalDefinition("Output 88", 88, 0, 1),
            new ProductionGoalDefinition("Output 92", 92, 0, 1),
            new ProductionGoalDefinition("Output 96", 96, 0, 1),
            new ProductionGoalDefinition("Output 100", 100, 0, 1),
            new ProductionGoalDefinition("Output 104", 104, 0, 1),
            new ProductionGoalDefinition("Output 108", 108, 0, 1),
            new ProductionGoalDefinition("Output 112", 112, 0, 1),
            new ProductionGoalDefinition("Output 116", 116, 0, 1),
            new ProductionGoalDefinition("Output 120", 120, 0, 0, 1),
            new ProductionGoalDefinition("Output 124", 124, 0, 0, 1),
            new ProductionGoalDefinition("Output 128", 128, 0, 0, 1)
        };

        private static StageGoalManager s_runtimeInstance;

        [SerializeField] private PickupCounter pickupCounter;
        [SerializeField] private WorkerManager workerManager;
        [SerializeField] private FacilityManager facilityManager;
        [SerializeField] private int currentStageIndex;
        [SerializeField] private int currentProductionGoalIndex;
        [SerializeField] private int basicBoxCount;
        [SerializeField] private int advancedBoxCount;
        [SerializeField] private int premiumBoxCount;
        [SerializeField] private int productionRewardCount;
        [SerializeField] private string lastRewardMessage = "Stage goals ready";

        public event Action<int, string> StageCleared;

        public int CurrentStageNumber => HasActiveStage ? currentStageIndex + 1 : StageGoals.Length;
        public string CurrentStageLabel => HasActiveStage ? StageGoals[currentStageIndex].Label : "All Clear";
        public int BasicBoxCount => Mathf.Max(0, basicBoxCount);
        public int AdvancedBoxCount => Mathf.Max(0, advancedBoxCount);
        public int PremiumBoxCount => Mathf.Max(0, premiumBoxCount);
        public int ProductionRewardCount => Mathf.Max(0, productionRewardCount);
        public string LastRewardMessage => string.IsNullOrWhiteSpace(lastRewardMessage) ? "Stage goals ready" : lastRewardMessage;
        public bool HasActiveStage => currentStageIndex >= 0 && currentStageIndex < StageGoals.Length;
        public bool HasCompletedAllStages => !HasActiveStage;
        public bool HasActiveProductionGoal => currentProductionGoalIndex >= 0 && currentProductionGoalIndex < ProductionGoals.Length;
        public string CurrentProductionGoalLabel => HasActiveProductionGoal ? ProductionGoals[currentProductionGoalIndex].Label : "All production rewards cleared";
        public int CompletedProductionGoalCount => Mathf.Clamp(currentProductionGoalIndex, 0, ProductionGoals.Length);
        public int TotalProductionGoalCount => ProductionGoals.Length;
        public int RemainingOrdersToNextProductionGoal => HasActiveProductionGoal
            ? Mathf.Max(0, ProductionGoals[currentProductionGoalIndex].RequiredCompletedOrders - GetCompletedOrders())
            : 0;

        public static StageGoalManager GetOrCreate()
        {
            StageGoalManager existing = FindFirstObjectByType<StageGoalManager>();
            if (existing != null)
            {
                return existing;
            }

            if (s_runtimeInstance != null)
            {
                return s_runtimeInstance;
            }

            GameObject runtimeObject = new GameObject("StageGoalManager");
            s_runtimeInstance = runtimeObject.AddComponent<StageGoalManager>();
            return s_runtimeInstance;
        }

        private void Awake()
        {
            if (s_runtimeInstance == null)
            {
                s_runtimeInstance = this;
            }

            ResolveReferences();
        }

        private void Update()
        {
            ResolveReferences();
            if (HasActiveStage && IsCurrentStageComplete())
            {
                CompleteCurrentStage();
            }

            if (IsCurrentProductionGoalComplete())
            {
                CompleteCurrentProductionGoal();
            }
        }

        private void OnDestroy()
        {
            if (s_runtimeInstance == this)
            {
                s_runtimeInstance = null;
            }
        }

        public string GetProgressText()
        {
            if (!HasActiveStage)
            {
                return "All stage goals cleared";
            }

            StageGoalDefinition definition = StageGoals[currentStageIndex];
            string progress = "Dispatch "
                + GetCompletedOrders()
                + "/"
                + definition.RequiredCompletedOrders
                + " / Workers "
                + GetWorkerCount()
                + "/"
                + definition.RequiredWorkers
                + " / Benches "
                + GetAssemblyBenchCount()
                + "/"
                + definition.RequiredAssemblyBenches;

            if (definition.RequireDispatchRack)
            {
                progress += " / Rack " + (HasDispatchRack() ? "1/1" : "0/1");
            }

            return progress;
        }

        public string GetProductionProgressText()
        {
            if (!HasActiveProductionGoal)
            {
                return "All production rewards cleared";
            }

            ProductionGoalDefinition definition = ProductionGoals[currentProductionGoalIndex];
            return definition.Label
                + " / Dispatch "
                + GetCompletedOrders()
                + "/"
                + definition.RequiredCompletedOrders
                + " / "
                + BuildRewardText(definition.BasicBoxReward, definition.AdvancedBoxReward, definition.PremiumBoxReward);
        }

        public string GetProductionRewardSummaryText()
        {
            int remainingOrders = 0;
            if (HasActiveProductionGoal)
            {
                remainingOrders = Mathf.Max(0, ProductionGoals[currentProductionGoalIndex].RequiredCompletedOrders - GetCompletedOrders());
            }

            return "Rewards "
                + CompletedProductionGoalCount
                + "/"
                + TotalProductionGoalCount
                + " / Next in "
                + remainingOrders
                + " dispatches";
        }

        public bool TryConsumeBasicBox(int amount = 1)
        {
            int sanitizedAmount = Mathf.Max(1, amount);
            if (basicBoxCount < sanitizedAmount)
            {
                return false;
            }

            basicBoxCount -= sanitizedAmount;
            return true;
        }

        public bool TryConsumeAdvancedBox(int amount = 1)
        {
            int sanitizedAmount = Mathf.Max(1, amount);
            if (advancedBoxCount < sanitizedAmount)
            {
                return false;
            }

            advancedBoxCount -= sanitizedAmount;
            return true;
        }

        public bool TryConsumePremiumBox(int amount = 1)
        {
            int sanitizedAmount = Mathf.Max(1, amount);
            if (premiumBoxCount < sanitizedAmount)
            {
                return false;
            }

            premiumBoxCount -= sanitizedAmount;
            return true;
        }

        public void DebugGrantBoxes(int basicBoxes, int advancedBoxes, int premiumBoxes)
        {
            basicBoxCount += Mathf.Max(0, basicBoxes);
            advancedBoxCount += Mathf.Max(0, advancedBoxes);
            premiumBoxCount += Mathf.Max(0, premiumBoxes);
            lastRewardMessage = "Debug grant / Basic +"
                + Mathf.Max(0, basicBoxes)
                + " / Advanced +"
                + Mathf.Max(0, advancedBoxes)
                + " / Premium +"
                + Mathf.Max(0, premiumBoxes);
        }

        public void DebugAdvanceProductionGoals(int goalCount)
        {
            int steps = Mathf.Max(0, goalCount);
            for (int i = 0; i < steps; i++)
            {
                if (!HasActiveProductionGoal)
                {
                    break;
                }

                CompleteCurrentProductionGoal();
            }
        }

        private void ResolveReferences()
        {
            if (pickupCounter == null)
            {
                pickupCounter = FindFirstObjectByType<PickupCounter>();
            }

            if (workerManager == null)
            {
                workerManager = FindFirstObjectByType<WorkerManager>();
            }

            if (facilityManager == null)
            {
                facilityManager = FindFirstObjectByType<FacilityManager>();
            }
        }

        private bool IsCurrentStageComplete()
        {
            StageGoalDefinition definition = StageGoals[currentStageIndex];
            if (GetCompletedOrders() < definition.RequiredCompletedOrders)
            {
                return false;
            }

            if (GetWorkerCount() < definition.RequiredWorkers)
            {
                return false;
            }

            if (GetAssemblyBenchCount() < definition.RequiredAssemblyBenches)
            {
                return false;
            }

            if (definition.RequireDispatchRack && !HasDispatchRack())
            {
                return false;
            }

            return true;
        }

        private void CompleteCurrentStage()
        {
            StageGoalDefinition definition = StageGoals[currentStageIndex];
            int clearedStageNumber = currentStageIndex + 1;
            basicBoxCount += definition.BasicBoxReward;
            advancedBoxCount += definition.AdvancedBoxReward;
            premiumBoxCount += definition.PremiumBoxReward;
            currentStageIndex += 1;
            lastRewardMessage = "Cleared " + definition.Label + ". " + BuildRewardText(definition.BasicBoxReward, definition.AdvancedBoxReward, definition.PremiumBoxReward) + ".";
            StageCleared?.Invoke(clearedStageNumber, definition.Label);
        }

        private bool IsCurrentProductionGoalComplete()
        {
            if (!HasActiveProductionGoal)
            {
                return false;
            }

            return GetCompletedOrders() >= ProductionGoals[currentProductionGoalIndex].RequiredCompletedOrders;
        }

        private void CompleteCurrentProductionGoal()
        {
            ProductionGoalDefinition definition = ProductionGoals[currentProductionGoalIndex];
            basicBoxCount += definition.BasicBoxReward;
            advancedBoxCount += definition.AdvancedBoxReward;
            premiumBoxCount += definition.PremiumBoxReward;
            productionRewardCount += 1;
            currentProductionGoalIndex += 1;
            lastRewardMessage = "Reached " + definition.Label + ". " + BuildRewardText(definition.BasicBoxReward, definition.AdvancedBoxReward, definition.PremiumBoxReward) + ".";
        }

        private static string BuildRewardText(int basicBoxReward, int advancedBoxReward, int premiumBoxReward)
        {
            if (basicBoxReward > 0 && advancedBoxReward > 0 && premiumBoxReward > 0)
            {
                return "Basic Box +" + basicBoxReward + " / Advanced Box +" + advancedBoxReward + " / Premium Box +" + premiumBoxReward;
            }

            if (basicBoxReward > 0 && premiumBoxReward > 0)
            {
                return "Basic Box +" + basicBoxReward + " / Premium Box +" + premiumBoxReward;
            }

            if (advancedBoxReward > 0 && premiumBoxReward > 0)
            {
                return "Advanced Box +" + advancedBoxReward + " / Premium Box +" + premiumBoxReward;
            }

            if (premiumBoxReward > 0)
            {
                return "Premium Box +" + premiumBoxReward;
            }

            if (advancedBoxReward > 0)
            {
                return "Advanced Box +" + advancedBoxReward;
            }

            return "Basic Box +" + basicBoxReward;
        }

        private int GetCompletedOrders()
        {
            return pickupCounter != null ? pickupCounter.CompletedOrderCount : 0;
        }

        private int GetWorkerCount()
        {
            return workerManager != null ? workerManager.WorkerCount : 0;
        }

        private int GetAssemblyBenchCount()
        {
            return facilityManager != null ? facilityManager.ActiveAssemblyBenchCount : 0;
        }

        private bool HasDispatchRack()
        {
            return facilityManager != null && facilityManager.HasDispatchRack;
        }
    }
}
