using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR;
using System.Threading;
using System;

namespace FaceScarfPatch
{
    [BepInPlugin(Guid, "FaceScarfPatch", "1.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public Harmony harmony;
        public const string Guid = "com.dev9998.gorillatag.facescarfpatch";

        public void Awake()
        {
            harmony = new Harmony(Guid);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        [HarmonyPatch(typeof(VRRig), "Awake")]
        internal class VRRigPatch
        {
            internal static bool Prefix(VRRig __instance, ref Vector3 ___lastPosition)
            {
                if (__instance.isOfflineVRRig) // Ensure this statement only goes through if this is both our player and if it is used in an offline state
                {
                    Dictionary<string, GameObject> dictionary = new Dictionary<string, GameObject>();
                    GameObject[] array = __instance.cosmetics;
                    GameObject value;
                    List<GameObject> objectsToOverwrite = new List<GameObject>();
                    foreach (GameObject cosmeticObject in array)
                    {
                        // If this specific cosmetic is presumed to be the Face Scarf and its parent is presumed to have the name "Old Cosmetics Head" then rename the object to be overridden.
                        bool correctParent = cosmeticObject.transform.parent.name == "Old Cosmetics Head" || cosmeticObject.transform.parent.name == "WinterJan2023 Head";
                        bool correctName = cosmeticObject.name == "FACE SCARF" || cosmeticObject.name.StartsWith("LFACC.");
                        if (correctName && correctParent) objectsToOverwrite.Add(cosmeticObject); //cosmeticObject.name = "OVERRIDDEN";
                        if (!dictionary.TryGetValue(cosmeticObject.name, out value)) dictionary.Add(cosmeticObject.name, cosmeticObject);
                    }

                    array = __instance.overrideCosmetics;
                    foreach (GameObject gameObject2 in array)
                    {
                        // Does the same thing as VRRig but there's a check for if the cosmetic's parent name doesn't contain "BODY"
                        // No idea why it doesn't check for if the cosmetic to override is apart of the player's body but it doesn't for whatever reason
                        if (dictionary.TryGetValue(gameObject2.name, out value) && value.name == gameObject2.name && !value.transform.parent.name.ToUpper().Contains("BODY"))
                        {
                            if (value.name == "CLOWN WIG")
                            {
                                gameObject2.name = "OVERRIDDEN";
                            }
                            else
                            {
                                value.name = "OVERRIDDEN";
                                bool correctName = gameObject2.name.StartsWith("LFACC.");
                                if (correctName && gameObject2.transform.Find("3rdmirror")) gameObject2.transform.GetChild(0).gameObject.SetActive(false);
                            }
                        }
                    }

                    foreach (GameObject gameObject3 in objectsToOverwrite)
                    {
                        // Fixes up the other cosmetics
                        gameObject3.name = "OVERRIDDEN";
                    }

                    __instance.cosmetics = __instance.cosmetics.Concat(__instance.overrideCosmetics).ToArray();
                    __instance.cosmeticsObjectRegistry.Initialize(__instance.cosmetics);
                    // Since the "lastPosition" variable in VRRig is private I had to use a bit of Harmony logic using Traverse to set the value correctly
                    ___lastPosition = __instance.transform.position;
                    return false;
                }

                return true;
            }
        }
    }
}
