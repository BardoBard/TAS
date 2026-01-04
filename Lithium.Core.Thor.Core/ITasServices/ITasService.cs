using Thor.Core;

namespace Lithium.Core.Thor.Core
{
    public interface ITasService : IService
    {
        /// <summary>
        /// Initializes the service, use this instead of InitializeAsync for TAS services.
        /// </summary>
        /// <returns> True if initialization was successful, false otherwise. </returns>
        bool Initialize();
    }
}