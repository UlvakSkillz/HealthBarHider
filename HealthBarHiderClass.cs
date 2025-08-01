using MelonLoader;
using Il2CppRUMBLE.Managers;
using Il2CppRUMBLE.Players;
using System;
using UnityEngine;
using RumbleModUI;
using RumbleModdingAPI;
using System.Collections;

namespace HealthBarHider
{
    public class HealthBarHiderClass : MelonMod
    {
        //variables
        private bool hideSelf = false;
        private bool hideOthers = false;
        private Mod HealthbarHider = new Mod();

        public override void OnLateInitializeMelon()
        {
            HealthbarHider.ModName = "Health Bar Hider";
            HealthbarHider.ModVersion = "2.2.0";
            HealthbarHider.SetFolder("HealthbarHider");
            HealthbarHider.AddDescription("Description", "Description", "Toggles Healthbars on Everyone", new Tags { IsSummary = true });
            HealthbarHider.AddToList("Hide Self", true, 0, "Turns On/Off Self Health", new Tags { });
            HealthbarHider.AddToList("Hide Others", false, 0, "Turns On/Off Others Health", new Tags { });
            HealthbarHider.GetFromFile();
            hideSelf = (bool)HealthbarHider.Settings[1].Value;
            hideOthers = (bool)HealthbarHider.Settings[2].Value;
            HealthbarHider.ModSaved += Save;
            UI.instance.UI_Initialized += UIInit;
            Calls.onMapInitialized += mapInit;
            Calls.onPlayerSpawned += playerSpawned;
        }

        public void UIInit()
        {
            UI.instance.AddMod(HealthbarHider);
        }

        public void Save()
        {
            hideSelf = (bool)HealthbarHider.Settings[1].Value;
            hideOthers = (bool)HealthbarHider.Settings[2].Value;
            Calls.Players.GetLocalHealthbarGameObject().transform.GetChild(1).gameObject.SetActive(!hideSelf);
            MelonCoroutines.Start(hideEnemyHealths());
        }

        private void mapInit()
        {
            Calls.Players.GetLocalHealthbarGameObject().transform.GetChild(1).gameObject.SetActive(!hideSelf);
        }
        
        private void playerSpawned()
        {
            if (PlayerManager.instance.AllPlayers.Count > 1)
            {
                MelonCoroutines.Start(hideEnemyHealths());
            }
        }

        private IEnumerator hideEnemyHealths()
        {
            yield return new WaitForSeconds(1);
            foreach (Player tempPlayer in PlayerManager.instance.AllPlayers)
            {
                if (tempPlayer.Controller != PlayerManager.instance.localPlayer.Controller)
                {
                    tempPlayer.Controller.gameObject.transform.GetChild(6).GetChild(1).gameObject.SetActive(!hideOthers);
                }
            }
            yield break;
        }
    }
}
