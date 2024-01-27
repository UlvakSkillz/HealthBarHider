using MelonLoader;
using RUMBLE.Managers;
using RUMBLE.Players;
using System;
using UnityEngine;

namespace HealthBarHider
{
    public class HealthBarHiderClass : MelonMod
    {
        //variables
        private PlayerManager playerManager;
        private bool sceneChanged = false;
        private string currentScene = "";
        private int playerCount = 1;
        private string settingsFile = @"UserData\HealthBarHider\Settings.txt";
        private bool hideSelf = false;
        private bool hideOthers = false;
        private DateTime EnemyHealthHideTimer;
        private bool EnemyHealthHideTimerActive;

        public override void OnFixedUpdate()
        {
            //if timer active and elapsed
            if ((EnemyHealthHideTimerActive) && (EnemyHealthHideTimer <= DateTime.Now))
            {
                EnemyHealthHideTimerActive = false;
                hideEnemyHealths();
                MelonLogger.Msg("Timer Ended - Set Players Health");
            }
            //if scene changed to acceptable scene
            if ((sceneChanged) && (currentScene != "") && (currentScene != "Loader"))
            {
                //initialize
                try
                {
                    //get variables
                    playerManager = GameObject.Find("Game Instance/Initializable/PlayerManager").GetComponent<PlayerManager>();
                    GameObject localHealth = GameObject.Find("Health/Local/Player health bar");
                    playerCount = 1;
                    //read file
                    readSettingsFile();
                    //if need to hide self
                    if (hideSelf)
                    {
                        //hide local health bar
                        localHealth.SetActive(false);
                    }
                    //if there's other players and need to hide others
                    if ((hideOthers) && (playerCount != playerManager.AllPlayers.Count))
                    {
                        playerCount = playerManager.AllPlayers.Count;
                        //hide remote healthbars
                        hideEnemyHealths();
                    }
                    sceneChanged = false;
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
                    MelonLogger.Msg("Timer Started");
                }
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            //not messing with scene change stuff here to allow retrying initializing variables
            sceneChanged = true;
            currentScene = sceneName;
        }

        //reads the file to see if it needs to hide self/other healths
        private void readSettingsFile()
        {
            if (System.IO.File.Exists(settingsFile))
            {
                try
                {
                    //reads file lines
                    string[] fileContents = System.IO.File.ReadAllLines(settingsFile);
                    //checks for hide self
                    if (fileContents[1].ToLower() == "true")
                    {
                        hideSelf = true;
                    }
                    else
                    {
                        hideSelf = false;
                    }
                    //checks for hide others
                    if (fileContents[3].ToLower() == "true")
                    {
                        hideOthers = true;
                    }
                    else
                    {
                        hideOthers = false;
                    }
                }
                catch
                {
                    //error catch
                    hideSelf = false;
                    hideOthers = false;
                }
            }
            else
            {
                //error catch
                hideSelf = false;
                hideOthers = false;
            }
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
                    tempPlayer.Controller.gameObject.transform.GetChild(2).GetChild(0).GetChild(0).gameObject.SetActive(false);
                }
            }
        }
    }
}
