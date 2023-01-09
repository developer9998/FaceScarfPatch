using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace FaceScarfPatch
{
    [BepInPlugin(Guid, "FaceScarfPatch", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public Harmony harmony;
        public const string Guid = "com.dev9998.gorillatag.facescarfpatch";

        public void Awake()
        {
            harmony = new Harmony("com.dev9998.gorillatag.facescarfpatch");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        [HarmonyPatch(typeof(VRRig), "Awake")]
        internal class VRRigPatch
        {
            internal static bool Prefix(VRRig __instance)
            {
                if (__instance.isOfflineVRRig) // Ensure this statement only goes through if this is both our player and if it is used in an offline state
                {
                    Dictionary<string, GameObject> dictionary = new Dictionary<string, GameObject>();
                    GameObject[] array = __instance.cosmetics;
                    GameObject value;
                    foreach (GameObject gameObject in array)
                    {
                        // If this specific cosmetic is presumed to be the Face Scarf and its parent is presumed to have the name "Old Cosmetics Head" then disable the cosmetic's renderer component
                        if (gameObject.name.Contains("FACE S") && gameObject.transform.parent.name.ToUpper().Contains("S HEAD")) gameObject.GetComponent<Renderer>().enabled = false;
                        if (!dictionary.TryGetValue(gameObject.name, out value)) dictionary.Add(gameObject.name, gameObject);
                    }

                    array = __instance.overrideCosmetics;
                    foreach (GameObject gameObject2 in array)
                    {
                        // Does the same thing as VRRig but there's a check for if the cosmetic's parent name doesn't contain "BODY"
                        // No idea why it doesn't check for if the cosmetic to override is apart of the player's body but it doesn't for whatever reason
                        if (dictionary.TryGetValue(gameObject2.name, out value) && value.name == gameObject2.name && !value.transform.parent.name.ToUpper().Contains("BODY")) value.name = "OVERRIDDEN";
                    }

                    __instance.cosmetics = __instance.cosmetics.Concat(__instance.overrideCosmetics).ToArray();
                    __instance.cosmeticsObjectRegistry.Initialize(__instance.cosmetics);
                    // Since the "lastPosition" variable in VRRig is private I had to use a bit of Harmony logic using Traverse to set the value correctly
                    Traverse.Create(__instance).Field("lastPosition").SetValue(__instance.transform.position);
                    return false;
                }

                return true;
            }
        }
    }
}
