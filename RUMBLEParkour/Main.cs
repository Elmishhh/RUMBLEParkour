using MelonLoader;
using UnityEngine;
using RUMBLE.MoveSystem;
using System;
using System.IO;
using System.Collections.Generic;
using TMPro;
using SmartLocalization.Editor;
using UnhollowerRuntimeLib;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using System.Collections;
using RUMBLE.Managers;
using UnityEngine.Animations;
using RUMBLE.Interactions.InteractionBase;
using UnityEngine.Rendering;
using System.Linq;
using UnityEngine.UIElements;
using RUMBLE.Poses;
using RUMBLE.Players.Subsystems;
using Mono.Cecil;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using RumbleModdingAPI;


namespace RUMBLEParkour
{
    public static class BuildInfo
    {
        public const string Name = "RUMBLE Parkour"; // Name of the Mod.  (MUST BE SET)
        public const string Description = "a simple obstacle course/parkour course creator for RUMBLE"; // Description for the Mod.  (Set as null if none)
        public const string Author = "elmish"; // Author of the Mod.  (MUST BE SET)
        public const string Company = null; // Company that made the Mod.  (Set as null if none)
        public const string Version = "1.0.0"; // Version of the Mod.  (MUST BE SET)
        public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
    }

    public class RumbleParkour : MelonMod
    {
        private bool modInitialized;
        public static string currentScene;

        private bool rightJoystickClicked;
        private bool prevRightJoystickClicked;

        private GameObject dummyObject;

        private GameObject consoleBase;
        private GameObject canvasObject;
        private GameObject mapnametextobject;

        private GameObject nextParkourButton;
        private GameObject nextParkourTextObject;
        private GameObject previousParkourButton;
        private GameObject previousParkourTextObject;

        private GameObject[] shiftstoneObjects = new GameObject[2];

        public List<GameObject> parkourObjects = new List<GameObject>();

        private string[] gymFilePaths;
        private string[] parkFilePaths;
        private int chosenfile = 0;

        private List<GameObject> infoTextObjects = new List<GameObject>();
        private List<Vector3> infoTextPositions = new List<Vector3>()
        {
            new Vector3(-26.4149f, 4.3526f, 2.9666f),
            new Vector3(-26.5468f, 4.2932f, 3.6266f),
            new Vector3(-26.6149f, 4.2126f, 2.9666f),
            new Vector3(-26.7449f, 4.1426f, 3.61f)
        };

        PlayerPoseSystem localPlayerPoseSystem;
        Il2CppSystem.Collections.Generic.List<PoseInputSource> playerPosesDummyList;
        Il2CppSystem.Collections.Generic.List<PoseInputSource> actualPlayerPoses = new Il2CppSystem.Collections.Generic.List<PoseInputSource>();
        Dictionary<string, string> poseNameDumbifier = new Dictionary<string, string>()
        {
            { "explode", "PoseSetExplode" },
            { "flick", "PoseSetFlick" },
            { "parry", "PoseSetParry" },
            { "holdl", "PoseSetHoldLeft" },
            { "holdr", "PoseSetHoldRight" },
            { "dash", "PoseSetDash" },
            { "cube", "PoseSetSpawnCube" },
            { "uppercut", "PoseSetUppercut" },
            { "jump", "PoseSetRockjump" },
            { "wall", "PoseSetWall_Grounded" },
            { "kick", "PoseSetKick" },
            { "stomp", "PoseSetStomp" },
            { "ball", "PoseSetBall" },
            { "disc", "PoseSetDisc" },
            { "straight", "PoseSetStraight" },
            { "pillar", "PoseSetSpawnPillar" },
            { "sprint", "SprintingPoseSet" }
        };
        List<string> poseNames = new List<string>()
        {
            "explode",
            "flick",
            "parry",
            "hold",
            "dash",
            "cube",
            "uppercut",
            "jump",
            "wall",
            "kick",
            "stomp",
            "ball",
            "disc",
            "straight",
            "pillar",
            "sprint"
        };
        List<string> shiftstoneNames = new List<string>()
        {
            "Vigor",
            "Guard",
            "Flow",
            "Stubborn",
            "Charge",
            "Volatile",
            "Surge",
            "Adamant"
        };

        public static PlayerResetSystem localPlayerResetSystem;
        public static Vector3 respawnPosition;
        public static Quaternion respawnRotation;

        GameObject baseMaterial;

        private PhysicMaterial loadAssets()
        {
            using (System.IO.Stream bundleStream = MelonAssembly.Assembly.GetManifestResourceStream("RUMBLEParkour.Resources.physicsmaterial"))
            {
                byte[] bundleBytes = new byte[bundleStream.Length];
                bundleStream.Read(bundleBytes, 0, bundleBytes.Length);
                AssetBundleLoader.AssetBundle bundle = AssetBundleLoader.AssetBundle.LoadFromMemory(bundleBytes);
                return UnityEngine.Object.Instantiate(bundle.LoadAsset<PhysicMaterial>("baseMaterial"));
            }
        }
        IEnumerator getLocalPlayerPoseSystem()
        {
            for (int i = 0; i < 60; i++) yield return new WaitForFixedUpdate();
            PlayerManager playerManager = PlayerManager.Instance;
            localPlayerPoseSystem = playerManager.localPlayer.Controller.gameObject.transform.GetComponentInChildren<PlayerPoseSystem>();
            playerPosesDummyList = localPlayerPoseSystem.currentInputPoses;
            foreach (PoseInputSource pose in playerPosesDummyList)
            {
                actualPlayerPoses.Add(pose);
            }

            localPlayerResetSystem = playerManager.localPlayer.Controller.gameObject.GetComponent<PlayerResetSystem>();
        }
        private void DisableMoves(string movesToDisable)
        {
            ResetMoves();
            List<string> movesToDisableList = movesToDisable.Split(',').ToList();
            for (int i = 0; i < movesToDisableList.Count; i++)
            {
                if (movesToDisableList[i] == "hold")
                {
                    movesToDisableList.RemoveAt(i);
                    movesToDisableList.Add("holdr");
                    movesToDisableList.Add("holdl");
                }
            }
            //MelonLogger.Msg("starting to remove moves");
            for (int i = 0; i < movesToDisableList.Count; i++)
            {
                //MelonLogger.Msg($"currently disabling:{movesToDisable[i]} at index {i}");
                foreach (string move in poseNames)
                {
                    //MelonLogger.Msg($"move = {move}");
                    if (movesToDisableList[i].ToLower().Contains(move.ToLower()))
                    {
                        //MelonLogger.Msg("move is in the list by name");
                        for (int x = 0; x < localPlayerPoseSystem.currentInputPoses.Count; x++)
                        {
                            //MelonLogger.Msg($"currently at: {localPlayerPoses.currentInputPoses[x].poseSet.name} at index {x} |||| {poseNameDumbifier[move]} |||| {localPlayerPoses.currentInputPoses[x].poseSet.name}");
                            if (localPlayerPoseSystem.currentInputPoses[x].poseSet.name == poseNameDumbifier[move])
                            {
                                MelonLogger.Msg($"Removed {localPlayerPoseSystem.currentInputPoses[x].poseSet.name} at index {x} successfully");
                                localPlayerPoseSystem.currentInputPoses.RemoveAt(x);
                            }
                        }
                    }
                }
            }
        }
        private void ResetMoves()
        {
            localPlayerPoseSystem.ClearPoseInputSources();
            for (int i = 0; i < actualPlayerPoses.Count; i++) localPlayerPoseSystem.currentInputPoses.Add(actualPlayerPoses[i]);
        }
        private IEnumerator DisableShiftstones(string[] shiftstones)
        {
            if (shiftstoneObjects[0] == null || shiftstoneObjects[1] == null)
            {
                MelonCoroutines.Start(GetShiftstoneObjects());
                while (shiftstoneObjects[0] == null || shiftstoneObjects[1] == null) { yield return new WaitForFixedUpdate(); }
            }
            bool disableFirst = false;
            bool disableSecond = false;
            for (int arrayIndex = 0; arrayIndex < shiftstones.Length; ++arrayIndex)
            {
                foreach (string shiftstonename in shiftstoneNames)
                {
                    if (shiftstones[arrayIndex].ToLower().Contains(shiftstonename.ToLower()))
                    {
                        if (shiftstoneObjects[0].name.ToLower() == shiftstonename.ToLower() + "stone")
                        {
                            disableFirst = true;
                        }
                        else if (shiftstoneObjects[1].name.ToLower() == shiftstonename.ToLower() + "stone")
                        {
                            disableSecond = true;
                        }
                    }
                }
                shiftstoneObjects[0].SetActive(!disableFirst);
                shiftstoneObjects[1].SetActive(!disableSecond);

            }
        }
        private IEnumerator GetShiftstoneObjects()
        {
            MelonLogger.Msg("test");
            for (int i = 0; i < 300; i-=-1) yield return new WaitForFixedUpdate(); // REMOVE ON RELEAS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! // nah i wont there's no need
            //while (PlayerManager.instance?.localPlayer?.Controller?.gameObject?.transform?.GetChild(5)?.GetChild(7)?.GetChild(0)?.GetChild(2)?.GetChild(0)?.GetChild(1)?.GetChild(0)?.GetChild(0)?.GetChild(0)?.GetChild(5)?.GetChild(0)?.GetChild(0)?.gameObject == null ?? true) yield return new WaitForFixedUpdate();
            MelonLogger.Msg("getting first shiftstone");
            shiftstoneObjects[0] = PlayerManager.instance.localPlayer.Controller.gameObject.transform.GetChild(5).GetChild(7).GetChild(0).GetChild(2).GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetChild(5).GetChild(0).GetChild(0).gameObject;
            MelonLogger.Msg("getting second shiftstone");
            shiftstoneObjects[1] = PlayerManager.instance.localPlayer.Controller.gameObject.transform.GetChild(5).GetChild(7).GetChild(0).GetChild(2).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(5).GetChild(1).GetChild(0).gameObject;
            //MelonLogger.Msg($"{shiftstoneObjects[0].name} {shiftstoneObjects[1].name}");
        }
        private void GetFilePaths()
        {
            gymFilePaths = Directory.GetFiles(@"UserData\ParkourMaps\Gym");
            parkFilePaths = Directory.GetFiles(@"UserData\ParkourMaps\Park");
        }

        private void GetParkourInfoFromPath(string filepath)
        {
            int linesPassed = 0;
            foreach (string line in File.ReadAllLines(filepath))
            {
                if (linesPassed < 4)
                {
                    switch (linesPassed)
                    {
                        case 0:
                            mapnametextobject.GetComponent<TextMeshProUGUI>().text = $"{line.Split(':')[1]}";
                            break;
                        case 1:
                            //canvasObject.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = $"by: {line.Split(':')[1]}";
                            break;
                        case 2:
                            canvasObject.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = $"{line.Split(':')[1]}";
                            break;
                        case 3:
                            canvasObject.transform.GetChild(5).GetComponent<TextMeshProUGUI>().text = line.Split(':')[1];
                            break;
                    }
                }
                linesPassed++;
            }
            //parkourObjectsArray = new GameObject[linesPassed - 3]; // the amount of objects is Lines passed + 1(cause it starts at 0) - 4 (cause of the initial data lines)
        }
        
        private GameObject CreatePlatform(Vector3 size, Vector3 position, Vector3 rotation, Color32 color, string[] modifiers)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.GetComponent<Renderer>().material.shader = Shader.Find("Universal Render Pipeline/Lit");
            cube.AddComponent<GroundCollider>().collider = cube.GetComponent<Collider>();
            cube.tag = "Audio_Dirt"; // does this even work?
            cube.GetComponent<GroundCollider>().tag = "Audio_Dirt"; // is this needed?
            cube.layer = 9; // combatfloor layer (the 10th layer)
            cube.transform.localScale = size;
            cube.transform.position = position;
            cube.transform.Rotate(rotation);
            cube.transform.SetParent(dummyObject.transform, true);
            cube.transform.GetComponent<MeshRenderer>().material.color = color;
            //parkourObjectsArray[objectscreated] = cube;

            if (modifiers.Length > 0)
            {
                foreach (string modifier in modifiers)
                {
                    if (modifier.ToLower().Contains("checkpoint"))
                    {
                        cube.AddComponent<CheckpointHandler>().rotation = Quaternion.Euler(StringToVector3(modifier.Split(':')[1]));
                    }
                    else if (modifier.ToLower().Contains("timed"))
                    {
                        string[] timeValues = modifier.Split(':')[1].Split(',');
                        cube.AddComponent<TimedPlatformHandler>().SetTimes(int.Parse(timeValues[0]), int.Parse(timeValues[1]));
                    }
                    else if (modifier.ToLower().Contains("physics"))
                    {
                        int bounciness = int.Parse(modifier.Split(':')[1]);
                        PhysicMaterial temp = UnityEngine.Object.Instantiate(baseMaterial.GetComponent<Collider>().material);
                        temp.bounciness = bounciness;
                        cube.GetComponent<Collider>().material = temp;
                    }
                    else if (modifier.ToLower() == "killoncontact")
                    {
                        cube.AddComponent<KillOnContact>();
                    }
                }
            }
            return cube;
        }
        private Vector3 StringToVector3(string line)
        {
            string[] stringarray = line.Split(',');
            float[] position = new float[3];
            for (int i = 0; i < 3; i++) position[i] = float.Parse(stringarray[i]);
            Vector3 temp = new Vector3(position[0], position[1], position[2]);
            return temp;
        }
        private Color32 StringToColor32(string line)
        {
            string[] stringarray = line.Split(',');
            Color32 temp = new Color32(byte.Parse(stringarray[0]), byte.Parse(stringarray[1]), byte.Parse(stringarray[2]), byte.Parse(stringarray[3]));
            return temp;
        }
        private void SpawnCourseFromFile(string filepath)
        {
            foreach (GameObject obj in parkourObjects) GameObject.Destroy(obj);
            MelonLogger.Msg($"starting to spawn {filepath}");
            int initialLinesPassed = 0;
            foreach (string line in File.ReadAllLines(filepath))
            {
                if (initialLinesPassed < 4)
                {
                    if (initialLinesPassed == 2)
                    {
                        DisableMoves(line.Split(':')[1]);
                    }
                    if (initialLinesPassed == 3)
                    {
                        MelonCoroutines.Start(DisableShiftstones(line.Split(':')[1].Split(',')));
                    }
                    initialLinesPassed++;
                }
                else
                {
                    string[] modifiers = new string[0];

                    string[] ObjectInfoArray;

                    if (line.Contains('\\'))
                    {
                        modifiers = line.Split('\\')[1].Split('|');
                        ObjectInfoArray = line.Split('\\')[0].Split('|');
                    }
                    else
                    {
                        ObjectInfoArray = line.Split('|');
                    }
                    Vector3[] values = new Vector3[3]
                    {
                    StringToVector3(ObjectInfoArray[0]), // size/scale
                    StringToVector3(ObjectInfoArray[1]), // position
                    StringToVector3(ObjectInfoArray[2]) // rotation (0-360)
                    };

                    parkourObjects.Add(CreatePlatform(values[0], values[1], values[2], StringToColor32(ObjectInfoArray[3]), modifiers));
                }
            }
            MelonLogger.Msg($"finished spawning {filepath}");
        }
        
        private void OnParkourChangeButtonClicked(bool isPositive)
        {
            if (isPositive)
            {
                if (chosenfile == gymFilePaths.Length - 1) chosenfile = 0;
                else chosenfile++;
            }
            else
            {
                if (chosenfile == 0) chosenfile = gymFilePaths.Length -1;
                else chosenfile--;
            }
            if (currentScene == "Gym") GetParkourInfoFromPath(gymFilePaths[chosenfile]);
            else if (currentScene == "Park") GetParkourInfoFromPath(parkFilePaths[chosenfile]);
        }

        private IEnumerator initialButtonsSetup()
        {
            for (int i = 0; i < 240; i++) yield return new WaitForFixedUpdate(); // waits for 2 seconds

            nextParkourButton = UnityEngine.Object.Instantiate(consoleBase.transform.GetChild(0).GetChild(1).gameObject);
            nextParkourButton.transform.name = "next parkour button";
            nextParkourButton.transform.GetChild(0).GetComponent<InteractionButton>().OnPressed.AddListener(new System.Action(() =>
            {
                OnParkourChangeButtonClicked(true);
            }));
            nextParkourButton.transform.SetParent(canvasObject.transform);
            nextParkourButton.transform.position = new Vector3(-26.2092f, 4.5775f, 3.4842f);
            nextParkourButton.transform.localRotation = Quaternion.Euler(0, 90, 270);
            infoTextObjects[4].transform.SetParent(nextParkourButton.transform.GetChild(0));
            infoTextObjects[4].transform.position = new Vector3(-26.2592f, 4.6375f, 3.4842f);
            infoTextObjects[4].transform.localRotation = Quaternion.Euler(90, 90, 0);
            infoTextObjects[4].transform.GetComponent<RectTransform>().sizeDelta = Vector2.one * 0.1f;
            infoTextObjects[4].transform.GetComponent<TextMeshProUGUI>().text = ">";
            infoTextObjects[4].transform.GetComponent<TextMeshProUGUI>().fontSizeMax = 0.4f;


            previousParkourButton = UnityEngine.Object.Instantiate(consoleBase.transform.GetChild(0).GetChild(1).gameObject);
            previousParkourButton.transform.name = "previous parkour button";
            previousParkourButton.transform.GetChild(0).GetComponent<InteractionButton>().OnPressed.AddListener(new System.Action(() =>
            {
                OnParkourChangeButtonClicked(false);
            }));
            previousParkourButton.transform.SetParent(canvasObject.transform);
            previousParkourButton.transform.position = new Vector3(-26.2392f, 4.5775f, 4.0442f);
            previousParkourButton.transform.localRotation = Quaternion.Euler(0, 270, 90);
            infoTextObjects[5].transform.SetParent(previousParkourButton.transform.GetChild(0));
            nextParkourButton.transform.position = new Vector3(-26.2092f, 4.5775f, 3.4842f);
            nextParkourButton.transform.localRotation = Quaternion.Euler(0, 90, 270);
            infoTextObjects[5].transform.position = new Vector3(-26.2842f, 4.6375f, 4.0392f);
            infoTextObjects[5].transform.localRotation = Quaternion.Euler(90, -90, 0);
            infoTextObjects[5].transform.GetComponent<RectTransform>().sizeDelta = Vector2.one * 0.1f;
            infoTextObjects[5].transform.GetComponent<TextMeshProUGUI>().text = "<";
            infoTextObjects[5].transform.GetComponent<TextMeshProUGUI>().fontSizeMax = 0.4f;

            mapnametextobject.GetComponent<RectTransform>().sizeDelta = Vector2.one * 0.3f;

            canvasObject.transform.GetChild(3).GetComponent<TextMeshProUGUI>().enableAutoSizing = true;
            canvasObject.transform.GetChild(3).GetComponent<RectTransform>().sizeDelta = new Vector2(1, 0.4f);
            canvasObject.transform.GetChild(3).GetComponent<TextMeshProUGUI>().fontSizeMax = 0.25f;
            canvasObject.transform.GetChild(3).localScale = new Vector3(0.35f, 0.35f, 1);

            canvasObject.transform.GetChild(5).GetComponent<TextMeshProUGUI>().enableAutoSizing = true;
            canvasObject.transform.GetChild(5).GetComponent<RectTransform>().sizeDelta = Vector2.one * 0.6f;
            canvasObject.transform.GetChild(5).GetComponent<TextMeshProUGUI>().fontSizeMax = 0.25f;
            canvasObject.transform.GetChild(5).localScale = new Vector3(0.45f, 0.45f, 1);

            canvasObject.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = $"disallowed shiftstones:";
            canvasObject.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"Disabled moves:";

            consoleBase.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<InteractionButton>().onPressed.AddListener(new System.Action(() =>
            {
                if (currentScene == "Gym") GetParkourInfoFromPath(gymFilePaths[chosenfile]);
                else if (currentScene == "Park") GetParkourInfoFromPath(parkFilePaths[chosenfile]);
            }));
            UnityEngine.Object.DontDestroyOnLoad(dummyObject);
            //SpawnCourseFromFile(gymFilePaths[0]);
            modInitialized = true;
        }
        private void initalSetup()
        {
            MelonLogger.Msg("setting up RUMBLE parkour!");
            GetFilePaths();
            consoleBase = UnityEngine.Object.Instantiate(GameObject.Find("--------------LOGIC--------------/Heinhouser products/RegionSelector"));

            GameObject firstchild = consoleBase.transform.GetChild(0).gameObject;
            for (int i = 0; i < firstchild.transform.childCount; i++)
            {
                GameObject child = firstchild.transform.GetChild(i).gameObject;
                switch (child.transform.name) // i hate this, there's probably a more effective way of doing this but nah
                {
                    case "Gear":
                        GameObject.Destroy(child);
                        break;
                    case "Pin":
                        GameObject.Destroy(child);
                        break;
                    case "WorldMap":
                        GameObject.Destroy(child);
                        break;
                    case "Screw":
                        GameObject.Destroy(child);
                        break;
                    case "Vertical slider":
                        GameObject.Destroy(child);
                        break;
                    case "UI":
                        GameObject.Destroy(child.transform.GetChild(0).GetChild(0).GetComponent<LocalizedTextTMPro>());
                        child.transform.GetChild(0).GetChild(0).GetComponent<TextMeshPro>().text = "Start selected parkour";
                        child.transform.GetChild(0).GetChild(0).localScale = Vector3.one * 0.75f;
                        mapnametextobject = child.transform.GetChild(1).gameObject;
                        GameObject.Destroy(mapnametextobject.GetComponent<LocalizedTextTMPro>());
                        mapnametextobject.GetComponent<TextMeshProUGUI>().text = "NO SELECTED PARKOUR";
                        mapnametextobject.GetComponent<TextMeshProUGUI>().enableAutoSizing = true;
                        mapnametextobject.transform.localScale = Vector3.one;
                        for (int timestoadd = 0; timestoadd < 6; timestoadd++) infoTextObjects.Add(UnityEngine.Object.Instantiate(mapnametextobject));
                        canvasObject = mapnametextobject.transform.parent.gameObject;

                        break;
                }
            }
            consoleBase.transform.SetParent(dummyObject.transform);
            consoleBase.transform.position = new Vector3(-26.6f, 4.16f, 3.6f);
            consoleBase.transform.rotation = Quaternion.Euler(0, 357, 305.3789f);
            mapnametextobject.transform.position = new Vector3(-26.2249f, 4.5826f, 3.7666f);
            mapnametextobject.transform.localScale = Vector3.one * 0.3f;
            mapnametextobject.GetComponent<TextMeshProUGUI>().horizontalAlignment = HorizontalAlignmentOptions.Left;
            mapnametextobject.GetComponent<TextMeshProUGUI>().autoSizeTextContainer = true;
            mapnametextobject.GetComponent<TextMeshProUGUI>().fontSizeMax = 0.4f;
            for (int i = 0; i < 4; i++)
            {
                infoTextObjects[i].transform.SetParent(canvasObject.transform);
                infoTextObjects[i].GetComponent<RectTransform>().sizeDelta = Vector2.one;
                infoTextObjects[i].GetComponent<TextMeshProUGUI>().fontSize = 0.1f;
                infoTextObjects[i].GetComponent<TextMeshProUGUI>().horizontalAlignment = HorizontalAlignmentOptions.Left;
                infoTextObjects[i].transform.localScale = Vector3.one;
                infoTextObjects[i].transform.localRotation = Quaternion.identity;
                infoTextObjects[i].transform.localRotation = Quaternion.Euler(0, 0, 180);
                infoTextObjects[i].transform.position = infoTextPositions[i];
            }
            infoTextObjects[3].GetComponent<TextMeshProUGUI>().fontSize = 0.09f;
            MelonCoroutines.Start(getLocalPlayerPoseSystem());
            MelonCoroutines.Start(initialButtonsSetup());
            GetParkourInfoFromPath(gymFilePaths[0]);
        }
        private void DisableMod()
        {
            ResetMoves();
            foreach (GameObject obj in parkourObjects)
            {
                GameObject.Destroy(obj);
            }
            shiftstoneObjects = new GameObject[2];
            dummyObject.SetActive(false);
        }
        private void EnableGym()
        {
            dummyObject.SetActive(true);
            consoleBase.transform.position = new Vector3(-26.6f, 4.16f, 3.6f);
            consoleBase.transform.rotation = Quaternion.Euler(0, 357, 305.3789f);
            GetParkourInfoFromPath(gymFilePaths[0]);
            SpawnCourseFromFile(gymFilePaths[0]);
        }
        private void EnablePark()
        {
            dummyObject.SetActive(true);
            consoleBase.transform.position = new Vector3(-14.1971f, - 2.336f, - 11.7755f);
            consoleBase.transform.rotation = Quaternion.Euler(0, 17, 305.3789f);
            GetParkourInfoFromPath(parkFilePaths[0]);
            SpawnCourseFromFile(parkFilePaths[0]);
        }
        public override void OnLateInitializeMelon() // Runs after OnApplicationStart.
        {
            ClassInjector.RegisterTypeInIl2Cpp<CheckpointHandler>(); // initiates the component?
            ClassInjector.RegisterTypeInIl2Cpp<TimedPlatformHandler>();
            ClassInjector.RegisterTypeInIl2Cpp<KillOnContact>();
        }

        public override void OnSceneWasLoaded(int buildindex, string sceneName) // Runs when a Scene has Loaded and is passed the Scene's Build Index and Name.
        {
            currentScene = sceneName;
            if (modInitialized) DisableMod();
            if (sceneName == "Gym" && !modInitialized)
            {
                dummyObject = new GameObject("RUMBLE Parkour");
                baseMaterial = GameObject.CreatePrimitive(PrimitiveType.Cube);
                baseMaterial.GetComponent<Collider>().material = loadAssets(); // hopefully stores the material so it doesnt get garbage collected?
                UnityEngine.Object.DontDestroyOnLoad(baseMaterial);
                baseMaterial.transform.SetParent(dummyObject.transform);
                baseMaterial.transform.position = Vector3.one * 9999;
                baseMaterial.name = "physics material object (so it doesnt get garbage collected)";
                initalSetup();
            }
            else if (sceneName == "Gym" && modInitialized) EnableGym();
            else if (sceneName == "Park" && modInitialized) EnablePark(); //modInitalized check isnt required since you really arent gonna be going into the park before Gym tbh
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            prevRightJoystickClicked = rightJoystickClicked;
            rightJoystickClicked = Calls.ControllerMap.RightController.GetJoystickClick() > 0;
            if (modInitialized && rightJoystickClicked && !prevRightJoystickClicked && currentScene == "Gym")
            {
                localPlayerResetSystem.RPC_RelocatePlayerController(respawnPosition, respawnRotation);
            }
        }
    }
}