using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lithium.Core.Thor.Core.VirtualInput;
using Thor.Core;
using UnityEngine;

namespace Lithium.Core.Thor.Core
{
    public class TasController : TasService, ITasController
    {
        public string Name => "TasController";
        public bool IsRunning => m_tasState == ITasController.TasState.Running;
        
        public string CurrentPlaybackDir => Path.Combine(TasServices.File.PathToTasDir, "Runs", m_currentDateTimeString);
        public string CurrentPlaybackFilePath => Path.Combine(TasServices.File.PathToTasDir, "Runs", m_currentDateTimeString, "CurrentPlayback.tas");
        
        private ITasController.TasState m_tasState = ITasController.TasState.Stopped;
        private ITasController.TasPlayMode m_tasPlayMode = ITasController.TasPlayMode.NormalSpeed;
        private TasMode m_tasMode = TasMode.Playing;
        private FrameAdvanceState m_frameAdvanceState = FrameAdvanceState.None;
        
        private bool m_skipFirstFrame = true;
        private StreamReader m_playbackReader;
        private readonly string m_currentDateTimeString = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        private readonly Dictionary<KeyCode, VirtualController.Button> m_keyToElementIndex = new()
        {
            { KeyCode.W, VirtualController.Button.W },
            { KeyCode.S, VirtualController.Button.S },
            { KeyCode.A, VirtualController.Button.A },
            { KeyCode.D, VirtualController.Button.D },
            { KeyCode.Space, VirtualController.Button.Space },
        };

        public void Update()
        {
            if (!IsRunning || !CanMove())
                return;

            if (m_skipFirstFrame)
            {
                m_skipFirstFrame = false;
                return;
            }
            
            switch (m_tasMode)
            {
                case TasMode.Playing when AdvancingThisFrame():
                    
                    var inputThisFrame = m_keyToElementIndex.Keys.Where(Input.GetKey).ToList();
                    TasServices.File.WriteToFile(CurrentPlaybackFilePath,
                        string.Join(",", inputThisFrame.Select(k => k.ToString())) + '\n', true);
                    
                    inputThisFrame.Clear();
                    break;
                case TasMode.Playback:
                    if (m_playbackReader.EndOfStream)
                    {
                        TasServices.Log.Log($"[{Name}]: Reached end of playback file, stopping TAS.");
                        StopTas();
                        return;
                    }

                    var line = m_playbackReader.ReadLine();
                    TasServices.Log.Log($"[{Name}]: Playback line: {line}");
                    
                    var button =
                        (from kvp in m_keyToElementIndex
                            let isPressed = line != null && line.Contains(kvp.Key.ToString())
                            where isPressed
                            select kvp).Aggregate(VirtualController.Button.None, (current, kvp) => current | kvp.Value);
                    
                    VirtualController.HandleMoveAndFace(Services.Players.PrimaryPlayer, button, Vector2.zero, out _);
                    break;
                default:
                    break;
            }
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
            m_tasState = ITasController.TasState.Running;
        }
        
        public bool Playback(string playbackDir)
        {
            if (!TasServices.File.ExistsDirectory(playbackDir))
            {
                TasServices.Log.Log($"[{Name}]: Playback directory '{playbackDir}' does not exist.");
                return false;
            }
            
            if (!Services.Saves.Load(1))
            {
                TasServices.Log.Log($"[{Name}]: Failed to load save slot 1 for TAS playback.");
                return false;
            }
            
            if (!TasServices.File.ExistsFile(Path.Combine(playbackDir, "CurrentPlayback.tas")))
            {
                TasServices.Log.Log($"[{Name}]: Playback file 'CurrentPlayback.tas' does not exist in directory '{playbackDir}'.");
                return false;
            }
            
            Reset();
            TasServices.Popup.Hide();
            m_playbackReader = new StreamReader(Path.Combine(playbackDir, "CurrentPlayback.tas"));
            m_tasMode = TasMode.Playback;
            m_tasState = ITasController.TasState.Running;
            return true;
        }
        
        public void StopTas()
        {
            Reset();
            ResumeGame();
        }
        
        public bool Initialize() => true;

        public bool SetState(ITasController.TasState state)
        {
            m_tasState = state;
            return true;
        }

        public bool SetPlayMode(ITasController.TasPlayMode playMode)
        {
            m_tasPlayMode = playMode;
            return true;
        }

        private bool CanMove()
        {
            return !Services.State.IsInState(IStateService.GameState.Loading) &&
                   !Services.State.IsInState(IStateService.GameState.Paused) &&
                   !Services.State.IsInState(IStateService.GameState.Cutscene) &&
                   !Services.State.IsInState(IStateService.GameState.Transitioning) &&
                   !Services.State.IsInState(IStateService.GameState.GameOver) &&
                   Services.Players.PrimaryPlayer.SimEntity != null &&
                   Services.Players.PrimaryPlayer.SimEntity.StateController.IsNot(SimEntityStateController.State
                       .Spawning) &&
                   Services.Players.PrimaryPlayer.SimEntity.StateController.IsNot(SimEntityStateController.State
                       .Teleporting);
        }

        private bool AdvancingThisFrame()
        {
            if (m_frameAdvanceState.HasFlag(FrameAdvanceState.Requested) &&
                !m_frameAdvanceState.HasFlag(FrameAdvanceState.Advancing))
            {
                m_frameAdvanceState |= FrameAdvanceState.Advancing;
                ResumeGame();
                return true;
            }

            PauseGame();
            m_frameAdvanceState = FrameAdvanceState.None;

            if (m_tasPlayMode == ITasController.TasPlayMode.NormalSpeed ||
                (Services.Input.IsKeyDown(KeyCode.F2) && !m_frameAdvanceState.HasFlag(FrameAdvanceState.Requested)))
                m_frameAdvanceState |= FrameAdvanceState.Requested;

            return false;
        }
        
        private void ResumeGame() => Time.timeScale = 1f;

        private void PauseGame() => Time.timeScale = 0f;

        private void Reset()
        {
            m_skipFirstFrame = true;
            m_tasState = ITasController.TasState.Stopped;
            m_tasMode = TasMode.Playing;
            m_tasPlayMode = ITasController.TasPlayMode.FrameByFrame;
            m_frameAdvanceState = FrameAdvanceState.None;
            m_playbackReader?.Close();
            m_playbackReader = null;
        }

        [Flags]
        private enum FrameAdvanceState
        {
            None = 0x00,
            Requested = 0x01,
            Advancing = 0x02
        }
        
        enum TasMode
        {
            Playing,
            Playback
        }
        
        // IService
        public float LoadProgress => 1f;
        public IEnumerator InitializeAsync() => null;

        public void CollectDebugState(Dictionary<string, object> debugStateProperties)
        {
        }

        public void Shutdown()
        {
        }
    }
}