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

        private GameObject GameObjectHolder;

        private IEnumerable<Player> _playerInfo;
        private IEnumerable<ExfiltrationPoint> _extract;
        private IEnumerable<LootableContainer> _containers;
        private IEnumerable<LootItem> _item;

        private float _playNextUpdateTime;
        private float _extNextUpdateTime;
        protected float _infoUpdateInterval = 10f;


        private bool _isInfoMenuActive;
        private bool _pInfor;
        private bool _showExtractInfo;
        private bool _showItems;
        private bool _showContainers;

        private double _lowDist = 250.00; // Default distance to midscreen
        private int _AimSpeed = 1; // Default speed = high speed for aimbot (higher value = smoother)
        private bool _smooth = true;
        private bool _aim;
        private float _ContainerDistance = 50f;
        private float _lootItemDistance = 50f;
        private float _weaponBoxesNextUpdateTime;
        private float _itemsNextUpdateTime;
        private float _espUpdateInterval = 500f;
        private float _viewdistance = 1200f;


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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.End))
            {
                Unload();
            }
            if (Input.GetKeyDown(KeyCode.F11))
            {
                _isInfoMenuActive = !_isInfoMenuActive;
            }
            if (Input.GetKeyDown(KeyCode.Mouse3)) // You can change it or create a GUI for change it in game
            {
                _aim = !_aim;
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

            if (_aim)
                Aimbot();

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
                    _containers = UnityEngine.Object.FindObjectsOfType<LootableContainer>();
                    _weaponBoxesNextUpdateTime = Time.time + _espUpdateInterval;
                }
                DrawWeaponBoxesContainers();
            }


            if (_showItems)
            {
                if (Time.time >= _itemsNextUpdateTime)
                {
                    _item = FindObjectsOfType<LootItem>();
                    _itemsNextUpdateTime = Time.time + _espUpdateInterval;
                }
                ShowItemESP();
            }
        }


        private void Aimbot()
        {
            int aimPosX = 0;
            int aimPosY = 0;
            foreach (var player in _playerInfo)
            {
                float distanceToObject = Vector3.Distance(Camera.main.transform.position, player.Transform.position);
                if (distanceToObject < 200)
                {
                    if (player.HealthController.IsAlive && player.IsVisible && EPointOfView.FirstPerson != player.PointOfView)
                    {

                        var playerHeadVector = new Vector2(
                            Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).x,
                            Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).y);
                        double distance = GetDistance(Screen.width / 2, Screen.height / 2, playerHeadVector.x, playerHeadVector.y);
                        if (distance < _lowDist)
                        {
                            aimPosX = (int)playerHeadVector.x;
                            aimPosY = (int)playerHeadVector.y;
                        }
                    }
                }
            }
            AimAtPos(aimPosX, aimPosY);
            //Circle((Screen.width / 2), (Screen.height / 2), (int)_lowDist);
        }

        // From a member of the CS GO section not by me but i've modified some thing for EFT
        private void AimAtPos(int x, int y)
        {
            int ScreenCenterX = (Screen.width / 2);
            int ScreenCenterY = (Screen.height / 2);
            float TargetX = 0;
            float TargetY = 0;
            if (x != 0 && x != ScreenCenterX)
            {
                if (x > ScreenCenterX)
                {
                    TargetX = -(ScreenCenterX - x);
                    if (TargetX + ScreenCenterX > ScreenCenterX * 2) TargetX = 0;
                }

                if (x < ScreenCenterX)
                {
                    TargetX = x - ScreenCenterX;
                    if (TargetX + ScreenCenterX < 0) TargetX = 0;
                }
            }
            if (y != 0 && y != ScreenCenterY)
            {
                if (y > ScreenCenterY)
                {
                    TargetY = ScreenCenterY - y;
                    if (TargetY + ScreenCenterY > ScreenCenterY * 2) TargetY = 0;
                }

                if (y < ScreenCenterY)
                {
                    TargetY = -(y - ScreenCenterY);
                    if (TargetY + ScreenCenterY < 0) TargetY = 0;
                }
            }
            if (!_smooth)
            {
                Move((int)TargetX, (int)TargetY);
                return;
            }
            TargetX /= _AimSpeed;
            TargetY /= _AimSpeed;
            if (Math.Abs(TargetX) < 1)
            {
                if (TargetX > 0)
                {
                    TargetX = 1;
                }
                if (TargetX < 0)
                {
                    TargetX = -1;
                }
            }
            if (Math.Abs(TargetY) < 1)
            {
                if (TargetY > 0)
                {
                    TargetY = 1;
                }
                if (TargetY < 0)
                {
                    TargetY = -1;
                }
            }
            Move((int)TargetX, (int)TargetY);
        }


        private void DrawExtractInfo()
        {
            foreach (var point in _extract)
            {
                if (point.isActiveAndEnabled)
                {
                    float distanceToObject = Vector3.Distance(Camera.main.transform.position, point.transform.position);
                    var exfilContainerBoundingVector = new Vector3(
                        Camera.main.WorldToScreenPoint(point.transform.position).x,
                        Camera.main.WorldToScreenPoint(point.transform.position).y,
                        Camera.main.WorldToScreenPoint(point.transform.position).z);
                    if (exfilContainerBoundingVector.z > 0.01)
                    {
                        GUI.color = Color.green;
                        int distance = (int)distanceToObject;
                        String exfilName = point.name;
                        string boxText = $"{exfilName} - {distance}m";
                        GUI.Label(new Rect(exfilContainerBoundingVector.x - 50f, (float)Screen.height - exfilContainerBoundingVector.y, 100f, 50f), boxText);
                    }
                }
            }
        }

        private void DrawWeaponBoxesContainers()
        {
            foreach (var contain in _containers)
            {
                float distance = Vector3.Distance(Camera.main.transform.position, contain.transform.position);
                if (contain != null && distance < _ContainerDistance)
                {
                    var containBoundingVector = new Vector3(
                        Camera.main.WorldToScreenPoint(contain.transform.position).x,
                        Camera.main.WorldToScreenPoint(contain.transform.position).y,
                        Camera.main.WorldToScreenPoint(contain.transform.position).z);
                    if (containBoundingVector.z > 0.01)
                    {
                        GUI.color = Color.cyan;
                        String contain_name = contain.name;
                        string boxText = $"{contain_name} - [{distance}]m";
                        GUI.Label(new Rect(containBoundingVector.x - 50f, (float)Screen.height - containBoundingVector.y, 100f, 50f), boxText);
                    }
                }
            }
        }

        private void ShowItemESP()
        {
            foreach (var Item in _item)
            {
                if (Item == null)
                    continue;

                float distance = Vector3.Distance(Camera.main.transform.position, Item.transform.position);
                Vector3 ItemBoundingVector = new Vector3(Camera.main.WorldToScreenPoint(Item.transform.position).x, Camera.main.WorldToScreenPoint(Item.transform.position).y, Camera.main.WorldToScreenPoint(Item.transform.position).z);
                if (ItemBoundingVector.z > 0.01 && Item != null && (Item.name.Contains("key") || Item.name.Contains("usb") || Item.name.Contains("alkali") || Item.name.Contains("ophalmo") || Item.name.Contains("gunpowder") || Item.name.Contains("phone") || Item.name.Contains("gas") || Item.name.Contains("money") || Item.name.Contains("document") || Item.name.Contains("quest") || Item.name.Contains("spark") || Item.name.Contains("grizzly") || Item.name.Contains("sv-98") || Item.name.Contains("sv98") || Item.name.Contains("rsas") || Item.name.Contains("salewa") || Item.name.Equals("bitcoin") || Item.name.Contains("dvl") || Item.name.Contains("m4a1") || Item.name.Contains("roler") || Item.name.Contains("chain") || Item.name.Contains("wallet") || Item.name.Contains("RSASS") || Item.name.Contains("glock") || Item.name.Contains("SA-58")) && distance <= _lootItemDistance)
                {
                    name = Item.name;

                    string text = string.Format($"{Item.name} - [{distance}]m");
                    GUI.color = Color.magenta;
                    GUI.Label(new Rect(ItemBoundingVector.x - 50f, (float)Screen.height - ItemBoundingVector.y, 100f, 50f), text);
                }
            }
        }



        private void DrawPlayers()
        {
            foreach (var player in _playerInfo)
            {
                float distanceToObject = Vector3.Distance(Camera.main.transform.position, player.Transform.position);
                Vector3 playerBoundingVector = Camera.main.WorldToScreenPoint(player.Transform.position);
                if (distanceToObject <= _viewdistance && playerBoundingVector.z > 0.01)
                {

                    var playerHeadVector = new Vector3(
                    Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).x,
                    Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).y,
                    Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).z);

                    float boxVectorX = Camera.main.WorldToScreenPoint(player.Transform.position).x;
                    float boxVectorY = Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).y + 10f;
                    float boxHeight = Math.Abs(Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).y - Camera.main.WorldToScreenPoint(player.Transform.position).y) + 10f;
                    float boxWidth = boxHeight * 0.65f;
                    var IsAI = player.Profile.Info.RegistrationDate <= 0;
                    var playerColor = player.HealthController.IsAlive ? GetPlayerColor(player.Side) : Color.gray;

                    Utility.DrawBox(boxVectorX - boxWidth / 2f, (float)Screen.height - boxVectorY, boxWidth, boxHeight, playerColor);
                    Utility.DrawLine(new Vector2(playerHeadVector.x - 2f, (float)Screen.height - playerHeadVector.y), new Vector2(playerHeadVector.x + 2f, (float)Screen.height - playerHeadVector.y), playerColor);
                    Utility.DrawLine(new Vector2(playerHeadVector.x, (float)Screen.height - playerHeadVector.y - 2f), new Vector2(playerHeadVector.x, (float)Screen.height - playerHeadVector.y + 2f), playerColor);

                    var playerName = player.IsAI ? "AI" : player.Profile.Info.Nickname;
                    string playerText = player.HealthController.IsAlive ? playerName : (playerName + " (Dead)");
                    string playerTextDraw = string.Format("{0} [{1}]", playerText, (int)distanceToObject);
                    var playerTextVector = GUI.skin.GetStyle(playerText).CalcSize(new GUIContent(playerText));
                    GUI.Label(new Rect(playerBoundingVector.x - playerTextVector.x / 2f, (float)Screen.height - boxVectorY - 20f, 300f, 50f), playerTextDraw);
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
                    /*
                    case EPlayerSide.Bear:
                        return ColorUtility.TryParseHtmlString(_bearColor);
                    case EPlayerSide.Usec:
                        return ColorUtility.TryParseHtmlString(_usecColor);
                    case EPlayerSide.Savage:
                        return ColorUtility.TryParseHtmlString(_scavColor);
                    default:
                        return Color.white;
                        */
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
            if (_aim)
            {
                GUI.Label(new Rect(110f, 200f, 150f, 20f), "AIM Center distance");
                _lowDist = (GUI.HorizontalSlider(new Rect(220f, 200f, 120f, 20f), (float)_lowDist, 20.0F, 1000.0F)); //Player distance on the mid screen to aim
            }
            _smooth = GUI.Toggle(new Rect(110f, 220f, 120f, 20f), _smooth, "Smooth aim");
            if (_smooth)
            {
                GUI.Label(new Rect(110f, 240f, 150f, 20f), "Speed smoothing");
                _AimSpeed = (int)(GUI.HorizontalSlider(new Rect(220f, 240f, 120f, 20f), _AimSpeed, 1.0F, 100.0F)); //Display  aimspeed (more great value = smoother aim)

            }
            _viewdistance = GUI.HorizontalSlider(new Rect(150f, 260f, 120f, 20f), _viewdistance, 0.0F, 1500.0F); // Distance of players ESP
            _showItems = GUI.Toggle(new Rect(110f, 280f, 120f, 20f), _showItems, "Show Items"); //Show items on map
            if (_showItems)
            {
                GUI.Label(new Rect(110f, 320f, 150f, 20f), "Items Distance");
                _lootItemDistance = GUI.HorizontalSlider(new Rect(210f, 320f, 120f, 20f), _lootItemDistance, 0.0F, 1500.0F);
            }


            _showContainers = GUI.Toggle(new Rect(110f, 340f, 120f, 20f), _showContainers, "Show Containers"); // Show containers on map
            if (_showContainers)
            {
                GUI.Label(new Rect(110f, 360f, 150f, 20f), "Containers Distance");
                _ContainerDistance = GUI.HorizontalSlider(new Rect(210f, 360f, 120f, 20f), _ContainerDistance, 0.0F, 1500.0F);
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