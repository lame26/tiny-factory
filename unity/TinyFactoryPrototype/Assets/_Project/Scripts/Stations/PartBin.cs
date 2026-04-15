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

        public Transform WorkPoint => workPoint != null ? workPoint : transform;
        public string StatusText => "Ready: " + partName;

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
            return ItemVisualFactory.CreateCircuitBoardPart("Carried_" + partName, partMaterial, itemScale);
        }
    }
}
