using TinyFactory.Items;
using TinyFactory.Economy;
using UnityEngine;

namespace TinyFactory.Stations
{
    public sealed class AssemblyBench : MonoBehaviour, IStationStatusProvider
    {
        private static AssemblyBench s_priorityBench;

        private enum ReservationKind
        {
            None,
            PartDelivery,
            ProductPickup
        }

        [SerializeField] private Transform workPoint;
        [SerializeField] private Transform productPoint;
        [SerializeField] private float assemblySeconds = 2.5f;
        [SerializeField] private int stationLevel = 1;
        [SerializeField] private int assemblySpeedLevel = 1;
        [SerializeField] private string productName = "Basic Gadget";
        [SerializeField] private Material productMaterial;
        [SerializeField] private Vector3 productScale = new Vector3(0.42f, 0.28f, 0.42f);
        [SerializeField] private int completedAssemblyCount;
        [SerializeField] private float lastCompletedAssemblySeconds = -1f;
        [SerializeField] private ProductProgressionManager productProgressionManager;

        private float assemblyTimer;
        private Item storedProduct;
        private int reservedWorkerId;
        private ReservationKind reservationKind;
        private float idleSinceTime;
        private float productReadySinceTime = -1f;
        private float currentAssemblyDuration;

        public Transform WorkPoint => workPoint != null ? workPoint : transform;
        public bool IsAssembling => assemblyTimer > 0f;
        public bool HasProductReady => storedProduct != null;
        public float RemainingAssemblySeconds => Mathf.Max(0f, assemblyTimer);
        public float AssemblySeconds => assemblySeconds;
        public int CompletedAssemblyCount => Mathf.Max(0, completedAssemblyCount);
        public float LastCompletedAssemblySeconds => lastCompletedAssemblySeconds;
        public int StationLevel => stationLevel;
        public int AssemblySpeedLevel => assemblySpeedLevel;
        public float IdleDuration => CanAcceptPartNow() ? Mathf.Max(0f, Time.timeSinceLevelLoad - idleSinceTime) : 0f;
        public float ProductReadyDuration => HasProductReady && productReadySinceTime >= 0f
            ? Mathf.Max(0f, Time.timeSinceLevelLoad - productReadySinceTime)
            : 0f;
        public bool IsPriorityFocus => ReferenceEquals(s_priorityBench, this);
        public static AssemblyBench PriorityBench => s_priorityBench;

        public string StatusText
        {
            get
            {
                string priorityLabel = IsPriorityFocus ? " / Priority" : string.Empty;
                if (storedProduct != null)
                {
                    return "Lv " + stationLevel + " / Product ready: " + GetCurrentProductName() + priorityLabel;
                }

                if (assemblyTimer > 0f)
                {
                    return "Lv " + stationLevel + " / Assembling: " + assemblyTimer.ToString("0.0") + "s" + priorityLabel;
                }

                return "Lv " + stationLevel + " / Waiting for part / Build " + assemblySeconds.ToString("0.0") + "s" + priorityLabel;
            }
        }

        private void Update()
        {
            if (assemblyTimer <= 0f)
            {
                return;
            }

            assemblyTimer -= Time.deltaTime;
            if (assemblyTimer <= 0f)
            {
                assemblyTimer = 0f;
                completedAssemblyCount++;
                lastCompletedAssemblySeconds = currentAssemblyDuration;
                currentAssemblyDuration = 0f;
                storedProduct = ItemVisualFactory.CreatePowerBankProduct(GetCurrentProductName(), productMaterial, productScale);
                productReadySinceTime = Time.timeSinceLevelLoad;
                ClearReservation();
                storedProduct.transform.SetParent(transform, false);
                storedProduct.transform.position = productPoint != null
                    ? productPoint.position
                    : transform.position + Vector3.up * 0.85f + Vector3.forward * 0.2f;
            }
        }

        private void Awake()
        {
            if (productProgressionManager == null)
            {
                productProgressionManager = ProductProgressionManager.GetOrCreate();
            }

            idleSinceTime = Time.timeSinceLevelLoad;
        }

        private void OnDisable()
        {
            if (ReferenceEquals(s_priorityBench, this))
            {
                s_priorityBench = null;
            }
        }

        private void OnDestroy()
        {
            if (ReferenceEquals(s_priorityBench, this))
            {
                s_priorityBench = null;
            }
        }

        public void SetPriorityFocus(bool enabled)
        {
            if (!enabled)
            {
                if (ReferenceEquals(s_priorityBench, this))
                {
                    s_priorityBench = null;
                }

                return;
            }

            s_priorityBench = this;
        }

        public void TogglePriorityFocus()
        {
            SetPriorityFocus(!IsPriorityFocus);
        }

        public bool TryReservePartDelivery(int workerId)
        {
            if (workerId == 0 || IsAssembling || HasProductReady)
            {
                return false;
            }

            if (reservationKind != ReservationKind.None && reservedWorkerId != workerId)
            {
                return false;
            }

            reservedWorkerId = workerId;
            reservationKind = ReservationKind.PartDelivery;
            return true;
        }

        public bool TryReserveProductPickup(int workerId)
        {
            if (workerId == 0 || !HasProductReady)
            {
                return false;
            }

            if (reservationKind != ReservationKind.None && reservedWorkerId != workerId)
            {
                return false;
            }

            reservedWorkerId = workerId;
            reservationKind = ReservationKind.ProductPickup;
            return true;
        }

        public bool IsReservedForPartDeliveryBy(int workerId)
        {
            return reservationKind == ReservationKind.PartDelivery && reservedWorkerId == workerId;
        }

        public bool IsReservedForProductPickupBy(int workerId)
        {
            return reservationKind == ReservationKind.ProductPickup && reservedWorkerId == workerId;
        }

        public void ReleaseReservation(int workerId)
        {
            if (reservedWorkerId != workerId)
            {
                return;
            }

            ClearReservation();
        }

        public bool TryAcceptPart(CarryHolder holder, float assemblySpeedMultiplier = 1f)
        {
            if (holder == null || !holder.HasItem || IsAssembling || HasProductReady)
            {
                return false;
            }

            Item carriedItem = holder.CarriedItem;
            if (carriedItem.ItemType != ItemType.Part)
            {
                return false;
            }

            Item consumedPart = holder.Drop();
            if (consumedPart != null)
            {
                Destroy(consumedPart.gameObject);
            }

            productReadySinceTime = -1f;
            ClearReservation();
            currentAssemblyDuration = Mathf.Max(0.1f, assemblySeconds / Mathf.Max(0.1f, assemblySpeedMultiplier));
            assemblyTimer = currentAssemblyDuration;
            return true;
        }

        public bool TryTakeProduct(CarryHolder holder)
        {
            if (holder == null || holder.HasItem || storedProduct == null)
            {
                return false;
            }

            Item product = storedProduct;
            storedProduct = null;
            productReadySinceTime = -1f;
            idleSinceTime = Time.timeSinceLevelLoad;
            ClearReservation();
            return holder.TryPickup(product);
        }

        public void UpgradeStationLevel()
        {
            stationLevel++;
            assemblySeconds = Mathf.Max(0.5f, assemblySeconds - 0.15f);
        }

        public void UpgradeAssemblySpeed()
        {
            assemblySpeedLevel++;
            assemblySeconds = Mathf.Max(0.5f, assemblySeconds * 0.8f);
        }

        private void ClearReservation()
        {
            reservedWorkerId = 0;
            reservationKind = ReservationKind.None;
        }

        private bool CanAcceptPartNow()
        {
            return !IsAssembling && !HasProductReady;
        }

        private string GetCurrentProductName()
        {
            return productProgressionManager != null ? productProgressionManager.ProductName : productName;
        }
    }
}
