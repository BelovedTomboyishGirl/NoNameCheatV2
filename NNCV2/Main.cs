using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using EFT;

namespace Nncv2
{
    public class Main : MonoBehaviour
    {

        public Main() { }
        #region variable
        private GameObject GameObjectHolder;

        private IEnumerable<Player> _playerInfo;
        private IEnumerable<ExfiltrationPoint> _extract;
        private IEnumerable<LootableContainer> _containers;
        private IEnumerable<LootItem> _item;

        private float _playNextUpdateTime;
        private float _extNextUpdateTime;
        protected float _infoUpdateInterval = 15f;


        private bool _isInfoMenuActive;
        private bool _pInfor;
        private bool _showExtractInfo;
        private bool _showItems;
        private bool _showContainers;

        private bool _smooth = true;
        private bool _aim;
        private float _ContainerDistance = 50f;
        private float _lootItemDistance = 50f;
        private float _weaponBoxesNextUpdateTime;
        private float _itemsNextUpdateTime;
        private float _espUpdateInterval = 500f;
        private float _itemUpdateInterval = 600f;
        private float _viewdistance = 1200f;
        private Player _localPlayer;
        private Vector3 camPos;
        private float _localPlayerRefresh;
        #endregion

        #region import

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        private const int MOUSEEVENTF_MOVE = 0x0001;
        public static void Move(int xDelta, int yDelta)
        {
            mouse_event(MOUSEEVENTF_MOVE, xDelta, yDelta, 0, 0);
        }
        public struct POINT
        {
            public int X;
            public int Y;
        }
        #endregion


        private void Start()
        {
            Clear();
        }
        private void Clear()
        {
            _playerInfo = null;
            _extract = null;
            _containers = null;
            _item = null;
            _weaponBoxesNextUpdateTime = 0;
            _itemsNextUpdateTime = 0;
            _localPlayer = null;
            _localPlayerRefresh = 0;
            GC.Collect();
        }

        public void Load()
        {
            GameObjectHolder = new GameObject();
            GameObjectHolder.AddComponent<Main>();
            DontDestroyOnLoad(GameObjectHolder);
        }


        public void Unload()
        {
            Destroy(GameObjectHolder);
            Destroy(this);
        }

        private void OnDisable()
        {
            Clear();
        }
        private void OnDestroy()
        {
            Clear();
        }


        void Update()
        {
            if (Input.GetKeyDown(KeyCode.End))
            {
                Unload();
            }
            if (Input.GetKeyDown(KeyCode.F11))
            {
                _isInfoMenuActive = !_isInfoMenuActive;
            }
            camPos = Camera.main.transform.position;
            if (Time.time > _localPlayerRefresh)
            {
                GetLocalPlayer();
                _localPlayerRefresh = Time.time + 20.0f;
            }
        }


        private void OnGUI()
        {
            if (_isInfoMenuActive)
            {
                GUIOverlay();
            }

            if ((_pInfor && Time.time >= _playNextUpdateTime) || (_aim && Time.time >= _playNextUpdateTime))
            {
                _playerInfo = FindObjectsOfType<Player>();
                _playNextUpdateTime = Time.time + _infoUpdateInterval;
            }

            if (_pInfor)
            {
                DrawPlayers();
            }

            if (_showExtractInfo && Time.time >= _extNextUpdateTime)
            {
                if (Time.time >= _extNextUpdateTime)
                {
                    _extract = FindObjectsOfType<ExfiltrationPoint>();
                    _extNextUpdateTime = Time.time + _infoUpdateInterval;
                }
                DrawExtractInfo();
            }

            if (_showContainers)
            {
                if (Time.time >= _weaponBoxesNextUpdateTime)
                {
                    _containers = FindObjectsOfType<LootableContainer>();
                    _weaponBoxesNextUpdateTime = Time.time + _espUpdateInterval;
                }
                DrawWeaponBoxesContainers();
            }


            if (_showItems)
            {
                if (Time.time >= _itemsNextUpdateTime)
                {
                    _item = FindObjectsOfType<LootItem>();
                    _itemsNextUpdateTime = Time.time + _itemUpdateInterval;
                }
                ShowItemESP();
            }
        }

        private void GetLocalPlayer()
        {
            foreach (Player player in FindObjectsOfType<Player>())
            {
                if (EPointOfView.FirstPerson == player.PointOfView && player != null)
                {
                    _localPlayer = player;
                }
            }
        }

        private void DrawExtractInfo()
        {
            foreach (var point in _extract)
            {
              // doesn't work
                if (point.isActiveAndEnabled)
                {
                    var exfilContainerBoundingVector = Camera.main.WorldToScreenPoint(point.transform.position);
                    if (exfilContainerBoundingVector.z > 0.01)
                    {
                        float distanceToObject = Vector3.Distance(camPos, point.transform.position);
                        GUI.color = Color.green;
                        string boxText = $"{point.name} - {(int)distanceToObject}m";
                        GUI.Label(new Rect(exfilContainerBoundingVector.x - 50f, (float)Screen.height - exfilContainerBoundingVector.y, 100f, 50f), boxText);
                    }
                }
            }
        }



        private void DrawWeaponBoxesContainers()
        {
            foreach (var contain in _containers)
            {
                if(contain != null)
                {
                    float distance = Vector3.Distance(camPos, contain.transform.position);
                    var containBoundingVector = Camera.main.WorldToScreenPoint(contain.transform.position);
                    // container distance added
                    if (containBoundingVector.z > 0.01 && distance <= _ContainerDistance)
                    {
                        GUI.color = Color.cyan;
                        string boxText = $"{contain.name} - {(int)distance}m";
                        GUI.Label(new Rect(containBoundingVector.x - 50f, (float)Screen.height - containBoundingVector.y, 100f, 50f), boxText);
                    }
                }
            }
        }

        private void ShowItemESP()
        {
            foreach (var Item in _item)
            {
                if(Item != null) {
                  /*
                   * performance optimization:
                   * first check if item.name contains before creating distance and vector variables
                   */
                  // alkali removed
                    if(Item.name.Contains("key") || 
                        Item.name.Contains("usb") ||  
                        Item.name.Contains("ophalmo") || 
                        Item.name.Contains("gunpowder") || 
                        Item.name.Contains("phone") || 
                        Item.name.Contains("gas") || 
                        Item.name.Contains("money") || 
                        Item.name.Contains("document") || 
                        Item.name.Contains("quest") || 
                        Item.name.Contains("spark") || 
                        Item.name.Contains("grizzly") || 
                        Item.name.Contains("sv-98") || 
                        Item.name.Contains("sv98") || 
                        Item.name.Contains("rsas") || 
                        Item.name.Contains("salewa") || 
                        Item.name.Equals("bitcoin") ||
                        Item.name.Contains("bitcoin") ||
                        Item.name.Contains("dvl") || 
                        Item.name.Contains("m4a1") || 
                        Item.name.Contains("roler") || 
                        Item.name.Contains("chain") || 
                        Item.name.Contains("wallet") || 
                        Item.name.Contains("RSASS") || 
                        Item.name.Contains("glock") ||
                        Item.name.Contains("weapon") ||
                        Item.name.Contains("car") ||
                        Item.name.Contains("SA-58")
                        ) {
                        float distance = Vector3.Distance(camPos, Item.transform.position);
                        Vector3 ItemBoundingVector = Camera.main.WorldToScreenPoint(Item.transform.position);
                        if (ItemBoundingVector.z > 0.01 && distance <= _lootItemDistance)
                        {
                            // delete unnecessary info to keep interface clean
                            String itemName = Item.name.Replace("item", "").Replace("_", " ").Replace("(Clone)", "");
                            string text = $"{itemName} - {(int)distance}m";
                            GUI.color = Color.magenta;
                            GUI.Label(new Rect(ItemBoundingVector.x - 50f, (float)Screen.height - ItemBoundingVector.y, 100f, 50f), text);
                        }
                    }
                }
            }
        }


        /*
         * Player ESP
         * Now with Weapon display in box
         * and dead people
         * todo: Extra option if weapon/dead people should be shown?
         */
        private void DrawPlayers()
        {
            foreach (var player in _playerInfo)
            {
                if (player != null)
                {
                    Vector3 playerPos = player.Transform.position;
                    float distanceToObject = Vector3.Distance(camPos, player.Transform.position);
                    Vector3 playerBoundingVector = Camera.main.WorldToScreenPoint(playerPos);
                    var playerColor = Color.gray;
                    if (distanceToObject <= _viewdistance && playerBoundingVector.z > 0.01)
                    {
                        Vector3 playerHeadVector = Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position);
                        // add weapon to esp
                        string weapon = "";
                        float boxVectorX = playerBoundingVector.x;
                        float boxVectorY = playerHeadVector.y + 10f;
                        float boxHeight = Math.Abs(playerHeadVector.y - playerBoundingVector.y) + 10f;
                        float boxWidth = boxHeight * 0.65f;
                        var IsAI = player.Profile.Info.RegistrationDate <= 0;

                        if (player.HealthController.IsAlive)
                        {
                            playerColor = GetPlayerColor(player.Side);
                        }
                        else if (!player.HealthController.IsAlive)
                        {
                            playerColor = Color.gray;
                        }
                        try
                        {
                            weapon = player.Weapon.Template.ShortName.Contains("Item") ? player.Weapon.Template.Name : player.Weapon.Template.ShortName;
                            if (weapon.Contains("weapon"))
                            {
                                weapon.Replace("weapon", "").Replace("_", " ");
                            }
                        } catch 
                            {
                                weapon = "";
                            }
                            Utility.DrawBox(boxVectorX - boxWidth / 2f, (float)Screen.height - boxVectorY, boxWidth, boxHeight, playerColor);
                            Utility.DrawLine(new Vector2(playerHeadVector.x - 2f, (float)Screen.height - playerHeadVector.y), new Vector2(playerHeadVector.x + 2f, (float)Screen.height - playerHeadVector.y), playerColor);
                            Utility.DrawLine(new Vector2(playerHeadVector.x, (float)Screen.height - playerHeadVector.y - 2f), new Vector2(playerHeadVector.x, (float)Screen.height - playerHeadVector.y + 2f), playerColor);
                            var playerName = IsAI ? "AI" : player.Profile.Info.Nickname;
                            string playerText = playerName;
                            string playerTextDraw = string.Format("{0} [{1}]", playerText, (int)distanceToObject);
                            var playerTextVector = GUI.skin.GetStyle(playerText).CalcSize(new GUIContent(playerText));
                            GUI.Label(new Rect(playerBoundingVector.x - playerTextVector.x / 2f, (float)Screen.height - boxVectorY - 20f, 300f, 50f), playerTextDraw,);
                            GUI.Label(new Rect(playerBoundingVector.x - playerTextVector.x / 2f, (float)Screen.height - boxVectorY, 300f, 50f), weapon);
                    }
                }
            }

        }


        private Color GetPlayerColor(EPlayerSide side)
        {
            switch (side)
            {
                case EPlayerSide.Bear:
                    return Color.red;
                case EPlayerSide.Usec:
                    return Color.blue;
                case EPlayerSide.Savage:
                    return Color.white;
                default:
                    return Color.white;
            }
        }

        private void GUIOverlay()
        {
            GUI.color = Color.gray;
            GUI.Box(new Rect(100f, 100f, 400f, 500f), "");
            GUI.color = Color.white;
            GUI.Label(new Rect(180f, 110f, 150f, 20f), "Settings");
            _pInfor = GUI.Toggle(new Rect(110f, 140f, 120f, 20f), _pInfor, "Players ESP"); // Display player
            _showExtractInfo = GUI.Toggle(new Rect(110f, 160f, 120f, 20f), _showExtractInfo, "Extract"); //Display  extraction
            _aim = GUI.Toggle(new Rect(110f, 180f, 120f, 20f), _aim, "Aimbot"); //Display  aimbot
            _smooth = GUI.Toggle(new Rect(110f, 200f, 120f, 20f), _smooth, "Smooth aim");
            _viewdistance = GUI.HorizontalSlider(new Rect(150f, 220, 120f, 20f), _viewdistance, 0.0F, 1500.0F); // Distance of players ESP
            _showItems = GUI.Toggle(new Rect(110f, 240, 120f, 20f), _showItems, "Show Items"); //Show items on map
            if (_showItems)
            {
                GUI.Label(new Rect(110f, 260, 150f, 20f), "Items Distance");
                _lootItemDistance = GUI.HorizontalSlider(new Rect(210f, 280f, 120f, 20f), _lootItemDistance, 0.0F, 1500.0F);
            }


            _showContainers = GUI.Toggle(new Rect(110f, 300, 120f, 20f), _showContainers, "Show Containers"); // Show containers on map
            if (_showContainers)
            {
                GUI.Label(new Rect(110f, 320, 150f, 20f), "Containers Distance");
                _ContainerDistance = GUI.HorizontalSlider(new Rect(210f, 340f, 120f, 20f), _ContainerDistance, 0.0F, 1500.0F);
            }
        }

        private double GetDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2.0) + Math.Pow(y2 - y1, 2.0));
        }

        private void Circle(int X, int Y, int radius)
        {
            float boxXOffset = X;
            float boxYOffset = Y;
            float boxHeight = radius;
            float boxwidth = radius;
            Utility.DrawBox(boxXOffset - (radius / 2), boxYOffset - (radius / 2), radius, radius, Color.yellow);
            Utility.DrawLine(new Vector2(960, 1080), new Vector2(960, 0), Color.white);
            Utility.DrawLine(new Vector2(0, 540), new Vector2(1920, 540), Color.white);
        }
    }
}