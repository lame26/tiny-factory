using System.Collections.Generic;
using TinyFactory.Stations;
using TinyFactory.Workers;
using UnityEngine;

namespace TinyFactory.Economy
{
    public sealed class FacilityManager : MonoBehaviour
    {
        private const int DefaultDispatchRackCost = 8;
        private const int DefaultDispatchRackOrderCapacityBonus = 1;
        private const float DefaultDispatchRackOrderIntervalMultiplier = 0.78f;
        private const float DefaultDispatchRackSpeedMultiplier = 0.68f;
        private const int DefaultDispatchRackBonusPerShipment = 6;
        private const int DefaultPackingStationCost = 52;

        [SerializeField] private MoneyManager moneyManager;
        [SerializeField] private WorkerManager workerManager;
        [SerializeField] private OrderCounter orderCounter;
        [SerializeField] private PickupCounter pickupCounter;
        [SerializeField] private AssemblyBench[] assemblyBenches;
        [SerializeField] private PackingStation packingStation;
        [SerializeField] private int assemblyBenchBaseCost = 34;
        [SerializeField] private float assemblyBenchCostGrowth = 1.45f;
        [SerializeField] private int packingStationCost = DefaultPackingStationCost;
        [SerializeField] private int dispatchRackCost = DefaultDispatchRackCost;
        [SerializeField] private int dispatchRackOrderCapacityBonus = DefaultDispatchRackOrderCapacityBonus;
        [SerializeField] private float dispatchRackOrderIntervalMultiplier = DefaultDispatchRackOrderIntervalMultiplier;
        [SerializeField] private float dispatchRackSpeedMultiplier = DefaultDispatchRackSpeedMultiplier;
        [SerializeField] private int dispatchRackBonusPerShipment = DefaultDispatchRackBonusPerShipment;
        [SerializeField] private string lastMessage = "Facilities ready";

        private bool hasDispatchRack;

        public int ActiveAssemblyBenchCount => CountActiveAssemblyBenches();
        public int TotalAssemblyBenchCount => assemblyBenches != null ? assemblyBenches.Length : 0;
        public bool CanBuildAssemblyBench => ActiveAssemblyBenchCount < TotalAssemblyBenchCount;
        public bool HasPackingStation => packingStation != null && packingStation.gameObject.activeInHierarchy;
        public bool CanBuildPackingStation => !HasPackingStation;
        public bool HasDispatchRack => hasDispatchRack;
        public bool CanBuildDispatchRack => !hasDispatchRack;
        public int BuildAssemblyBenchCost => UpgradeCostCalculator.Calculate(assemblyBenchBaseCost, assemblyBenchCostGrowth, ActiveAssemblyBenchCount);
        public int BuildPackingStationCost => packingStationCost;
        public int BuildDispatchRackCost => dispatchRackCost;
        public string LastMessage => lastMessage;

        private void Awake()
        {
            NormalizeDispatchRackDefaults();
            ResolveReferences();
            RefreshAssemblyBenches();
            PublishAssemblyBenches();
        }

        private void OnValidate()
        {
            NormalizeDispatchRackDefaults();
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

            if (orderCounter == null)
            {
                orderCounter = FindFirstObjectByType<OrderCounter>();
            }

            if (pickupCounter == null)
            {
                pickupCounter = FindFirstObjectByType<PickupCounter>();
            }

            if (packingStation == null)
            {
                packingStation = FindFirstObjectByType<PackingStation>();
            }
        }

        private void NormalizeDispatchRackDefaults()
        {
            // Keep prototype tuning deterministic even if the scene serialized older test values.
            dispatchRackCost = DefaultDispatchRackCost;
            dispatchRackOrderCapacityBonus = DefaultDispatchRackOrderCapacityBonus;
            dispatchRackOrderIntervalMultiplier = DefaultDispatchRackOrderIntervalMultiplier;
            dispatchRackSpeedMultiplier = DefaultDispatchRackSpeedMultiplier;
            dispatchRackBonusPerShipment = DefaultDispatchRackBonusPerShipment;
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
                workerManager.SetPackingStation(packingStation);
            }
        }

        public bool TryBuildPackingStation()
        {
            if (HasPackingStation)
            {
                lastMessage = "Packing Station already online.";
                return false;
            }

            if (moneyManager == null || !moneyManager.TrySpend(packingStationCost))
            {
                lastMessage = "Not enough money. Need " + MoneyFormatter.Format(packingStationCost) + ".";
                return false;
            }

            if (packingStation == null)
            {
                packingStation = CreatePackingStationVisual();
            }
            else
            {
                packingStation.gameObject.SetActive(true);
            }

            if (orderCounter != null)
            {
                orderCounter.SetGiftOrdersUnlocked(true);
            }

            PublishAssemblyBenches();
            lastMessage = "Packing Station is online.";
            return true;
        }

        public bool TryBuildDispatchRack()
        {
            if (hasDispatchRack)
            {
                lastMessage = "Dispatch Rack already online.";
                return false;
            }

            if (moneyManager == null || !moneyManager.TrySpend(dispatchRackCost))
            {
                lastMessage = "Not enough money. Need " + MoneyFormatter.Format(dispatchRackCost) + ".";
                return false;
            }

            hasDispatchRack = true;
            if (orderCounter != null)
            {
                orderCounter.ApplyDispatchSupport(dispatchRackOrderCapacityBonus, dispatchRackOrderIntervalMultiplier);
            }

            if (pickupCounter != null)
            {
                pickupCounter.ApplyDispatchSupport(dispatchRackSpeedMultiplier);
                pickupCounter.ApplyDispatchSupportBonus(dispatchRackBonusPerShipment);
            }

            CreateDispatchRackVisual();
            lastMessage = "Dispatch Rack is online.";
            return true;
        }

        private void CreateDispatchRackVisual()
        {
            if (pickupCounter == null)
            {
                return;
            }

            Transform pickupTransform = pickupCounter.transform;
            Transform existing = pickupTransform.Find("DispatchRack");
            if (existing != null)
            {
                existing.gameObject.SetActive(true);
                return;
            }

            GameObject rack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rack.name = "DispatchRack";
            rack.transform.SetParent(pickupTransform, false);
            rack.transform.localScale = new Vector3(0.45f, 0.85f, 0.75f);
            rack.transform.localPosition = new Vector3(1.1f, 0.4f, 0f);

            Renderer renderer = rack.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = pickupTransform.GetComponent<Renderer>() != null
                    ? pickupTransform.GetComponent<Renderer>().sharedMaterial
                    : renderer.sharedMaterial;
                renderer.material.color = new Color(0.32f, 0.82f, 0.92f, 1f);
            }
        }

        private PackingStation CreatePackingStationVisual()
        {
            if (pickupCounter == null)
            {
                return null;
            }

            Transform pickupTransform = pickupCounter.transform;
            Transform existing = pickupTransform.Find("PackingStation");
            if (existing != null)
            {
                existing.gameObject.SetActive(true);
                return existing.GetComponent<PackingStation>();
            }

            GameObject stationRoot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stationRoot.name = "PackingStation";
            stationRoot.transform.SetParent(pickupTransform.parent, false);
            stationRoot.transform.position = pickupTransform.position + new Vector3(-1.4f, 0.45f, 0f);
            stationRoot.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);

            Renderer renderer = stationRoot.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.92f, 0.68f, 0.36f, 1f);
            }

            GameObject belt = GameObject.CreatePrimitive(PrimitiveType.Cube);
            belt.name = "PackingBelt";
            belt.transform.SetParent(stationRoot.transform, false);
            belt.transform.localPosition = new Vector3(0f, -0.18f, 0f);
            belt.transform.localScale = new Vector3(0.78f, 0.12f, 0.46f);
            Renderer beltRenderer = belt.GetComponent<Renderer>();
            if (beltRenderer != null)
            {
                beltRenderer.material.color = new Color(0.18f, 0.2f, 0.24f, 1f);
            }

            GameObject packPoint = new GameObject("PackPoint");
            packPoint.transform.SetParent(stationRoot.transform, false);
            packPoint.transform.localPosition = new Vector3(0f, 0.62f, 0f);

            PackingStation station = stationRoot.AddComponent<PackingStation>();

            StationSelectable selectable = stationRoot.AddComponent<StationSelectable>();
            selectable.Configure("Packing Station", "Packs finished products for extra sale value");

            return station;
        }
    }
}
