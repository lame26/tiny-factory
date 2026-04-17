using System;
using UnityEngine;

namespace TinyFactory.Economy
{
    public sealed class ProductProgressionManager : MonoBehaviour
    {
        private readonly struct ProductDefinition
        {
            public ProductDefinition(string productName, string partName, string packedProductName, int unlockLevel, int basePickupValue)
            {
                ProductName = productName;
                PartName = partName;
                PackedProductName = packedProductName;
                UnlockLevel = unlockLevel;
                BasePickupValue = basePickupValue;
            }

            public string ProductName { get; }
            public string PartName { get; }
            public string PackedProductName { get; }
            public int UnlockLevel { get; }
            public int BasePickupValue { get; }
        }

        private static readonly ProductDefinition[] ProductDefinitions =
        {
            new ProductDefinition("Power Bank", "Power Cell Part", "Packed Power Bank", 1, 6),
            new ProductDefinition("Mini Fan", "Mini Fan Part", "Packed Mini Fan", 4, 11),
            new ProductDefinition("Smart Watch", "Smart Watch Part", "Packed Smart Watch", 8, 18)
        };

        [SerializeField] private MoneyManager moneyManager;
        [SerializeField] private int productLevel = 1;
        [SerializeField] private float valueGrowth = 1.52f;
        [SerializeField] private int levelUpBaseCost = 8;
        [SerializeField] private float levelUpCostGrowth = 1.45f;
        [SerializeField] private int selectedProductIndex;
        [SerializeField] private string lastMessage = "Product progression ready";

        private static ProductProgressionManager s_runtimeInstance;

        public event Action<int> ProductLevelChanged;

        public string ProductName => CurrentDefinition.ProductName;
        public string PartName => CurrentDefinition.PartName;
        public string PackedProductName => CurrentDefinition.PackedProductName;
        public int ProductLevel => Mathf.Max(1, productLevel);
        public int CurrentPickupValue => EvaluateValue(CurrentDefinition.BasePickupValue, valueGrowth, ProductLevel);
        public int LevelUpCost => UpgradeCostCalculator.Calculate(levelUpBaseCost, levelUpCostGrowth, ProductLevel);
        public string LastMessage => lastMessage;
        public int UnlockedProductCount => GetUnlockedProductCount();
        public bool CanCycleProduct => UnlockedProductCount > 1;

        private ProductDefinition CurrentDefinition => ProductDefinitions[Mathf.Clamp(selectedProductIndex, 0, Mathf.Max(0, UnlockedProductCount - 1))];

        public static ProductProgressionManager GetOrCreate()
        {
            ProductProgressionManager existing = FindFirstObjectByType<ProductProgressionManager>();
            if (existing != null)
            {
                return existing;
            }

            if (s_runtimeInstance != null)
            {
                return s_runtimeInstance;
            }

            GameObject runtimeObject = new GameObject("ProductProgressionManager");
            s_runtimeInstance = runtimeObject.AddComponent<ProductProgressionManager>();
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

        private void OnDestroy()
        {
            if (s_runtimeInstance == this)
            {
                s_runtimeInstance = null;
            }
        }

        public bool TryLevelUpProduct()
        {
            ResolveReferences();

            int cost = LevelUpCost;
            if (moneyManager == null || !moneyManager.TrySpend(cost))
            {
                lastMessage = "Not enough money. Need " + MoneyFormatter.Format(cost) + ".";
                return false;
            }

            productLevel = ProductLevel + 1;
            int unlockedCount = UnlockedProductCount;
            int maxSelectableIndex = Mathf.Max(0, unlockedCount - 1);
            selectedProductIndex = Mathf.Clamp(selectedProductIndex, 0, maxSelectableIndex);
            ProductDefinition newestDefinition = ProductDefinitions[maxSelectableIndex];
            bool unlockedNewProduct = newestDefinition.UnlockLevel == ProductLevel && maxSelectableIndex > 0;
            lastMessage = ProductName + " reached Lv " + ProductLevel + ". Pickup value " + MoneyFormatter.Format(CurrentPickupValue) + ".";
            if (unlockedNewProduct)
            {
                lastMessage += " New product unlocked: " + newestDefinition.ProductName + ".";
            }
            ProductLevelChanged?.Invoke(ProductLevel);
            return true;
        }

        public bool CycleProduct()
        {
            int unlockedCount = UnlockedProductCount;
            if (unlockedCount <= 1)
            {
                lastMessage = "No alternate product unlocked yet.";
                return false;
            }

            selectedProductIndex = (selectedProductIndex + 1) % unlockedCount;
            lastMessage = "Switched production to " + ProductName + ". Pickup value " + MoneyFormatter.Format(CurrentPickupValue) + ".";
            return true;
        }

        private void ResolveReferences()
        {
            if (moneyManager == null)
            {
                moneyManager = FindFirstObjectByType<MoneyManager>();
            }
        }

        private static int EvaluateValue(int baseValue, float growthRate, int level)
        {
            int safeBaseValue = Mathf.Max(1, baseValue);
            float safeGrowthRate = Mathf.Max(1f, growthRate);
            int safeLevel = Mathf.Max(1, level);
            double rawValue = safeBaseValue * Math.Pow(safeGrowthRate, safeLevel - 1);

            if (rawValue >= int.MaxValue)
            {
                return int.MaxValue;
            }

            return Mathf.Max(1, Mathf.RoundToInt((float)rawValue));
        }

        private int GetUnlockedProductCount()
        {
            int unlockedCount = 0;
            for (int i = 0; i < ProductDefinitions.Length; i++)
            {
                if (ProductLevel >= ProductDefinitions[i].UnlockLevel)
                {
                    unlockedCount++;
                }
            }

            return Mathf.Max(1, unlockedCount);
        }
    }
}
