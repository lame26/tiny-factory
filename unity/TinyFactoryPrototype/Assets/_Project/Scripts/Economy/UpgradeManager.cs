using TinyFactory.Interaction;
using TinyFactory.Stations;
using UnityEngine;

namespace TinyFactory.Economy
{
    public sealed class UpgradeManager : MonoBehaviour
    {
        [SerializeField] private MoneyManager moneyManager;
        [SerializeField] private StationSelectionController selectionController;
        [SerializeField] private AssemblyBench assemblyBench;
        [SerializeField] private PickupCounter pickupCounter;
        [SerializeField] private ProductProgressionManager productProgressionManager;
        [SerializeField] private int stationLevelBaseCost = 8;
        [SerializeField] private float stationLevelCostGrowth = 1.22f;
        [SerializeField] private int assemblySpeedBaseCost = 12;
        [SerializeField] private float assemblySpeedCostGrowth = 1.32f;
        [SerializeField] private string lastMessage = "Upgrades ready";

        public int StationLevelCost
        {
            get
            {
                if (selectionController == null || selectionController.CurrentSelection == null)
                {
                    return UpgradeCostCalculator.Calculate(stationLevelBaseCost, stationLevelCostGrowth, 1);
                }

                AssemblyBench selectedAssemblyBench = GetSelectedComponent<AssemblyBench>();
                if (selectedAssemblyBench != null)
                {
                    return UpgradeCostCalculator.Calculate(stationLevelBaseCost, stationLevelCostGrowth, selectedAssemblyBench.StationLevel);
                }

                PickupCounter selectedPickupCounter = GetSelectedComponent<PickupCounter>();
                if (selectedPickupCounter != null)
                {
                    return UpgradeCostCalculator.Calculate(stationLevelBaseCost, stationLevelCostGrowth, selectedPickupCounter.StationLevel);
                }

                PackingStation selectedPackingStation = GetSelectedComponent<PackingStation>();
                if (selectedPackingStation != null)
                {
                    return UpgradeCostCalculator.Calculate(stationLevelBaseCost, stationLevelCostGrowth, selectedPackingStation.StationLevel);
                }

                return UpgradeCostCalculator.Calculate(stationLevelBaseCost, stationLevelCostGrowth, 1);
            }
        }

        public int AssemblySpeedCost => UpgradeCostCalculator.Calculate(assemblySpeedBaseCost, assemblySpeedCostGrowth, assemblyBench != null ? assemblyBench.AssemblySpeedLevel : 1);
        public int ProductLevelCost => productProgressionManager != null ? productProgressionManager.LevelUpCost : 0;
        public int SaleValueCost => ProductLevelCost;
        public string LastMessage => lastMessage;

        private void Awake()
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

            if (productProgressionManager == null)
            {
                productProgressionManager = ProductProgressionManager.GetOrCreate();
            }
        }

        public bool TryUpgradeSelectedStation()
        {
            if (selectionController == null || selectionController.CurrentSelection == null)
            {
                lastMessage = "Select Assembly Bench or Pickup Counter first.";
                return false;
            }

            AssemblyBench selectedAssemblyBench = GetSelectedComponent<AssemblyBench>();
            if (selectedAssemblyBench != null)
            {
                int cost = UpgradeCostCalculator.Calculate(stationLevelBaseCost, stationLevelCostGrowth, selectedAssemblyBench.StationLevel);
                if (!TrySpend(cost))
                {
                    return false;
                }

                selectedAssemblyBench.UpgradeStationLevel();
                lastMessage = selectionController.CurrentSelectionName + " leveled up. Next: " + MoneyFormatter.Format(StationLevelCost) + ".";
                return true;
            }

            PickupCounter selectedPickupCounter = GetSelectedComponent<PickupCounter>();
            if (selectedPickupCounter != null)
            {
                int cost = UpgradeCostCalculator.Calculate(stationLevelBaseCost, stationLevelCostGrowth, selectedPickupCounter.StationLevel);
                if (!TrySpend(cost))
                {
                    return false;
                }

                selectedPickupCounter.UpgradeStationLevel();
                lastMessage = "Pickup Counter leveled up. Next: " + MoneyFormatter.Format(StationLevelCost) + ".";
                return true;
            }

            PackingStation selectedPackingStation = GetSelectedComponent<PackingStation>();
            if (selectedPackingStation != null)
            {
                int cost = UpgradeCostCalculator.Calculate(stationLevelBaseCost, stationLevelCostGrowth, selectedPackingStation.StationLevel);
                if (!TrySpend(cost))
                {
                    return false;
                }

                selectedPackingStation.UpgradeStationLevel();
                lastMessage = "Packing Station leveled up. Next: " + MoneyFormatter.Format(StationLevelCost) + ".";
                return true;
            }

            string selectedName = selectionController.CurrentSelectionName;
            lastMessage = selectedName + " has no station level upgrade yet.";
            return false;
        }

        public bool TryUpgradeAssemblySpeed()
        {
            if (!TrySpend(AssemblySpeedCost))
            {
                return false;
            }

            assemblyBench.UpgradeAssemblySpeed();
            lastMessage = "Assembly speed upgraded. Next: " + MoneyFormatter.Format(AssemblySpeedCost) + ".";
            return true;
        }

        public bool TryUpgradeSaleValue()
        {
            return TryLevelUpProduct();
        }

        public bool TryLevelUpProduct()
        {
            if (productProgressionManager == null)
            {
                lastMessage = "Product progression missing.";
                return false;
            }

            bool upgraded = productProgressionManager.TryLevelUpProduct();
            lastMessage = productProgressionManager.LastMessage;
            return upgraded;
        }

        private bool TrySpend(int cost)
        {
            if (moneyManager == null || !moneyManager.TrySpend(cost))
            {
                lastMessage = "Not enough money. Need " + MoneyFormatter.Format(cost) + ".";
                return false;
            }

            return true;
        }

        private T GetSelectedComponent<T>() where T : Component
        {
            if (selectionController == null || selectionController.CurrentSelection == null)
            {
                return null;
            }

            Component selectedComponent = selectionController.CurrentSelection as Component;
            return selectedComponent != null ? selectedComponent.GetComponent<T>() : null;
        }
    }
}
