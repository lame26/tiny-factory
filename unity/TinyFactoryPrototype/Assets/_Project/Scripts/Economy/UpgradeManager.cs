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
        [SerializeField] private int stationLevelBaseCost = 10;
        [SerializeField] private float stationLevelCostGrowth = 1.32f;
        [SerializeField] private int assemblySpeedBaseCost = 15;
        [SerializeField] private float assemblySpeedCostGrowth = 1.45f;
        [SerializeField] private int saleValueBaseCost = 20;
        [SerializeField] private float saleValueCostGrowth = 1.45f;
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

                return UpgradeCostCalculator.Calculate(stationLevelBaseCost, stationLevelCostGrowth, 1);
            }
        }

        public int AssemblySpeedCost => UpgradeCostCalculator.Calculate(assemblySpeedBaseCost, assemblySpeedCostGrowth, assemblyBench != null ? assemblyBench.AssemblySpeedLevel : 1);
        public int SaleValueCost => UpgradeCostCalculator.Calculate(saleValueBaseCost, saleValueCostGrowth, pickupCounter != null ? pickupCounter.SaleValueLevel : 1);
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
            if (!TrySpend(SaleValueCost))
            {
                return false;
            }

            pickupCounter.UpgradeSaleValue();
            lastMessage = "Pickup value upgraded. Next: " + MoneyFormatter.Format(SaleValueCost) + ".";
            return true;
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
