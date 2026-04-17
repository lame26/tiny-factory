using UnityEngine;

namespace TinyFactory.Stations
{
    public sealed class OrderCounter : MonoBehaviour, IStationStatusProvider
    {
        public enum OrderType
        {
            Standard,
            Rush,
            Bulk,
            Gift
        }

        public enum OrderFocusMode
        {
            Balanced,
            RushFocus,
            BulkFocus
        }

        public readonly struct OrderReservation
        {
            public OrderReservation(OrderType type, int bonusValue, string label)
            {
                Type = type;
                BonusValue = bonusValue;
                Label = label;
            }

            public OrderType Type { get; }
            public int BonusValue { get; }
            public string Label { get; }
        }

        private sealed class ActiveOrder
        {
            public OrderType Type;
            public int RemainingUnits;
            public int BonusValue;
            public int Sequence;
        }

        [SerializeField] private string requestedProductName = "Basic Gadget";
        [SerializeField] private int maxPendingOrders = 3;
        [SerializeField] private float secondsBetweenOrders = 6f;
        [SerializeField] private OrderFocusMode focusMode = OrderFocusMode.Balanced;
        [SerializeField] private bool giftOrdersUnlocked;

        private readonly System.Collections.Generic.List<ActiveOrder> activeOrders = new System.Collections.Generic.List<ActiveOrder>();
        private float orderTimer;
        private int generatedOrderCount;
        private int extraPendingOrderCapacity;
        private float orderIntervalMultiplier = 1f;

        public int PendingOrderCount
        {
            get
            {
                int totalUnits = 0;
                for (int i = 0; i < activeOrders.Count; i++)
                {
                    totalUnits += activeOrders[i].RemainingUnits;
                }

                return totalUnits;
            }
        }

        public int ActiveOrderCount => activeOrders.Count;
        public int MaxPendingOrders => maxPendingOrders + extraPendingOrderCapacity;
        public float EffectiveSecondsBetweenOrders => secondsBetweenOrders * Mathf.Max(0.2f, orderIntervalMultiplier);
        public string RequestedProductName => requestedProductName;
        public bool HasPendingOrder => PendingOrderCount > 0;
        public OrderFocusMode FocusMode => focusMode;
        public string FocusModeLabel => GetFocusModeLabel(focusMode);
        public string NextOrderSummary => TryGetHighestPriorityOrder(out ActiveOrder order)
            ? GetOrderLabel(order.Type) + " x" + order.RemainingUnits + " / Bonus $" + order.BonusValue
            : "None";
        public string StatusText => "Orders: " + PendingOrderCount + "/" + MaxPendingOrders + " / Next: " + NextOrderSummary + " / Focus: " + FocusModeLabel;

        private void Update()
        {
            if (PendingOrderCount >= MaxPendingOrders)
            {
                orderTimer = 0f;
                return;
            }

            orderTimer += Time.deltaTime;
            if (orderTimer < EffectiveSecondsBetweenOrders)
            {
                return;
            }

            orderTimer = 0f;
            GenerateNextOrder();
        }

        public bool TryReserveOrder()
        {
            return TryReserveOrder(out _);
        }

        public bool TryReserveOrder(out OrderReservation reservation)
        {
            if (!TryGetHighestPriorityOrder(out ActiveOrder order))
            {
                reservation = default;
                return false;
            }

            reservation = new OrderReservation(order.Type, order.BonusValue, GetOrderLabel(order.Type));
            order.RemainingUnits--;
            if (order.RemainingUnits <= 0)
            {
                activeOrders.Remove(order);
            }

            return true;
        }

        public void ApplyDispatchSupport(int capacityBonus, float intervalMultiplierFactor)
        {
            extraPendingOrderCapacity = Mathf.Max(extraPendingOrderCapacity, capacityBonus);
            orderIntervalMultiplier = Mathf.Min(orderIntervalMultiplier, intervalMultiplierFactor);
        }

        public void SetGiftOrdersUnlocked(bool unlocked)
        {
            giftOrdersUnlocked = unlocked;
        }

        public void CycleFocusMode()
        {
            switch (focusMode)
            {
                case OrderFocusMode.Balanced:
                    focusMode = OrderFocusMode.RushFocus;
                    break;
                case OrderFocusMode.RushFocus:
                    focusMode = OrderFocusMode.BulkFocus;
                    break;
                default:
                    focusMode = OrderFocusMode.Balanced;
                    break;
            }
        }

        private void GenerateNextOrder()
        {
            generatedOrderCount++;

            OrderType type = OrderType.Standard;
            int bonusValue = 0;
            int units = 1;
            int cycleIndex = (generatedOrderCount - 1) % 5;
            switch (cycleIndex)
            {
                case 1:
                    if (giftOrdersUnlocked)
                    {
                        type = OrderType.Gift;
                        bonusValue = 6;
                    }
                    break;
                case 2:
                    type = OrderType.Rush;
                    bonusValue = 3;
                    break;
                case 4:
                    type = OrderType.Bulk;
                    bonusValue = 1;
                    units = 2;
                    break;
            }

            int availableSlots = Mathf.Max(0, MaxPendingOrders - PendingOrderCount);
            if (availableSlots <= 0)
            {
                return;
            }

            activeOrders.Add(new ActiveOrder
            {
                Type = type,
                BonusValue = bonusValue,
                RemainingUnits = Mathf.Min(units, availableSlots),
                Sequence = generatedOrderCount
            });
        }

        private bool TryGetHighestPriorityOrder(out ActiveOrder order)
        {
            if (TryGetFocusedOrder(out order))
            {
                return true;
            }

            order = null;
            int bestPriority = int.MaxValue;
            int bestSequence = int.MaxValue;

            for (int i = 0; i < activeOrders.Count; i++)
            {
                ActiveOrder candidate = activeOrders[i];
                if (candidate == null || candidate.RemainingUnits <= 0)
                {
                    continue;
                }

                int priority = GetPriority(candidate.Type);
                if (priority > bestPriority)
                {
                    continue;
                }

                if (priority == bestPriority && candidate.Sequence >= bestSequence)
                {
                    continue;
                }

                bestPriority = priority;
                bestSequence = candidate.Sequence;
                order = candidate;
            }

            return order != null;
        }

        private bool TryGetFocusedOrder(out ActiveOrder order)
        {
            order = null;
            if (focusMode == OrderFocusMode.Balanced)
            {
                return false;
            }

            OrderType focusedType = focusMode == OrderFocusMode.RushFocus
                ? OrderType.Rush
                : OrderType.Bulk;

            int bestSequence = int.MaxValue;
            for (int i = 0; i < activeOrders.Count; i++)
            {
                ActiveOrder candidate = activeOrders[i];
                if (candidate == null || candidate.RemainingUnits <= 0 || candidate.Type != focusedType)
                {
                    continue;
                }

                if (candidate.Sequence >= bestSequence)
                {
                    continue;
                }

                bestSequence = candidate.Sequence;
                order = candidate;
            }

            return order != null;
        }

        private static int GetPriority(OrderType type)
        {
            switch (type)
            {
                case OrderType.Rush:
                    return 0;
                case OrderType.Gift:
                    return 1;
                case OrderType.Bulk:
                    return 2;
                default:
                    return 3;
            }
        }

        private static string GetOrderLabel(OrderType type)
        {
            switch (type)
            {
                case OrderType.Rush:
                    return "Rush";
                case OrderType.Gift:
                    return "Gift";
                case OrderType.Bulk:
                    return "Bulk";
                default:
                    return "Standard";
            }
        }

        private static string GetFocusModeLabel(OrderFocusMode mode)
        {
            switch (mode)
            {
                case OrderFocusMode.RushFocus:
                    return "Rush";
                case OrderFocusMode.BulkFocus:
                    return "Bulk";
                default:
                    return "Balanced";
            }
        }
    }
}
