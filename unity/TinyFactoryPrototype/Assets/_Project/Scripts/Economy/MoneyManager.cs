using System;
using UnityEngine;

namespace TinyFactory.Economy
{
    public sealed class MoneyManager : MonoBehaviour
    {
        [SerializeField] private int currentMoney;
        [SerializeField] private bool showDebugPanel;

        public event Action<int> MoneyChanged;

        public int CurrentMoney => currentMoney;

        public void AddMoney(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            currentMoney += amount;
            MoneyChanged?.Invoke(currentMoney);
        }

        public void DebugAddMoney(int amount)
        {
            AddMoney(amount);
        }

        public bool CanSpend(int amount)
        {
            return amount >= 0 && currentMoney >= amount;
        }

        public bool TrySpend(int amount)
        {
            if (!CanSpend(amount))
            {
                return false;
            }

            currentMoney -= amount;
            MoneyChanged?.Invoke(currentMoney);
            return true;
        }

        private void OnGUI()
        {
            if (!showDebugPanel)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(12f, 106f, 180f, 44f), GUI.skin.box);
            GUILayout.Label("Money");
            GUILayout.Label(currentMoney.ToString());
            GUILayout.EndArea();
        }
    }
}
