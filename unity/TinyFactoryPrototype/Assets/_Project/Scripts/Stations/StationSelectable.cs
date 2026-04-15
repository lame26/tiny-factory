using TinyFactory.Interaction;
using UnityEngine;

namespace TinyFactory.Stations
{
    public sealed class StationSelectable : MonoBehaviour, ISelectable
    {
        [SerializeField] private string displayName = "Station";
        [SerializeField] private string stationRole = "Placeholder station";
        [SerializeField] private Renderer[] highlightRenderers;
        [SerializeField] private Color selectedTint = new Color(1f, 0.92f, 0.35f, 1f);

        private Color[] originalColors;
        private IStationStatusProvider statusProvider;

        public string DisplayName => displayName;
        public string StatusText => statusProvider != null ? statusProvider.StatusText : stationRole;

        private void Awake()
        {
            statusProvider = GetComponent<IStationStatusProvider>();

            if (highlightRenderers == null || highlightRenderers.Length == 0)
            {
                highlightRenderers = GetComponentsInChildren<Renderer>();
            }

            originalColors = new Color[highlightRenderers.Length];
            for (int i = 0; i < highlightRenderers.Length; i++)
            {
                if (highlightRenderers[i] != null && TryGetColor(highlightRenderers[i].material, out Color originalColor))
                {
                    originalColors[i] = originalColor;
                }
            }
        }

        public void Select(StationSelectionController selectionController)
        {
            for (int i = 0; i < highlightRenderers.Length; i++)
            {
                Renderer targetRenderer = highlightRenderers[i];
                if (targetRenderer != null)
                {
                    SetColor(targetRenderer.material, selectedTint);
                }
            }
        }

        public void Deselect(StationSelectionController selectionController)
        {
            for (int i = 0; i < highlightRenderers.Length; i++)
            {
                Renderer targetRenderer = highlightRenderers[i];
                if (targetRenderer != null)
                {
                    SetColor(targetRenderer.material, originalColors[i]);
                }
            }
        }

        private static bool TryGetColor(Material material, out Color color)
        {
            if (material.HasProperty("_BaseColor"))
            {
                color = material.GetColor("_BaseColor");
                return true;
            }

            if (material.HasProperty("_Color"))
            {
                color = material.GetColor("_Color");
                return true;
            }

            color = Color.white;
            return false;
        }

        private static void SetColor(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
                return;
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }
    }
}
