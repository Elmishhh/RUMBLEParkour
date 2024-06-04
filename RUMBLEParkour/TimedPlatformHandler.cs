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
    internal class TimedPlatformHandler : MonoBehaviour
    {
        public TimedPlatformHandler(IntPtr ptr) : base(ptr) { }

        int respawnTime;
        int despawnTime;
        bool currentlyOnCooldown;

        Collider collider;
        MeshRenderer renderer;

        private void Start()
        {
            collider = GetComponent<Collider>();
            renderer = GetComponent<MeshRenderer>();
        }

        public void SetTimes(int respawn, int despawn)
        {
            respawnTime = respawn;
            despawnTime = despawn;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.name == "VR" && !currentlyOnCooldown) MelonCoroutines.Start(actualHandler());
        }
        
        private IEnumerator actualHandler()
        {
            currentlyOnCooldown = true;
            //MelonLogger.Msg($"disabling object in {despawnTime} seconds");
            for (int i = 0; i < 50 * despawnTime; i++) yield return new WaitForFixedUpdate();
            collider.enabled = false;
            renderer.enabled = false;
            //MelonLogger.Msg($"enabling object in {respawnTime} seconds");
            for (int i = 0; i < 50 * respawnTime; i++) yield return new WaitForFixedUpdate();
            collider.enabled = true;
            renderer.enabled = true;
            currentlyOnCooldown = false;
        }
    }
}
