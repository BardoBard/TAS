using System;
using Thor.Core;
using UnityEngine;
using AsyncOperation = Thor.Core.AsyncOperation;

namespace Lithium.Core.Thor.Core.VirtualInput
{
    public static class VirtualController
    {
        [Flags]
        public enum Button
        {
            None = 0,
            W = 1,
            S = 2,
            A = 4,
            D = 8,
            Space = 16,
        }

        private static Vector3 ButtonToMoveDirection(Button button)
        {
            // Check things like wd, wa, etc
            Vector3 direction = Vector3.up;
            if (button.HasFlag(Button.W)) direction += Vector3.forward;
            if (button.HasFlag(Button.S)) direction += Vector3.back;
            if (button.HasFlag(Button.A)) direction += Vector3.left;
            if (button.HasFlag(Button.D)) direction += Vector3.right;
            return direction;
        }

        public static bool HandleMoveAndFace(SimulationPlayer player, Button button, Vector2 mousePosition,
            out AsyncOperation operation)
        {
            operation = AsyncOperation.sFailure;
            if (player == null) return false;
            
            if (button == Button.None)
            {
                operation = AsyncOperation.sSuccess;
                return true;
            }

            operation = AsyncOperation.sFailure;
            player = Services.Players.PrimaryPlayer;
            var mRawMoveDirection = ButtonToMoveDirection(button);

            player.SimEntity.StopWatch();
            if (!Services.State.IsInState(IStateService.GameState.Transitioning))
            {
                if (mRawMoveDirection.sqrMagnitude > 0.01f)
                {
                    Vector3 vector = mRawMoveDirection;
                    if (Mathf.Abs(mRawMoveDirection.x) == 1f && Mathf.Abs(mRawMoveDirection.z) == 1f)
                    {
                        vector = new Vector3(mRawMoveDirection.x * 0.81f, mRawMoveDirection.y,
                            mRawMoveDirection.z * 0.8f);
                    }

                    object mv = new MoveCommand();

                    if (!TasServices.Reflection.SetFieldValue(mv, "target", new Target(TargetType.GlobalDirection)
                        {
                            entity = player.SimEntity,
                            point = new Vector3(vector.x * 5f, 0f, vector.z * 5f)
                        }))
                    {
                        TasServices.Log.Log("Failed to set MoveCommand target field via reflection.");
                        return false;
                    }

                    if (!TasServices.Reflection.SetFieldValue(mv, "speedScale",
                            new Vector3(Mathf.Abs(mRawMoveDirection.x), 1f, Mathf.Abs(mRawMoveDirection.z))))
                    {
                        TasServices.Log.Log("Failed to set MoveCommand speedScale field via reflection.");
                        return false;
                    }

                    if (!TasServices.Reflection.SetFieldValue(mv, "useNavMesh", false))
                    {
                        TasServices.Log.Log("Failed to set MoveCommand useNavMesh field via reflection.");
                        return false;
                    }

                    operation = player.SimEntity.Move((MoveCommand)mv);
                }
                else
                {
                    player.SimEntity.StopMove();
                }
            }

            return true;
        }
    }
}