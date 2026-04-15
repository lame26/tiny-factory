using UnityEngine;

namespace TinyFactory.Economy
{
    public static class UpgradeCostCalculator
    {
        public static int Calculate(int baseCost, float growthRate, int currentLevel)
        {
            int safeLevel = Mathf.Max(1, currentLevel);
            float safeGrowthRate = Mathf.Max(1f, growthRate);
            double rawCost = baseCost * System.Math.Pow(safeGrowthRate, safeLevel - 1);

            if (rawCost >= int.MaxValue)
            {
                return int.MaxValue;
            }

            return Mathf.Max(1, Mathf.CeilToInt((float)rawCost));
        }
    }
}
