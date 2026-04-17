using System.Collections.Generic;
using TinyFactory.Core;
using TinyFactory.Economy;
using TinyFactory.Interaction;
using TinyFactory.Stations;
using TinyFactory.Workers;
using UnityEngine;

namespace TinyFactory.UI
{
    public sealed class HudController : MonoBehaviour
    {
        [SerializeField] private MoneyManager moneyManager;
        [SerializeField] private StationSelectionController selectionController;
        [SerializeField] private AssemblyBench assemblyBench;
        [SerializeField] private OrderCounter orderCounter;
        [SerializeField] private PickupCounter pickupCounter;
        [SerializeField] private UpgradeManager upgradeManager;
        [SerializeField] private ProductProgressionManager productProgressionManager;
        [SerializeField] private FactoryBoostManager factoryBoostManager;
        [SerializeField] private WorkerManager workerManager;
        [SerializeField] private FacilityManager facilityManager;
        [SerializeField] private StageGoalManager stageGoalManager;
        [SerializeField] private EquipmentManager equipmentManager;
        [SerializeField] private SupportBonusSlots supportBonusSlots;
        [SerializeField] private Rect panelRect = new Rect(12f, 12f, 360f, 340f);
        [SerializeField] private float pickupFeedbackSeconds = 1.4f;
        [SerializeField] private bool showDevelopmentPanel = true;
        [SerializeField] private string developmentStatus = "Dev tools ready";
        [SerializeField] private string verificationStatus = "Verification idle";

        private readonly List<PickupFeedback> pickupFeedbacks = new List<PickupFeedback>();
        private GUIStyle feedbackStyle;

        private struct PickupFeedback
        {
            public string Text;
            public Vector3 WorldPosition;
            public float StartTime;
            public float EndTime;
        }

        private struct PlaytestSnapshot
        {
            public int Money;
            public int CompletedOrders;
            public int ProductValue;
            public int RewardCount;
            public int RemainingOrdersToReward;
            public int PremiumOpenedCount;
            public int LastShipmentPayout;
            public float WorkerMoveSpeed;
            public float MoveMultiplier;
            public float SaleMultiplier;
            public float AssemblyMultiplier;
            public float LastAssemblySeconds;
        }

        private bool hasPlaytestBaseline;
        private PlaytestSnapshot playtestBaseline;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (pickupCounter != null)
            {
                pickupCounter.PickupCompleted += HandlePickupCompleted;
            }
        }

        private void OnDisable()
        {
            if (pickupCounter != null)
            {
                pickupCounter.PickupCompleted -= HandlePickupCompleted;
            }
        }

        private void ResolveReferences()
        {
            if (moneyManager == null)
            {
                moneyManager = FindFirstObjectByType<MoneyManager>();
            }

            if (selectionController == null)
            {
                selectionController = FindFirstObjectByType<StationSelectionController>();
            }

            if (assemblyBench == null)
            {
                assemblyBench = FindFirstObjectByType<AssemblyBench>();
            }

            if (pickupCounter == null)
            {
                pickupCounter = FindFirstObjectByType<PickupCounter>();
            }

            if (orderCounter == null)
            {
                orderCounter = FindFirstObjectByType<OrderCounter>();
            }

            if (upgradeManager == null)
            {
                upgradeManager = FindFirstObjectByType<UpgradeManager>();
            }

            if (productProgressionManager == null)
            {
                productProgressionManager = ProductProgressionManager.GetOrCreate();
            }

            if (factoryBoostManager == null)
            {
                factoryBoostManager = FactoryBoostManager.GetOrCreate();
            }

            if (workerManager == null)
            {
                workerManager = FindFirstObjectByType<WorkerManager>();
            }

            if (facilityManager == null)
            {
                facilityManager = FindFirstObjectByType<FacilityManager>();
            }

            if (stageGoalManager == null)
            {
                stageGoalManager = StageGoalManager.GetOrCreate();
            }

            if (equipmentManager == null)
            {
                equipmentManager = EquipmentManager.GetOrCreate();
            }

            if (supportBonusSlots == null)
            {
                supportBonusSlots = FindFirstObjectByType<SupportBonusSlots>();
            }
        }

        private void OnGUI()
        {
            Rect actualPanelRect = new Rect(panelRect.x, panelRect.y, panelRect.width, Mathf.Max(panelRect.height, 920f));
            GUILayout.BeginArea(actualPanelRect, GUI.skin.box);
            GUILayout.Label("Tiny Factory");
            GUILayout.Space(4f);

            GUILayout.Label("Money: " + GetMoneyText());
            GUILayout.Label("Stage: " + GetStageText());
            GUILayout.Label("Rewards: " + GetRewardText());
            GUILayout.Label("Production: " + GetProductionRewardText());
            GUILayout.Label("Reward Loop: " + GetProductionRewardSummaryText());
            DrawRecommendedGoal();
            GUILayout.Space(6f);

            GUILayout.Label("Selected: " + GetSelectedName());
            GUILayout.Label(GetSelectedStatus());
            DrawSelectedAssemblyBenchControls();
            DrawSelectedOrderControls();
            GUILayout.Space(4f);

            GUILayout.Label("Assembly: " + GetAssemblyStatus());
            GUILayout.Label("Product: " + GetProductStatus());
            DrawProductCycleControls();
            DrawBoostControls();
            GUILayout.Label("Orders: " + GetOrderStatus());
            GUILayout.Space(8f);

            DrawUpgradeButton("Level Selected Station", upgradeManager != null ? upgradeManager.StationLevelCost : 0, () => upgradeManager.TryUpgradeSelectedStation());
            DrawUpgradeButton("Assembly Speed", upgradeManager != null ? upgradeManager.AssemblySpeedCost : 0, () => upgradeManager.TryUpgradeAssemblySpeed());
            DrawUpgradeButton(GetProductLevelButtonLabel(), upgradeManager != null ? upgradeManager.ProductLevelCost : 0, () => upgradeManager.TryLevelUpProduct());

            GUILayout.Space(6f);
            GUILayout.Label("Workers: " + GetWorkerText());
            DrawWorkerButton(GetHireWorkerLabel(), workerManager != null ? workerManager.HireWorkerCost : 0, () => workerManager.TryHireWorker());
            DrawWorkerButton("Worker Throughput", workerManager != null ? workerManager.WorkerThroughputCost : 0, () => workerManager.TryUpgradeWorkerThroughput());

            GUILayout.Space(6f);
            GUILayout.Label("Facilities: " + GetFacilityText());
            DrawFacilityButton("Build Assembly Bench", facilityManager != null ? facilityManager.BuildAssemblyBenchCost : 0, () => facilityManager.TryBuildAssemblyBench());
            DrawPackingButton("Build Packing Station", facilityManager != null ? facilityManager.BuildPackingStationCost : 0, () => facilityManager.TryBuildPackingStation());
            DrawDispatchButton("Build Dispatch Rack", facilityManager != null ? facilityManager.BuildDispatchRackCost : 0, () => facilityManager.TryBuildDispatchRack());

            GUILayout.Space(6f);
            GUILayout.Label(upgradeManager != null ? upgradeManager.LastMessage : "Upgrade system missing");
            GUILayout.Label(workerManager != null ? workerManager.LastMessage : "Worker system missing");
            GUILayout.Label(facilityManager != null ? facilityManager.LastMessage : "Facility system missing");
            GUILayout.Space(6f);
            DrawEquipmentSection();
            GUILayout.Space(8f);
            DrawDevelopmentSection();
            GUILayout.EndArea();

            DrawPickupFeedbacks();
        }

        private void DrawUpgradeButton(string label, int cost, System.Func<bool> onClick)
        {
            GUI.enabled = upgradeManager != null;
            if (GUILayout.Button(label + " ($" + MoneyFormatter.Format(cost) + ")", GUILayout.Height(28f)))
            {
                onClick();
            }
            GUI.enabled = true;
        }

        private void DrawWorkerButton(string label, int cost, System.Func<bool> onClick)
        {
            GUI.enabled = workerManager != null;
            if (GUILayout.Button(label + " ($" + MoneyFormatter.Format(cost) + ")", GUILayout.Height(28f)))
            {
                onClick();
            }
            GUI.enabled = true;
        }

        private void DrawFacilityButton(string label, int cost, System.Func<bool> onClick)
        {
            GUI.enabled = facilityManager != null && facilityManager.CanBuildAssemblyBench;
            string suffix = facilityManager != null && facilityManager.CanBuildAssemblyBench
                ? " ($" + MoneyFormatter.Format(cost) + ")"
                : " (Max)";
            if (GUILayout.Button(label + suffix, GUILayout.Height(28f)))
            {
                onClick();
            }
            GUI.enabled = true;
        }

        private void DrawDispatchButton(string label, int cost, System.Func<bool> onClick)
        {
            GUI.enabled = facilityManager != null && facilityManager.CanBuildDispatchRack;
            string suffix = facilityManager != null && facilityManager.CanBuildDispatchRack
                ? " ($" + MoneyFormatter.Format(cost) + ")"
                : " (Built)";
            if (GUILayout.Button(label + suffix, GUILayout.Height(28f)))
            {
                onClick();
            }
            GUI.enabled = true;
        }

        private void DrawPackingButton(string label, int cost, System.Func<bool> onClick)
        {
            GUI.enabled = facilityManager != null && facilityManager.CanBuildPackingStation;
            string suffix = facilityManager != null && facilityManager.CanBuildPackingStation
                ? " ($" + MoneyFormatter.Format(cost) + ")"
                : " (Built)";
            if (GUILayout.Button(label + suffix, GUILayout.Height(28f)))
            {
                onClick();
            }
            GUI.enabled = true;
        }

        private string GetMoneyText()
        {
            return moneyManager != null ? MoneyFormatter.Format(moneyManager.CurrentMoney) : "0";
        }

        private string GetSelectedName()
        {
            return selectionController != null ? selectionController.CurrentSelectionName : "None";
        }

        private string GetSelectedStatus()
        {
            return selectionController != null ? selectionController.CurrentSelectionStatus : "No station selected";
        }

        private string GetAssemblyStatus()
        {
            return assemblyBench != null ? assemblyBench.StatusText : "Missing Assembly Bench";
        }

        private string GetProductStatus()
        {
            if (productProgressionManager == null)
            {
                return "Missing Product Progression";
            }

            return productProgressionManager.ProductName
                + " Lv "
                + productProgressionManager.ProductLevel
                + " / Value $"
                + MoneyFormatter.Format(productProgressionManager.CurrentPickupValue)
                + " / Unlocked "
                + productProgressionManager.UnlockedProductCount;
        }

        private string GetProductLevelButtonLabel()
        {
            if (productProgressionManager == null)
            {
                return "Level Product";
            }

            return "Level " + productProgressionManager.ProductName;
        }

        private void DrawProductCycleControls()
        {
            if (productProgressionManager == null)
            {
                return;
            }

            GUILayout.BeginHorizontal();
            GUI.enabled = productProgressionManager.CanCycleProduct;
            string cycleLabel = productProgressionManager.CanCycleProduct ? "Switch Product" : "Switch Product (Locked)";
            if (GUILayout.Button(cycleLabel, GUILayout.Height(24f)))
            {
                productProgressionManager.CycleProduct();
            }
            GUI.enabled = true;
            GUILayout.Label("Part: " + productProgressionManager.PartName, GUILayout.Width(180f));
            GUILayout.EndHorizontal();
        }

        private void DrawBoostControls()
        {
            if (factoryBoostManager == null)
            {
                return;
            }

            GUILayout.BeginHorizontal();
            GUI.enabled = !factoryBoostManager.IsBoostActive && !factoryBoostManager.IsOnCooldown;
            if (GUILayout.Button("Activate Boost", GUILayout.Height(24f)))
            {
                factoryBoostManager.TryActivateBoost();
            }
            GUI.enabled = true;
            GUILayout.Label("Boost: " + factoryBoostManager.StatusText, GUILayout.Width(170f));
            GUILayout.EndHorizontal();
        }

        private string GetOrderStatus()
        {
            if (orderCounter == null)
            {
                return "Missing Order Counter";
            }

            return orderCounter.PendingOrderCount
                + "/" + orderCounter.ActiveOrderCount
                + " units / Next "
                + orderCounter.NextOrderSummary
                + " / Dispatch "
                + GetDispatchStatus();
        }

        private string GetDispatchStatus()
        {
            if (pickupCounter == null)
            {
                return "Missing Pickup Counter";
            }

            return pickupCounter.CurrentDispatchLabel + " / Queue " + pickupCounter.QueuedShipmentCount;
        }

        private string GetWorkerText()
        {
            if (workerManager == null)
            {
                return "Missing Worker Manager";
            }

            return workerManager.WorkerCount
                + " / Speed x"
                + workerManager.WorkerMoveSpeed.ToString("0.00")
                + " / "
                + workerManager.WorkerRoleSummary;
        }

        private string GetFacilityText()
        {
            if (facilityManager == null)
            {
                return "Missing Facility Manager";
            }

            return "Assembly Benches "
                + facilityManager.ActiveAssemblyBenchCount
                + "/"
                + facilityManager.TotalAssemblyBenchCount
                + " / Packing "
                + (facilityManager.HasPackingStation ? "Online" : "Offline")
                + " / Dispatch Rack "
                + (facilityManager.HasDispatchRack ? "Online" : "Offline");
        }

        private string GetStageText()
        {
            if (stageGoalManager == null)
            {
                return "Missing Stage Goal Manager";
            }

            return stageGoalManager.CurrentStageLabel + " / " + stageGoalManager.GetProgressText();
        }

        private string GetRewardText()
        {
            if (stageGoalManager == null)
            {
                return "No stage rewards";
            }

            return "Basic x"
                + stageGoalManager.BasicBoxCount
                + " / Advanced x"
                + stageGoalManager.AdvancedBoxCount
                + " / Premium x"
                + stageGoalManager.PremiumBoxCount
                + " / "
                + stageGoalManager.LastRewardMessage;
        }

        private string GetProductionRewardText()
        {
            if (stageGoalManager == null)
            {
                return "No production rewards";
            }

            return stageGoalManager.GetProductionProgressText();
        }

        private string GetProductionRewardSummaryText()
        {
            if (stageGoalManager == null)
            {
                return "No production reward summary";
            }

            return stageGoalManager.GetProductionRewardSummaryText();
        }

        private AssemblyBench GetSelectedAssemblyBench()
        {
            if (selectionController == null || selectionController.CurrentSelection == null)
            {
                return null;
            }

            Component selectedComponent = selectionController.CurrentSelection as Component;
            return selectedComponent != null ? selectedComponent.GetComponent<AssemblyBench>() : null;
        }

        private void DrawSelectedAssemblyBenchControls()
        {
            AssemblyBench selectedBench = GetSelectedAssemblyBench();
            if (selectedBench == null)
            {
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Bench Priority: " + (selectedBench.IsPriorityFocus ? "On" : "Off"), GUILayout.Width(150f));
            string buttonLabel = selectedBench.IsPriorityFocus ? "Clear Priority" : "Set Priority";
            if (GUILayout.Button(buttonLabel, GUILayout.Height(24f)))
            {
                selectedBench.TogglePriorityFocus();
            }
            GUILayout.EndHorizontal();
        }

        private OrderCounter GetSelectedOrderCounter()
        {
            if (selectionController == null || selectionController.CurrentSelection == null)
            {
                return null;
            }

            Component selectedComponent = selectionController.CurrentSelection as Component;
            return selectedComponent != null ? selectedComponent.GetComponent<OrderCounter>() : null;
        }

        private void DrawSelectedOrderControls()
        {
            OrderCounter selectedOrderCounter = GetSelectedOrderCounter();
            if (selectedOrderCounter == null)
            {
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Order Focus: " + selectedOrderCounter.FocusModeLabel, GUILayout.Width(150f));
            if (GUILayout.Button("Cycle Focus", GUILayout.Height(24f)))
            {
                selectedOrderCounter.CycleFocusMode();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawEquipmentSection()
        {
            GUILayout.Label("Worker Gear");
            GUILayout.Label(equipmentManager != null ? equipmentManager.GetSlotSummary(EquipmentManager.EquipmentSlot.Head) : "Head: Empty");
            GUILayout.Label(equipmentManager != null ? equipmentManager.GetSlotSummary(EquipmentManager.EquipmentSlot.Body) : "Body: Empty");
            GUILayout.Label(equipmentManager != null ? equipmentManager.GetSlotSummary(EquipmentManager.EquipmentSlot.Tool) : "Tool: Empty");
            GUILayout.Label(equipmentManager != null ? "Equipped bonus: " + equipmentManager.GetEquippedBonusSummary() : "Equipped bonus: None");
            GUILayout.Label(equipmentManager != null ? equipmentManager.GetEpicSetSummary() : "Epic Set: Unknown");
            GUILayout.Label(equipmentManager != null ? equipmentManager.GetBasicBoxNextSummary() : "Basic next: Unknown");
            GUILayout.Label(equipmentManager != null ? equipmentManager.GetAdvancedBoxNextSummary() : "Advanced next: Unknown");
            GUILayout.Label(equipmentManager != null ? equipmentManager.GetPremiumBoxNextSummary() : "Premium next: Unknown");
            GUILayout.Label(equipmentManager != null ? equipmentManager.GetBlueprintProgressSummary() : "Epic Prep: Unknown");
            GUILayout.Label(equipmentManager != null ? equipmentManager.GetBlueprintReadySummary() : "Blueprint Ready 0/3");
            GUILayout.Label(equipmentManager != null ? equipmentManager.GetEpicCraftSummary(EquipmentManager.EquipmentSlot.Head) : "Head Epic: Unknown");
            GUILayout.Label(equipmentManager != null ? equipmentManager.GetEpicCraftSummary(EquipmentManager.EquipmentSlot.Body) : "Body Epic: Unknown");
            GUILayout.Label(equipmentManager != null ? equipmentManager.GetEpicCraftSummary(EquipmentManager.EquipmentSlot.Tool) : "Tool Epic: Unknown");

            GUI.enabled = equipmentManager != null && equipmentManager.BasicBoxCount > 0;
            string openBoxLabel = equipmentManager != null
                ? "Open Basic Box (" + equipmentManager.BasicBoxCount + ")"
                : "Open Basic Box";
            if (GUILayout.Button(openBoxLabel, GUILayout.Height(26f)))
            {
                equipmentManager.TryOpenBasicBox();
            }
            GUI.enabled = true;

            GUI.enabled = equipmentManager != null && equipmentManager.AdvancedBoxCount > 0;
            string openAdvancedBoxLabel = equipmentManager != null
                ? "Open Advanced Box (" + equipmentManager.AdvancedBoxCount + ")"
                : "Open Advanced Box";
            if (GUILayout.Button(openAdvancedBoxLabel, GUILayout.Height(26f)))
            {
                equipmentManager.TryOpenAdvancedBox();
            }
            GUI.enabled = true;

            GUI.enabled = equipmentManager != null && equipmentManager.PremiumBoxCount > 0;
            string openPremiumBoxLabel = equipmentManager != null
                ? "Open Premium Box (" + equipmentManager.PremiumBoxCount + ")"
                : "Open Premium Box";
            if (GUILayout.Button(openPremiumBoxLabel, GUILayout.Height(26f)))
            {
                equipmentManager.TryOpenPremiumBox();
            }
            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            GUI.enabled = equipmentManager != null && equipmentManager.CanCraftEpic(EquipmentManager.EquipmentSlot.Head);
            if (GUILayout.Button("Craft Head Epic", GUILayout.Height(24f)))
            {
                equipmentManager.TryCraftEpic(EquipmentManager.EquipmentSlot.Head);
            }

            GUI.enabled = equipmentManager != null && equipmentManager.CanCraftEpic(EquipmentManager.EquipmentSlot.Body);
            if (GUILayout.Button("Craft Body Epic", GUILayout.Height(24f)))
            {
                equipmentManager.TryCraftEpic(EquipmentManager.EquipmentSlot.Body);
            }

            GUI.enabled = equipmentManager != null && equipmentManager.CanCraftEpic(EquipmentManager.EquipmentSlot.Tool);
            if (GUILayout.Button("Craft Tool Epic", GUILayout.Height(24f)))
            {
                equipmentManager.TryCraftEpic(EquipmentManager.EquipmentSlot.Tool);
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            if (equipmentManager == null || equipmentManager.OwnedEquipmentGroupCount <= 0)
            {
                GUILayout.Label("Inventory: Empty");
            }
            else
            {
                GUILayout.Label(
                    "Inventory: "
                    + equipmentManager.OwnedEquipmentCount
                    + " items / "
                    + equipmentManager.OwnedEquipmentGroupCount
                    + " groups / Ready "
                    + equipmentManager.CombineReadyGroupCount);

                for (int i = 0; i < equipmentManager.OwnedEquipmentGroupCount; i++)
                {
                    int inventoryIndex = i;
                    GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical(GUILayout.Width(200f));
                    GUILayout.Label(equipmentManager.GetOwnedEquipmentGroupSummary(inventoryIndex));
                    GUILayout.Label(equipmentManager.GetOwnedEquipmentGroupPreview(inventoryIndex));
                    GUILayout.EndVertical();
                    GUI.enabled = equipmentManager.CanEquipOwnedEquipmentGroup(inventoryIndex);
                    if (GUILayout.Button("Equip", GUILayout.Width(60f), GUILayout.Height(42f)))
                    {
                        equipmentManager.TryEquipOwnedEquipmentGroup(inventoryIndex);
                    }
                    GUI.enabled = equipmentManager.CanCombineOwnedEquipmentGroup(inventoryIndex);
                    if (GUILayout.Button(equipmentManager.GetCombineButtonLabel(inventoryIndex), GUILayout.Width(70f), GUILayout.Height(42f)))
                    {
                        equipmentManager.TryCombineOwnedEquipmentGroup(inventoryIndex);
                    }
                    GUI.enabled = true;
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.Label(equipmentManager != null ? equipmentManager.LastMessage : "Equipment system missing");
        }

        private void DrawDevelopmentSection()
        {
            if (!showDevelopmentPanel)
            {
                return;
            }

            verificationStatus = BuildVerificationSummary();

            GUILayout.Label("Dev Boost");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+$100", GUILayout.Height(24f)))
            {
                moneyManager?.DebugAddMoney(100);
            }

            if (GUILayout.Button("+$1K", GUILayout.Height(24f)))
            {
                moneyManager?.DebugAddMoney(1000);
            }

            if (GUILayout.Button("+$10K", GUILayout.Height(24f)))
            {
                moneyManager?.DebugAddMoney(10000);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+1 Goal", GUILayout.Height(24f)))
            {
                stageGoalManager?.DebugAdvanceProductionGoals(1);
            }

            if (GUILayout.Button("+3 Goal", GUILayout.Height(24f)))
            {
                stageGoalManager?.DebugAdvanceProductionGoals(3);
            }

            if (GUILayout.Button("+6 Goal", GUILayout.Height(24f)))
            {
                stageGoalManager?.DebugAdvanceProductionGoals(6);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Run Recommended", GUILayout.Height(24f)))
            {
                RunRecommendedAction();
            }

            if (GUILayout.Button("One Shot", GUILayout.Height(24f)))
            {
                RunOneShotPreset();
            }

            if (GUILayout.Button("Jump All", GUILayout.Height(24f)))
            {
                RunFullFactoryPreset();
            }

            if (GUILayout.Button("Auto Fix", GUILayout.Height(24f)))
            {
                RunAutoFixPreset();
            }

            if (GUILayout.Button("Jump Rare", GUILayout.Height(24f)))
            {
                RunRarePreset();
            }

            if (GUILayout.Button("Jump Prem", GUILayout.Height(24f)))
            {
                RunPremiumPreset();
            }

            if (GUILayout.Button("Jump BP", GUILayout.Height(24f)))
            {
                RunBlueprintPreset();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+1 Basic", GUILayout.Height(24f)))
            {
                stageGoalManager?.DebugGrantBoxes(1, 0, 0);
            }

            if (GUILayout.Button("+1 Adv", GUILayout.Height(24f)))
            {
                stageGoalManager?.DebugGrantBoxes(0, 1, 0);
            }

            if (GUILayout.Button("+1 Prem", GUILayout.Height(24f)))
            {
                stageGoalManager?.DebugGrantBoxes(0, 0, 1);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Fix Factory", GUILayout.Height(24f)))
            {
                RunFactoryFix();
            }

            if (GUILayout.Button("Fix Rare", GUILayout.Height(24f)))
            {
                RunRareFix();
            }

            if (GUILayout.Button("Fix Prem", GUILayout.Height(24f)))
            {
                RunPremiumFix();
            }

            if (GUILayout.Button("Fix BP", GUILayout.Height(24f)))
            {
                RunBlueprintFix();
            }

            if (GUILayout.Button("Fix Loadout", GUILayout.Height(24f)))
            {
                RunLoadoutFix();
            }
            GUILayout.EndHorizontal();

            GUILayout.Label(GetDevelopmentSnapshot());
            GUILayout.Label(GetDevelopmentChecklistLine("Factory", IsFactoryPresetReady()));
            GUILayout.Label(GetDevelopmentChecklistDetail("Factory Need", GetFactoryChecklistDetail()));
            GUILayout.Label(GetDevelopmentChecklistLine("Rare", IsRareLoopReady()));
            GUILayout.Label(GetDevelopmentChecklistDetail("Rare Need", GetRareChecklistDetail()));
            GUILayout.Label(GetDevelopmentChecklistLine("Premium", IsPremiumLoopReady()));
            GUILayout.Label(GetDevelopmentChecklistDetail("Premium Need", GetPremiumChecklistDetail()));
            GUILayout.Label(GetDevelopmentChecklistLine("Blueprint", IsBlueprintPrepReady()));
            GUILayout.Label(GetDevelopmentChecklistDetail("Blueprint Need", GetBlueprintChecklistDetail()));
            GUILayout.Label(GetDevelopmentChecklistLine("Loadout", HasEquippedLoadout()));
            GUILayout.Label(GetDevelopmentChecklistDetail("Loadout Need", GetLoadoutChecklistDetail()));
            GUILayout.Label("Recommended: " + GetRecommendedActionLabel());
            GUILayout.Label("Why: " + GetRecommendedActionReason());
            GUILayout.Label(developmentStatus);
            GUILayout.Label(verificationStatus);
            DrawPlaytestFocusSection();
        }

        private void RunRarePreset()
        {
            moneyManager?.DebugAddMoney(2500);
            if (stageGoalManager != null)
            {
                int targetGoals = Mathf.Max(0, 18 - stageGoalManager.CompletedProductionGoalCount);
                stageGoalManager.DebugAdvanceProductionGoals(targetGoals);
            }

            if (equipmentManager != null)
            {
                equipmentManager.DebugOpenAllAvailableBoxes(false);
                equipmentManager.DebugCombineAllPossible();
                equipmentManager.DebugCraftAllReadyEpics();
                equipmentManager.DebugEquipBestLoadout();
            }

            developmentStatus = "Jump Rare applied.";
        }

        private void RunFactoryFix()
        {
            moneyManager?.DebugAddMoney(10000);

            if (workerManager != null)
            {
                int safety = 0;
                while (workerManager.WorkerCount < 3 && safety < 10)
                {
                    if (!workerManager.TryHireWorker())
                    {
                        break;
                    }

                    safety++;
                }

                safety = 0;
                while (workerManager.WorkerThroughputLevel < 5 && safety < 10)
                {
                    if (!workerManager.TryUpgradeWorkerThroughput())
                    {
                        break;
                    }

                    safety++;
                }
            }

            if (facilityManager != null)
            {
                int safety = 0;
                while (facilityManager.ActiveAssemblyBenchCount < 2 && facilityManager.CanBuildAssemblyBench && safety < 10)
                {
                    if (!facilityManager.TryBuildAssemblyBench())
                    {
                        break;
                    }

                    safety++;
                }

                if (facilityManager.CanBuildDispatchRack)
                {
                    facilityManager.TryBuildDispatchRack();
                }
            }

            if (productProgressionManager != null)
            {
                int safety = 0;
                while (productProgressionManager.ProductLevel < 6 && safety < 12)
                {
                    if (!upgradeManager.TryLevelUpProduct())
                    {
                        break;
                    }

                    safety++;
                }
            }

            developmentStatus = "Fix Factory applied.";
        }

        private void RunRareFix()
        {
            moneyManager?.DebugAddMoney(3000);
            if (stageGoalManager != null)
            {
                int rareGoalGap = Mathf.Max(0, 18 - stageGoalManager.CompletedProductionGoalCount);
                if (rareGoalGap > 0)
                {
                    stageGoalManager.DebugAdvanceProductionGoals(rareGoalGap);
                }
            }

            if (equipmentManager != null)
            {
                equipmentManager.DebugOpenAllAvailableBoxes(false);
                equipmentManager.DebugCombineAllPossible();
                equipmentManager.DebugCraftAllReadyEpics();
                equipmentManager.DebugEquipBestLoadout();
            }

            developmentStatus = "Fix Rare applied.";
        }

        private void RunPremiumFix()
        {
            moneyManager?.DebugAddMoney(5000);
            if (stageGoalManager != null)
            {
                if (stageGoalManager.CompletedProductionGoalCount < stageGoalManager.TotalProductionGoalCount)
                {
                    stageGoalManager.DebugAdvanceProductionGoals(stageGoalManager.TotalProductionGoalCount - stageGoalManager.CompletedProductionGoalCount);
                }
                else if (stageGoalManager.PremiumBoxCount <= 0 && equipmentManager != null && equipmentManager.OpenedPremiumBoxCount <= 0)
                {
                    stageGoalManager.DebugGrantBoxes(0, 0, 1);
                }
            }

            if (equipmentManager != null)
            {
                equipmentManager.DebugOpenAllAvailableBoxes(true);
                equipmentManager.DebugCombineAllPossible();
                equipmentManager.DebugCraftAllReadyEpics();
                equipmentManager.DebugEquipBestLoadout();
            }

            developmentStatus = "Fix Premium applied.";
        }

        private void RunBlueprintFix()
        {
            moneyManager?.DebugAddMoney(5000);
            if (equipmentManager != null && equipmentManager.BlueprintReadySlotCount <= 0)
            {
                stageGoalManager?.DebugGrantBoxes(0, 0, 3);
            }

            if (equipmentManager != null)
            {
                equipmentManager.DebugOpenAllAvailableBoxes(true);
                equipmentManager.DebugCombineAllPossible();
                equipmentManager.DebugCraftAllReadyEpics();
                equipmentManager.DebugEquipBestLoadout();
            }

            developmentStatus = "Fix Blueprint applied.";
        }

        private void RunLoadoutFix()
        {
            if (equipmentManager != null)
            {
                equipmentManager.DebugEquipBestLoadout();
            }

            developmentStatus = "Fix Loadout applied.";
        }

        private void RunFullFactoryPreset()
        {
            moneyManager?.DebugAddMoney(50000);

            if (workerManager != null)
            {
                int safety = 0;
                while (workerManager.WorkerCount < 3 && safety < 10)
                {
                    if (!workerManager.TryHireWorker())
                    {
                        break;
                    }

                    safety++;
                }

                safety = 0;
                while (workerManager.WorkerThroughputLevel < 5 && safety < 10)
                {
                    if (!workerManager.TryUpgradeWorkerThroughput())
                    {
                        break;
                    }

                    safety++;
                }
            }

            if (facilityManager != null)
            {
                int safety = 0;
                while (facilityManager.CanBuildAssemblyBench && safety < 10)
                {
                    if (!facilityManager.TryBuildAssemblyBench())
                    {
                        break;
                    }

                    safety++;
                }

                if (facilityManager.CanBuildDispatchRack)
                {
                    facilityManager.TryBuildDispatchRack();
                }
            }

            if (productProgressionManager != null)
            {
                int safety = 0;
                while (productProgressionManager.ProductLevel < 6 && safety < 12)
                {
                    if (!upgradeManager.TryLevelUpProduct())
                    {
                        break;
                    }

                    safety++;
                }
            }

            if (stageGoalManager != null)
            {
                int targetGoals = Mathf.Max(0, stageGoalManager.TotalProductionGoalCount - stageGoalManager.CompletedProductionGoalCount);
                stageGoalManager.DebugAdvanceProductionGoals(targetGoals);
            }

            if (equipmentManager != null)
            {
                equipmentManager.DebugOpenAllAvailableBoxes(true);
                equipmentManager.DebugCombineAllPossible();
                equipmentManager.DebugCraftAllReadyEpics();
                equipmentManager.DebugEquipBestLoadout();
            }

            developmentStatus = "Jump All applied.";
        }

        private void RunPremiumPreset()
        {
            moneyManager?.DebugAddMoney(10000);
            if (stageGoalManager != null)
            {
                int targetGoals = Mathf.Max(0, stageGoalManager.TotalProductionGoalCount - stageGoalManager.CompletedProductionGoalCount);
                stageGoalManager.DebugAdvanceProductionGoals(targetGoals);
            }

            if (equipmentManager != null)
            {
                equipmentManager.DebugOpenAllAvailableBoxes(true);
                equipmentManager.DebugCombineAllPossible();
                equipmentManager.DebugCraftAllReadyEpics();
                equipmentManager.DebugEquipBestLoadout();
            }

            developmentStatus = "Jump Prem applied.";
        }

        private void RunBlueprintPreset()
        {
            moneyManager?.DebugAddMoney(5000);
            stageGoalManager?.DebugGrantBoxes(0, 0, 3);
            if (equipmentManager != null)
            {
                equipmentManager.DebugOpenAllAvailableBoxes(true);
                equipmentManager.DebugCombineAllPossible();
                equipmentManager.DebugEquipBestLoadout();
            }

            developmentStatus = "Jump BP applied.";
        }

        private void RunAutoFixPreset()
        {
            moneyManager?.DebugAddMoney(25000);

            if (workerManager != null)
            {
                int safety = 0;
                while (workerManager.WorkerCount < 3 && safety < 10)
                {
                    if (!workerManager.TryHireWorker())
                    {
                        break;
                    }

                    safety++;
                }

                safety = 0;
                while (workerManager.WorkerThroughputLevel < 5 && safety < 10)
                {
                    if (!workerManager.TryUpgradeWorkerThroughput())
                    {
                        break;
                    }

                    safety++;
                }
            }

            if (facilityManager != null)
            {
                int safety = 0;
                while (facilityManager.CanBuildAssemblyBench && safety < 10)
                {
                    if (!facilityManager.TryBuildAssemblyBench())
                    {
                        break;
                    }

                    safety++;
                }

                if (facilityManager.CanBuildDispatchRack)
                {
                    facilityManager.TryBuildDispatchRack();
                }
            }

            if (productProgressionManager != null)
            {
                int safety = 0;
                while (productProgressionManager.ProductLevel < 6 && safety < 12)
                {
                    if (!upgradeManager.TryLevelUpProduct())
                    {
                        break;
                    }

                    safety++;
                }
            }

            if (stageGoalManager != null)
            {
                int rareGoalGap = Mathf.Max(0, 18 - stageGoalManager.CompletedProductionGoalCount);
                if (rareGoalGap > 0)
                {
                    stageGoalManager.DebugAdvanceProductionGoals(rareGoalGap);
                }

                if (stageGoalManager.PremiumBoxCount <= 0 && stageGoalManager.CompletedProductionGoalCount < stageGoalManager.TotalProductionGoalCount)
                {
                    stageGoalManager.DebugAdvanceProductionGoals(stageGoalManager.TotalProductionGoalCount - stageGoalManager.CompletedProductionGoalCount);
                }

                if (equipmentManager != null && equipmentManager.BlueprintReadySlotCount <= 0)
                {
                    stageGoalManager.DebugGrantBoxes(0, 0, 3);
                }
            }

            if (equipmentManager != null)
            {
                equipmentManager.DebugOpenAllAvailableBoxes(true);
                equipmentManager.DebugCombineAllPossible();
                equipmentManager.DebugEquipBestLoadout();
            }

            developmentStatus = IsFactoryPresetReady() && IsRareLoopReady() && IsPremiumLoopReady() && IsBlueprintPrepReady() && HasEquippedLoadout()
                ? "Auto Fix complete / PASS."
                : "Auto Fix ran / some checks still WAIT.";
            verificationStatus = BuildVerificationSummary();
        }

        private void RunOneShotPreset()
        {
            RunFullFactoryPreset();
            RunAutoFixPreset();
            developmentStatus = "One Shot complete.";
            verificationStatus = BuildVerificationSummary();
        }

        private void RunRecommendedAction()
        {
            string actionLabel = GetRecommendedActionLabel();
            string actionReason = GetRecommendedActionReason();

            if (actionLabel == "Done")
            {
                developmentStatus = "Recommended action: none needed.";
                verificationStatus = BuildVerificationSummary();
                return;
            }

            switch (actionLabel)
            {
                case "One Shot":
                    RunOneShotPreset();
                    break;
                case "Fix Factory":
                    RunFactoryFix();
                    break;
                case "Fix Rare":
                    RunRareFix();
                    break;
                case "Fix Premium":
                    RunPremiumFix();
                    break;
                case "Fix Blueprint":
                    RunBlueprintFix();
                    break;
                case "Fix Loadout":
                    RunLoadoutFix();
                    break;
                default:
                    developmentStatus = "Recommended action unavailable.";
                    break;
            }

            if (actionLabel != "One Shot")
            {
                developmentStatus = "Recommended action ran: " + actionLabel + " / " + actionReason + ".";
                verificationStatus = BuildVerificationSummary();
            }
        }

        private void DrawPlaytestFocusSection()
        {
            GUILayout.Space(6f);
            GUILayout.Label("Playtest Focus");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Mark Baseline", GUILayout.Height(24f)))
            {
                CapturePlaytestBaseline();
            }

            if (GUILayout.Button("Clear Baseline", GUILayout.Height(24f)))
            {
                ClearPlaytestBaseline();
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Current: " + BuildCurrentPlaytestSummary());
            GUILayout.Label("Delta: " + BuildPlaytestDeltaSummary());
            GUILayout.Label("Economy Feel: " + BuildEconomyFeelSummary());
            GUILayout.Label("Reward Feel: " + BuildRewardFeelSummary());
            GUILayout.Label("Gear Feel: " + BuildGearFeelSummary());
        }

        private string GetDevelopmentSnapshot()
        {
            string workerText = workerManager != null
                ? "Workers " + workerManager.WorkerCount + " / Throughput Lv " + workerManager.WorkerThroughputLevel
                : "Workers ?";
            string productText = productProgressionManager != null
                ? "Product Lv " + productProgressionManager.ProductLevel + " / $" + MoneyFormatter.Format(productProgressionManager.CurrentPickupValue)
                : "Product ?";
            string facilityText = facilityManager != null
                ? "Benches " + facilityManager.ActiveAssemblyBenchCount + "/" + facilityManager.TotalAssemblyBenchCount + " / Rack " + (facilityManager.HasDispatchRack ? "On" : "Off")
                : "Facilities ?";
            string rewardText = stageGoalManager != null
                ? "Boxes B" + stageGoalManager.BasicBoxCount + " A" + stageGoalManager.AdvancedBoxCount + " P" + stageGoalManager.PremiumBoxCount
                : "Boxes ?";
            string blueprintText = equipmentManager != null
                ? equipmentManager.GetBlueprintReadySummary()
                : "Blueprint Ready ?";

            return "Preset Snapshot / "
                + workerText
                + " / "
                + productText
                + " / "
                + facilityText
                + " / "
                + rewardText
                + " / "
                + blueprintText;
        }

        private bool IsFactoryPresetReady()
        {
            return workerManager != null
                && workerManager.WorkerCount >= 3
                && workerManager.WorkerThroughputLevel >= 5
                && facilityManager != null
                && facilityManager.ActiveAssemblyBenchCount >= 2
                && facilityManager.HasDispatchRack
                && productProgressionManager != null
                && productProgressionManager.ProductLevel >= 6;
        }

        private bool IsRareLoopReady()
        {
            return stageGoalManager != null
                && stageGoalManager.CompletedProductionGoalCount >= 18;
        }

        private bool IsPremiumLoopReady()
        {
            return equipmentManager != null
                && (equipmentManager.OpenedPremiumBoxCount > 0
                    || (stageGoalManager != null && stageGoalManager.PremiumBoxCount > 0));
        }

        private bool IsBlueprintPrepReady()
        {
            return equipmentManager != null
                && equipmentManager.BlueprintReadySlotCount > 0;
        }

        private bool HasEquippedLoadout()
        {
            return equipmentManager != null
                && !string.IsNullOrWhiteSpace(equipmentManager.GetEquippedDefinitionId(EquipmentManager.EquipmentSlot.Head))
                && !string.IsNullOrWhiteSpace(equipmentManager.GetEquippedDefinitionId(EquipmentManager.EquipmentSlot.Body))
                && !string.IsNullOrWhiteSpace(equipmentManager.GetEquippedDefinitionId(EquipmentManager.EquipmentSlot.Tool));
        }

        private static string GetDevelopmentChecklistLine(string label, bool passed)
        {
            return "Check " + label + ": " + (passed ? "PASS" : "WAIT");
        }

        private static string GetDevelopmentChecklistDetail(string label, string detail)
        {
            return label + ": " + detail;
        }

        private string BuildVerificationSummary()
        {
            bool factory = IsFactoryPresetReady();
            bool rare = IsRareLoopReady();
            bool premium = IsPremiumLoopReady();
            bool blueprint = IsBlueprintPrepReady();
            bool loadout = HasEquippedLoadout();
            bool passed = factory && rare && premium && blueprint && loadout;

            return "Verification: "
                + (passed ? "PASS" : "WAIT")
                + " / F:"
                + (factory ? "Y" : "N")
                + " R:"
                + (rare ? "Y" : "N")
                + " P:"
                + (premium ? "Y" : "N")
                + " B:"
                + (blueprint ? "Y" : "N")
                + " L:"
                + (loadout ? "Y" : "N");
        }

        private string GetRecommendedActionLabel()
        {
            bool factory = IsFactoryPresetReady();
            bool rare = IsRareLoopReady();
            bool premium = IsPremiumLoopReady();
            bool blueprint = IsBlueprintPrepReady();
            bool loadout = HasEquippedLoadout();

            if (!factory && !rare && !premium && !blueprint && !loadout)
            {
                return "One Shot";
            }

            if (!factory)
            {
                return "Fix Factory";
            }

            if (!rare)
            {
                return "Fix Rare";
            }

            if (!premium)
            {
                return "Fix Premium";
            }

            if (!blueprint)
            {
                return "Fix Blueprint";
            }

            if (!loadout)
            {
                return "Fix Loadout";
            }

            return "Done";
        }

        private string GetRecommendedActionReason()
        {
            string actionLabel = GetRecommendedActionLabel();
            switch (actionLabel)
            {
                case "One Shot":
                    return "Fresh bootstrap for factory, rewards, premium, blueprint, and loadout";
                case "Fix Factory":
                    return GetFactoryChecklistDetail();
                case "Fix Rare":
                    return GetRareChecklistDetail();
                case "Fix Premium":
                    return GetPremiumChecklistDetail();
                case "Fix Blueprint":
                    return GetBlueprintChecklistDetail();
                case "Fix Loadout":
                    return GetLoadoutChecklistDetail();
                default:
                    return "All checks passed";
            }
        }

        private void CapturePlaytestBaseline()
        {
            playtestBaseline = CaptureCurrentPlaytestSnapshot();
            hasPlaytestBaseline = true;
            developmentStatus = "Playtest baseline marked.";
        }

        private void ClearPlaytestBaseline()
        {
            hasPlaytestBaseline = false;
            developmentStatus = "Playtest baseline cleared.";
        }

        private PlaytestSnapshot CaptureCurrentPlaytestSnapshot()
        {
            return new PlaytestSnapshot
            {
                Money = moneyManager != null ? moneyManager.CurrentMoney : 0,
                CompletedOrders = pickupCounter != null ? pickupCounter.CompletedOrderCount : 0,
                ProductValue = productProgressionManager != null ? productProgressionManager.CurrentPickupValue : 0,
                RewardCount = stageGoalManager != null ? stageGoalManager.CompletedProductionGoalCount : 0,
                RemainingOrdersToReward = stageGoalManager != null ? stageGoalManager.RemainingOrdersToNextProductionGoal : 0,
                PremiumOpenedCount = equipmentManager != null ? equipmentManager.OpenedPremiumBoxCount : 0,
                LastShipmentPayout = pickupCounter != null ? pickupCounter.LastShipmentPayout : 0,
                WorkerMoveSpeed = workerManager != null ? workerManager.WorkerMoveSpeed : 0f,
                MoveMultiplier = supportBonusSlots != null ? supportBonusSlots.EquipmentMoveSpeedMultiplier : 1f,
                SaleMultiplier = supportBonusSlots != null ? supportBonusSlots.EquipmentSaleValueMultiplier : 1f,
                AssemblyMultiplier = supportBonusSlots != null ? supportBonusSlots.EquipmentAssemblySpeedMultiplier : 1f,
                LastAssemblySeconds = assemblyBench != null ? assemblyBench.LastCompletedAssemblySeconds : -1f
            };
        }

        private string BuildCurrentPlaytestSummary()
        {
            PlaytestSnapshot current = CaptureCurrentPlaytestSnapshot();
            return "$"
                + MoneyFormatter.Format(current.Money)
                + " / Payout $"
                + MoneyFormatter.Format(current.LastShipmentPayout)
                + " / Product $"
                + MoneyFormatter.Format(current.ProductValue)
                + " / Reward in "
                + current.RemainingOrdersToReward
                + " / Move "
                + current.WorkerMoveSpeed.ToString("0.00")
                + " / Gear "
                + FormatMultiplierTriplet(current.MoveMultiplier, current.SaleMultiplier, current.AssemblyMultiplier);
        }

        private string BuildPlaytestDeltaSummary()
        {
            if (!hasPlaytestBaseline)
            {
                return "Mark baseline to compare current run";
            }

            PlaytestSnapshot current = CaptureCurrentPlaytestSnapshot();
            return "Money +$"
                + MoneyFormatter.Format(current.Money - playtestBaseline.Money)
                + " / Dispatch +"
                + Mathf.Max(0, current.CompletedOrders - playtestBaseline.CompletedOrders)
                + " / Product +$"
                + MoneyFormatter.Format(current.ProductValue - playtestBaseline.ProductValue)
                + " / Rewards +"
                + Mathf.Max(0, current.RewardCount - playtestBaseline.RewardCount)
                + " / Premium +"
                + Mathf.Max(0, current.PremiumOpenedCount - playtestBaseline.PremiumOpenedCount);
        }

        private string BuildEconomyFeelSummary()
        {
            if (!hasPlaytestBaseline)
            {
                return "Mark baseline first";
            }

            PlaytestSnapshot current = CaptureCurrentPlaytestSnapshot();
            int moneyDelta = current.Money - playtestBaseline.Money;
            int dispatchDelta = Mathf.Max(0, current.CompletedOrders - playtestBaseline.CompletedOrders);
            int payoutDelta = current.LastShipmentPayout - playtestBaseline.LastShipmentPayout;
            int productDelta = current.ProductValue - playtestBaseline.ProductValue;
            string mood = payoutDelta >= 10 || productDelta >= 12 || moneyDelta >= 250
                ? "HOT"
                : (moneyDelta > 0 || payoutDelta > 0 || productDelta > 0 ? "BUILDING" : "FLAT");

            return mood
                + " / +$"
                + MoneyFormatter.Format(moneyDelta)
                + " over "
                + dispatchDelta
                + " dispatches"
                + " / payout +$"
                + MoneyFormatter.Format(payoutDelta)
                + " / product +$"
                + MoneyFormatter.Format(productDelta);
        }

        private string BuildRewardFeelSummary()
        {
            if (!hasPlaytestBaseline)
            {
                return "Mark baseline first";
            }

            PlaytestSnapshot current = CaptureCurrentPlaytestSnapshot();
            int rewardDelta = Mathf.Max(0, current.RewardCount - playtestBaseline.RewardCount);
            int premiumDelta = Mathf.Max(0, current.PremiumOpenedCount - playtestBaseline.PremiumOpenedCount);
            int rewardDistanceDrop = Mathf.Max(0, playtestBaseline.RemainingOrdersToReward - current.RemainingOrdersToReward);
            string mood = premiumDelta > 0 || rewardDelta >= 2
                ? "HOT"
                : (rewardDelta > 0 || rewardDistanceDrop > 0 ? "BUILDING" : "FLAT");

            return mood
                + " / rewards +"
                + rewardDelta
                + " / premium +"
                + premiumDelta
                + " / next reward in "
                + current.RemainingOrdersToReward
                + " (down "
                + rewardDistanceDrop
                + ")";
        }

        private string BuildGearFeelSummary()
        {
            if (!hasPlaytestBaseline)
            {
                return "Mark baseline first";
            }

            PlaytestSnapshot current = CaptureCurrentPlaytestSnapshot();
            float moveDelta = current.MoveMultiplier - playtestBaseline.MoveMultiplier;
            float saleDelta = current.SaleMultiplier - playtestBaseline.SaleMultiplier;
            float assemblyDelta = current.AssemblyMultiplier - playtestBaseline.AssemblyMultiplier;
            float bestDelta = Mathf.Max(moveDelta, Mathf.Max(saleDelta, assemblyDelta));
            string mood = bestDelta >= 0.08f
                ? "HOT"
                : (bestDelta > 0f ? "BUILDING" : "FLAT");

            return mood
                + " / "
                + FormatMultiplierTriplet(current.MoveMultiplier, current.SaleMultiplier, current.AssemblyMultiplier)
                + " / assembly "
                + FormatAssemblyDelta(playtestBaseline.LastAssemblySeconds, current.LastAssemblySeconds);
        }

        private static string FormatMultiplierTriplet(float moveMultiplier, float saleMultiplier, float assemblyMultiplier)
        {
            return "M "
                + moveMultiplier.ToString("0.00")
                + " / S "
                + saleMultiplier.ToString("0.00")
                + " / A "
                + assemblyMultiplier.ToString("0.00");
        }

        private static string FormatAssemblyDelta(float baselineSeconds, float currentSeconds)
        {
            if (baselineSeconds <= 0f || currentSeconds <= 0f)
            {
                return "n/a";
            }

            float delta = baselineSeconds - currentSeconds;
            string sign = delta >= 0f ? "-" : "+";
            return currentSeconds.ToString("0.00") + "s (" + sign + Mathf.Abs(delta).ToString("0.00") + "s)";
        }

        private string GetFactoryChecklistDetail()
        {
            if (IsFactoryPresetReady())
            {
                return "Ready";
            }

            if (workerManager == null || facilityManager == null || productProgressionManager == null)
            {
                return "Missing manager reference";
            }

            return "Workers "
                + workerManager.WorkerCount
                + "/3"
                + " / Throughput "
                + workerManager.WorkerThroughputLevel
                + "/5"
                + " / Benches "
                + facilityManager.ActiveAssemblyBenchCount
                + "/2"
                + " / Rack "
                + (facilityManager.HasDispatchRack ? "1/1" : "0/1")
                + " / Product Lv "
                + productProgressionManager.ProductLevel
                + "/6";
        }

        private string GetRareChecklistDetail()
        {
            if (IsRareLoopReady())
            {
                return "Ready";
            }

            if (stageGoalManager == null)
            {
                return "Missing stage goal manager";
            }

            return "Goals "
                + stageGoalManager.CompletedProductionGoalCount
                + "/18"
                + " / Next "
                + stageGoalManager.CurrentProductionGoalLabel;
        }

        private string GetPremiumChecklistDetail()
        {
            if (IsPremiumLoopReady())
            {
                return "Ready";
            }

            if (equipmentManager == null)
            {
                return "Missing equipment manager";
            }

            int queuedPremiumBoxes = stageGoalManager != null ? stageGoalManager.PremiumBoxCount : 0;
            return "Opened "
                + equipmentManager.OpenedPremiumBoxCount
                + "/1"
                + " / Box queue "
                + queuedPremiumBoxes
                + " / Goals "
                + (stageGoalManager != null ? stageGoalManager.CompletedProductionGoalCount.ToString() : "0")
                + "/"
                + (stageGoalManager != null ? stageGoalManager.TotalProductionGoalCount.ToString() : "?");
        }

        private string GetBlueprintChecklistDetail()
        {
            if (IsBlueprintPrepReady())
            {
                return "Ready";
            }

            if (equipmentManager == null)
            {
                return "Missing equipment manager";
            }

            return equipmentManager.GetBlueprintReadySummary() + " / " + equipmentManager.GetBlueprintProgressSummary();
        }

        private string GetLoadoutChecklistDetail()
        {
            if (HasEquippedLoadout())
            {
                return "Ready";
            }

            if (equipmentManager == null)
            {
                return "Missing equipment manager";
            }

            List<string> missingSlots = new List<string>();
            if (string.IsNullOrWhiteSpace(equipmentManager.GetEquippedDefinitionId(EquipmentManager.EquipmentSlot.Head)))
            {
                missingSlots.Add("Head");
            }

            if (string.IsNullOrWhiteSpace(equipmentManager.GetEquippedDefinitionId(EquipmentManager.EquipmentSlot.Body)))
            {
                missingSlots.Add("Body");
            }

            if (string.IsNullOrWhiteSpace(equipmentManager.GetEquippedDefinitionId(EquipmentManager.EquipmentSlot.Tool)))
            {
                missingSlots.Add("Tool");
            }

            return missingSlots.Count > 0
                ? "Missing " + string.Join(", ", missingSlots)
                : "Ready";
        }

        private string GetHireWorkerLabel()
        {
            if (workerManager != null && workerManager.WorkerCount < 2)
            {
                return "Hire Worker - Goal";
            }

            return "Hire Worker";
        }

        private void DrawRecommendedGoal()
        {
            string title = "Goal: Build cash for first worker";
            string detail = "Worker system missing";
            float progress = 0f;

            if (workerManager != null && moneyManager != null)
            {
                if (workerManager.WorkerCount < 2)
                {
                    int cost = workerManager.HireWorkerCost;
                    progress = cost > 0 ? Mathf.Clamp01((float)moneyManager.CurrentMoney / cost) : 1f;
                    detail = MoneyFormatter.Format(moneyManager.CurrentMoney) + " / " + MoneyFormatter.Format(cost) + " for Hire Worker";
                }
                else if (productProgressionManager != null && productProgressionManager.ProductLevel < 2)
                {
                    int cost = productProgressionManager.LevelUpCost;
                    progress = cost > 0 ? Mathf.Clamp01((float)moneyManager.CurrentMoney / cost) : 1f;
                    title = "Goal: Level " + productProgressionManager.ProductName + " to Lv 2";
                    detail = MoneyFormatter.Format(moneyManager.CurrentMoney) + " / " + MoneyFormatter.Format(cost) + " for Product Lv 2";
                }
                else if (productProgressionManager != null && productProgressionManager.ProductLevel < 3)
                {
                    int cost = productProgressionManager.LevelUpCost;
                    progress = cost > 0 ? Mathf.Clamp01((float)moneyManager.CurrentMoney / cost) : 1f;
                    title = "Goal: Level " + productProgressionManager.ProductName + " to Lv 3";
                    detail = MoneyFormatter.Format(moneyManager.CurrentMoney) + " / " + MoneyFormatter.Format(cost) + " for Product Lv 3";
                }
                else if (facilityManager != null && facilityManager.CanBuildAssemblyBench)
                {
                    int cost = facilityManager.BuildAssemblyBenchCost;
                    progress = cost > 0 ? Mathf.Clamp01((float)moneyManager.CurrentMoney / cost) : 1f;
                    title = "Goal: Build second assembly bench";
                    detail = MoneyFormatter.Format(moneyManager.CurrentMoney) + " / " + MoneyFormatter.Format(cost) + " for Assembly Bench";
                }
                else if (workerManager != null)
                {
                    int cost = workerManager.WorkerThroughputCost;
                    progress = cost > 0 ? Mathf.Clamp01((float)moneyManager.CurrentMoney / cost) : 1f;
                    title = "Goal: Improve worker throughput";
                    detail = MoneyFormatter.Format(moneyManager.CurrentMoney) + " / " + MoneyFormatter.Format(cost) + " for Worker Throughput";
                }
                else if (facilityManager != null && facilityManager.CanBuildDispatchRack)
                {
                    int cost = facilityManager.BuildDispatchRackCost;
                    progress = cost > 0 ? Mathf.Clamp01((float)moneyManager.CurrentMoney / cost) : 1f;
                    title = "Goal: Build dispatch rack";
                    detail = MoneyFormatter.Format(moneyManager.CurrentMoney) + " / " + MoneyFormatter.Format(cost) + " for Dispatch Rack";
                }
            }

            GUILayout.Label(title);
            Rect barRect = GUILayoutUtility.GetRect(1f, 12f, GUILayout.ExpandWidth(true));
            GUI.Box(barRect, GUIContent.none);
            Rect fillRect = new Rect(barRect.x + 2f, barRect.y + 2f, Mathf.Max(0f, (barRect.width - 4f) * progress), barRect.height - 4f);
            Color oldColor = GUI.color;
            GUI.color = new Color(0.35f, 0.8f, 0.55f, 1f);
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
            GUI.color = oldColor;
            GUILayout.Label(detail);
        }

        private void HandlePickupCompleted(int amount, string orderLabel, Vector3 worldPosition)
        {
            string feedbackText = "+$" + MoneyFormatter.Format(amount);
            if (!string.Equals(orderLabel, "Standard", System.StringComparison.Ordinal))
            {
                feedbackText += " " + orderLabel;
            }

            pickupFeedbacks.Add(new PickupFeedback
            {
                Text = feedbackText,
                WorldPosition = worldPosition + Vector3.up * 0.8f,
                StartTime = Time.time,
                EndTime = Time.time + pickupFeedbackSeconds
            });
        }

        private void DrawPickupFeedbacks()
        {
            if (pickupFeedbacks.Count == 0)
            {
                return;
            }

            if (feedbackStyle == null)
            {
                feedbackStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 18,
                    fontStyle = FontStyle.Bold
                };
            }

            Camera feedbackCamera = Camera.main;
            for (int i = pickupFeedbacks.Count - 1; i >= 0; i--)
            {
                PickupFeedback feedback = pickupFeedbacks[i];
                if (Time.time >= feedback.EndTime)
                {
                    pickupFeedbacks.RemoveAt(i);
                    continue;
                }

                float normalizedAge = Mathf.InverseLerp(feedback.StartTime, feedback.EndTime, Time.time);
                Vector3 screenPosition = feedbackCamera != null
                    ? feedbackCamera.WorldToScreenPoint(feedback.WorldPosition)
                    : new Vector3(panelRect.x + panelRect.width * 0.5f, Screen.height - panelRect.y - 36f, 1f);

                if (screenPosition.z < 0f)
                {
                    continue;
                }

                float yOffset = normalizedAge * 32f;
                Rect feedbackRect = new Rect(screenPosition.x - 50f, Screen.height - screenPosition.y - 20f - yOffset, 100f, 28f);
                Color oldColor = GUI.color;
                feedbackStyle.normal.textColor = new Color(0.25f, 0.95f, 0.48f, 1f - normalizedAge);
                GUI.Label(feedbackRect, feedback.Text, feedbackStyle);
                GUI.color = oldColor;
            }
        }
    }
}
