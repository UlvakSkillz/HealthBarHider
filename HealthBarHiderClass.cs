using MelonLoader;
using RUMBLE.Managers;
using RUMBLE.Players;
using UnityEngine;

namespace HealthBarHider
{
    public class HealthBarHiderClass : MelonMod
    {
        private bool sceneChanged = false;
        private string currentScene = "";

        public override void OnFixedUpdate()
        {
            if ((sceneChanged) && (currentScene != "") && (currentScene != "Loader"))
            {
                try
                {
                    PlayerManager playerManager = GameObject.Find("Game Instance/Initializable/PlayerManager").GetComponent<PlayerManager>();
                    GameObject localHealth = GameObject.Find("Health/Local/Player health bar");
                    localHealth.SetActive(false);
                    foreach (Player tempPlayer in playerManager.AllPlayers)
                    {
                        if (tempPlayer.Controller != playerManager.localPlayer.Controller)
                        {
                            tempPlayer.Controller.gameObject.transform.GetChild(2).gameObject.SetActive(false);
                        }
                    }
                    sceneChanged = false;
                }
                catch { }
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            sceneChanged = true;
            currentScene = sceneName;
            //not messing with scene change stuff here to allow retrying initializing variables
        }

    }
}
