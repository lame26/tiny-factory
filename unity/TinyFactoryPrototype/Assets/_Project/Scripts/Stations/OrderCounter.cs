using UnityEngine;

namespace TinyFactory.Stations
{
    public sealed class OrderCounter : MonoBehaviour, IStationStatusProvider
    {
        [SerializeField] private string requestedProductName = "Basic Gadget";
        [SerializeField] private int pendingOrderCount;
        [SerializeField] private int maxPendingOrders = 3;
        [SerializeField] private float secondsBetweenOrders = 6f;

        private float orderTimer;

        public int PendingOrderCount => pendingOrderCount;
        public string RequestedProductName => requestedProductName;
        public bool HasPendingOrder => pendingOrderCount > 0;
        public string StatusText => "Orders: " + pendingOrderCount + "/" + maxPendingOrders + " / Wants: " + requestedProductName;

        private void Update()
        {
            if (pendingOrderCount >= maxPendingOrders)
            {
                orderTimer = 0f;
                return;
            }

            orderTimer += Time.deltaTime;
            if (orderTimer < secondsBetweenOrders)
            {
                return;
            }

            orderTimer = 0f;
            pendingOrderCount++;
        }

        public bool TryReserveOrder()
        {
            if (pendingOrderCount <= 0)
            {
                return false;
            }

            pendingOrderCount--;
            return true;
        }
    }
}
