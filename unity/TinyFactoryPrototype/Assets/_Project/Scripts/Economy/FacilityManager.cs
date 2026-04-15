using System.Collections.Generic;
using TinyFactory.Stations;
using TinyFactory.Workers;
using UnityEngine;

namespace TinyFactory.Economy
{
    public sealed class FacilityManager : MonoBehaviour
    {
        [SerializeField] private MoneyManager moneyManager;
        [SerializeField] private WorkerManager workerManager;
        [SerializeField] private AssemblyBench[] assemblyBenches;
        [SerializeField] private int assemblyBenchBaseCost = 45;
        [SerializeField] private float assemblyBenchCostGrowth = 1.7f;
        [SerializeField] private string lastMessage = "Facilities ready";

        public int ActiveAssemblyBenchCount => CountActiveAssemblyBenches();
        public int TotalAssemblyBenchCount => assemblyBenches != null ? assemblyBenches.Length : 0;
        public bool CanBuildAssemblyBench => ActiveAssemblyBenchCount < TotalAssemblyBenchCount;
        public int BuildAssemblyBenchCost => UpgradeCostCalculator.Calculate(assemblyBenchBaseCost, assemblyBenchCostGrowth, ActiveAssemblyBenchCount);
        public string LastMessage => lastMessage;

        private void Awake()
        {
            ResolveReferences();
            RefreshAssemblyBenches();
            PublishAssemblyBenches();
        }

        public bool TryBuildAssemblyBench()
        {
            RefreshAssemblyBenches();
            if (!CanBuildAssemblyBench)
            {
                lastMessage = "All assembly benches are online.";
                return false;
            }

            int cost = BuildAssemblyBenchCost;
            if (moneyManager == null || !moneyManager.TrySpend(cost))
            {
                lastMessage = "Not enough money. Need " + MoneyFormatter.Format(cost) + ".";
                return false;
            }

            AssemblyBench bench = GetFirstInactiveAssemblyBench();
            if (bench == null)
            {
                lastMessage = "No locked assembly bench found.";
                return false;
            }

            bench.gameObject.SetActive(true);
            PublishAssemblyBenches();
            lastMessage = "Built Assembly Bench " + ActiveAssemblyBenchCount + ".";
            return true;
        }

        public AssemblyBench[] GetActiveAssemblyBenches()
        {
            RefreshAssemblyBenches();
            List<AssemblyBench> activeBenches = new List<AssemblyBench>();
            for (int i = 0; i < assemblyBenches.Length; i++)
            {
                if (assemblyBenches[i] != null && assemblyBenches[i].gameObject.activeInHierarchy)
                {
                    activeBenches.Add(assemblyBenches[i]);
                }
            }

            return activeBenches.ToArray();
        }

        private void ResolveReferences()
        {
            if (moneyManager == null)
            {
                moneyManager = FindFirstObjectByType<MoneyManager>();
            }

            if (workerManager == null)
            {
                workerManager = FindFirstObjectByType<WorkerManager>();
            }
        }

        private void RefreshAssemblyBenches()
        {
            if (assemblyBenches != null && assemblyBenches.Length > 0)
            {
                return;
            }

            AssemblyBench[] allBenches = Resources.FindObjectsOfTypeAll<AssemblyBench>();
            List<AssemblyBench> sceneBenches = new List<AssemblyBench>();
            for (int i = 0; i < allBenches.Length; i++)
            {
                AssemblyBench bench = allBenches[i];
                if (bench == null || string.IsNullOrEmpty(bench.gameObject.scene.name))
                {
                    continue;
                }

                sceneBenches.Add(bench);
            }

            sceneBenches.Sort((left, right) => string.Compare(left.name, right.name, System.StringComparison.Ordinal));
            assemblyBenches = sceneBenches.ToArray();
        }

        private int CountActiveAssemblyBenches()
        {
            RefreshAssemblyBenches();
            int count = 0;
            for (int i = 0; i < assemblyBenches.Length; i++)
            {
                if (assemblyBenches[i] != null && assemblyBenches[i].gameObject.activeInHierarchy)
                {
                    count++;
                }
            }

            return count;
        }

        private AssemblyBench GetFirstInactiveAssemblyBench()
        {
            for (int i = 0; i < assemblyBenches.Length; i++)
            {
                if (assemblyBenches[i] != null && !assemblyBenches[i].gameObject.activeSelf)
                {
                    return assemblyBenches[i];
                }
            }

            return null;
        }

        private void PublishAssemblyBenches()
        {
            if (workerManager != null)
            {
                workerManager.SetAssemblyBenches(GetActiveAssemblyBenches());
            }
        }
    }
}
