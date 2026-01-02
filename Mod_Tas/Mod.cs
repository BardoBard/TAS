using UnityEngine;
using static Lithium.Core.Thor.Core.TAS;

namespace Mod_1cc0850504174e90b99c12c59de2b40f
{
    public class Mod : MonoBehaviour
    {
        void Awake()
        {
            Debug.Log("[Mod]: Awake called");
            AwakeTas();
        }

        void Start()
        {
            Debug.Log("[Mod]: Start called");
            StartTas();
        }

        void Update()
        {
            UpdateTas();
        }
    }
}