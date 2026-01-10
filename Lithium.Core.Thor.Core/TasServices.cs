using System;
using System.Collections.Generic;
using Thor.Core;

namespace Lithium.Core.Thor.Core
{
    public static class TasServices
    {
        private static Dictionary<Type, ITasService> sServices = new Dictionary<Type, ITasService>();

        public static ITasFileService File => GetTasService<ITasFileService>();
        public static ITasLogService Log => GetTasService<ITasLogService>();
        public static ITasReflectionService Reflection => GetTasService<ITasReflectionService>();
        public static ITasController TasController => GetTasService<ITasController>();
        public static ITasPopupService Popup => GetTasService<ITasPopupService>();
        
        /// <summary>
        /// Checks if the service of the specified type exists.
        /// </summary>
        /// <typeparam name="T"> The type of the service. </typeparam>
        /// <returns> True if the service exists, false otherwise. </returns>
        public static bool HasTasService<T>() where T : ITasService => sServices.ContainsKey(typeof(T));

        /// <summary>
        /// Gets the service of the specified type.
        /// </summary>
        /// <typeparam name="T"> The type of the service. </typeparam>
        /// <returns> The service of the specified type. </returns>
        public static T GetService<T>() where T : IService
        {
            if (sServices.ContainsKey(typeof(T)))
                return (T)sServices[typeof(T)];

            if (HasTasService<ITasReflectionService>())
            {
                ITasReflectionService tasReflectionService = GetTasService<ITasReflectionService>();
                
                // Get function delegate for `private static T Get<T>() where T : IService`
                tasReflectionService.GetFunctionDelegate(typeof(Services), "Get", out Func<T> getServiceFunc);
                return getServiceFunc();
            }

            return default;
        }
        
        /// <summary>
        /// Gets the TasService of the specified type.
        /// </summary>
        /// <typeparam name="T"> The type of the service. </typeparam>
        /// <returns> The service of the specified type. </returns>
        public static T GetTasService<T>() where T : ITasService
        {
            return (T)sServices[typeof(T)];
        }
        
        /// <summary>
        /// Adds only the TasService to the internal list.
        /// </summary>
        /// <param name="service"> The service to add. </param>
        /// <typeparam name="T"> The type of the service. </typeparam>
        public static void AddTasService<T>(ITasService service)
        {
            sServices.Remove(typeof(T));
            sServices.Add(typeof(T), service);
        }
        
        /// <summary>
        /// Adds the TasService to the internal list and also to Thor.Services.
        /// </summary>
        /// <param name="service"> The service to add. </param>
        /// <typeparam name="T"> The type of the service. </typeparam>
        public static void AddService<T>(ITasService service)
        {
            sServices.Remove(typeof(T));
            sServices.Add(typeof(T), service);
            
            // Call to Thor.Services too
            Services.AddService<T>(service);
        }
        
        /// <summary>
        /// Gets all the TasServices only and adds them to the provided list.
        /// </summary>
        /// <param name="services"> The list to add the services to. </param>
        /// <returns> True if any services were added, false otherwise. </returns>
        public static bool GetAllTasServices(List<ITasService> services)
        {
            services.AddRange(sServices.Values);
            return services.Count > 0;
        }
        
        /// <summary>
        /// Adds the TasServices first to a list and then adds the Thor.Services
        /// </summary>
        /// <param name="services"> The list to add the services to. </param>
        /// <returns> True if any services were added, false otherwise. </returns>
        public static bool GetAllServices(List<IService> services)
        {
            services.AddRange(sServices.Values);
            
            // Add services from Thor.Services too
            Services.GetAllServices(services);
            return services.Count > 0;
        }
        
        /// <summary>
        /// Clears only the TasServices
        /// </summary>
        public static void ClearTasServices()
        {
            sServices.Clear();
        }
        
        /// <summary>
        /// Clears both the TasServices and the Thor.Services
        /// </summary>
        public static void ClearServices()
        {
            sServices.Clear();
            Services.ClearServices();
        }
    }
}