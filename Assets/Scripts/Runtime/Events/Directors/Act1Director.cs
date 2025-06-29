using PlazmaGames.Animation;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using PlazmaGames.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HTJ21
{
    public class Act1Director : Director
    {
        [SerializeField] private List<EventTrigger> _triggers;
        [SerializeField] private List<GameObject> _items;
        [SerializeField] private List<GameObject> _boards;

        private List<Vector3> _boardPos;
        private List<Quaternion> _boardRot;

        [Header("Tree Fall Event")]
        [SerializeField] private WalkAndDie _meanCultist;
        [SerializeField] private TreeFall _fallenTree;

        [Header("Items")]
        [SerializeField] private ItemPickup _flashlightPickup;
        [SerializeField] private OpenInteractable _trunk;
        [SerializeField] private Crowbar _crowbar;

        [SerializeField] private Transform _gpsTarget;
        [SerializeField] private Transform _gpsTargetReroute1;
        [SerializeField] private Transform _gpsTargetReroute2;
        [SerializeField] private Transform _gpsTargetHome;
        [SerializeField] private TreeFall _tree;
        [SerializeField] private WalkAndDie _cultistAtTree;

        [SerializeField] private Door _murderHouseDoor;
        [SerializeField] private Portal _murderHousePortal;
        [SerializeField] private Portal _murderHouseReturnPortal;
        
        [SerializeField] private GameObject _murderHouseDoorBoards;
        [SerializeField] private float _initialDialogueDelay = 2.5f;
        [SerializeField] private float _treeFallDialogueDelay = 2.0f;

        [SerializeField] private Portal _toAct1;
        [SerializeField] private Portal _atAct1;

        [Header("Music")]
        [SerializeField] private AudioSource _oooSource;
        [SerializeField] private AudioSource _whisperSource;
        [SerializeField] private AudioSource _keybaordSource;
        [SerializeField] private AudioSource _darkSource;
        [SerializeField] private float _whisperVolume = 0.4f;
        [SerializeField] private float _keybaodVolume = 0.4f;
        [SerializeField] private float _darkVolume = 0.6f;


        private float _startTime = 0;
        private float _treeFallTime = 0;

        private List<Rigidbody> _doorBoards;

        private IGPSMonoSystem _gpsMs;
        
        private Dictionary<string, DialogueSO> _dialogues = new();

        private void LoadDialogue(string name)
        {
            _dialogues.Add(name, Resources.Load<DialogueSO>($"Dialogue/Act1/{name}"));
        }
        
        public override void OnActInit()
        {
            LoadDialogue("1");
            LoadDialogue("2");
            LoadDialogue("3");
            LoadDialogue("4");

            ClosePortals();
            _gpsMs = GameManager.GetMonoSystem<IGPSMonoSystem>();

            _doorBoards = _murderHouseDoorBoards.GetComponentsInChildren<Rigidbody>().ToList();
                
            GameManager.AddEventListener<Events.TreeFall>(Events.NewTreeFall((from, data) =>
            {
                GameManager.GetMonoSystem<IWeatherMonoSystem>().SpawnLightingAt(_tree.transform.position);
                _tree.GetComponent<AudioSource>().Play();
                _treeFallTime = Time.time;
                _cultistAtTree.Walk();
                _tree.Fall();
            }));

            GameManager.AddEventListener<Events.TreeReroute1>(Events.NewTreeFall((from, data) =>
            {
                StartCoroutine(AudioHelper.FadeIn(_whisperSource, _whisperVolume, 5f));
                _gpsMs.MoveTarget(_gpsTargetReroute2.position);
            }));

            GameManager.AddEventListener<Events.TreeReroute2>(Events.NewTreeFall((from, data) =>
            {
                _gpsMs.MoveTarget(_gpsTargetHome.position);
            }));


            GameManager.AddEventListener<Events.ArriveAtNeighborhood>(Events.NewArriveAtNeighborhood((from, data) =>
            {
                StartCoroutine(AudioHelper.FadeOut(_whisperSource, 5f));
                StartCoroutine(AudioHelper.FadeIn(_keybaordSource, _keybaodVolume, 5f));
                StartCoroutine(AudioHelper.FadeIn(_darkSource, _darkVolume, 5f));
                _gpsMs.TurnOff();
                GameManager.GetMonoSystem<IDialogueMonoSystem>().Load(_dialogues["3"]);
            }));

            GameManager.AddEventListener<Events.ArriveAtHouse>(Events.NewArriveAtHouse((from, data) =>
            {
                GameManager.GetMonoSystem<IDialogueMonoSystem>().Load(_dialogues["4"]);
            }));

            _murderHouseDoor.OnOpen.AddListener(() =>
            {
                OpenPortals();
                HTJ21GameManager.HouseController.EnableHouse();
                HTJ21GameManager.HouseController.PreloadAct(GameManager.GetMonoSystem<IDirectorMonoSystem>().GetCurrentAct().Next());
            });

            _boardPos = new List<Vector3>();
            _boardRot = new List<Quaternion>();
            foreach (GameObject board in _boards)
            {
                _boardPos.Add(board.transform.position);
                _boardRot.Add(board.transform.rotation);
            }
        }

        private void OpenPortals()
        {
            _murderHousePortal.Enable();
            _murderHouseReturnPortal.Enable();
        }

        private void ClosePortals()
        {
            _murderHousePortal.Disable();
            _murderHouseReturnPortal.Disable();
        }

        private void RestartInteractablesRecursive(GameObject obj)
        {
            if (obj.TryGetComponent(out IInteractable interactable))
            {
                interactable.Restart();
            }

            foreach (Transform child in obj.transform)
            {
                RestartInteractablesRecursive(child.gameObject);
            }
        }

        private void Setup()
        {
            GameManager.GetMonoSystem<IAnimationMonoSystem>().StopAllAnimations(this);
            GameManager.GetMonoSystem<IAnimationMonoSystem>().StopAllAnimations(HTJ21GameManager.Player);
            GameManager.GetMonoSystem<IAnimationMonoSystem>().StopAllAnimations(HTJ21GameManager.Car);
            GameManager.GetMonoSystem<IAnimationMonoSystem>().StopAllAnimations(HTJ21GameManager.Inspector);

            HTJ21GameManager.Car.RestartHitch();

            foreach (EventTrigger trigger in _triggers) trigger.Restart();

            _crowbar.Restart();

            foreach (GameObject item in _items)
            {
                RestartInteractablesRecursive(item);
            }

            for (int i = 0; i < _boards.Count; i++)
            {
                if (_boards[i].TryGetComponent(out Rigidbody rb))
                {
                    rb.isKinematic = true;
                }

                _boards[i].transform.position = _boardPos[i];
                _boards[i].transform.rotation = _boardRot[i];
            }

            _murderHouseDoor.Restart();
            _murderHouseDoor.Lock();

            _meanCultist.Restart();
            _fallenTree.Restart();

            HTJ21GameManager.Player.EnablePlayer();
            HTJ21GameManager.Player.StopLookAt();
            HTJ21GameManager.Player.LockMoving = false;
            HTJ21GameManager.Player.LockMovement = false;
            HTJ21GameManager.Player.ResetHead();
            HTJ21GameManager.Player.ResetCamera();

            HTJ21GameManager.Inspector.EndInspect();

            _flashlightPickup.Restart();
            HTJ21GameManager.PickupManager.DropAll();
            HTJ21GameManager.Player.TurnOffLight();

            if (!HTJ21GameManager.CinematicCar.IsEnabled())
            {
                HTJ21GameManager.Car.ZeroVelocity();
                HTJ21GameManager.Car.Restart();
            }
            else
            {
                HTJ21GameManager.CinematicCar.TransferCurrentSpeedToCar();
                HTJ21GameManager.CinematicCar.Disable();
            }

            _trunk.Close();
            HTJ21GameManager.Car.SetDrivingProfile("Normal");
            HTJ21GameManager.Car.SuperDuperEnterCarForReal();
            HTJ21GameManager.Car.EnableMirrors();
            HTJ21GameManager.Car.Lock();
            HTJ21GameManager.CarTutorial.DisableAllTutorial();
            HTJ21GameManager.PlayerTutorial.DisableAllTutorial();
            HTJ21GameManager.CarTutorial.ShowTutorial(0);
            HTJ21GameManager.Car.EnterCar();

            if (!GameManager.GetMonoSystem<IUIMonoSystem>().GetCurrentViewIs<GameView>()) GameManager.GetMonoSystem<IUIMonoSystem>().Show<GameView>();
            HTJ21GameManager.IsPaused = false;

            _murderHouseDoor.Lock();

            ClosePortals();
            HTJ21GameManager.Player.GetComponent<PortalObject>().OnPortalEnter.AddListener(OnPortalEnter);

            _toAct1.Disable();
            _atAct1.Disable();

            GameManager.GetMonoSystem<IUIMonoSystem>().GetView<GameView>().SkipLocation();
            GameManager.GetMonoSystem<IDialogueMonoSystem>().ResetDialogueAll();
            GameManager.GetMonoSystem<IUIMonoSystem>().GetView<GameView>().ForceStopDialogue();
            GameManager.GetMonoSystem<IAudioMonoSystem>().StopAudio(PlazmaGames.Audio.AudioType.Music);
            GameManager.GetMonoSystem<IScreenEffectMonoSystem>().RestoreDefaults();
            GameManager.GetMonoSystem<IWeatherMonoSystem>().EnableRain();
            GameManager.GetMonoSystem<IWeatherMonoSystem>().EnableThunder();
            _gpsMs.TurnOff();

            StopAllMusic();
            _oooSource.Play();

            _startTime = Time.time;
        }

        private void StopAllMusic()
        {
            _oooSource.Stop();
            _whisperSource.Stop();
            _darkSource.Stop();
            _keybaordSource.Stop();
        }

        public override void OnActStart()
        {
            Setup();
        }
        
        private void OnPortalEnter(Portal p1, Portal p2)
        {
            GameManager.GetMonoSystem<IDirectorMonoSystem>().NextAct();
        }

        public override void OnActUpdate()
        {
            if (_startTime > 0 && Time.time - _startTime >= _initialDialogueDelay)
            {
                _startTime = 0;
                GameManager.GetMonoSystem<IDialogueMonoSystem>().Load(_dialogues["1"]);
            }

            if (_treeFallTime > 0 && Time.time - _treeFallTime >= _treeFallDialogueDelay)
            {
                _treeFallTime = 0;
                GameManager.GetMonoSystem<IDialogueMonoSystem>().Load(_dialogues["2"]);
            }
            
            if (_doorBoards.All(e => e.isKinematic == false))
            {
                _murderHouseDoor.Unlock();
            }
        }

        public override void OnActEnd()
        {
            ClosePortals();
            HTJ21GameManager.Player.GetComponent<PortalObject>().OnPortalEnter.RemoveListener(OnPortalEnter);
            StopAllMusic();
        }

        public void UpdateGps()
        {
            _gpsMs.TurnOn();
        }
    }
}
