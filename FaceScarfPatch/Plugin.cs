using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace FaceScarfPatch
{
    [BepInPlugin(Guid, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        private const string 
            Guid = "com.dev9998.gorillatag.facescarfpatch",
            Name = "FaceScarfPatch",
            Version = "1.1.2";

        public void Awake()
            => new Harmony(Guid).PatchAll(Assembly.GetExecutingAssembly());

        [HarmonyPatch(typeof(VRRig), "Awake")]
        private class Main
        {
            private static bool Prefix(VRRig __instance)
            {
                __instance.GuidedRefInitialize();
                __instance.fxSettings = Instantiate((FXSystemSettings)AccessTools.Field(__instance.GetType(), "sharedFXSettings").GetValue(__instance));
                __instance.fxSettings.forLocalRig = __instance.isOfflineVRRig;

                if (__instance.isOfflineVRRig)
                {
                    Dictionary<string, GameObject> dictionary = new Dictionary<string, GameObject>();
                    GameObject[] array = __instance.cosmetics;
                    GameObject value;
                    List<GameObject> objectsToOverwrite = new List<GameObject>();
                    foreach (GameObject cosmeticObject in array)
                    {
                        bool correctParent = cosmeticObject.transform.parent.name == "Old Cosmetics Head" || cosmeticObject.transform.parent.name == "WinterJan2023 Head";
                        bool correctName = cosmeticObject.name == "FACE SCARF" || cosmeticObject.name.StartsWith("LFACC.");

                        if (correctName && correctParent)
                            objectsToOverwrite.Add(cosmeticObject);
                        if (!dictionary.TryGetValue(cosmeticObject.name, out value))
                            dictionary.Add(cosmeticObject.name, cosmeticObject);
                    }

                    array = __instance.overrideCosmetics;
                    foreach (GameObject gameObject2 in array)
                    {
                        if (dictionary.TryGetValue(gameObject2.name, out value) && value.name == gameObject2.name && !value.transform.parent.name.ToUpper().Contains("BODY"))
                        {
                            if (value.name == "CLOWN WIG")
                            {
                                gameObject2.name = "OVERRIDDEN";
                                continue;
                            }
                            value.name = "OVERRIDDEN";

                            bool correctName = gameObject2.name.StartsWith("LFACC.");
                            if (correctName && gameObject2.transform.Find("3rdmirror")) gameObject2.transform.GetChild(0).gameObject.SetActive(false);
                        }
                    }

                    objectsToOverwrite.Do(gameObject3 => gameObject3.name = "OVERRIDDEN");
                    __instance.cosmetics = __instance.cosmetics.Concat(__instance.overrideCosmetics).ToArray();
                    __instance.cosmeticsObjectRegistry.Initialize(__instance.cosmetics);

                    AccessTools.Field(__instance.GetType(), "lastPosition").SetValue(__instance, __instance.transform.position);
                    AccessTools.Method(__instance.GetType(), "SharedStart").Invoke(__instance, null);
                }
                
                return false;
            }
        }
    }
}
