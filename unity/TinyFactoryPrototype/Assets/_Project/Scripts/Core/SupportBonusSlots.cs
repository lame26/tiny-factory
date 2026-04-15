using UnityEngine;

namespace TinyFactory.Core
{
    public sealed class SupportBonusSlots : MonoBehaviour
    {
        [SerializeField] private float tipBonusPercent;
        [SerializeField] private int temporaryHelperSlots;
        [SerializeField] private float equipmentMoveSpeedBonusPercent;
        [SerializeField] private float equipmentSaleValueBonusPercent;

        public float TipBonusMultiplier => 1f + Mathf.Max(0f, tipBonusPercent) * 0.01f;
        public int TemporaryHelperSlots => Mathf.Max(0, temporaryHelperSlots);
        public float EquipmentMoveSpeedMultiplier => 1f + Mathf.Max(0f, equipmentMoveSpeedBonusPercent) * 0.01f;
        public float EquipmentSaleValueMultiplier => 1f + Mathf.Max(0f, equipmentSaleValueBonusPercent) * 0.01f;
    }
}
