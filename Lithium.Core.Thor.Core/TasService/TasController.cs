using System;
using System.Collections;
using System.Collections.Generic;
using Thor;
using Thor.Core;
using UnityEngine;

namespace Lithium.Core.Thor.Core
{
    public class TasController : TasService, ITasController
    {
        private enum TasState
        {
            Stopped,
            Running,
            Paused
        }

        private enum TasMode
        {
            FrameByFrame,
            NormalSpeed,
            Playback
        }
        
        [Flags]
        private enum FrameAdvanceState
        {
            None = 0x00,
            Requested = 0x01,
            Advancing = 0x02
        }
        
        public string Name => "TasController";
        public float LoadProgress => 1f;
        public bool IsRunning => m_tasState == TasState.Running;
        private TasState m_tasState = TasState.Stopped;
        private TasMode m_tasMode = TasMode.NormalSpeed;
        private FrameAdvanceState m_frameAdvanceState = FrameAdvanceState.None;

        public bool Initialize()
        {
            return true;
        }

        public void Update()
        {
            if (!IsRunning || !CanMove())
                return;

            if (m_tasMode != TasMode.Playback)
            {
                if (!AdvancingThisFrame())
                    return;
                
            }
        }
        
        private void ResumeGame() => Time.timeScale = 1f;

        private void PauseGame() => Time.timeScale = 0f;

        private bool AdvancingThisFrame()
        {
            if (m_frameAdvanceState == FrameAdvanceState.Requested && m_frameAdvanceState != FrameAdvanceState.Advancing)
            {
                m_frameAdvanceState |= FrameAdvanceState.Advancing;
                ResumeGame();
                return true;
            }

            PauseGame();
            m_frameAdvanceState = FrameAdvanceState.None;

            if (m_tasMode == TasMode.NormalSpeed || (Services.Input.IsKeyDown(KeyCode.F2) && m_frameAdvanceState != FrameAdvanceState.Requested))
                m_frameAdvanceState |= FrameAdvanceState.Requested;

            return false;
        }
        private bool CanMove()
        {
            return !Services.State.IsInState(IStateService.GameState.Loading) &&
                   !Services.State.IsInState(IStateService.GameState.Paused) &&
                   !Services.State.IsInState(IStateService.GameState.Cutscene) &&
                   Services.Players.PrimaryPlayer.SimEntity != null &&
                   Services.Players.PrimaryPlayer.SimEntity.StateController.IsNot(SimEntityStateController.State
                       .Spawning);
        }

        public void StartTas(int saveIndex)
        {
            if (!Services.Saves.Load(saveIndex))
            {
                TasServices.Log.Log($"[{Name}]: Failed to load save slot {saveIndex}.");
                return;
            }
            
            Reset();
            PauseGame();
            m_tasState = TasState.Running;
        }

        public void StopTas()
        {
            Reset();
            ResumeGame();
        }

        public bool Playback(string playbackDir)
        {
            return true;
        }

        public void PlayAtNormalSpeed() => m_tasMode = TasMode.NormalSpeed;

        public void PlayAtFrameSpeed() => m_tasMode = TasMode.FrameByFrame;

        private void Reset()
        {
            m_tasState = TasState.Stopped;
            m_tasMode = TasMode.FrameByFrame;
            m_frameAdvanceState = FrameAdvanceState.None;
        }
        
        // IService
        
        public IEnumerator InitializeAsync() => null;

        public void CollectDebugState(Dictionary<string, object> debugStateProperties)
        {
        }

        public void Shutdown()
        {
        }
    }
}