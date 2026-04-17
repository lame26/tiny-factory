using TinyFactory.Items;
using TinyFactory.Economy;
using UnityEngine;

namespace TinyFactory.Stations
{
    public sealed class PackingStation : MonoBehaviour, IStationStatusProvider
    {
        private enum ReservationKind
        {
            None,
            ProductDelivery,
            ProductPickup
        }

        [SerializeField] private Transform workPoint;
        [SerializeField] private Transform productPoint;
        [SerializeField] private float packingSeconds = 1.4f;
        [SerializeField] private int stationLevel = 1;
        [SerializeField] private string packagedProductName = "Packed Gadget";
        [SerializeField] private Material packagedMaterial;
        [SerializeField] private Vector3 packagedScale = new Vector3(0.46f, 0.34f, 0.46f);
        [SerializeField] private int completedPackCount;
        [SerializeField] private float lastCompletedPackSeconds = -1f;
        [SerializeField] private ProductProgressionManager productProgressionManager;
        [SerializeField] private FactoryBoostManager factoryBoostManager;

        private float packingTimer;
        private Item storedPackagedProduct;
        private int reservedWorkerId;
        private ReservationKind reservationKind;
        private float idleSinceTime;
        private float productReadySinceTime = -1f;
        private float currentPackDuration;

        public Transform WorkPoint => workPoint != null ? workPoint : transform;
        public bool IsPacking => packingTimer > 0f;
        public bool HasPackagedProductReady => storedPackagedProduct != null;
        public float RemainingPackSeconds => Mathf.Max(0f, packingTimer);
        public int StationLevel => stationLevel;
        public int CompletedPackCount => Mathf.Max(0, completedPackCount);
        public float LastCompletedPackSeconds => lastCompletedPackSeconds;
        public float IdleDuration => CanAcceptProductNow() ? Mathf.Max(0f, Time.timeSinceLevelLoad - idleSinceTime) : 0f;
        public float ProductReadyDuration => HasPackagedProductReady && productReadySinceTime >= 0f
            ? Mathf.Max(0f, Time.timeSinceLevelLoad - productReadySinceTime)
            : 0f;
        public string StatusText => HasPackagedProductReady
            ? "Lv " + stationLevel + " / Packed ready: " + GetCurrentPackedProductName()
            : (IsPacking
                ? "Lv " + stationLevel + " / Packing: " + packingTimer.ToString("0.0") + "s"
                : "Lv " + stationLevel + " / Waiting for product / Pack " + packingSeconds.ToString("0.0") + "s");

        private void Awake()
        {
            if (productProgressionManager == null)
            {
                productProgressionManager = ProductProgressionManager.GetOrCreate();
            }

            if (factoryBoostManager == null)
            {
                factoryBoostManager = FactoryBoostManager.GetOrCreate();
            }

            idleSinceTime = Time.timeSinceLevelLoad;
        }

        private void Update()
        {
            if (packingTimer <= 0f)
            {
                return;
            }

            packingTimer -= Time.deltaTime;
            if (packingTimer > 0f)
            {
                return;
            }

            packingTimer = 0f;
            completedPackCount++;
            lastCompletedPackSeconds = currentPackDuration;
            currentPackDuration = 0f;
            storedPackagedProduct = ItemVisualFactory.CreatePackedProduct(GetCurrentPackedProductName(), packagedMaterial, packagedScale);
            productReadySinceTime = Time.timeSinceLevelLoad;
            ClearReservation();
            storedPackagedProduct.transform.SetParent(transform, false);
            storedPackagedProduct.transform.position = productPoint != null
                ? productPoint.position
                : transform.position + Vector3.up * 0.8f + Vector3.forward * 0.18f;
        }

        public bool TryReserveProductDelivery(int workerId)
        {
            if (workerId == 0 || IsPacking || HasPackagedProductReady)
            {
                return false;
            }

            if (reservationKind != ReservationKind.None && reservedWorkerId != workerId)
            {
                return false;
            }

            reservedWorkerId = workerId;
            reservationKind = ReservationKind.ProductDelivery;
            return true;
        }

        public bool TryReserveProductPickup(int workerId)
        {
            if (workerId == 0 || !HasPackagedProductReady)
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

        public bool IsReservedForProductDeliveryBy(int workerId)
        {
            return reservationKind == ReservationKind.ProductDelivery && reservedWorkerId == workerId;
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

        public bool TryAcceptProduct(CarryHolder holder)
        {
            if (holder == null || !holder.HasItem || IsPacking || HasPackagedProductReady)
            {
                return false;
            }

            Item carriedItem = holder.CarriedItem;
            if (carriedItem.ItemType != ItemType.Product)
            {
                return false;
            }

            Item consumedProduct = holder.Drop();
            if (consumedProduct != null)
            {
                Destroy(consumedProduct.gameObject);
            }

            productReadySinceTime = -1f;
            ClearReservation();
            float boostMultiplier = factoryBoostManager != null ? factoryBoostManager.PackingSpeedMultiplier : 1f;
            currentPackDuration = Mathf.Max(0.1f, packingSeconds / Mathf.Max(0.1f, boostMultiplier));
            packingTimer = currentPackDuration;
            return true;
        }

        public bool TryTakePackagedProduct(CarryHolder holder)
        {
            if (holder == null || holder.HasItem || storedPackagedProduct == null)
            {
                return false;
            }

            Item product = storedPackagedProduct;
            storedPackagedProduct = null;
            productReadySinceTime = -1f;
            idleSinceTime = Time.timeSinceLevelLoad;
            ClearReservation();
            return holder.TryPickup(product);
        }

        public void UpgradeStationLevel()
        {
            stationLevel++;
            packingSeconds = Mathf.Max(0.45f, packingSeconds * 0.86f);
        }

        private void ClearReservation()
        {
            reservedWorkerId = 0;
            reservationKind = ReservationKind.None;
        }

        private bool CanAcceptProductNow()
        {
            return !IsPacking && !HasPackagedProductReady;
        }

        private string GetCurrentPackedProductName()
        {
            return productProgressionManager != null ? productProgressionManager.PackedProductName : packagedProductName;
        }
    }
}
