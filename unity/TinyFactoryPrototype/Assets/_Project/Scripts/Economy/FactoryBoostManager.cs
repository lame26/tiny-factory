using System;
using UnityEngine;

namespace TinyFactory.Economy
{
    public sealed class FactoryBoostManager : MonoBehaviour
    {
        private static FactoryBoostManager s_runtimeInstance;

        [SerializeField] private float activeSeconds = 12f;
        [SerializeField] private float cooldownSeconds = 24f;
        [SerializeField] private float moveSpeedMultiplier = 1.6f;
        [SerializeField] private float assemblySpeedMultiplier = 1.8f;
        [SerializeField] private float packingSpeedMultiplier = 1.9f;
        [SerializeField] private float dispatchSpeedMultiplier = 0.65f;
        [SerializeField] private float activeTimer;
        [SerializeField] private float cooldownTimer;
        [SerializeField] private string lastMessage = "Boost ready";

        public event Action BoostStateChanged;

        public bool IsBoostActive => activeTimer > 0f;
        public bool IsOnCooldown => !IsBoostActive && cooldownTimer > 0f;
        public float RemainingActiveSeconds => Mathf.Max(0f, activeTimer);
        public float RemainingCooldownSeconds => Mathf.Max(0f, cooldownTimer);
        public float MoveSpeedMultiplier => IsBoostActive ? Mathf.Max(1f, moveSpeedMultiplier) : 1f;
        public float AssemblySpeedMultiplier => IsBoostActive ? Mathf.Max(1f, assemblySpeedMultiplier) : 1f;
        public float PackingSpeedMultiplier => IsBoostActive ? Mathf.Max(1f, packingSpeedMultiplier) : 1f;
        public float DispatchSpeedMultiplier => IsBoostActive ? Mathf.Clamp(dispatchSpeedMultiplier, 0.2f, 1f) : 1f;
        public string StatusText => IsBoostActive
            ? "Active " + RemainingActiveSeconds.ToString("0.0") + "s"
            : (IsOnCooldown ? "Cooldown " + RemainingCooldownSeconds.ToString("0.0") + "s" : "Ready");
        public string LastMessage => string.IsNullOrWhiteSpace(lastMessage) ? "Boost ready" : lastMessage;

        public static FactoryBoostManager GetOrCreate()
        {
            FactoryBoostManager existing = FindFirstObjectByType<FactoryBoostManager>();
            if (existing != null)
            {
                return existing;
            }

            if (s_runtimeInstance != null)
            {
                return s_runtimeInstance;
            }

            GameObject runtimeObject = new GameObject("FactoryBoostManager");
            s_runtimeInstance = runtimeObject.AddComponent<FactoryBoostManager>();
            return s_runtimeInstance;
        }

        private void Awake()
        {
            if (s_runtimeInstance == null)
            {
                s_runtimeInstance = this;
            }
        }

        private void Update()
        {
            if (activeTimer > 0f)
            {
                activeTimer -= Time.deltaTime;
                if (activeTimer > 0f)
                {
                    return;
                }

                activeTimer = 0f;
                cooldownTimer = Mathf.Max(0f, cooldownSeconds);
                lastMessage = "Boost cooling down.";
                BoostStateChanged?.Invoke();
                return;
            }

            if (cooldownTimer <= 0f)
            {
                return;
            }

            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer > 0f)
            {
                return;
            }

            cooldownTimer = 0f;
            lastMessage = "Boost ready.";
            BoostStateChanged?.Invoke();
        }

        private void OnDestroy()
        {
            if (s_runtimeInstance == this)
            {
                s_runtimeInstance = null;
            }
        }

        public bool TryActivateBoost()
        {
            if (IsBoostActive)
            {
                lastMessage = "Boost already active.";
                return false;
            }

            if (IsOnCooldown)
            {
                lastMessage = "Boost cooling down.";
                return false;
            }

            activeTimer = Mathf.Max(1f, activeSeconds);
            cooldownTimer = 0f;
            lastMessage = "Factory boost active.";
            BoostStateChanged?.Invoke();
            return true;
        }
    }
}
