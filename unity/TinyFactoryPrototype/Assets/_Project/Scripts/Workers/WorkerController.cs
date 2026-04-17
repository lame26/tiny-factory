using TinyFactory.Items;
using TinyFactory.Stations;
using UnityEngine;

namespace TinyFactory.Workers
{
    public sealed class WorkerController : MonoBehaviour
    {
        public enum WorkerAssignment
        {
            Flexible,
            PartSupplier,
            PickupRunner
        }

        private enum WorkerState
        {
            WaitForOrder,
            MoveToPartBin,
            MoveToAssemblyBench,
            MoveToPackingStation,
            WaitForProduct,
            MoveToPickupCounter
        }

        [SerializeField] private Transform[] routePoints;
        [SerializeField] private OrderCounter orderCounter;
        [SerializeField] private PartBin partBin;
        [SerializeField] private AssemblyBench assemblyBench;
        [SerializeField] private AssemblyBench[] assemblyBenches;
        [SerializeField] private PackingStation packingStation;
        [SerializeField] private PickupCounter pickupCounter;
        [SerializeField] private CarryHolder carryHolder;
        [SerializeField] private WorkerAssignment assignment = WorkerAssignment.Flexible;
        [SerializeField] private float moveSpeed = 1.8f;
        [SerializeField] private float equipmentAssemblySpeedMultiplier = 1f;
        [SerializeField] private float waitSecondsAtStation = 0.6f;
        [SerializeField] private bool faceTravelDirection = true;

        private int currentTargetIndex;
        private float waitTimer;
        private WorkerState state = WorkerState.WaitForOrder;
        private AssemblyBench targetAssemblyBench;
        private OrderCounter.OrderType reservedOrderType = OrderCounter.OrderType.Standard;
        private int reservedPickupBonus;
        private string reservedOrderLabel = "Standard";
        private int WorkerId => gameObject.GetInstanceID();

        public int CurrentTargetIndex => currentTargetIndex;
        public string CurrentTask => state.ToString();
        public WorkerAssignment Assignment => assignment;
        public string AssignmentLabel => assignment.ToString();

        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = Mathf.Max(0f, value);
        }

        public void Initialize(OrderCounter targetOrderCounter, PartBin targetPartBin, AssemblyBench targetAssemblyBench, PickupCounter targetPickupCounter, CarryHolder targetCarryHolder, float targetMoveSpeed)
        {
            Initialize(targetOrderCounter, targetPartBin, new[] { targetAssemblyBench }, null, targetPickupCounter, targetCarryHolder, targetMoveSpeed);
        }

        public void Initialize(OrderCounter targetOrderCounter, PartBin targetPartBin, AssemblyBench[] targetAssemblyBenches, PickupCounter targetPickupCounter, CarryHolder targetCarryHolder, float targetMoveSpeed)
        {
            Initialize(targetOrderCounter, targetPartBin, targetAssemblyBenches, null, targetPickupCounter, targetCarryHolder, targetMoveSpeed);
        }

        public void Initialize(OrderCounter targetOrderCounter, PartBin targetPartBin, AssemblyBench[] targetAssemblyBenches, PackingStation targetPackingStation, PickupCounter targetPickupCounter, CarryHolder targetCarryHolder, float targetMoveSpeed)
        {
            orderCounter = targetOrderCounter;
            partBin = targetPartBin;
            SetAssemblyBenches(targetAssemblyBenches);
            SetPackingStation(targetPackingStation);
            pickupCounter = targetPickupCounter;
            carryHolder = targetCarryHolder;
            MoveSpeed = targetMoveSpeed;
            state = WorkerState.WaitForOrder;
            waitTimer = 0f;
            reservedOrderType = OrderCounter.OrderType.Standard;
            reservedPickupBonus = 0;
            reservedOrderLabel = "Standard";
            equipmentAssemblySpeedMultiplier = 1f;
        }

        public void SetAssignment(WorkerAssignment targetAssignment)
        {
            assignment = targetAssignment;
        }

        public void SetEquipmentAssemblySpeedMultiplier(float value)
        {
            equipmentAssemblySpeedMultiplier = Mathf.Max(0.1f, value);
        }

        private void Awake()
        {
            if (carryHolder == null)
            {
                carryHolder = GetComponent<CarryHolder>();
            }

            if (routePoints == null || routePoints.Length == 0)
            {
                routePoints = FindDefaultRoute();
            }

            if (orderCounter == null)
            {
                orderCounter = FindStation<OrderCounter>("CustomerOrderCounter");
            }

            if (partBin == null)
            {
                partBin = FindStation<PartBin>("PartBin");
            }

            if (assemblyBench == null)
            {
                assemblyBench = FindStation<AssemblyBench>("AssemblyBench");
            }

            if (assemblyBenches == null || assemblyBenches.Length == 0)
            {
                assemblyBenches = FindObjectsByType<AssemblyBench>(FindObjectsSortMode.None);
            }

            if (pickupCounter == null)
            {
                pickupCounter = FindStation<PickupCounter>("PickupCounter");
            }

            if (packingStation == null)
            {
                packingStation = FindFirstObjectByType<PackingStation>();
            }
        }

        private void Update()
        {
            if (orderCounter == null || partBin == null || GetPrimaryAssemblyBench() == null || pickupCounter == null || carryHolder == null)
            {
                return;
            }

            if (waitTimer > 0f)
            {
                waitTimer -= Time.deltaTime;
                return;
            }

            switch (state)
            {
                case WorkerState.WaitForOrder:
                    if (!carryHolder.HasItem && HasPackagedOutputReady())
                    {
                        state = WorkerState.MoveToPackingStation;
                        return;
                    }

                    if (assignment == WorkerAssignment.PartSupplier)
                    {
                        if (orderCounter.HasPendingOrder)
                        {
                            targetAssemblyBench = AcquireBenchReadyForPart();
                            if (targetAssemblyBench != null)
                            {
                                state = WorkerState.MoveToPartBin;
                                return;
                            }
                        }

                        targetAssemblyBench = AcquireBenchWithProductReady();
                        if (targetAssemblyBench != null)
                        {
                            state = WorkerState.MoveToAssemblyBench;
                        }
                    }
                    else if (assignment == WorkerAssignment.PickupRunner)
                    {
                        targetAssemblyBench = AcquireBenchWithProductReady();
                        if (targetAssemblyBench != null)
                        {
                            state = WorkerState.MoveToAssemblyBench;
                            return;
                        }

                        targetAssemblyBench = FindBenchFinishingSoonest();
                        if (targetAssemblyBench != null)
                        {
                            state = WorkerState.MoveToAssemblyBench;
                        }
                    }
                    else
                    {
                        targetAssemblyBench = AcquireBenchWithProductReady();
                        if (targetAssemblyBench != null)
                        {
                            state = WorkerState.MoveToAssemblyBench;
                            return;
                        }

                        targetAssemblyBench = AcquireBenchReadyForPart();
                        if (targetAssemblyBench != null)
                        {
                            state = WorkerState.MoveToPartBin;
                        }
                    }
                    break;

                case WorkerState.MoveToPartBin:
                    if (MoveTo(partBin.WorkPoint))
                    {
                        ReleaseBenchReservation();
                        if (!carryHolder.HasItem && HasPackagedOutputReady())
                        {
                            state = WorkerState.MoveToPackingStation;
                            return;
                        }

                        targetAssemblyBench = AcquireBenchWithProductReady();
                        if (targetAssemblyBench != null)
                        {
                            state = WorkerState.MoveToAssemblyBench;
                            return;
                        }

                        targetAssemblyBench = AcquireBenchReadyForPart();
                        if (targetAssemblyBench == null)
                        {
                            state = WorkerState.WaitForProduct;
                            return;
                        }

                        if (!carryHolder.HasItem && orderCounter.TryReserveOrder(out OrderCounter.OrderReservation reservation))
                        {
                            partBin.TryTakePart(carryHolder);
                            if (carryHolder.HasItem)
                            {
                                reservedOrderType = reservation.Type;
                                reservedPickupBonus = reservation.BonusValue;
                                reservedOrderLabel = reservation.Label;
                            }
                        }

                        state = carryHolder.HasItem ? WorkerState.MoveToAssemblyBench : WorkerState.WaitForOrder;
                        waitTimer = waitSecondsAtStation;
                    }
                    break;

                case WorkerState.MoveToAssemblyBench:
                    if (targetAssemblyBench == null)
                    {
                        targetAssemblyBench = carryHolder.HasItem ? AcquireBenchReadyForPart() : AcquireBenchWithProductReady();
                    }

                    if (targetAssemblyBench != null && MoveTo(targetAssemblyBench.WorkPoint))
                    {
                        HandleAssemblyBenchArrival();
                    }
                    break;

                case WorkerState.MoveToPackingStation:
                    if (packingStation == null || !packingStation.gameObject.activeInHierarchy)
                    {
                        state = WorkerState.MoveToPickupCounter;
                        break;
                    }

                    if (MoveTo(packingStation.WorkPoint))
                    {
                        HandlePackingStationArrival();
                    }
                    break;

                case WorkerState.WaitForProduct:
                    if (assignment != WorkerAssignment.PickupRunner)
                    {
                        ReleaseBenchReservation();
                    }
                    if (carryHolder.HasItem)
                    {
                        targetAssemblyBench = AcquireBenchReadyForPart();
                        if (targetAssemblyBench != null)
                        {
                            state = WorkerState.MoveToAssemblyBench;
                        }

                        return;
                    }

                    if (HasPackagedOutputReady())
                    {
                        state = WorkerState.MoveToPackingStation;
                        return;
                    }

                    targetAssemblyBench = AcquireBenchWithProductReady();
                    if (targetAssemblyBench != null)
                    {
                        state = WorkerState.MoveToAssemblyBench;
                        return;
                    }

                    if (assignment == WorkerAssignment.PickupRunner)
                    {
                        if (targetAssemblyBench != null && targetAssemblyBench.IsAssembling)
                        {
                            return;
                        }

                        targetAssemblyBench = FindBenchFinishingSoonest();
                        if (targetAssemblyBench != null)
                        {
                            state = WorkerState.MoveToAssemblyBench;
                            return;
                        }

                        state = WorkerState.WaitForOrder;
                        return;
                    }

                    AssemblyBench availableBench = AcquireBenchReadyForPart();
                    if (availableBench != null)
                    {
                        targetAssemblyBench = availableBench;
                        state = orderCounter.HasPendingOrder ? WorkerState.MoveToPartBin : WorkerState.WaitForOrder;
                    }
                    break;

                case WorkerState.MoveToPickupCounter:
                    if (MoveTo(pickupCounter.WorkPoint))
                    {
                        if (pickupCounter.TryCompletePickup(carryHolder, reservedOrderType, reservedPickupBonus, reservedOrderLabel))
                        {
                            reservedOrderType = OrderCounter.OrderType.Standard;
                            reservedPickupBonus = 0;
                            reservedOrderLabel = "Standard";
                            state = WorkerState.WaitForOrder;
                            waitTimer = waitSecondsAtStation;
                        }
                    }
                    break;
            }
        }

        public void SetRoute(Transform[] points)
        {
            routePoints = points;
            currentTargetIndex = 0;
            waitTimer = 0f;
        }

        public void SetAssemblyBenches(AssemblyBench[] targetAssemblyBenches)
        {
            ReleaseBenchReservation();
            assemblyBenches = targetAssemblyBenches != null ? targetAssemblyBenches : System.Array.Empty<AssemblyBench>();
            assemblyBench = assemblyBenches.Length > 0 ? assemblyBenches[0] : assemblyBench;
            if (targetAssemblyBench != null && !IsBenchUsable(targetAssemblyBench))
            {
                targetAssemblyBench = null;
            }
        }

        public void SetPackingStation(PackingStation targetPackingStation)
        {
            packingStation = targetPackingStation;
        }

        private void HandleAssemblyBenchArrival()
        {
            AssemblyBench activeBench = targetAssemblyBench != null ? targetAssemblyBench : GetPrimaryAssemblyBench();
            if (activeBench == null)
            {
                ReleaseBenchReservation();
                state = WorkerState.WaitForOrder;
                return;
            }

            if (carryHolder.HasItem && carryHolder.CarriedItem.ItemType == ItemType.Part)
            {
                if (activeBench.TryAcceptPart(carryHolder, equipmentAssemblySpeedMultiplier))
                {
                    if (assignment == WorkerAssignment.PartSupplier)
                    {
                        targetAssemblyBench = null;
                        state = WorkerState.WaitForOrder;
                    }
                    else
                    {
                        targetAssemblyBench = activeBench;
                        state = WorkerState.WaitForProduct;
                    }
                }
                else
                {
                    activeBench.ReleaseReservation(WorkerId);
                    targetAssemblyBench = AcquireBenchReadyForPart();
                    state = targetAssemblyBench != null ? WorkerState.MoveToAssemblyBench : WorkerState.WaitForProduct;
                }

                waitTimer = waitSecondsAtStation;
                return;
            }

            if (!carryHolder.HasItem && activeBench.HasProductReady)
            {
                if (activeBench.TryTakeProduct(carryHolder))
                {
                    activeBench.ReleaseReservation(WorkerId);
                    targetAssemblyBench = null;
                    state = ShouldUsePackingStation() ? WorkerState.MoveToPackingStation : WorkerState.MoveToPickupCounter;
                }

                waitTimer = waitSecondsAtStation;
                return;
            }

            if (activeBench.IsAssembling)
            {
                activeBench.ReleaseReservation(WorkerId);
                state = assignment == WorkerAssignment.PartSupplier ? WorkerState.WaitForOrder : WorkerState.WaitForProduct;
                return;
            }

            activeBench.ReleaseReservation(WorkerId);
            state = orderCounter.HasPendingOrder ? WorkerState.MoveToPartBin : WorkerState.WaitForOrder;
        }

        private void HandlePackingStationArrival()
        {
            if (packingStation == null)
            {
                state = WorkerState.MoveToPickupCounter;
                return;
            }

            if (carryHolder.HasItem && carryHolder.CarriedItem.ItemType == ItemType.Product)
            {
                if (packingStation.TryAcceptProduct(carryHolder))
                {
                    state = WorkerState.WaitForProduct;
                }
                else
                {
                    state = WorkerState.MoveToPickupCounter;
                }

                waitTimer = waitSecondsAtStation;
                return;
            }

            if (!carryHolder.HasItem && packingStation.HasPackagedProductReady)
            {
                if (packingStation.TryTakePackagedProduct(carryHolder))
                {
                    state = WorkerState.MoveToPickupCounter;
                }

                waitTimer = waitSecondsAtStation;
                return;
            }

            state = WorkerState.WaitForProduct;
        }

        private bool MoveTo(Transform target)
        {
            if (target == null)
            {
                return false;
            }

            Vector3 currentPosition = transform.position;
            Vector3 targetPosition = new Vector3(target.position.x, currentPosition.y, target.position.z);
            Vector3 nextPosition = Vector3.MoveTowards(currentPosition, targetPosition, moveSpeed * Time.deltaTime);
            Vector3 travel = nextPosition - currentPosition;

            transform.position = nextPosition;

            if (faceTravelDirection && travel.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(travel.normalized, Vector3.up);
            }

            if ((transform.position - targetPosition).sqrMagnitude <= 0.0025f)
            {
                return true;
            }

            return false;
        }

        private static T FindStation<T>(string objectName) where T : Component
        {
            GameObject station = GameObject.Find(objectName);
            return station != null ? station.GetComponent<T>() : null;
        }

        private static Transform[] FindDefaultRoute()
        {
            string[] names = { "PartBin", "AssemblyBench", "PickupCounter" };
            Transform[] route = new Transform[names.Length];

            for (int i = 0; i < names.Length; i++)
            {
                GameObject station = GameObject.Find(names[i]);
                if (station == null)
                {
                    return System.Array.Empty<Transform>();
                }

                route[i] = station.transform;
            }

            return route;
        }

        private AssemblyBench GetPrimaryAssemblyBench()
        {
            if (assemblyBench != null && IsBenchUsable(assemblyBench))
            {
                return assemblyBench;
            }

            if (assemblyBenches == null)
            {
                return null;
            }

            for (int i = 0; i < assemblyBenches.Length; i++)
            {
                if (IsBenchUsable(assemblyBenches[i]))
                {
                    return assemblyBenches[i];
                }
            }

            return null;
        }

        private AssemblyBench AcquireBenchWithProductReady()
        {
            if (targetAssemblyBench != null && targetAssemblyBench.IsReservedForProductPickupBy(WorkerId) && targetAssemblyBench.HasProductReady)
            {
                return targetAssemblyBench;
            }

            ReleaseBenchReservation();

            if (TryAcquirePriorityBenchWithProductReady(out AssemblyBench priorityBench))
            {
                return priorityBench;
            }

            if (assemblyBenches == null || assemblyBenches.Length == 0)
            {
                if (assemblyBench != null && assemblyBench.HasProductReady && assemblyBench.TryReserveProductPickup(WorkerId))
                {
                    return assemblyBench;
                }

                return null;
            }

            AssemblyBench nearestBench = null;
            float highestReadyDuration = -1f;
            float nearestDistance = float.MaxValue;
            for (int i = 0; i < assemblyBenches.Length; i++)
            {
                AssemblyBench bench = assemblyBenches[i];
                if (!IsBenchUsable(bench) || !bench.HasProductReady || !bench.TryReserveProductPickup(WorkerId))
                {
                    continue;
                }

                float readyDuration = bench.ProductReadyDuration;
                float distance = (transform.position - bench.WorkPoint.position).sqrMagnitude;
                bool isBetterCandidate = readyDuration > highestReadyDuration + 0.001f
                    || (Mathf.Abs(readyDuration - highestReadyDuration) <= 0.001f && distance < nearestDistance);

                if (isBetterCandidate)
                {
                    if (nearestBench != null)
                    {
                        nearestBench.ReleaseReservation(WorkerId);
                    }

                    highestReadyDuration = readyDuration;
                    nearestDistance = distance;
                    nearestBench = bench;
                }
                else
                {
                    bench.ReleaseReservation(WorkerId);
                }
            }

            return nearestBench;
        }

        private AssemblyBench AcquireBenchReadyForPart()
        {
            if (targetAssemblyBench != null && targetAssemblyBench.IsReservedForPartDeliveryBy(WorkerId) && IsBenchReadyForPart(targetAssemblyBench))
            {
                return targetAssemblyBench;
            }

            ReleaseBenchReservation();

            if (TryAcquirePriorityBenchReadyForPart(out AssemblyBench priorityBench))
            {
                return priorityBench;
            }

            if (assemblyBenches == null || assemblyBenches.Length == 0)
            {
                if (assemblyBench != null && IsBenchReadyForPart(assemblyBench) && assemblyBench.TryReservePartDelivery(WorkerId))
                {
                    return assemblyBench;
                }

                return null;
            }

            AssemblyBench nearestBench = null;
            float highestIdleDuration = -1f;
            float nearestDistance = float.MaxValue;
            for (int i = 0; i < assemblyBenches.Length; i++)
            {
                AssemblyBench bench = assemblyBenches[i];
                if (!IsBenchReadyForPart(bench) || !bench.TryReservePartDelivery(WorkerId))
                {
                    continue;
                }

                float idleDuration = bench.IdleDuration;
                float distance = (transform.position - bench.WorkPoint.position).sqrMagnitude;
                bool isBetterCandidate = idleDuration > highestIdleDuration + 0.001f
                    || (Mathf.Abs(idleDuration - highestIdleDuration) <= 0.001f && distance < nearestDistance);

                if (isBetterCandidate)
                {
                    if (nearestBench != null)
                    {
                        nearestBench.ReleaseReservation(WorkerId);
                    }

                    highestIdleDuration = idleDuration;
                    nearestDistance = distance;
                    nearestBench = bench;
                }
                else
                {
                    bench.ReleaseReservation(WorkerId);
                }
            }

            return nearestBench;
        }


        private void ReleaseBenchReservation()
        {
            if (targetAssemblyBench == null)
            {
                return;
            }

            targetAssemblyBench.ReleaseReservation(WorkerId);
            targetAssemblyBench = null;
        }

        private AssemblyBench FindBenchFinishingSoonest()
        {
            AssemblyBench priorityBench = AssemblyBench.PriorityBench;
            if (IsBenchUsable(priorityBench) && priorityBench.IsAssembling)
            {
                return priorityBench;
            }

            if (assemblyBenches == null || assemblyBenches.Length == 0)
            {
                return assemblyBench != null && assemblyBench.IsAssembling ? assemblyBench : null;
            }

            AssemblyBench bestBench = null;
            float shortestRemainingSeconds = float.MaxValue;
            float nearestDistance = float.MaxValue;
            for (int i = 0; i < assemblyBenches.Length; i++)
            {
                AssemblyBench bench = assemblyBenches[i];
                if (!IsBenchUsable(bench) || !bench.IsAssembling)
                {
                    continue;
                }

                float remainingSeconds = bench.RemainingAssemblySeconds;
                float distance = (transform.position - bench.WorkPoint.position).sqrMagnitude;
                bool isBetterCandidate = remainingSeconds < shortestRemainingSeconds - 0.001f
                    || (Mathf.Abs(remainingSeconds - shortestRemainingSeconds) <= 0.001f && distance < nearestDistance);

                if (!isBetterCandidate)
                {
                    continue;
                }

                shortestRemainingSeconds = remainingSeconds;
                nearestDistance = distance;
                bestBench = bench;
            }

            return bestBench;
        }

        private bool TryAcquirePriorityBenchWithProductReady(out AssemblyBench bench)
        {
            bench = AssemblyBench.PriorityBench;
            if (!IsBenchUsable(bench) || !bench.HasProductReady || !bench.TryReserveProductPickup(WorkerId))
            {
                bench = null;
                return false;
            }

            return true;
        }

        private bool TryAcquirePriorityBenchReadyForPart(out AssemblyBench bench)
        {
            bench = AssemblyBench.PriorityBench;
            if (!IsBenchReadyForPart(bench) || !bench.TryReservePartDelivery(WorkerId))
            {
                bench = null;
                return false;
            }

            return true;
        }

        private static bool IsBenchReadyForPart(AssemblyBench bench)
        {
            return IsBenchUsable(bench) && !bench.IsAssembling && !bench.HasProductReady;
        }

        private bool ShouldUsePackingStation()
        {
            return packingStation != null && packingStation.gameObject.activeInHierarchy;
        }

        private bool HasPackagedOutputReady()
        {
            return ShouldUsePackingStation() && packingStation.HasPackagedProductReady;
        }

        private static bool IsBenchUsable(AssemblyBench bench)
        {
            return bench != null && bench.gameObject.activeInHierarchy;
        }

        private void OnDrawGizmosSelected()
        {
            if (routePoints == null || routePoints.Length == 0)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            for (int i = 0; i < routePoints.Length; i++)
            {
                Transform from = routePoints[i];
                Transform to = routePoints[(i + 1) % routePoints.Length];
                if (from == null || to == null)
                {
                    continue;
                }

                Gizmos.DrawLine(from.position + Vector3.up * 0.2f, to.position + Vector3.up * 0.2f);
            }
        }
    }
}
