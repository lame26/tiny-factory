using TinyFactory.Economy;
using TinyFactory.Items;
using TinyFactory.Stations;
using TinyFactory.Core;
using UnityEngine;

namespace TinyFactory.Workers
{
    public sealed class WorkerManager : MonoBehaviour
    {
        [SerializeField] private MoneyManager moneyManager;
        [SerializeField] private OrderCounter orderCounter;
        [SerializeField] private PartBin partBin;
        [SerializeField] private AssemblyBench assemblyBench;
        [SerializeField] private AssemblyBench[] assemblyBenches;
        [SerializeField] private PackingStation packingStation;
        [SerializeField] private PickupCounter pickupCounter;
        [SerializeField] private SupportBonusSlots supportBonusSlots;
        [SerializeField] private FactoryBoostManager factoryBoostManager;
        [SerializeField] private Transform workersRoot;
        [SerializeField] private Material workerMaterial;
        [SerializeField] private int hireWorkerBaseCost = 32;
        [SerializeField] private float hireWorkerCostGrowth = 1.48f;
        [SerializeField] private int workerThroughputBaseCost = 24;
        [SerializeField] private float workerThroughputCostGrowth = 1.34f;
        [SerializeField] private float baseMoveSpeed = 1.65f;
        [SerializeField] private float speedBonusPerLevel = 0.2f;
        [SerializeField] private int workerThroughputLevel = 1;
        [SerializeField] private int hiredWorkerCount = 1;
        [SerializeField] private string lastMessage = "Worker automation ready";

        public int WorkerCount => Mathf.Max(1, hiredWorkerCount);
        public int WorkerThroughputLevel => workerThroughputLevel;
        public int HireWorkerCost => UpgradeCostCalculator.Calculate(hireWorkerBaseCost, hireWorkerCostGrowth, WorkerCount);
        public int WorkerThroughputCost => UpgradeCostCalculator.Calculate(workerThroughputBaseCost, workerThroughputCostGrowth, workerThroughputLevel);
        public float WorkerMoveSpeed => baseMoveSpeed * (1f + (workerThroughputLevel - 1) * speedBonusPerLevel);
        public string LastMessage => lastMessage;
        public string WorkerRoleSummary => BuildWorkerRoleSummary();

        private void Awake()
        {
            if (moneyManager == null)
            {
                moneyManager = FindFirstObjectByType<MoneyManager>();
            }

            if (orderCounter == null)
            {
                orderCounter = FindFirstObjectByType<OrderCounter>();
            }

            if (partBin == null)
            {
                partBin = FindFirstObjectByType<PartBin>();
            }

            if (assemblyBench == null)
            {
                assemblyBench = FindFirstObjectByType<AssemblyBench>();
            }

            if (assemblyBenches == null || assemblyBenches.Length == 0)
            {
                assemblyBenches = FindObjectsByType<AssemblyBench>(FindObjectsSortMode.None);
            }

            if (pickupCounter == null)
            {
                pickupCounter = FindFirstObjectByType<PickupCounter>();
            }

            if (packingStation == null)
            {
                packingStation = FindFirstObjectByType<PackingStation>();
            }

            if (supportBonusSlots == null)
            {
                supportBonusSlots = FindFirstObjectByType<SupportBonusSlots>();
            }

            if (factoryBoostManager == null)
            {
                factoryBoostManager = FactoryBoostManager.GetOrCreate();
            }

            if (workersRoot == null)
            {
                GameObject workersObject = GameObject.Find("Workers");
                workersRoot = workersObject != null ? workersObject.transform : null;
            }

            if (workerMaterial == null)
            {
                WorkerController firstWorker = FindFirstObjectByType<WorkerController>();
                if (firstWorker != null)
                {
                    Renderer workerRenderer = firstWorker.GetComponent<Renderer>();
                    if (workerRenderer != null)
                    {
                        workerMaterial = workerRenderer.sharedMaterial;
                    }
                }
            }

            WorkerController[] existingWorkers = FindObjectsByType<WorkerController>(FindObjectsSortMode.None);
            hiredWorkerCount = Mathf.Max(1, existingWorkers.Length);
            ApplyWorkerStats();
        }

        private void OnEnable()
        {
            if (factoryBoostManager == null)
            {
                factoryBoostManager = FactoryBoostManager.GetOrCreate();
            }

            if (factoryBoostManager != null)
            {
                factoryBoostManager.BoostStateChanged += HandleBoostStateChanged;
            }
        }

        private void OnDisable()
        {
            if (factoryBoostManager != null)
            {
                factoryBoostManager.BoostStateChanged -= HandleBoostStateChanged;
            }
        }

        public bool TryHireWorker()
        {
            int cost = HireWorkerCost;
            if (moneyManager == null || !moneyManager.TrySpend(cost))
            {
                lastMessage = "Not enough money. Need " + MoneyFormatter.Format(cost) + ".";
                return false;
            }

            hiredWorkerCount++;
            CreateWorker(hiredWorkerCount);
            ApplyWorkerStats();
            lastMessage = "Hired Worker " + hiredWorkerCount + ". Next: " + MoneyFormatter.Format(HireWorkerCost) + ".";
            return true;
        }

        public bool TryUpgradeWorkerThroughput()
        {
            int cost = WorkerThroughputCost;
            if (moneyManager == null || !moneyManager.TrySpend(cost))
            {
                lastMessage = "Not enough money. Need " + MoneyFormatter.Format(cost) + ".";
                return false;
            }

            workerThroughputLevel++;
            ApplyWorkerStats();
            lastMessage = "Worker throughput upgraded. Next: " + MoneyFormatter.Format(WorkerThroughputCost) + ".";
            return true;
        }

        private void CreateWorker(int workerIndex)
        {
            GameObject workerObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            workerObject.name = "Worker_" + workerIndex.ToString("00");
            if (workersRoot != null)
            {
                workerObject.transform.SetParent(workersRoot, false);
            }

            workerObject.transform.position = GetSpawnPosition(workerIndex);
            workerObject.transform.localScale = new Vector3(0.42f, 0.55f, 0.42f);

            Renderer workerRenderer = workerObject.GetComponent<Renderer>();
            if (workerRenderer != null && workerMaterial != null)
            {
                workerRenderer.sharedMaterial = workerMaterial;
            }

            GameObject holdPoint = new GameObject("CarryHoldPoint");
            holdPoint.transform.SetParent(workerObject.transform, false);
            holdPoint.transform.localPosition = new Vector3(0f, 1.25f, 0.42f);

            CarryHolder carryHolder = workerObject.AddComponent<CarryHolder>();
            carryHolder.SetHoldPoint(holdPoint.transform);

            WorkerController workerController = workerObject.AddComponent<WorkerController>();
            workerController.Initialize(orderCounter, partBin, GetActiveAssemblyBenches(), packingStation, pickupCounter, carryHolder, GetWorkerMoveSpeedForIndex(workerIndex - 1));
            workerController.SetEquipmentAssemblySpeedMultiplier(GetWorkerAssemblySpeedMultiplierForIndex(workerIndex - 1));
            workerController.SetAssignment(GetAssignmentForIndex(workerIndex - 1, hiredWorkerCount));
        }

        public void SetAssemblyBenches(AssemblyBench[] targetAssemblyBenches)
        {
            if (targetAssemblyBenches == null || targetAssemblyBenches.Length == 0)
            {
                return;
            }

            assemblyBenches = targetAssemblyBenches;
            assemblyBench = assemblyBenches[0];

            WorkerController[] workers = FindObjectsByType<WorkerController>(FindObjectsSortMode.None);
            for (int i = 0; i < workers.Length; i++)
            {
                workers[i].SetAssemblyBenches(assemblyBenches);
            }
        }

        public void SetPackingStation(PackingStation targetPackingStation)
        {
            packingStation = targetPackingStation;

            WorkerController[] workers = FindObjectsByType<WorkerController>(FindObjectsSortMode.None);
            for (int i = 0; i < workers.Length; i++)
            {
                workers[i].SetPackingStation(packingStation);
            }
        }

        private Vector3 GetSpawnPosition(int workerIndex)
        {
            Vector3 origin = partBin != null ? partBin.WorkPoint.position : Vector3.zero;
            float sideOffset = ((workerIndex % 2) == 0 ? 0.45f : -0.45f);
            float rowOffset = Mathf.Floor((workerIndex - 1) * 0.5f) * 0.35f;
            return origin + new Vector3(sideOffset, 0.55f, -0.35f - rowOffset);
        }

        private void ApplyWorkerStats()
        {
            WorkerController[] workers = FindObjectsByType<WorkerController>(FindObjectsSortMode.None);
            System.Array.Sort(workers, (left, right) => string.Compare(left.name, right.name, System.StringComparison.Ordinal));
            for (int i = 0; i < workers.Length; i++)
            {
                workers[i].MoveSpeed = GetWorkerMoveSpeedForIndex(i);
                workers[i].SetEquipmentAssemblySpeedMultiplier(GetWorkerAssemblySpeedMultiplierForIndex(i));
                workers[i].SetAssemblyBenches(GetActiveAssemblyBenches());
                workers[i].SetPackingStation(packingStation);
                workers[i].SetAssignment(GetAssignmentForIndex(i, workers.Length));
            }
        }

        public void RefreshWorkerStats()
        {
            ApplyWorkerStats();
        }

        private WorkerController.WorkerAssignment GetAssignmentForIndex(int workerIndex, int totalWorkers)
        {
            if (totalWorkers <= 1)
            {
                return WorkerController.WorkerAssignment.Flexible;
            }

            if (workerIndex == 0)
            {
                return WorkerController.WorkerAssignment.PartSupplier;
            }

            if (workerIndex == 1)
            {
                return WorkerController.WorkerAssignment.PickupRunner;
            }

            return WorkerController.WorkerAssignment.Flexible;
        }

        private string BuildWorkerRoleSummary()
        {
            WorkerController[] workers = FindObjectsByType<WorkerController>(FindObjectsSortMode.None);
            System.Array.Sort(workers, (left, right) => string.Compare(left.name, right.name, System.StringComparison.Ordinal));
            int partSuppliers = 0;
            int pickupRunners = 0;
            int flexibleWorkers = 0;

            for (int i = 0; i < workers.Length; i++)
            {
                switch (workers[i].Assignment)
                {
                    case WorkerController.WorkerAssignment.PartSupplier:
                        partSuppliers++;
                        break;
                    case WorkerController.WorkerAssignment.PickupRunner:
                        pickupRunners++;
                        break;
                    default:
                        flexibleWorkers++;
                        break;
                }
            }

            return "Roles F:" + flexibleWorkers + " / P:" + partSuppliers + " / R:" + pickupRunners;
        }

        private AssemblyBench[] GetActiveAssemblyBenches()
        {
            if (assemblyBenches == null || assemblyBenches.Length == 0)
            {
                return assemblyBench != null ? new[] { assemblyBench } : System.Array.Empty<AssemblyBench>();
            }

            System.Collections.Generic.List<AssemblyBench> activeBenches = new System.Collections.Generic.List<AssemblyBench>();
            for (int i = 0; i < assemblyBenches.Length; i++)
            {
                if (assemblyBenches[i] != null && assemblyBenches[i].gameObject.activeInHierarchy)
                {
                    activeBenches.Add(assemblyBenches[i]);
                }
            }

            if (activeBenches.Count == 0 && assemblyBench != null)
            {
                activeBenches.Add(assemblyBench);
            }

            return activeBenches.ToArray();
        }

        private float GetWorkerMoveSpeedForIndex(int workerIndex)
        {
            float equipmentMultiplier = workerIndex == 0 && supportBonusSlots != null
                ? supportBonusSlots.EquipmentMoveSpeedMultiplier
                : 1f;
            float boostMultiplier = factoryBoostManager != null ? factoryBoostManager.MoveSpeedMultiplier : 1f;
            return WorkerMoveSpeed * equipmentMultiplier * boostMultiplier;
        }

        private float GetWorkerAssemblySpeedMultiplierForIndex(int workerIndex)
        {
            float baseMultiplier = workerIndex == 0 && supportBonusSlots != null
                ? supportBonusSlots.EquipmentAssemblySpeedMultiplier
                : 1f;
            float boostMultiplier = factoryBoostManager != null ? factoryBoostManager.AssemblySpeedMultiplier : 1f;
            return baseMultiplier * boostMultiplier;
        }

        private void HandleBoostStateChanged()
        {
            ApplyWorkerStats();
        }
    }
}
