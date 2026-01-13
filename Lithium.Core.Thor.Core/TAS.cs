using System.Collections.Generic;
using Thor.Core;
using UnityEngine;

namespace Lithium.Core.Thor.Core
{
    public static class TAS
    {
        private static bool m_initialized = false;
        private static List<ITasService> m_services = new List<ITasService>();

        public static void AwakeTas()
        {
            Services.Events.RegisterGameEvent(GameEventType.BootComplete, (gameEvent) =>
            {
                Debug.Log("[Tas]: Initializing TAS services...");
                TasServices.AddService<ITasFileService>(new TasFileService());
                TasServices.AddService<ITasLogService>(new TasLogService());
                TasServices.AddService<ITasReflectionService>(new TasReflectionService());
                TasServices.AddService<ITasController>(new TasController());
                TasServices.AddService<ITasPopupService>(new TasPopupService());
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
                TasServices.GetAllTasServices(m_services);

                foreach (var service in m_services)
                    InitializeService(service);
            });
        }

        public static void UpdateTas()
        {
            if (!m_initialized)
                return;

            foreach (var service in m_services) service.Update();

            if (Services.Input.IsKeyDown(KeyCode.F1))
            {
                if (TasServices.Popup.IsShowing())
                    TasServices.Popup.Hide();
                else
                    TasServices.Popup.Show();
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