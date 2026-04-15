using System.Collections.Generic;
using TinyFactory.Economy;
using TinyFactory.Interaction;
using TinyFactory.Stations;
using TinyFactory.Workers;
using UnityEngine;

namespace TinyFactory.UI
{
    public sealed class HudController : MonoBehaviour
    {
        [SerializeField] private MoneyManager moneyManager;
        [SerializeField] private StationSelectionController selectionController;
        [SerializeField] private AssemblyBench assemblyBench;
        [SerializeField] private PickupCounter pickupCounter;
        [SerializeField] private UpgradeManager upgradeManager;
        [SerializeField] private WorkerManager workerManager;
        [SerializeField] private FacilityManager facilityManager;
        [SerializeField] private Rect panelRect = new Rect(12f, 12f, 360f, 340f);
        [SerializeField] private float pickupFeedbackSeconds = 1.4f;

        private readonly List<PickupFeedback> pickupFeedbacks = new List<PickupFeedback>();
        private GUIStyle feedbackStyle;

        private struct PickupFeedback
        {
            public string Text;
            public Vector3 WorldPosition;
            public float StartTime;
            public float EndTime;
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (pickupCounter != null)
            {
                pickupCounter.PickupCompleted += HandlePickupCompleted;
            }
        }

        private void OnDisable()
        {
            if (pickupCounter != null)
            {
                pickupCounter.PickupCompleted -= HandlePickupCompleted;
            }
        }

        private void ResolveReferences()
        {
            if (moneyManager == null)
            {
                moneyManager = FindFirstObjectByType<MoneyManager>();
            }

            if (selectionController == null)
            {
                selectionController = FindFirstObjectByType<StationSelectionController>();
            }

            if (assemblyBench == null)
            {
                assemblyBench = FindFirstObjectByType<AssemblyBench>();
            }

            if (pickupCounter == null)
            {
                pickupCounter = FindFirstObjectByType<PickupCounter>();
            }

            if (upgradeManager == null)
            {
                upgradeManager = FindFirstObjectByType<UpgradeManager>();
            }

            if (workerManager == null)
            {
                workerManager = FindFirstObjectByType<WorkerManager>();
            }

            if (facilityManager == null)
            {
                facilityManager = FindFirstObjectByType<FacilityManager>();
            }
        }

        private void OnGUI()
        {
            Rect actualPanelRect = new Rect(panelRect.x, panelRect.y, panelRect.width, Mathf.Max(panelRect.height, 486f));
            GUILayout.BeginArea(actualPanelRect, GUI.skin.box);
            GUILayout.Label("Tiny Factory");
            GUILayout.Space(4f);

            GUILayout.Label("Money: " + GetMoneyText());
            DrawRecommendedGoal();
            GUILayout.Space(6f);

            GUILayout.Label("Selected: " + GetSelectedName());
            GUILayout.Label(GetSelectedStatus());
            GUILayout.Space(4f);

            GUILayout.Label("Assembly: " + GetAssemblyStatus());
            GUILayout.Space(8f);

            DrawUpgradeButton("Level Selected Station", upgradeManager != null ? upgradeManager.StationLevelCost : 0, () => upgradeManager.TryUpgradeSelectedStation());
            DrawUpgradeButton("Assembly Speed", upgradeManager != null ? upgradeManager.AssemblySpeedCost : 0, () => upgradeManager.TryUpgradeAssemblySpeed());
            DrawUpgradeButton("Sale Value", upgradeManager != null ? upgradeManager.SaleValueCost : 0, () => upgradeManager.TryUpgradeSaleValue());

            GUILayout.Space(6f);
            GUILayout.Label("Workers: " + GetWorkerText());
            DrawWorkerButton(GetHireWorkerLabel(), workerManager != null ? workerManager.HireWorkerCost : 0, () => workerManager.TryHireWorker());
            DrawWorkerButton("Worker Throughput", workerManager != null ? workerManager.WorkerThroughputCost : 0, () => workerManager.TryUpgradeWorkerThroughput());

            GUILayout.Space(6f);
            GUILayout.Label("Facilities: " + GetFacilityText());
            DrawFacilityButton("Build Assembly Bench", facilityManager != null ? facilityManager.BuildAssemblyBenchCost : 0, () => facilityManager.TryBuildAssemblyBench());

            GUILayout.Space(6f);
            GUILayout.Label(upgradeManager != null ? upgradeManager.LastMessage : "Upgrade system missing");
            GUILayout.Label(workerManager != null ? workerManager.LastMessage : "Worker system missing");
            GUILayout.Label(facilityManager != null ? facilityManager.LastMessage : "Facility system missing");
            GUILayout.EndArea();

            DrawPickupFeedbacks();
        }

        private void DrawUpgradeButton(string label, int cost, System.Func<bool> onClick)
        {
            GUI.enabled = upgradeManager != null;
            if (GUILayout.Button(label + " ($" + MoneyFormatter.Format(cost) + ")", GUILayout.Height(28f)))
            {
                onClick();
            }
            GUI.enabled = true;
        }

        private void DrawWorkerButton(string label, int cost, System.Func<bool> onClick)
        {
            GUI.enabled = workerManager != null;
            if (GUILayout.Button(label + " ($" + MoneyFormatter.Format(cost) + ")", GUILayout.Height(28f)))
            {
                onClick();
            }
            GUI.enabled = true;
        }

        private void DrawFacilityButton(string label, int cost, System.Func<bool> onClick)
        {
            GUI.enabled = facilityManager != null && facilityManager.CanBuildAssemblyBench;
            string suffix = facilityManager != null && facilityManager.CanBuildAssemblyBench
                ? " ($" + MoneyFormatter.Format(cost) + ")"
                : " (Max)";
            if (GUILayout.Button(label + suffix, GUILayout.Height(28f)))
            {
                onClick();
            }
            GUI.enabled = true;
        }

        private string GetMoneyText()
        {
            return moneyManager != null ? MoneyFormatter.Format(moneyManager.CurrentMoney) : "0";
        }

        private string GetSelectedName()
        {
            return selectionController != null ? selectionController.CurrentSelectionName : "None";
        }

        private string GetSelectedStatus()
        {
            return selectionController != null ? selectionController.CurrentSelectionStatus : "No station selected";
        }

        private string GetAssemblyStatus()
        {
            return assemblyBench != null ? assemblyBench.StatusText : "Missing Assembly Bench";
        }

        private string GetWorkerText()
        {
            if (workerManager == null)
            {
                return "Missing Worker Manager";
            }

            return workerManager.WorkerCount + " / Speed x" + workerManager.WorkerMoveSpeed.ToString("0.00");
        }

        private string GetFacilityText()
        {
            if (facilityManager == null)
            {
                return "Missing Facility Manager";
            }

            return "Assembly Benches " + facilityManager.ActiveAssemblyBenchCount + "/" + facilityManager.TotalAssemblyBenchCount;
        }

        private string GetHireWorkerLabel()
        {
            if (workerManager != null && workerManager.WorkerCount < 2)
            {
                return "Hire Worker - Goal";
            }

            return "Hire Worker";
        }

        private void DrawRecommendedGoal()
        {
            string title = "Goal: Build cash for first worker";
            string detail = "Worker system missing";
            float progress = 0f;

            if (workerManager != null && moneyManager != null)
            {
                if (workerManager.WorkerCount < 2)
                {
                    int cost = workerManager.HireWorkerCost;
                    progress = cost > 0 ? Mathf.Clamp01((float)moneyManager.CurrentMoney / cost) : 1f;
                    detail = MoneyFormatter.Format(moneyManager.CurrentMoney) + " / " + MoneyFormatter.Format(cost) + " for Hire Worker";
                }
                else if (facilityManager != null && facilityManager.CanBuildAssemblyBench)
                {
                    int cost = facilityManager.BuildAssemblyBenchCost;
                    progress = cost > 0 ? Mathf.Clamp01((float)moneyManager.CurrentMoney / cost) : 1f;
                    title = "Goal: Build second assembly bench";
                    detail = MoneyFormatter.Format(moneyManager.CurrentMoney) + " / " + MoneyFormatter.Format(cost) + " for Assembly Bench";
                }
                else if (upgradeManager != null && assemblyBench != null && assemblyBench.AssemblySpeedLevel < 3)
                {
                    int cost = upgradeManager.AssemblySpeedCost;
                    progress = cost > 0 ? Mathf.Clamp01((float)moneyManager.CurrentMoney / cost) : 1f;
                    title = "Goal: Speed up assembly";
                    detail = MoneyFormatter.Format(moneyManager.CurrentMoney) + " / " + MoneyFormatter.Format(cost) + " for Assembly Speed";
                }
                else if (upgradeManager != null)
                {
                    int cost = upgradeManager.SaleValueCost;
                    progress = cost > 0 ? Mathf.Clamp01((float)moneyManager.CurrentMoney / cost) : 1f;
                    title = "Goal: Raise pickup value";
                    detail = MoneyFormatter.Format(moneyManager.CurrentMoney) + " / " + MoneyFormatter.Format(cost) + " for Pickup Value";
                }
            }

            GUILayout.Label(title);
            Rect barRect = GUILayoutUtility.GetRect(1f, 12f, GUILayout.ExpandWidth(true));
            GUI.Box(barRect, GUIContent.none);
            Rect fillRect = new Rect(barRect.x + 2f, barRect.y + 2f, Mathf.Max(0f, (barRect.width - 4f) * progress), barRect.height - 4f);
            Color oldColor = GUI.color;
            GUI.color = new Color(0.35f, 0.8f, 0.55f, 1f);
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
            GUI.color = oldColor;
            GUILayout.Label(detail);
        }

        private void HandlePickupCompleted(int amount, Vector3 worldPosition)
        {
            pickupFeedbacks.Add(new PickupFeedback
            {
                Text = "+$" + MoneyFormatter.Format(amount),
                WorldPosition = worldPosition + Vector3.up * 0.8f,
                StartTime = Time.time,
                EndTime = Time.time + pickupFeedbackSeconds
            });
        }

        private void DrawPickupFeedbacks()
        {
            if (pickupFeedbacks.Count == 0)
            {
                return;
            }

            if (feedbackStyle == null)
            {
                feedbackStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 18,
                    fontStyle = FontStyle.Bold
                };
            }

            Camera feedbackCamera = Camera.main;
            for (int i = pickupFeedbacks.Count - 1; i >= 0; i--)
            {
                PickupFeedback feedback = pickupFeedbacks[i];
                if (Time.time >= feedback.EndTime)
                {
                    pickupFeedbacks.RemoveAt(i);
                    continue;
                }

                float normalizedAge = Mathf.InverseLerp(feedback.StartTime, feedback.EndTime, Time.time);
                Vector3 screenPosition = feedbackCamera != null
                    ? feedbackCamera.WorldToScreenPoint(feedback.WorldPosition)
                    : new Vector3(panelRect.x + panelRect.width * 0.5f, Screen.height - panelRect.y - 36f, 1f);

                if (screenPosition.z < 0f)
                {
                    continue;
                }

                float yOffset = normalizedAge * 32f;
                Rect feedbackRect = new Rect(screenPosition.x - 50f, Screen.height - screenPosition.y - 20f - yOffset, 100f, 28f);
                Color oldColor = GUI.color;
                feedbackStyle.normal.textColor = new Color(0.25f, 0.95f, 0.48f, 1f - normalizedAge);
                GUI.Label(feedbackRect, feedback.Text, feedbackStyle);
                GUI.color = oldColor;
            }
        }
    }
}
