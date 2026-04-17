using UnityEngine;

namespace TinyFactory.Core
{
    public sealed class SupportBonusSlots : MonoBehaviour
    {
        [SerializeField] private float tipBonusPercent;
        [SerializeField] private int temporaryHelperSlots;
        [SerializeField] private float equipmentMoveSpeedBonusPercent;
        [SerializeField] private float equipmentSaleValueBonusPercent;
        [SerializeField] private float equipmentAssemblySpeedBonusPercent;

        public float TipBonusMultiplier => 1f + Mathf.Max(0f, tipBonusPercent) * 0.01f;
        public int TemporaryHelperSlots => Mathf.Max(0, temporaryHelperSlots);
        public float EquipmentMoveSpeedMultiplier => 1f + Mathf.Max(0f, equipmentMoveSpeedBonusPercent) * 0.01f;
        public float EquipmentSaleValueMultiplier => 1f + Mathf.Max(0f, equipmentSaleValueBonusPercent) * 0.01f;
        public float EquipmentAssemblySpeedMultiplier => 1f + Mathf.Max(0f, equipmentAssemblySpeedBonusPercent) * 0.01f;

        public void ApplyEquipmentBonuses(float moveSpeedBonusPercent, float saleValueBonusPercent, float assemblySpeedBonusPercent)
        {
            equipmentMoveSpeedBonusPercent = Mathf.Max(0f, moveSpeedBonusPercent);
            equipmentSaleValueBonusPercent = Mathf.Max(0f, saleValueBonusPercent);
            equipmentAssemblySpeedBonusPercent = Mathf.Max(0f, assemblySpeedBonusPercent);
        }
    }
}
