namespace Lithium.Core.Thor.Core
{
    /// <summary>
    /// Service for showing popups to the user.
    /// </summary>
    public interface ITasPopupService : ITasService
    {
        /// <summary>
        /// Shows the popup service UI.
        /// </summary>
        /// <remarks> There is no Close/Hide method, as the user is expected to close the popup via the UI itself. </remarks>
        void Show();
        
        /// <summary>
        /// Returns whether the popup service is in a valid state to show popups.
        /// </summary>
        /// <returns> True if the popup service is valid, false otherwise. </returns>
        bool IsValid();
    }
}