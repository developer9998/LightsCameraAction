using BepInEx;
using LightsCameraAction.Modules;
using System;
using UnityEngine;
using Utilla.Attributes;
using LightsCameraAction.GUI;
using LightsCameraAction.Tools;
using System.IO;
using System.Reflection;
using Player = GorillaLocomotion.GTPlayer;
using LightsCameraAction.Extensions;
using LightsCameraAction.Interactions;

namespace LightsCameraAction
{
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0"), ModdedGamemode]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;

        public bool GameInitialized { get; private set; }
        public bool Initialized { get; private set; }
        public GameObject menu;

        public LCAModule targetModule;
        public LCAModule[] modules;
        public FirstPerson firstPerson;
        public SelfieStick selfieStick;
        public Tripod tripod;
        public Cinematic cinematic;

        public GestureTracker gestureTracker;
        public MenuController menuController;
        public InteractionManager interactionManager;
        public RepositionListener repositionListener;
        public AssetBundle bundle;

        void Awake()
        {
            Instance = this;
            Logging.Init();
        }

        void Setup()
        {
            Logging.Debug("Setup");
            if (!GameInitialized || menu) return;
            try
            {
                gestureTracker = this.gameObject.AddComponent<GestureTracker>();
                gestureTracker.leftStick.OnPressed += ShowMenu;
                gestureTracker.leftStick.OnReleased += HideMenu;
                repositionListener =  this.gameObject.AddComponent<RepositionListener>();

                LCAModule.Baseline();
                firstPerson = this.gameObject.AddComponent<FirstPerson>().Disable();
                selfieStick = this.gameObject.AddComponent<SelfieStick>().Disable();
                cinematic = this.gameObject.AddComponent<Cinematic>().Disable();
                tripod = this.gameObject.AddComponent<Tripod>().Disable();

                modules = 
                [
                    firstPerson,
                    selfieStick,
                    cinematic,
                    tripod
                ];

                menu = Instantiate(
                    bundle.LoadAsset<GameObject>("LCA Menu")
                );
                menuController = menu.AddComponent<MenuController>();
                interactionManager = menu.AddComponent<InteractionManager>();
                HideMenu();
                Initialized = true;
            }
            catch (Exception e) { Logging.Exception(e); }
        }

        void ShowMenu()
        {
            menu.SetActive(true);
            targetModule = null;
        }

        void FixedUpdate()
        {
            try
            {
                if (!menu || !menu.activeSelf) return;
                var l = Player.Instance.LeftHand.controllerTransform;
                menu.transform.position = l.position;
                menu.transform.rotation = Quaternion.Euler(
                    l.eulerAngles.x + 180, l.eulerAngles.y, 180
                );
                menu.transform.localScale = Vector3.one * Player.Instance.scale;
            } catch(Exception e) { Logging.Exception(e); }
        }

        void HideMenu()
        {
            try
            {
                menuController.cinematicButton.gameObject.SetActive(inRoom);
                menu.SetActive(false);
                foreach (var module in modules)
                    module.enabled = false;

                foreach (var module in modules)
                    module.enabled = (module == targetModule);

                if (targetModule == null)
                    foreach (var button in menuController.modeButtons)
                        button.Unhover();
            }catch(Exception e) { Logging.Exception(e);}
        }

        public static T Load<T>(string name) where T : UnityEngine.Object
        {
            return GameObject.Instantiate(Instance.bundle.LoadAsset<T>(name));
        }

        void Start()
        {
            /* A lot of Gorilla Tag systems will not be set up when start is called /*
			/* Put code in OnGameInitialized to avoid null references */
            Utilla.Events.GameInitialized += OnGameInitialized;
        }

        void OnEnable()
        {
            /* Set up your mod here */
            /* Code here runs at the start and whenever your mod is enabled*/
            HarmonyPatches.ApplyHarmonyPatches();
        }

        void OnDisable()
        {
            /* Undo mod setup here */
            /* This provides support for toggling mods with ComputerInterface, please implement it :) */
            /* Code here runs whenever your mod is disabled (including if it disabled on startup)*/
            firstPerson?.Obliterate();
            gestureTracker?.Obliterate();
            repositionListener?.Obliterate();
            menu?.Obliterate();
            HarmonyPatches.RemoveHarmonyPatches();
        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            /* Code here runs after the game initializes (i.e. GorillaLocomotion.Player.Instance != null) */
            GameInitialized = true;
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LightsCameraAction.Resources.lcabundle");
            bundle = AssetBundle.LoadFromStream(stream);
            if (!bundle)
                Logging.Fatal("Asset Bundle is null");
            Setup();
        }

        public bool inRoom = false;

        [ModdedGamemodeJoin]
        void RoomJoined()
        {
            inRoom = true;
            HideMenu();
        }

        [ModdedGamemodeLeave]
        void RoomLeft()
        {
            inRoom = false;
            if (cinematic && targetModule == cinematic)
            {
                targetModule = null;
                HideMenu();
            }
        }

    }
}
