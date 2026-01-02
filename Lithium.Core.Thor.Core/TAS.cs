using System.Collections.Generic;
using HarmonyLib;
using Thor.Core;
using UnityEngine;

namespace Lithium.Core.Thor.Core
{
    public static class TAS
    {
        public static AssetReference<PopupData> TasPopupData;

        private static bool m_initialized = false;
        private static bool m_opened = false;

        public static void AwakeTas()
        {
            new Harmony("com.tas").PatchAll();
            
            Services.Events.RegisterGameEvent(GameEventType.BootComplete, (gameEvent) =>
            {
                Debug.Log("[Tas]: Initializing TAS services...");
                ServicesTas.AddService<ITasLogService>(new TasLogService());
                ServicesTas.AddService<ITasReflectionService>(new TasReflectionService());
                ServicesTas.AddService<ITasPopupService>(new TasPopupService());
                Debug.Log("[Tas]: Core TAS services initialized");
                m_initialized = true;
            });
        }

        public static void StartTas()
        {
            // Create a detached task to initialize TAS services
            Services.Events.RegisterGameEvent(GameEventType.BootComplete, (gameEvent) =>
            {
                if (!m_initialized)
                {
                    Debug.LogError("[Tas]: TAS services not initialized. Call AwakeTas() first.");
                    return;
                }
                
                Debug.Log("[Tas]: Starting TAS services initialization...");
                var services = new List<ITasService>();
                ServicesTas.GetAllTasServices(services);

                foreach (var service in services)
                    InitializeService(service);
            });
        }

        public static void UpdateTas()
        {
            if (Services.Input.IsKeyDown(KeyCode.F1))
            {

                if (m_opened)
                {
                    m_opened = false;
                    ServicesTas.Popup.Hide();
                    return;
                }

                ServicesTas.Popup.Show();
                m_opened = true;
            }
        }

        private static void InitializeService(ITasService service)
        {
            var startTime = Time.realtimeSinceStartup;
            service.Initialize();
            Debug.LogFormat("[Tas]: [Timing] Loading {0} took {1}s", service.Name,
                Time.realtimeSinceStartup - startTime);
        }
    }
}