using System;
using TinyFactory.Economy;
using TinyFactory.Items;
using UnityEngine;

namespace TinyFactory.Stations
{
    public sealed class PickupCounter : MonoBehaviour, IStationStatusProvider
    {
        [SerializeField] private Transform workPoint;
        [SerializeField] private MoneyManager moneyManager;
        [SerializeField] private int saleValue = 5;
        [SerializeField] private int stationLevel = 1;
        [SerializeField] private int saleValueLevel = 1;
        [SerializeField] private int completedOrderCount;

        public event Action<int, Vector3> PickupCompleted;

        public Transform WorkPoint => workPoint != null ? workPoint : transform;
        public int CompletedOrderCount => completedOrderCount;
        public int SaleValue => saleValue;
        public int StationLevel => stationLevel;
        public int SaleValueLevel => saleValueLevel;
        public string StatusText => "Lv " + stationLevel + " / Pickup value: " + saleValue + " / Done: " + completedOrderCount;

        private void Awake()
        {
            if (moneyManager == null)
            {
                moneyManager = FindFirstObjectByType<MoneyManager>();
            }
        }

        public bool TryCompletePickup(CarryHolder holder)
        {
            if (holder == null || !holder.HasItem)
            {
                return false;
            }

            Item carriedItem = holder.CarriedItem;
            if (carriedItem.ItemType != ItemType.Product)
            {
                return false;
            }

            Item completedItem = holder.Drop();
            if (completedItem != null)
            {
                Destroy(completedItem.gameObject);
            }

            completedOrderCount++;
            if (moneyManager != null)
            {
                moneyManager.AddMoney(saleValue);
            }

            PickupCompleted?.Invoke(saleValue, WorkPoint.position);
            return true;
        }

        public void UpgradeStationLevel()
        {
            stationLevel++;
            saleValue += 1;
        }

        public void UpgradeSaleValue()
        {
            saleValueLevel++;
            saleValue += 3;
        }
    }
}
