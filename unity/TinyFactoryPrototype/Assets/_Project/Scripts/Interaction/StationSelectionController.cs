using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace TinyFactory.Interaction
{
    public sealed class StationSelectionController : MonoBehaviour
    {
        [SerializeField] private Camera selectionCamera;
        [SerializeField] private LayerMask selectableLayers = ~0;
        [SerializeField] private float maxRaycastDistance = 100f;
        [SerializeField] private bool showDebugPanel;

        private ISelectable currentSelection;

        public event Action<ISelectable> SelectionChanged;

        public ISelectable CurrentSelection => currentSelection;
        public string CurrentSelectionName => currentSelection?.DisplayName ?? "None";
        public string CurrentSelectionStatus => currentSelection?.StatusText ?? "No station selected";

        private void Awake()
        {
            if (selectionCamera == null)
            {
                selectionCamera = Camera.main;
            }
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            Mouse mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
            {
                return;
            }

            TrySelectFromPointer(mouse.position.ReadValue());
#else
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            TrySelectFromPointer(Input.mousePosition);
#endif
        }

        public bool TrySelectFromPointer(Vector3 screenPosition)
        {
            Camera rayCamera = selectionCamera != null ? selectionCamera : Camera.main;
            if (rayCamera == null)
            {
                return false;
            }

            Ray ray = rayCamera.ScreenPointToRay(screenPosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, selectableLayers, QueryTriggerInteraction.Ignore))
            {
                SetSelection(null);
                return false;
            }

            ISelectable selectable = FindSelectable(hit.collider);
            SetSelection(selectable);
            return selectable != null;
        }

        public void SetSelection(ISelectable selectable)
        {
            if (ReferenceEquals(currentSelection, selectable))
            {
                return;
            }

            currentSelection?.Deselect(this);
            currentSelection = selectable;
            currentSelection?.Select(this);
            SelectionChanged?.Invoke(currentSelection);

        }

        private static ISelectable FindSelectable(Component hitComponent)
        {
            MonoBehaviour[] behaviours = hitComponent.GetComponentsInParent<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is ISelectable selectable)
                {
                    return selectable;
                }
            }

            return null;
        }

        private void OnGUI()
        {
            if (!showDebugPanel)
            {
                return;
            }

            const float width = 260f;
            GUILayout.BeginArea(new Rect(12f, 12f, width, 84f), GUI.skin.box);
            GUILayout.Label("Selected:");
            GUILayout.Label(CurrentSelectionName);
            GUILayout.Label(CurrentSelectionStatus);
            GUILayout.EndArea();
        }
    }
}
