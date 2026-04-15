using UnityEngine;

namespace TinyFactory.Items
{
    public sealed class CarryHolder : MonoBehaviour
    {
        [SerializeField] private Transform holdPoint;
        [SerializeField] private Item carriedItem;

        public bool HasItem => carriedItem != null;
        public Item CarriedItem => carriedItem;

        public void SetHoldPoint(Transform targetHoldPoint)
        {
            holdPoint = targetHoldPoint != null ? targetHoldPoint : transform;
        }

        private void Awake()
        {
            if (holdPoint == null)
            {
                holdPoint = transform;
            }
        }

        public bool TryPickup(Item item)
        {
            if (item == null || carriedItem != null)
            {
                return false;
            }

            carriedItem = item;
            Transform itemTransform = item.transform;
            itemTransform.SetParent(holdPoint, false);
            itemTransform.localPosition = Vector3.zero;
            itemTransform.localRotation = Quaternion.identity;

            return true;
        }

        public Item Drop(Transform dropParent = null)
        {
            if (carriedItem == null)
            {
                return null;
            }

            Item droppedItem = carriedItem;
            carriedItem = null;
            droppedItem.transform.SetParent(dropParent, true);

            return droppedItem;
        }
    }
}
