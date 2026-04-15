using TinyFactory.Items;
using UnityEngine;

namespace TinyFactory.Stations
{
    public sealed class AssemblyBench : MonoBehaviour, IStationStatusProvider
    {
        [SerializeField] private Transform workPoint;
        [SerializeField] private Transform productPoint;
        [SerializeField] private float assemblySeconds = 2.5f;
        [SerializeField] private int stationLevel = 1;
        [SerializeField] private int assemblySpeedLevel = 1;
        [SerializeField] private string productName = "Basic Gadget";
        [SerializeField] private Material productMaterial;
        [SerializeField] private Vector3 productScale = new Vector3(0.42f, 0.28f, 0.42f);

        private float assemblyTimer;
        private Item storedProduct;

        public Transform WorkPoint => workPoint != null ? workPoint : transform;
        public bool IsAssembling => assemblyTimer > 0f;
        public bool HasProductReady => storedProduct != null;
        public float AssemblySeconds => assemblySeconds;
        public int StationLevel => stationLevel;
        public int AssemblySpeedLevel => assemblySpeedLevel;

        public string StatusText
        {
            get
            {
                if (storedProduct != null)
                {
                    return "Lv " + stationLevel + " / Product ready: " + productName;
                }

                if (assemblyTimer > 0f)
                {
                    return "Lv " + stationLevel + " / Assembling: " + assemblyTimer.ToString("0.0") + "s";
                }

                return "Lv " + stationLevel + " / Waiting for part / Build " + assemblySeconds.ToString("0.0") + "s";
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
                storedProduct = ItemVisualFactory.CreatePowerBankProduct(productName, productMaterial, productScale);
                storedProduct.transform.SetParent(transform, false);
                storedProduct.transform.position = productPoint != null
                    ? productPoint.position
                    : transform.position + Vector3.up * 0.85f + Vector3.forward * 0.2f;
            }
        }

        public bool TryAcceptPart(CarryHolder holder)
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

            assemblyTimer = Mathf.Max(0.1f, assemblySeconds);
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
    }
}
