using MelonLoader;
using Il2CppRUMBLE.Managers;
using Il2CppRUMBLE.Players;
using System;
using UnityEngine;
using RumbleModUI;

namespace HealthBarHider
{
    public class HealthBarHiderClass : MelonMod
    {
        //variables
        private PlayerManager playerManager;
        private bool sceneChanged = false;
        private string currentScene = "Loader";
        private int playerCount = 1;
        private bool hideSelf = false;
        private bool hideOthers = false;
        private DateTime EnemyHealthHideTimer;
        private bool EnemyHealthHideTimerActive;
        private DateTime healthTimer = DateTime.Now;
        private bool healthTimerActive = false;
        private GameObject localHealth;
        private bool init = false;
        UI UI = UI.instance;
        private Mod HealthbarHider = new Mod();

        public override void OnLateInitializeMelon()
        {
            HealthbarHider.ModName = "Health Bar Hider";
            HealthbarHider.ModVersion = "2.0.1";
            HealthbarHider.SetFolder("HealthbarHider");
            HealthbarHider.AddDescription("Description", "Description", "Toggles Healthbars on Everyone", new Tags { IsSummary = true });
            HealthbarHider.AddToList("Hide Self", true, 0, "Turns On/Off Self Health", new Tags { });
            HealthbarHider.AddToList("Hide Others", false, 0, "Turns On/Off Others Health", new Tags { });
            HealthbarHider.GetFromFile();
            hideSelf = (bool)HealthbarHider.Settings[1].Value;
            hideOthers = (bool)HealthbarHider.Settings[2].Value;
            HealthbarHider.ModSaved += Save;
            UI.instance.UI_Initialized += UIInit;
        }

        public void UIInit()
        {
            UI.AddMod(HealthbarHider);
        }

        public void Save()
        {
            hideSelf = (bool)HealthbarHider.Settings[1].Value;
            hideOthers = (bool)HealthbarHider.Settings[2].Value;
            localHealth.SetActive(!hideSelf);
            hideEnemyHealths();
        }

        public override void OnFixedUpdate()
        {
            //if enemy timer active and elapsed
            if ((init) && (EnemyHealthHideTimerActive) && (EnemyHealthHideTimer <= DateTime.Now))
            {
                EnemyHealthHideTimerActive = false;
                hideEnemyHealths();
            }
            //if self timer active and elapsed
            if ((init) && (healthTimerActive) && (healthTimer <= DateTime.Now))
            {
                localHealth = GameObject.Find("Health/Local/Player health bar");
                //hide local health bar
                localHealth.SetActive(false);
                healthTimerActive = false;
            }
            //if scene changed to acceptable scene
            if ((!init) && (sceneChanged) && (currentScene != "Loader"))
            {
                //initialize
                try
                {
                    //get variables
                    playerManager = PlayerManager.instance;
                    playerCount = 1;
                    //if need to hide self
                    if (hideSelf)
                    {
                        //start timer
                        healthTimer = DateTime.Now.AddSeconds(2f);
                        healthTimerActive = true;
                    }
                    //if there's other players and need to hide others
                    if ((hideOthers) && (playerCount != playerManager.AllPlayers.Count))
                    {
                        playerCount = playerManager.AllPlayers.Count;
                        //hide remote healthbars
                        hideEnemyHealths();
                    }
                    sceneChanged = false;
                    init = true;
                }
                catch { }
            }
            //if not initializing and at the Park
            else if (currentScene == "Park")
            {
                //listen for player count change
                if ((hideOthers) && (playerCount != playerManager.AllPlayers.Count))
                {
                    playerCount = playerManager.AllPlayers.Count;
                    //start timer
                    EnemyHealthHideTimer = DateTime.Now.AddSeconds(2);
                    EnemyHealthHideTimerActive = true;
                }
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            //not messing with scene change stuff here to allow retrying initializing variables
            sceneChanged = true;
            currentScene = sceneName;
            init = false;
        }

        //hides the enemy healths
        private void hideEnemyHealths()
        {
            //for each player in all players
            foreach (Player tempPlayer in playerManager.AllPlayers)
            {
                //if player isn't the local player
                if (tempPlayer.Controller != playerManager.localPlayer.Controller)
                {
                    //hide health
                    tempPlayer.Controller.gameObject.transform.GetChild(2).GetChild(0).GetChild(0).gameObject.SetActive(!hideOthers);
                }
            }
        }
    }
}
