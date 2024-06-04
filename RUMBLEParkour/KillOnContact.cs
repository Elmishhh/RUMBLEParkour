using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RUMBLEParkour
{
    [RegisterTypeInIl2Cpp]
    internal class KillOnContact : MonoBehaviour
    {
        public KillOnContact(IntPtr ptr) : base(ptr) { }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.name == "VR" && RumbleParkour.currentScene == "Gym")
            {
                RumbleParkour.localPlayerResetSystem.RPC_RelocatePlayerController(RumbleParkour.respawnPosition, RumbleParkour.respawnRotation);
            }
        }
    }
}
