using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.UI;
using StayInTarkov.Coop.Matchmaker;
using StayInTarkov.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StayInTarkov.Coop.SITGameModes.Headless
{
    public sealed class HeadlessUIRemovalComponent : MonoBehaviour
    {
        private CustomTextMeshProUGUI MainText { get; set; }

        private ManualLogSource Logger { get; set; }

        private ISession BackEndSession { get; set; }

        void Awake()
        {
        }

        void Start()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource($"{nameof(HeadlessUIRemovalComponent)}");
            Logger.LogDebug(nameof(Start));
            StartCoroutine(EverySecond());
        }

        float? lastInput = null;

        void Update()
        {

            if(Singleton<GameUI>.Instantiated)
            {
                Singleton<GameUI>.Instance.enabled = false;
            }

            if (Singleton<MenuUI>.Instantiated)
            {
                //UnityEngine.Object.DestroyImmediate(MonoBehaviourSingleton<MenuUI>.Instance.gameObject);
                //MainMenuController mmc =
                //        (MainMenuController)ReflectionHelpers.GetFieldFromTypeByFieldType(typeof(TarkovApplication), typeof(MainMenuController)).GetValue(__instance);
                //mmc.Unsubscribe();
            }

            if (Singleton<CommonUI>.Instantiated)
            {
                Singleton<CommonUI>.Instance.enabled = false;
            }

            if (Singleton<LoginUI>.Instantiated)
            {
                //Singleton<LoginUI>.Instance.enabled = false;
                //UnityEngine.Object.DestroyImmediate(MonoBehaviourSingleton<LoginUI>.Instance.gameObject);
                //Singleton<LoginUI>.Instance.LoginScreen.CanvasGroup.

                if(MainText == null)
                    MainText = GameObject.FindObjectsOfType<CustomTextMeshProUGUI>().FirstOrDefault(x => x.name == "Main Text");


                if (MainText != null)
                    MainText.text = "Waiting to start Raid";
            }

            //if (Singleton<PreloaderUI>.Instantiated)
            //{
            //    Singleton<PreloaderUI>.Instance.enabled = false;
            //}

            if(Input.GetKeyUp(KeyCode.F4) && !lastInput.HasValue) 
            {
                HeadlessHelpers.StartGame("factory4_day");
            }

            if (BackEndSession == null)
                BackEndSession = StayInTarkovHelperConstants.BackEndSession;
        }

        private IEnumerator EverySecond()
        {
            Logger.LogDebug(nameof(EverySecond));
            yield return new WaitForSeconds(1);
            StartCoroutine(EverySecond());
        }
    }
}
