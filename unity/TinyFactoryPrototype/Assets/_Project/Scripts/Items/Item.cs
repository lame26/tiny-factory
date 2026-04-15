using UnityEngine;

namespace TinyFactory.Items
{
    public sealed class Item : MonoBehaviour
    {
        [SerializeField] private ItemType itemType = ItemType.Part;
        [SerializeField] private string displayName = "Basic Part";

        public ItemType ItemType => itemType;
        public string DisplayName => displayName;

        public void Configure(ItemType type, string itemDisplayName)
        {
            itemType = type;
            displayName = itemDisplayName;
        }
    }
}
