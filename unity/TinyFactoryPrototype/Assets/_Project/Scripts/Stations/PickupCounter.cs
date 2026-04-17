using System;
using TinyFactory.Core;
using TinyFactory.Economy;
using TinyFactory.Items;
using UnityEngine;

namespace TinyFactory.Stations
{
    public sealed class PickupCounter : MonoBehaviour, IStationStatusProvider
    {
        private sealed class ShipmentEntry
        {
            public int Payout;
            public int BonusValue;
            public string OrderLabel;
            public float DispatchSeconds;
        }

        [SerializeField] private Transform workPoint;
        [SerializeField] private MoneyManager moneyManager;
        [SerializeField] private int saleValue = 5;
        [SerializeField] private ProductProgressionManager productProgressionManager;
        [SerializeField] private SupportBonusSlots supportBonusSlots;
        [SerializeField] private FactoryBoostManager factoryBoostManager;
        [SerializeField] private int stationLevel = 1;
        [SerializeField] private int saleValueLevel = 1;
        [SerializeField] private int completedOrderCount;
        [SerializeField] private int totalOrderBonusEarned;
        [SerializeField] private string lastCompletedOrderLabel = "None";
        [SerializeField] private int lastShipmentPayout;
        [SerializeField] private int lastShipmentBonusValue;
        [SerializeField] private int lastShipmentBaseValue;
        [SerializeField] private int packagedProductBonusValue = 8;
        [SerializeField] private float standardDispatchSeconds = 0.55f;
        [SerializeField] private float rushDispatchSeconds = 0.3f;
        [SerializeField] private float bulkDispatchSeconds = 0.8f;

        public event Action<int, string, Vector3> PickupCompleted;

        private readonly System.Collections.Generic.Queue<ShipmentEntry> queuedShipments = new System.Collections.Generic.Queue<ShipmentEntry>();
        private ShipmentEntry activeShipment;
        private float dispatchTimer;
        private float dispatchSpeedMultiplier = 1f;
        private int dispatchSupportBonus;

        public Transform WorkPoint => workPoint != null ? workPoint : transform;
        public int CompletedOrderCount => completedOrderCount;
        public int SaleValue => productProgressionManager != null ? productProgressionManager.CurrentPickupValue : saleValue;
        public int StationLevel => stationLevel;
        public int SaleValueLevel => productProgressionManager != null ? productProgressionManager.ProductLevel : saleValueLevel;
        public int TotalOrderBonusEarned => totalOrderBonusEarned;
        public string LastCompletedOrderLabel => string.IsNullOrWhiteSpace(lastCompletedOrderLabel) ? "None" : lastCompletedOrderLabel;
        public int LastShipmentPayout => Mathf.Max(0, lastShipmentPayout);
        public int LastShipmentBonusValue => Mathf.Max(0, lastShipmentBonusValue);
        public int LastShipmentBaseValue => Mathf.Max(0, lastShipmentBaseValue);
        public int PackagedProductBonusValue => Mathf.Max(0, packagedProductBonusValue);
        public int QueuedShipmentCount => queuedShipments.Count + (activeShipment != null ? 1 : 0);
        public string CurrentDispatchLabel => activeShipment != null ? activeShipment.OrderLabel : "Idle";
        public string StatusText => "Lv " + stationLevel + " / Product value: " + SaleValue + " / Queue: " + QueuedShipmentCount + " / Dispatch: " + CurrentDispatchLabel + " / Done: " + completedOrderCount;

        private void Awake()
        {
            if (moneyManager == null)
            {
                moneyManager = FindFirstObjectByType<MoneyManager>();
            }

            if (productProgressionManager == null)
            {
                productProgressionManager = ProductProgressionManager.GetOrCreate();
            }

            if (supportBonusSlots == null)
            {
                supportBonusSlots = FindFirstObjectByType<SupportBonusSlots>();
            }

            if (factoryBoostManager == null)
            {
                factoryBoostManager = FactoryBoostManager.GetOrCreate();
            }
        }

        private void Update()
        {
            if (activeShipment == null)
            {
                if (queuedShipments.Count <= 0)
                {
                    return;
                }

                activeShipment = queuedShipments.Dequeue();
                dispatchTimer = Mathf.Max(0.05f, activeShipment.DispatchSeconds);
            }

            dispatchTimer -= Time.deltaTime;
            if (dispatchTimer > 0f)
            {
                return;
            }

            FinalizeShipment(activeShipment);
            activeShipment = null;
            dispatchTimer = 0f;
        }

        public bool TryCompletePickup(CarryHolder holder, OrderCounter.OrderType orderType = OrderCounter.OrderType.Standard, int orderBonus = 0, string orderLabel = "Standard")
        {
            if (holder == null || !holder.HasItem)
            {
                return false;
            }

            Item carriedItem = holder.CarriedItem;
            if (carriedItem.ItemType != ItemType.Product && carriedItem.ItemType != ItemType.PackagedProduct)
            {
                return false;
            }

            bool requiresPackagedProduct = orderType == OrderCounter.OrderType.Gift;
            if (requiresPackagedProduct && carriedItem.ItemType != ItemType.PackagedProduct)
            {
                return false;
            }

            Item completedItem = holder.Drop();
            if (completedItem != null)
            {
                Destroy(completedItem.gameObject);
            }

            int packagedBonus = carriedItem.ItemType == ItemType.PackagedProduct ? packagedProductBonusValue : 0;
            int sanitizedOrderBonus = Mathf.Max(0, orderBonus) + packagedBonus;
            float equipmentMultiplier = supportBonusSlots != null ? supportBonusSlots.EquipmentSaleValueMultiplier : 1f;
            int baseValue = SaleValue + sanitizedOrderBonus + dispatchSupportBonus;
            int pickupValue = Mathf.Max(1, Mathf.RoundToInt(baseValue * equipmentMultiplier));
            queuedShipments.Enqueue(new ShipmentEntry
            {
                Payout = pickupValue,
                BonusValue = sanitizedOrderBonus,
                OrderLabel = string.IsNullOrWhiteSpace(orderLabel) ? "Standard" : orderLabel,
                DispatchSeconds = GetDispatchSeconds(orderType)
            });
            return true;
        }

        public void UpgradeStationLevel()
        {
            stationLevel++;
        }

        public void UpgradeSaleValue()
        {
            if (productProgressionManager != null)
            {
                productProgressionManager.TryLevelUpProduct();
                return;
            }

            saleValueLevel++;
            saleValue += 3;
        }

        public void ApplyDispatchSupport(float speedMultiplier)
        {
            dispatchSpeedMultiplier = Mathf.Min(dispatchSpeedMultiplier, Mathf.Clamp(speedMultiplier, 0.2f, 1f));
        }

        public void ApplyDispatchSupportBonus(int bonusPerShipment)
        {
            dispatchSupportBonus = Mathf.Max(dispatchSupportBonus, bonusPerShipment);
        }

        private float GetDispatchSeconds(OrderCounter.OrderType orderType)
        {
            float baseSeconds;
            switch (orderType)
            {
                case OrderCounter.OrderType.Rush:
                    baseSeconds = rushDispatchSeconds;
                    break;
                case OrderCounter.OrderType.Gift:
                    baseSeconds = standardDispatchSeconds * 0.85f;
                    break;
                case OrderCounter.OrderType.Bulk:
                    baseSeconds = bulkDispatchSeconds;
                    break;
                default:
                    baseSeconds = standardDispatchSeconds;
                    break;
            }

            float boostMultiplier = factoryBoostManager != null ? factoryBoostManager.DispatchSpeedMultiplier : 1f;
            return baseSeconds * dispatchSpeedMultiplier * boostMultiplier;
        }

        private void FinalizeShipment(ShipmentEntry shipment)
        {
            if (shipment == null)
            {
                return;
            }

            completedOrderCount++;
            totalOrderBonusEarned += shipment.BonusValue;
            lastCompletedOrderLabel = string.IsNullOrWhiteSpace(shipment.OrderLabel) ? "Standard" : shipment.OrderLabel;
            lastShipmentPayout = Mathf.Max(0, shipment.Payout);
            lastShipmentBonusValue = Mathf.Max(0, shipment.BonusValue);
            lastShipmentBaseValue = Mathf.Max(0, shipment.Payout - shipment.BonusValue);
            if (moneyManager != null)
            {
                moneyManager.AddMoney(shipment.Payout);
            }

            PickupCompleted?.Invoke(shipment.Payout, lastCompletedOrderLabel, WorkPoint.position);
        }
    }
}
