using TinyFactory.Economy;
using TinyFactory.Items;
using UnityEngine;

namespace TinyFactory.Stations
{
    public sealed class SellCounter : MonoBehaviour, IStationStatusProvider
    {
        [SerializeField] private Transform workPoint;
        [SerializeField] private MoneyManager moneyManager;
        [SerializeField] private int saleValue = 5;
        [SerializeField] private int stationLevel = 1;
        [SerializeField] private int saleValueLevel = 1;
        [SerializeField] private int totalSold;

        public Transform WorkPoint => workPoint != null ? workPoint : transform;
        public int TotalSold => totalSold;
        public int SaleValue => saleValue;
        public int StationLevel => stationLevel;
        public int SaleValueLevel => saleValueLevel;
        public string StatusText => "Lv " + stationLevel + " / Sale value: " + saleValue + " / Sold: " + totalSold;

        private void Awake()
        {
            if (moneyManager == null)
            {
                moneyManager = FindFirstObjectByType<MoneyManager>();
            }
        }

        public bool TrySell(CarryHolder holder)
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

            Item soldItem = holder.Drop();
            if (soldItem != null)
            {
                Destroy(soldItem.gameObject);
            }

            totalSold++;
            if (moneyManager != null)
            {
                moneyManager.AddMoney(saleValue);
            }

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
