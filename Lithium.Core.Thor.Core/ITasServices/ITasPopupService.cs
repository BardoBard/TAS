namespace Lithium.Core.Thor.Core
{
    /// <summary>
    /// Service for showing popups to the user.
    /// </summary>
    public interface ITasPopupService : ITasService
    {
        /// <summary>
        /// Hides the popup service UI.
        /// </summary>
        void Hide();
        
        /// <summary>
        /// Shows the popup service UI.
        /// </summary>
        void Show();
        
        /// <summary>
        /// Returns whether the popup service UI is currently showing.
        /// </summary>
        /// <returns> True if the popup service is showing, false otherwise. </returns>
        bool IsShowing();
        
        /// <summary>
        /// Returns whether the popup service is in a valid state to show popups.
        /// </summary>
        /// <returns> True if the popup service is valid, false otherwise. </returns>
        bool IsValid();
    }
}