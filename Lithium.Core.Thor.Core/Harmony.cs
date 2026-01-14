using System.Threading;
using HarmonyLib;
using Thor.Core;
using UnityEngine;

namespace Lithium.Core.Thor.Core
{
    namespace Lithium.Core.Thor.Core
    {
        public static class HarmonyLoader
        {
            private static bool _patched = false;

            public static void ApplyPatches()
            {
                if (_patched) return;
                var harmony = new Harmony("com.TAS.Lithium.Core.Thor.Core");
                harmony.PatchAll();
                Debug.Log("[TAS]: Harmony patches applied.");
                _patched = true;
            }
        }
    }
    
    [HarmonyPatch(typeof(GetRandomMovePosition), "OnExecute")]
    public class GetRandomMovePositionPatch
    {
        private static readonly ThreadLocal<Rand.StateScope?> m_stateScope = new ThreadLocal<Rand.StateScope?>();

        static void Prefix(GetRandomMovePosition __instance)
        {
            m_stateScope.Value = new Rand.StateScope(123456);
        }

        static void Postfix(GetRandomMovePosition __instance)
        {
            m_stateScope.Value?.Dispose();
            m_stateScope.Value = null;
        }
    }
}