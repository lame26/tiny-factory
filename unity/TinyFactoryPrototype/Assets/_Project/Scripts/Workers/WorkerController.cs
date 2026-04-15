using TinyFactory.Items;
using TinyFactory.Stations;
using UnityEngine;

namespace TinyFactory.Workers
{
    public sealed class WorkerController : MonoBehaviour
    {
        private enum WorkerState
        {
            WaitForOrder,
            MoveToPartBin,
            MoveToAssemblyBench,
            WaitForProduct,
            MoveToPickupCounter
        }

        [SerializeField] private Transform[] routePoints;
        [SerializeField] private OrderCounter orderCounter;
        [SerializeField] private PartBin partBin;
        [SerializeField] private AssemblyBench assemblyBench;
        [SerializeField] private AssemblyBench[] assemblyBenches;
        [SerializeField] private PickupCounter pickupCounter;
        [SerializeField] private CarryHolder carryHolder;
        [SerializeField] private float moveSpeed = 1.8f;
        [SerializeField] private float waitSecondsAtStation = 0.6f;
        [SerializeField] private bool faceTravelDirection = true;

        private int currentTargetIndex;
        private float waitTimer;
        private WorkerState state = WorkerState.WaitForOrder;
        private AssemblyBench targetAssemblyBench;

        public int CurrentTargetIndex => currentTargetIndex;
        public string CurrentTask => state.ToString();

        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = Mathf.Max(0f, value);
        }

        public void Initialize(OrderCounter targetOrderCounter, PartBin targetPartBin, AssemblyBench targetAssemblyBench, PickupCounter targetPickupCounter, CarryHolder targetCarryHolder, float targetMoveSpeed)
        {
            Initialize(targetOrderCounter, targetPartBin, new[] { targetAssemblyBench }, targetPickupCounter, targetCarryHolder, targetMoveSpeed);
        }

        public void Initialize(OrderCounter targetOrderCounter, PartBin targetPartBin, AssemblyBench[] targetAssemblyBenches, PickupCounter targetPickupCounter, CarryHolder targetCarryHolder, float targetMoveSpeed)
        {
            orderCounter = targetOrderCounter;
            partBin = targetPartBin;
            SetAssemblyBenches(targetAssemblyBenches);
            pickupCounter = targetPickupCounter;
            carryHolder = targetCarryHolder;
            MoveSpeed = targetMoveSpeed;
            state = WorkerState.WaitForOrder;
            waitTimer = 0f;
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
                    targetAssemblyBench = FindBenchWithProductReady();
                    if (targetAssemblyBench != null)
                    {
                        state = WorkerState.MoveToAssemblyBench;
                        return;
                    }

                    if (orderCounter.HasPendingOrder)
                    {
                        targetAssemblyBench = FindBenchReadyForPart();
                        if (targetAssemblyBench != null)
                        {
                            state = WorkerState.MoveToPartBin;
                        }
                    }
                    break;

                case WorkerState.MoveToPartBin:
                    if (MoveTo(partBin.WorkPoint))
                    {
                        targetAssemblyBench = FindBenchWithProductReady();
                        if (targetAssemblyBench != null)
                        {
                            state = WorkerState.MoveToAssemblyBench;
                            return;
                        }

                        targetAssemblyBench = FindBenchReadyForPart();
                        if (targetAssemblyBench == null)
                        {
                            state = WorkerState.WaitForProduct;
                            return;
                        }

                        if (!carryHolder.HasItem && orderCounter.TryReserveOrder())
                        {
                            partBin.TryTakePart(carryHolder);
                        }

                        state = carryHolder.HasItem ? WorkerState.MoveToAssemblyBench : WorkerState.WaitForOrder;
                        waitTimer = waitSecondsAtStation;
                    }
                    break;

                case WorkerState.MoveToAssemblyBench:
                    if (targetAssemblyBench == null)
                    {
                        targetAssemblyBench = carryHolder.HasItem ? FindBenchReadyForPart() : FindBenchWithProductReady();
                    }

                    if (targetAssemblyBench != null && MoveTo(targetAssemblyBench.WorkPoint))
                    {
                        HandleAssemblyBenchArrival();
                    }
                    break;

                case WorkerState.WaitForProduct:
                    targetAssemblyBench = FindBenchWithProductReady();
                    if (targetAssemblyBench != null && !carryHolder.HasItem)
                    {
                        state = WorkerState.MoveToAssemblyBench;
                        return;
                    }

                    if (carryHolder.HasItem)
                    {
                        targetAssemblyBench = FindBenchReadyForPart();
                        if (targetAssemblyBench != null)
                        {
                            state = WorkerState.MoveToAssemblyBench;
                        }

                        return;
                    }

                    if (FindBenchReadyForPart() != null)
                    {
                        state = orderCounter.HasPendingOrder ? WorkerState.MoveToPartBin : WorkerState.WaitForOrder;
                    }
                    break;

                case WorkerState.MoveToPickupCounter:
                    if (MoveTo(pickupCounter.WorkPoint))
                    {
                        pickupCounter.TryCompletePickup(carryHolder);
                        state = WorkerState.WaitForOrder;
                        waitTimer = waitSecondsAtStation;
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
            assemblyBenches = targetAssemblyBenches != null ? targetAssemblyBenches : System.Array.Empty<AssemblyBench>();
            assemblyBench = assemblyBenches.Length > 0 ? assemblyBenches[0] : assemblyBench;
            if (targetAssemblyBench != null && !IsBenchUsable(targetAssemblyBench))
            {
                targetAssemblyBench = null;
            }
        }

        private void HandleAssemblyBenchArrival()
        {
            AssemblyBench activeBench = targetAssemblyBench != null ? targetAssemblyBench : GetPrimaryAssemblyBench();
            if (activeBench == null)
            {
                state = WorkerState.WaitForOrder;
                return;
            }

            if (carryHolder.HasItem && carryHolder.CarriedItem.ItemType == ItemType.Part)
            {
                if (activeBench.TryAcceptPart(carryHolder))
                {
                    targetAssemblyBench = activeBench;
                    state = WorkerState.WaitForProduct;
                }
                else
                {
                    targetAssemblyBench = FindBenchReadyForPart();
                    state = targetAssemblyBench != null ? WorkerState.MoveToAssemblyBench : WorkerState.WaitForProduct;
                }

                waitTimer = waitSecondsAtStation;
                return;
            }

            if (!carryHolder.HasItem && activeBench.HasProductReady)
            {
                if (activeBench.TryTakeProduct(carryHolder))
                {
                    targetAssemblyBench = null;
                    state = WorkerState.MoveToPickupCounter;
                }

                waitTimer = waitSecondsAtStation;
                return;
            }

            if (activeBench.IsAssembling)
            {
                state = WorkerState.WaitForProduct;
                return;
            }

            state = orderCounter.HasPendingOrder ? WorkerState.MoveToPartBin : WorkerState.WaitForOrder;
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

        private AssemblyBench FindBenchWithProductReady()
        {
            if (assemblyBenches == null || assemblyBenches.Length == 0)
            {
                return assemblyBench != null && assemblyBench.HasProductReady ? assemblyBench : null;
            }

            AssemblyBench nearestBench = null;
            float nearestDistance = float.MaxValue;
            for (int i = 0; i < assemblyBenches.Length; i++)
            {
                AssemblyBench bench = assemblyBenches[i];
                if (!IsBenchUsable(bench) || !bench.HasProductReady)
                {
                    continue;
                }

                float distance = (transform.position - bench.WorkPoint.position).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestBench = bench;
                }
            }

            return nearestBench;
        }

        private AssemblyBench FindBenchReadyForPart()
        {
            if (assemblyBenches == null || assemblyBenches.Length == 0)
            {
                return assemblyBench != null && IsBenchReadyForPart(assemblyBench) ? assemblyBench : null;
            }

            AssemblyBench nearestBench = null;
            float nearestDistance = float.MaxValue;
            for (int i = 0; i < assemblyBenches.Length; i++)
            {
                AssemblyBench bench = assemblyBenches[i];
                if (!IsBenchReadyForPart(bench))
                {
                    continue;
                }

                float distance = (transform.position - bench.WorkPoint.position).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestBench = bench;
                }
            }

            return nearestBench;
        }

        private static bool IsBenchReadyForPart(AssemblyBench bench)
        {
            return IsBenchUsable(bench) && !bench.IsAssembling && !bench.HasProductReady;
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
