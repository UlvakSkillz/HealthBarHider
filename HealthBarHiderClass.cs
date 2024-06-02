using MelonLoader;
using RUMBLE.Managers;
using RUMBLE.Players;
using System;
using System.Collections;
using System.IO;
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
        private string FILEPATH = @"UserData\HealthBarHider";
        private string FILENAME = @"Settings.txt";
        private bool hideSelf = false;
        private bool hideOthers = false;
        private DateTime EnemyHealthHideTimer;
        private bool EnemyHealthHideTimerActive;
        private DateTime healthTimer = DateTime.Now;
        private bool healthTimerActive = false;
        private GameObject localHealth;
        private bool init = false;
        private bool newFile = false;

        public override void OnLateInitializeMelon()
        {
            MelonCoroutines.Start(CheckIfFileExists(FILEPATH, FILENAME));
        }


        public IEnumerator CheckIfFileExists(string filePath, string fileName)
        {
            if (!File.Exists($"{filePath}\\{fileName}"))
            {
                if (!Directory.Exists(filePath))
                {
                    MelonLogger.Msg($"Folder Not Found, Creating Folder: {filePath}");
                    Directory.CreateDirectory(filePath);
                }
                if (!File.Exists($"{filePath}\\{fileName}"))
                {
                    MelonLogger.Msg($"Creating File {filePath}\\{fileName}");
                    File.Create($"{filePath}\\{fileName}");
                }
                newFile = true;
                for (int i = 0; i < 60; i++) { yield return new WaitForFixedUpdate(); }
                string[] newFileText = new string[4];
                newFileText[0] = "Show On Self:";
                newFileText[1] = "False";
                newFileText[2] = "Show on Others:";
                newFileText[3] = "True";
                File.WriteAllLines($"{filePath}\\{fileName}", newFileText);
            }
            yield return null;
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
            if ((!init) && (sceneChanged) && (currentScene != "") && (currentScene != "Loader"))
            {
                //initialize
                try
                {
                    //get variables
                    playerManager = GameObject.Find("Game Instance/Initializable/PlayerManager").GetComponent<PlayerManager>();
                    playerCount = 1;
                    //read file
                    readSettingsFile();
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

        //reads the file to see if it needs to hide self/other healths
        private void readSettingsFile()
        {
            if (newFile)
            {
                hideSelf = true;
                hideOthers = false;
                newFile = false;
                return;
            }
            try
            {
                //reads file lines
                string[] fileContents = File.ReadAllLines(settingsFile);
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
            catch (Exception e)
            {
                //error catch
                MelonLogger.Error(e);
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
