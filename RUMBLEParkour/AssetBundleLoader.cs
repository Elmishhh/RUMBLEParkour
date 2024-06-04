using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Attributes;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace AssetBundleLoader
{
    // Both of these classes are from UniverseLib, I just extracted them to here to not have to have a dependency. I did NOT make these two classes!
    public class AssetBundle : UnityEngine.Object
    {
        public readonly IntPtr m_bundlePtr = IntPtr.Zero;

        static AssetBundle() => ClassInjector.RegisterTypeInIl2Cpp<AssetBundle>();

        [HideFromIl2Cpp]
        public static AssetBundle LoadFromFile(string path)
        {
            IntPtr ptr =
                ICallManager.GetICallUnreliable<AssetBundle.d_LoadFromFile>(
                    "UnityEngine.AssetBundle::LoadFromFile_Internal", "UnityEngine.AssetBundle::LoadFromFile")(
                    IL2CPP.ManagedStringToIl2Cpp(path), 0U, 0UL);
            return ptr != IntPtr.Zero ? new AssetBundle(ptr) : (AssetBundle)null;
        }

        [HideFromIl2Cpp]
        public static AssetBundle LoadFromMemory(byte[] binary, uint crc = 0)
        {
            IntPtr ptr =
                ICallManager.GetICallUnreliable<AssetBundle.d_LoadFromMemory>(
                    "UnityEngine.AssetBundle::LoadFromMemory_Internal", "UnityEngine.AssetBundle::LoadFromMemory")(
                    ((Il2CppStructArray<byte>)binary).Pointer, crc);
            return ptr != IntPtr.Zero ? new AssetBundle(ptr) : (AssetBundle)null;
        }

        [HideFromIl2Cpp]
        public static AssetBundle[] GetAllLoadedAssetBundles()
        {
            IntPtr nativeObject =
                ICallManager.GetICall<AssetBundle.d_GetAllLoadedAssetBundles_Native>(
                    "UnityEngine.AssetBundle::GetAllLoadedAssetBundles_Native")();
            return nativeObject != IntPtr.Zero
                ? (AssetBundle[])(Il2CppArrayBase<AssetBundle>)new Il2CppReferenceArray<AssetBundle>(nativeObject)
                : (AssetBundle[])null;
        }

        public AssetBundle(IntPtr ptr)
            : base(ptr)
        {
            this.m_bundlePtr = ptr;
        }

        [HideFromIl2Cpp]
        public UnityEngine.Object[] LoadAllAssets()
        {
            IntPtr nativeObject =
                ICallManager.GetICall<AssetBundle.d_LoadAssetWithSubAssets_Internal>(
                    "UnityEngine.AssetBundle::LoadAssetWithSubAssets_Internal")(this.m_bundlePtr,
                    IL2CPP.ManagedStringToIl2Cpp(""), UnhollowerRuntimeLib.Il2CppType.Of<UnityEngine.Object>().Pointer);
            return nativeObject != IntPtr.Zero
                ? (UnityEngine.Object[])(Il2CppArrayBase<UnityEngine.Object>)
                new Il2CppReferenceArray<UnityEngine.Object>(nativeObject)
                : new UnityEngine.Object[0];
        }

        [HideFromIl2Cpp]
        public T LoadAsset<T>(string name) where T : UnityEngine.Object
        {
            IntPtr pointer =
                ICallManager.GetICall<AssetBundle.d_LoadAsset_Internal>("UnityEngine.AssetBundle::LoadAsset_Internal")(
                    this.m_bundlePtr, IL2CPP.ManagedStringToIl2Cpp(name),
                    UnhollowerRuntimeLib.Il2CppType.Of<T>().Pointer);
            return pointer != IntPtr.Zero ? new UnityEngine.Object(pointer).TryCast<T>() : default(T);
        }

        [HideFromIl2Cpp]
        public void Unload(bool unloadAllLoadedObjects) =>
            ICallManager.GetICall<AssetBundle.d_Unload>("UnityEngine.AssetBundle::Unload")(this.m_bundlePtr,
                unloadAllLoadedObjects);

        internal delegate IntPtr d_LoadFromFile(IntPtr path, uint crc, ulong offset);

        private delegate IntPtr d_LoadFromMemory(IntPtr binary, uint crc);

        public delegate IntPtr d_GetAllLoadedAssetBundles_Native();

        internal delegate IntPtr d_LoadAssetWithSubAssets_Internal(
            IntPtr _this,
            IntPtr name,
            IntPtr type);

        internal delegate IntPtr d_LoadAsset_Internal(IntPtr _this, IntPtr name, IntPtr type);

        internal delegate void d_Unload(IntPtr _this, bool unloadAllLoadedObjects);
    }

    /// <summary>
    /// Helper class for using Unity ICalls (internal calls).
    /// </summary>
    public static class ICallManager
    {
        // cache used by GetICall
        private static readonly Dictionary<string, Delegate> iCallCache = new Dictionary<string, Delegate>();

        // cache used by GetICallUnreliable
        private static readonly Dictionary<string, Delegate> unreliableCache = new Dictionary<string, Delegate>();

        /// <summary>
        /// Helper to get and cache an iCall by providing the signature (eg. "UnityEngine.Resources::FindObjectsOfTypeAll").
        /// </summary>
        /// <typeparam name="T">The Type of Delegate to provide for the iCall.</typeparam>
        /// <param name="signature">The signature of the iCall you want to get.</param>
        /// <returns>The <typeparamref name="T"/> delegate if successful.</returns>
        /// <exception cref="MissingMethodException" />
        public static T GetICall<T>(string signature) where T : Delegate
        {
            if (iCallCache.ContainsKey(signature))
                return (T)iCallCache[signature];

            IntPtr ptr = IL2CPP.il2cpp_resolve_icall(signature);

            if (ptr == IntPtr.Zero)
                throw new MissingMethodException($"Could not find any iCall with the signature '{signature}'!");

            Delegate iCall = Marshal.GetDelegateForFunctionPointer(ptr, typeof(T));
            iCallCache.Add(signature, iCall);

            return (T)iCall;
        }

        /// <summary>
        /// Get an iCall which may be one of multiple different signatures (ie, the name changed in different Unity versions).
        /// Each possible signature must have the same Delegate type, it can only vary by name.
        /// </summary>
        public static T GetICallUnreliable<T>(params string[] possibleSignatures) where T : Delegate
        {
            // use the first possible signature as the 'key'.
            string key = possibleSignatures.First();

            if (unreliableCache.ContainsKey(key))
                return (T)unreliableCache[key];

            T iCall;
            IntPtr ptr;
            foreach (string sig in possibleSignatures)
            {
                ptr = IL2CPP.il2cpp_resolve_icall(sig);
                if (ptr != IntPtr.Zero)
                {
                    iCall = (T)Marshal.GetDelegateForFunctionPointer(ptr, typeof(T));
                    unreliableCache.Add(key, iCall);
                    return iCall;
                }
            }

            throw new MissingMethodException(
                $"Could not find any iCall from list of provided signatures starting with '{key}'!");
        }
    }
}