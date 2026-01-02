namespace Lithium.Core.Thor.Core
{
    public interface ITasPopupService : ITasService
    {
        void Hide();
        void Show();
        bool IsValid();
    }
}