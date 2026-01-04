using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Thor;
using Thor.Core;
using TMPro;
using UnityEngine;
using HarmonyLib;
using UnityEngine.EventSystems;

namespace Lithium.Core.Thor.Core
{
    public class TasPopupService : ITasPopupService, ITasService
    {
        public string Name => "TasPopupService";
        public float LoadProgress => 1f;

        private UIButton m_pauseButton; // moneyplease
        private UIButton m_seedButton; // hpimmune

        private TMP_InputField m_seedInput; // seed input

        private PopupData m_consolePopupData;
        private ConsolePopup m_consolePopup;

        [HarmonyPatch(typeof(ConsolePopup), "MoneyPlease")]
        public class MoneyPleaseWatcher
        {
            static void Prefix(BaseEventData eventData)
            {
                ServicesTas.Log.Log("[TasPopupService]: MoneyPlease button pressed");
            }
        }
        public bool Initialize()
        {
            if (!ServicesTas.TasReflection.GetFieldValue(Services.Pop, "m_consolePopup",
                    out AssetReference<PopupData> existingConsolePopupData))
            {
                ServicesTas.Log.Log($"[{Name}]: Failed to get existing ConsolePopupData reference");
                return false;
            }

            if (!ServicesTas.TasReflection.DeepCopyAssetReference(existingConsolePopupData, out var copiedConsolePopupData))
            {
                ServicesTas.Log.Log($"[{Name}]: Failed to copy ConsolePopup data");
                return false;
            }

            if (!copiedConsolePopupData.IsValid)
            {
                ServicesTas.Log.Log($"[{Name}]: Copied PopupData is not valid"); 
                return false;
            }

            m_consolePopupData = copiedConsolePopupData.Asset;

            if (!ServicesTas.TasReflection.GetFieldValue(m_consolePopupData, "m_popup",
                    out PrefabReference<Popup> copyTasPopup))
            {
                ServicesTas.Log.Log($"[{Name}]: Failed to get copy of ConsolePopup");
                return false;
            }

            m_consolePopup = copyTasPopup.Asset as ConsolePopup;
            InitializeButtons();
            return true;
        }

        public bool IsValid()
        {
            return m_consolePopupData != null && m_consolePopup != null;
        }

        public void Show()
        {
            if (Services.Players.PrimaryPlayer == null || !IsValid())
                return;

            Services.Pop.ShowPopup(m_consolePopupData, new PopupParams()
            {
                Owner = Services.Players.PrimaryPlayer.SimEntity
            }, out _);
        }

        private void SetButtonText(UIButton button, string newText)
        {
            foreach (Transform child in button.transform)
            {
                var textComponent = child.GetComponent<TMPro.TMP_Text>();
                if (textComponent == null) continue;
                textComponent.text = newText;
                break;
            }
        }

        private void InitializeButtons()
        {
            if (!ServicesTas.TasReflection.GetFieldValue(m_consolePopup, "m_moneyPleaseButton", out m_pauseButton))
            {
                ServicesTas.Log.Log($"[{Name}]: Failed to copy m_moneyPleaseButton data");
                return;
            }

            if (!ServicesTas.TasReflection.GetFieldValue(m_consolePopup, "m_hpImmuneButton", out m_seedButton))
            {
                ServicesTas.Log.Log($"[{Name}]: Failed to copy m_hpImmuneButton data");
                return;
            }
            if (!ServicesTas.TasReflection.GetFieldValue(m_consolePopup, "m_seedInput", out m_seedInput))
            {
                ServicesTas.Log.Log($"[{Name}]: Failed to copy m_seedInput data");
                return;
            }

            SetButtonText(m_pauseButton, "Pause TAS");
            SetButtonText(m_seedButton, "Set Seed TAS");

            m_pauseButton.OnClicked = new UIButton.ButtonEvent();
            m_pauseButton.OnClicked.AddListener((eventData) =>
            {
                ServicesTas.Log.Log("[TasPopupService]: Pause TAS button clicked");
            });
        }
        
        // IService 
        
        public void CollectDebugState(Dictionary<string, object> debugStateProperties)
        {
        }

        public void Shutdown()
        {
        }
        
        public IEnumerator InitializeAsync()
        {
            yield return null;
        }
    }
}