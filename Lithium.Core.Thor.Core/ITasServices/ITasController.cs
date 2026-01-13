namespace Lithium.Core.Thor.Core
{
    public interface ITasController : ITasService
    {
        bool IsRunning { get; }
        void StartTas(int saveIndex);
        void StopTas();
        bool Playback(string playbackDir);
        void PlayAtNormalSpeed();
        void PlayAtFrameSpeed();
    }
}