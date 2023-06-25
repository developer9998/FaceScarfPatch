using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

// Comments provided for C# beginners
namespace FaceScarfPatch
{
    [BepInPlugin(Guid, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        private const string Guid = "com.dev9998.gorillatag.facescarfpatch";
        private const string Name = "FaceScarfPatch";
        private const string Version = "1.1.1";

        public void Awake()
            => new Harmony(Guid).PatchAll(Assembly.GetExecutingAssembly());

        [HarmonyPatch(typeof(VRRig), "Awake")]
        internal class Main
        {
            internal static bool Prefix(VRRig __instance)
            {
                // If this rig isn't for our local rig, just go on as usual
                if (!__instance.isOfflineVRRig)
                    return true;

                Dictionary<string, GameObject> dictionary = new Dictionary<string, GameObject>();
                GameObject[] array = __instance.cosmetics;
                GameObject value;
                List<GameObject> objectsToOverwrite = new List<GameObject>();
                foreach (GameObject cosmeticObject in array)
                {
                    // See if the parent of the object matches the one we need to overwrite
                    bool correctParent = cosmeticObject.transform.parent.name == "Old Cosmetics Head" || cosmeticObject.transform.parent.name == "WinterJan2023 Head";
                    // Set if the name of the object matches the one we need to overwrite
                    bool correctName = cosmeticObject.name == "FACE SCARF" || cosmeticObject.name.StartsWith("LFACC.");

                    // If everything matches up, add the object to a list of objects soon to be overwritten
                    if (correctName && correctParent)
                        objectsToOverwrite.Add(cosmeticObject);
                    if (!dictionary.TryGetValue(cosmeticObject.name, out value))
                        dictionary.Add(cosmeticObject.name, cosmeticObject);
                }

                array = __instance.overrideCosmetics;
                foreach (GameObject gameObject2 in array)
                {
                    // Go through all the overrided cosmetics, pair them with all the cosmetics, and see if we can both match an overrided cosmetic with a
                    // cosmetic (I assume the same object) and we can ensure the object isn't parented in an object that's name doesn't begin with "BODY"
                    if (dictionary.TryGetValue(gameObject2.name, out value) && value.name == gameObject2.name && !value.transform.parent.name.ToUpper().Contains("BODY"))
                    {
                        // If the non-overrided cosmetic has the name "Clown Wig", change the overrided cosmetic to be overridden. For K9
                        if (value.name == "CLOWN WIG")
                        {
                            gameObject2.name = "OVERRIDDEN";
                            continue;
                        }
                        // Change the non-overrided cosmetic to be overridden
                        value.name = "OVERRIDDEN";

                        // See if the name of the object matches the one we need to be edited
                        bool correctName = gameObject2.name.StartsWith("LFACC.");

                        // If everything matches up and we can find a "3rdmirror" transform in the object, get the first
                        // child of that object and deactivate it
                        if (correctName && gameObject2.transform.Find("3rdmirror"))
                            gameObject2.transform.GetChild(0).gameObject.SetActive(false);
                    }
                }

                // Overwrite the objects we've concluded needed to be overwritten
                foreach (GameObject gameObject3 in objectsToOverwrite)
                    gameObject3.name = "OVERRIDDEN";

                // Concat the list of cosmetics with the list of overwritten cosmetics, then create an array using the list
                __instance.cosmetics = __instance.cosmetics.Concat(__instance.overrideCosmetics).ToArray();
                // Initalize all of those cosmetics including the changes we just did
                __instance.cosmeticsObjectRegistry.Initialize(__instance.cosmetics);

                // Using AccessTools, we're able to set the value and invoke private fields and methods
                // Here we set the last position of the rig to the rig's current position, and then we invoke the "SharedStart"
                // which is really important as that's used for docking holdables, preparing the player's skin, and in general initalization
                AccessTools.Field(__instance.GetType(), "lastPosition").SetValue(__instance, __instance.transform.position);
                AccessTools.Method(__instance.GetType(), "SharedStart").Invoke(__instance, null);
                return false;
            }
        }
    }
}
