namespace Lithium.Core.Thor.Core
{
    public interface ITasController : ITasService
    {
        enum TasState
        {
            Stopped,
            Running,
            Paused
        }
        enum TasPlayMode
        {
            FrameByFrame,
            NormalSpeed
        }
        
        bool IsRunning { get; }
        
        void StartTas(int saveIndex);
        void StopTas();
        
        bool SetState(TasState state);
        bool SetPlayMode(TasPlayMode playMode);
        
        bool Playback(string playbackDir);
    }
}