using TinyFactory.Items;
using UnityEngine;

namespace TinyFactory.Stations
{
    public sealed class PartBin : MonoBehaviour, IStationStatusProvider
    {
        [SerializeField] private Transform workPoint;
        [SerializeField] private string partName = "Basic Gadget Part";
        [SerializeField] private Material partMaterial;
        [SerializeField] private Vector3 itemScale = new Vector3(0.32f, 0.32f, 0.32f);
        [SerializeField] private TinyFactory.Economy.ProductProgressionManager productProgressionManager;

        public Transform WorkPoint => workPoint != null ? workPoint : transform;
        public string StatusText => "Ready: " + GetCurrentPartName();

        private void Awake()
        {
            if (productProgressionManager == null)
            {
                productProgressionManager = TinyFactory.Economy.ProductProgressionManager.GetOrCreate();
            }
        }

        public bool TryTakePart(CarryHolder holder)
        {
            if (holder == null || holder.HasItem)
            {
                return false;
            }

            Item part = CreatePartItem();
            return holder.TryPickup(part);
        }

        private Item CreatePartItem()
        {
            string currentPartName = GetCurrentPartName();
            return ItemVisualFactory.CreateCircuitBoardPart("Carried_" + currentPartName, partMaterial, itemScale);
        }

        private string GetCurrentPartName()
        {
            return productProgressionManager != null ? productProgressionManager.PartName : partName;
        }
    }
}
