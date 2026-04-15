using UnityEngine;

namespace TinyFactory.Items
{
    public static class ItemVisualFactory
    {
        public static Item CreateCircuitBoardPart(string itemName, Material baseMaterial, Vector3 scale)
        {
            GameObject root = new GameObject(itemName);
            root.transform.localScale = scale;

            CreateBlock(root.transform, "PCB", new Vector3(1f, 0.18f, 0.72f), new Vector3(0f, 0f, 0f), baseMaterial, new Color(0.08f, 0.55f, 0.42f));
            CreateBlock(root.transform, "Connector", new Vector3(0.18f, 0.22f, 0.78f), new Vector3(0.43f, 0.03f, 0f), null, new Color(0.95f, 0.78f, 0.28f));
            CreateBlock(root.transform, "Chip_A", new Vector3(0.26f, 0.18f, 0.24f), new Vector3(-0.18f, 0.12f, -0.12f), null, new Color(0.04f, 0.06f, 0.07f));
            CreateBlock(root.transform, "Chip_B", new Vector3(0.18f, 0.16f, 0.18f), new Vector3(0.12f, 0.11f, 0.18f), null, new Color(0.05f, 0.08f, 0.1f));

            Item item = root.AddComponent<Item>();
            item.Configure(ItemType.Part, itemName);
            return item;
        }

        public static Item CreatePowerBankProduct(string itemName, Material baseMaterial, Vector3 scale)
        {
            GameObject root = new GameObject(itemName);
            root.transform.localScale = scale;

            CreateBlock(root.transform, "Body", new Vector3(1f, 0.22f, 0.62f), Vector3.zero, baseMaterial, new Color(0.82f, 0.86f, 0.9f));
            CreateBlock(root.transform, "Screen", new Vector3(0.46f, 0.04f, 0.32f), new Vector3(-0.13f, 0.14f, 0f), null, new Color(0.1f, 0.16f, 0.22f));
            CreateBlock(root.transform, "Port", new Vector3(0.11f, 0.06f, 0.24f), new Vector3(0.5f, 0.08f, 0f), null, new Color(0.02f, 0.03f, 0.04f));
            CreateBlock(root.transform, "Button", new Vector3(0.12f, 0.05f, 0.12f), new Vector3(0.2f, 0.15f, 0.21f), null, new Color(0.16f, 0.35f, 0.72f));

            Item item = root.AddComponent<Item>();
            item.Configure(ItemType.Product, itemName);
            return item;
        }

        private static GameObject CreateBlock(Transform parent, string name, Vector3 localScale, Vector3 localPosition, Material material, Color fallbackColor)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent, false);
            block.transform.localPosition = localPosition;
            block.transform.localScale = localScale;

            Collider collider = block.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            Renderer renderer = block.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (material != null)
                {
                    renderer.sharedMaterial = material;
                }
                else
                {
                    renderer.material.color = fallbackColor;
                }
            }

            return block;
        }
    }
}
