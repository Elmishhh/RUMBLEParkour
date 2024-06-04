using System;
using MelonLoader;
using UnityEngine;

namespace RUMBLEParkour
{
    [RegisterTypeInIl2Cpp]
    internal class CheckpointHandler : MonoBehaviour
    {
        public CheckpointHandler(IntPtr ptr) : base(ptr) { } // idk tbh but it's needed

        bool checkpointClaimed;
        public Quaternion rotation;

        private void Start()
        {
            //GetComponent<MeshRenderer>().material.color = Color.yellow;
        }
        private void OnCollisionEnter(Collision collision)
        {
            //MelonLogger.Error(collision.gameObject.name);
            if (!checkpointClaimed && collision.gameObject.name == "VR")
            {
                checkpointClaimed = true;
                RumbleParkour.respawnPosition = new Vector3(0, transform.localScale.y, 0) + transform.position;
                RumbleParkour.respawnRotation = rotation;
                //MelonLogger.Msg("set checkpoint!");
            }
        }
    }
}
