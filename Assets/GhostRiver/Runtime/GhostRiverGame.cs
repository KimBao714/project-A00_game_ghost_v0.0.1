using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GhostRiver
{
    public sealed class GhostRiverGame : MonoBehaviour
    {
        private const float PixelsPerUnit = 100f;
        private const float WorldEnd = 3390f;
        private const float ExitX = 3305f;
        private const int GroundLayer = 8;
        public static readonly LayerMask GroundMask = 1 << GroundLayer;

        private static readonly Color Gold = new Color(1f, 0.70f, 0.24f);
        private static readonly Dictionary<string, Sprite> ArtSprites = new Dictionary<string, Sprite>();
        private static Sprite pixelSprite;
        private static Sprite frameSprite;

        private static readonly Vector3Int[] PlatformData =
        {
            new Vector3Int(0, 120, 191), new Vector3Int(120, 230, 185), new Vector3Int(230, 345, 191), new Vector3Int(345, 460, 188),
            new Vector3Int(540, 625, 180), new Vector3Int(625, 700, 175), new Vector3Int(700, 790, 180), new Vector3Int(790, 860, 176),
            new Vector3Int(955, 1035, 202), new Vector3Int(1035, 1110, 196), new Vector3Int(1110, 1210, 202),
            new Vector3Int(1300, 1385, 186), new Vector3Int(1385, 1470, 179), new Vector3Int(1470, 1550, 186), new Vector3Int(1550, 1625, 182),
            new Vector3Int(1710, 1800, 208), new Vector3Int(1800, 1880, 202), new Vector3Int(1880, 1960, 208), new Vector3Int(1960, 2010, 204),
            new Vector3Int(2095, 2180, 177), new Vector3Int(2180, 2260, 171), new Vector3Int(2260, 2325, 177), new Vector3Int(2325, 2390, 173),
            new Vector3Int(2470, 2555, 199), new Vector3Int(2555, 2635, 191), new Vector3Int(2635, 2710, 199), new Vector3Int(2710, 2795, 193),
            new Vector3Int(2875, 3400, 184)
        };

        private static readonly Vector3Int[] LedgeData =
        {
            new Vector3Int(96, 168, 185), new Vector3Int(252, 342, 180), new Vector3Int(386, 474, 150), new Vector3Int(514, 592, 177),
            new Vector3Int(475, 537, 174), new Vector3Int(680, 750, 177), new Vector3Int(788, 882, 167), new Vector3Int(875, 943, 154),
            new Vector3Int(885, 933, 203), new Vector3Int(929, 1007, 139), new Vector3Int(1031, 1098, 171),
            new Vector3Int(1152, 1230, 155), new Vector3Int(1222, 1288, 171), new Vector3Int(1240, 1284, 215),
            new Vector3Int(1320, 1404, 176), new Vector3Int(1442, 1533, 145), new Vector3Int(1576, 1658, 119),
            new Vector3Int(1691, 1781, 157), new Vector3Int(1773, 1847, 169), new Vector3Int(1884, 1974, 178),
            new Vector3Int(2011, 2093, 147), new Vector3Int(2020, 2070, 207), new Vector3Int(2020, 2090, 174),
            new Vector3Int(2133, 2220, 118), new Vector3Int(2250, 2330, 166), new Vector3Int(2398, 2464, 161),
            new Vector3Int(2405, 2453, 204), new Vector3Int(2440, 2526, 180), new Vector3Int(2571, 2666, 150),
            new Vector3Int(2709, 2797, 124), new Vector3Int(2805, 2871, 162), new Vector3Int(2844, 2938, 154),
            new Vector3Int(3005, 3079, 145), new Vector3Int(3092, 3182, 177)
        };

        private readonly List<GhostEnemy> enemies = new List<GhostEnemy>();
        private readonly List<CoinDrop> coins = new List<CoinDrop>();
        private readonly List<EnemyShot> shots = new List<EnemyShot>();
        private readonly bool[] collectedKeys = new bool[3];
        private readonly bool[] discoveredCreatures = new bool[3];
        private readonly GameObject[] keyObjects = new GameObject[3];
        private readonly string[] creatureNames = { "Bóng bùn", "Bọ giáp", "Đom đóm ma" };
        private readonly string[] creatureHints = { "Kẻ phục kích", "Thợ săn bật nhảy", "Xạ thủ lơ lửng" };
        private readonly string[] creatureDetails =
        {
            "Bóng bùn tuần tra trên nền đá và lao thẳng tới sau khi co mình báo hiệu. Hãy nhảy qua hoặc lướt xuyên đúng lúc.",
            "Bọ giáp bật vọt về phía bạn. Quan sát tư thế hạ thấp trước khi nó lao tới.",
            "Đom đóm ma lơ lửng và ngắm theo vị trí bạn trước khi phóng đạn sáng. Tiếp tục di chuyển để tránh đường ngắm."
        };
        private readonly float[] keyX = { 420f, 1580f, 2715f };
        private readonly float[] keyY = { 132f, 101f, 106f };
        private readonly float[] checkpointX = { 68f, 548f, 1308f, 2103f, 2478f, 2883f };

        private GhostPlayerController player;
        private Camera gameCamera;
        private float cameraVelocityX;
        private float cameraVelocityY;
        private float noticeUntil;
        private float gateMessageCooldown;
        private float mapRefresh;
        private int goldCoins;
        private int selectedCreature = -1;
        private GhostEnemy gateBoss;
        private bool bossAwake;
        private bool started;
        private bool paused;
        private bool won;
        private bool mapOpen;
        private bool catalogOpen;
        private string notice = "";

        private Canvas canvas;
        private Text hudStats;
        private Text hpText;
        private Text potionText;
        private Text staminaText;
        private Text stressText;
        private Text keyText;
        private Text coinText;
        private Image healthFill;
        private Image staminaFill;
        private Image stressFill;
        private GameObject menuPanel;
        private GameObject pausePanel;
        private GameObject endPanel;
        private GameObject mapPanel;
        private GameObject catalogPanel;
        private GameObject mobileControls;
        private Text catalogDetail;
        private Button[] catalogButtons;
        private RawImage mapImage;
        private Texture2D mapTexture;
        private Text noticeText;
        private Image noticeBackground;
        private SpriteRenderer lockedGateSprite;
        private GameObject gateDoor;
        private GameObject bossHud;
        private Image bossHealthFill;
        private Text bossHealthText;

        public bool GameStarted => started;
        public bool IsPaused => paused;
        public bool IsCatalogOpen => catalogOpen;
        public bool HasWon => won;

        private void Start()
        {
            Application.targetFrameRate = 60;
            Physics2D.gravity = new Vector2(0f, -8.8f);
            BuildWorld();
            BuildPlayer();
            BuildCamera();
            BuildHud();
            ResetToMenu();
        }

        private void Update()
        {
            HandleGlobalInput();
            if (gameCamera == null || player == null)
                return;

            UpdateCamera();
            if (!started || paused || won || catalogOpen)
            {
                UpdateHud();
                return;
            }

            var dt = Time.deltaTime;
            UpdateEnemies(dt);
            UpdateShots(dt);
            UpdateCoins(dt);
            UpdateCollectibles();
            UpdateCheckpoints();
            EnforceGate();
            UpdateHud();
        }

        private void HandleGlobalInput()
        {
            if (Input.GetKeyDown(KeyCode.M))
                ToggleMap();
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (catalogOpen)
                    CloseCatalog();
                else if (started && !won)
                    TogglePause();
            }
        }

        private void BuildWorld()
        {
            for (var x = -213f; x < WorldEnd; x += 426f)
                AddPixelSprite("Trời chiều pixel", x, 0f, 426f, 216f, "sky_tile", -100);
            AddPixelTile("Dòng nước", -80f, 216f, 3600f, 100f, "river_tile", -70, false);

            for (var i = 0; i < 18; i++)
            {
                var x = 80f + i * 196f;
                AddRuinPillar(x, 214f, 0.58f + i % 3 * 0.08f, -40);
            }
            foreach (var x in new[] { 260f, 725f, 1160f, 1690f, 2225f, 2690f, 2915f })
                AddCrystal(x, 208f, 0.75f + x % 3 * 0.1f, -12);

            for (var i = 0; i < PlatformData.Length; i++)
            {
                var data = PlatformData[i];
                var x = data.x;
                var width = data.y - data.x;
                var surface = data.z;
                var ground =                 AddPixelTile("Nền đá cổ", x, surface, width, 86f, "soil_tile", 0, true);
                ground.layer = GroundLayer;
                AddPixelTile("Rêu phát sáng", x, surface, width, 9f, "grass_edge", 1, false);
                AddPixelRect("Vết khắc đá", x, surface + 5f, width, 2f, new Color(0.40f, 0.56f, 0.55f), 1);
                for (var px = (int)x + 22; px < data.y; px += 58)
                    AddPixelRect("Mảnh đá", px, surface + 19f + px % 4 * 7f, 8f + px % 3 * 3f, 3f, new Color(0.27f, 0.34f, 0.38f), 2);
            }

            for (var i = 0; i < LedgeData.Length; i++)
            {
                var ledge = LedgeData[i];
                var platform = AddPixelTile("Bậc đá nổi", ledge.x, ledge.z, ledge.y - ledge.x, 12f, "platform", 5, true);
                platform.layer = GroundLayer;
            }

            for (var x = 330f; x < WorldEnd; x += 410f)
            {
                AddPixelRect("Trụ đá đổ", x, 194f, 4f, 29f, new Color(0.29f, 0.38f, 0.42f), 3);
                AddPixelRect("Tinh thể", x - 5f, 187f, 13f, 5f, new Color(0.22f, 0.66f, 0.72f), 4);
            }

            AddStartSign();
            BuildBossArena();
            AddGateVisual();
            BuildEnemies();
            BuildKeys();
        }

        private void BuildPlayer()
        {
            var playerObject = new GameObject("Bóng đen · nhân vật");
            playerObject.layer = 9;
            playerObject.transform.position = GetPlayerSpawnPosition();
            var renderer = playerObject.AddComponent<SpriteRenderer>();
            renderer.sprite = LoadPixelSprite("orb");
            renderer.sortingOrder = 20;
            var rigidbody = playerObject.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 1f;
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            var collider = playerObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.17f, 0.2f);
            player = playerObject.AddComponent<GhostPlayerController>();
            player.Initialize(this);
            for (var i = 0; i < enemies.Count; i++)
                Physics2D.IgnoreCollision(collider, enemies[i].GetComponent<Collider2D>());
        }

        private void BuildCamera()
        {
            var cameraObject = new GameObject("Camera 2D · pixel art");
            cameraObject.tag = "MainCamera";
            gameCamera = cameraObject.AddComponent<Camera>();
            gameCamera.orthographic = true;
            gameCamera.orthographicSize = 1.2f;
            gameCamera.nearClipPlane = 0.1f;
            gameCamera.farClipPlane = 120f;
            gameCamera.backgroundColor = new Color(0.43f, 0.72f, 0.77f);
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<PixelArtDisplay>();
            cameraObject.transform.position = new Vector3(2.13f, 1.2f, -10f);

            var previewObject = new GameObject("Camera · cửa sổ Game");
            var previewCamera = previewObject.AddComponent<Camera>();
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = gameCamera.backgroundColor;
            previewCamera.cullingMask = 0;
            previewCamera.depth = -20f;
        }

        private void BuildHud()
        {
            var root = new GameObject("UI · HUD GHOST", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem),
                    typeof(UnityEngine.EventSystems.StandaloneInputModule));

            var screen = new GameObject("Màn hình pixel", typeof(RawImage), typeof(AspectRatioFitter));
            screen.transform.SetParent(root.transform, false);
            screen.transform.SetAsFirstSibling();
            var rawImage = screen.GetComponent<RawImage>();
            rawImage.raycastTarget = false;
            var screenRect = rawImage.rectTransform;
            screenRect.anchorMin = Vector2.zero;
            screenRect.anchorMax = Vector2.one;
            screenRect.offsetMin = Vector2.zero;
            screenRect.offsetMax = Vector2.zero;
            var aspect = screen.GetComponent<AspectRatioFitter>();
            aspect.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            aspect.aspectRatio = 16f / 9f;
            gameCamera.GetComponent<PixelArtDisplay>().Attach(rawImage);

            var top = CreatePanel("Bảng trạng thái", root.transform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(374f, 178f), new Vector2(18f, -18f), new Color(0.04f, 0.10f, 0.13f, 0.94f));
            CreateIcon("Chân dung quả cầu", "orb", top.transform, new Vector2(0f, 1f),
                new Vector2(30f, 30f), new Vector2(22f, -20f));
            CreateText("Tiêu đề", "GHOST · TÀN TÍCH BÓNG ĐÊM", top.transform,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(-70f, 34f), new Vector2(34f, -20f),
                21, new Color(1f, 0.88f, 0.59f), TextAnchor.MiddleLeft);
            CreateIcon("Biểu tượng máu", "heart", top.transform, new Vector2(0f, 1f),
                new Vector2(20f, 20f), new Vector2(22f, -57f));
            hpText = CreateText("Máu", "", top.transform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(-58f, 25f), new Vector2(34f, -57f), 16, Color.white, TextAnchor.MiddleLeft);
            healthFill = CreateBar("Thanh máu", top.transform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(-44f, 8f), new Vector2(10f, -82f), new Color(0.92f, 0.35f, 0.30f));
            CreateIcon("Biểu tượng căng thẳng", "stress_icon", top.transform, new Vector2(0f, 1f),
                new Vector2(16f, 16f), new Vector2(20f, -103f));
            stressText = CreateText("Căng thẳng", "", top.transform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(-54f, 21f), new Vector2(26f, -103f), 12, new Color(0.96f, 0.78f, 0.59f), TextAnchor.MiddleLeft);
            stressFill = CreateBar("Thanh căng thẳng", top.transform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(-44f, 6f), new Vector2(10f, -123f), new Color(0.78f, 0.34f, 0.39f));
            CreateIcon("Biểu tượng thể lực", "stamina_icon", top.transform, new Vector2(0f, 1f),
                new Vector2(16f, 16f), new Vector2(20f, -146f));
            staminaText = CreateText("Thể lực", "", top.transform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(-54f, 20f), new Vector2(26f, -146f), 12, new Color(0.68f, 0.92f, 0.79f), TextAnchor.MiddleLeft);
            staminaFill = CreateBar("Thanh thể lực", top.transform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(-44f, 5f), new Vector2(10f, -165f), new Color(0.34f, 0.80f, 0.63f));

            var status = CreatePanel("Vật phẩm", root.transform, new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(300f, 106f), new Vector2(-18f, -18f), new Color(0.04f, 0.10f, 0.13f, 0.94f));
            CreateText("Vật phẩm tiêu đề", "HÀNH TRÌNH", status.transform, new Vector2(0f, 1f),
                new Vector2(1f, 1f), new Vector2(-24f, 22f), new Vector2(0f, -17f),
                13, new Color(0.72f, 0.87f, 0.78f), TextAnchor.MiddleLeft);
            CreateIcon("Biểu tượng chìa khóa", "key", status.transform, new Vector2(0f, 1f),
                new Vector2(16f, 22f), new Vector2(22f, -49f));
            keyText = CreateText("Chìa khóa", "", status.transform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(-56f, 24f), new Vector2(34f, -49f), 16, new Color(1f, 0.88f, 0.59f), TextAnchor.MiddleLeft);
            CreateIcon("Biểu tượng đồng", "coin", status.transform, new Vector2(0f, 1f),
                new Vector2(18f, 18f), new Vector2(22f, -79f));
            coinText = CreateText("Vàng", "", status.transform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(-56f, 24f), new Vector2(34f, -79f), 14, Color.white, TextAnchor.MiddleLeft);

            hudStats = CreateText("Khu vực", "KHU VỰC · TÀN TÍCH NGOÀI", root.transform, new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(350f, 32f), new Vector2(0f, 24f), 14,
                new Color(0.96f, 0.93f, 0.81f), TextAnchor.MiddleCenter);

            noticeBackground = CreatePanel("Thông báo", root.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(760f, 52f), new Vector2(0f, -92f),
                new Color(0.04f, 0.10f, 0.13f, 0.96f), false).GetComponent<Image>();
            noticeText = CreateText("Thông báo chữ", "", root.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(720f, 38f), new Vector2(0f, -92f), 17, Color.white, TextAnchor.MiddleCenter);

            bossHud = CreatePanel("Thanh máu boss", root.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(420f, 58f), new Vector2(0f, -18f), new Color(0.04f, 0.08f, 0.12f, 0.95f));
            bossHealthText = CreateText("Tên boss", "KẺ CANH CỔNG", bossHud.transform,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(-22f, 24f),
                new Vector2(0f, -7f), 16, new Color(1f, 0.79f, 0.55f), TextAnchor.MiddleCenter);
            bossHealthFill = CreateBar("Máu boss", bossHud.transform, new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(-30f, 10f), new Vector2(0f, 12f), new Color(0.77f, 0.20f, 0.29f));
            bossHud.SetActive(false);

            var mapButton = CreateButton("Bản đồ · M", "BẢN ĐỒ", root.transform, new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(150f, 38f), new Vector2(-93f, -132f), ToggleMap);
            AddButtonIcon(mapButton, "map_icon");
            var catalogButton = CreateButton("Mục lục", "MỤC LỤC", root.transform, new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(150f, 38f), new Vector2(-93f, -178f), OpenCatalog);
            AddButtonIcon(catalogButton, "book_icon");
            var potionButton = CreateButton("Bình máu", "HỒI MÁU · H", root.transform,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(190f, 42f),
                new Vector2(116f, 28f), () => player.UsePotion());
            AddButtonIcon(potionButton, "potion");
            potionText = potionButton.GetComponentInChildren<Text>();

            BuildMapPanel(root.transform);
            BuildCatalog(root.transform);
            if (Application.isMobilePlatform)
                BuildMobileControls(root.transform);
            BuildMenuScreens(root.transform);
            UpdateHud();
        }

        private void BuildMobileControls(Transform parent)
        {
            mobileControls = new GameObject("Điều khiển cảm ứng", typeof(RectTransform));
            mobileControls.transform.SetParent(parent, false);
            var rect = mobileControls.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            AddMobileButton(mobileControls.transform, "←", GhostMobileAction.Left, new Vector2(74f, 78f));
            AddMobileButton(mobileControls.transform, "→", GhostMobileAction.Right, new Vector2(166f, 78f));
            AddMobileButton(mobileControls.transform, "NHẢY", GhostMobileAction.Jump, new Vector2(-285f, 78f));
            AddMobileButton(mobileControls.transform, "LƯỚT", GhostMobileAction.Dash, new Vector2(-190f, 78f));
            AddMobileButton(mobileControls.transform, "ĐÁNH", GhostMobileAction.Attack, new Vector2(-95f, 78f));
        }

        private void AddMobileButton(Transform parent, string label, GhostMobileAction action, Vector2 position)
        {
            var button = CreateButton("Điều khiển · " + label, label, parent,
                new Vector2(position.x < 0f ? 1f : 0f, 0f), new Vector2(position.x < 0f ? 1f : 0f, 0f),
                new Vector2(82f, 64f), position, () => { });
            button.gameObject.AddComponent<GhostMobileButton>().Initialize(player, action);
        }

        private void BuildMapPanel(Transform parent)
        {
            mapPanel = CreatePanel("Bản đồ", parent, Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero, new Color(0.02f, 0.06f, 0.08f, 0.78f));
            var card = CreatePanel("Thẻ bản đồ", mapPanel.transform, new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(900f, 380f), Vector2.zero,
                new Color(0.04f, 0.12f, 0.15f, 0.98f));
            CreateText("Bản đồ tiêu đề", "BẢN ĐỒ HÀNH TRÌNH", card.transform,
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(-44f, 38f), new Vector2(18f, -24f), 26,
                new Color(1f, 0.88f, 0.59f), TextAnchor.MiddleLeft);
            CreateText("Chú giải bản đồ", "GHOST  ·  CHÌA KHÓA  ·  LINH VẬT",
                card.transform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(-44f, 24f), new Vector2(18f, -62f), 14,
                new Color(0.78f, 0.91f, 0.82f), TextAnchor.MiddleLeft);
            var imageObject = new GameObject("Sơ đồ màn chơi", typeof(RawImage));
            imageObject.transform.SetParent(card.transform, false);
            mapImage = imageObject.GetComponent<RawImage>();
            mapImage.raycastTarget = false;
            var rect = mapImage.rectTransform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = new Vector2(22f, 58f);
            rect.offsetMax = new Vector2(-22f, -102f);
            mapTexture = new Texture2D(256, 64, TextureFormat.RGBA32, false)
            {
                name = "Bản đồ pixel",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            mapImage.texture = mapTexture;
            CreateButton("Đóng bản đồ", "ĐÓNG · M", card.transform, new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(184f, 44f), new Vector2(0f, 27f), ToggleMap);
            mapPanel.SetActive(false);
        }

        private void BuildCatalog(Transform parent)
        {
            catalogPanel = CreatePanel("Mục lục linh vật", parent, Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero, new Color(0.02f, 0.05f, 0.07f, 0.94f));
            var card = CreatePanel("Thẻ mục lục", catalogPanel.transform, new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(1000f, 560f), Vector2.zero,
                new Color(0.04f, 0.12f, 0.15f, 0.99f));
            CreateText("Mục lục tiêu đề", "SỔ TAY LINH VẬT", card.transform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(760f, 54f),
                new Vector2(0f, -42f), 30, new Color(1f, 0.88f, 0.59f), TextAnchor.MiddleCenter);
            CreateText("Mục lục hướng dẫn", "Khám phá và quan sát để mở khóa thông tin",
                card.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(720f, 30f), new Vector2(0f, -83f), 15,
                new Color(0.78f, 0.91f, 0.82f), TextAnchor.MiddleCenter);
            catalogDetail = CreateText("Chi tiết linh vật", "Gặp một linh vật để ghi nhận thông tin về nó.",
                card.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(390f, 300f), new Vector2(-34f, -16f), 19,
                Color.white, TextAnchor.MiddleLeft);
            catalogButtons = new Button[3];
            for (var i = 0; i < catalogButtons.Length; i++)
            {
                var index = i;
                catalogButtons[i] = CreateButton("Linh vật " + i, "CHƯA GHI NHẬN",
                    card.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                    new Vector2(410f, 92f), new Vector2(42f, 92f - i * 112f),
                    () => SelectCreature(index));
                AddButtonIcon(catalogButtons[i], i == 0 ? "mud_spirit" : i == 1 ? "crab" : "firefly");
                catalogButtons[i].interactable = false;
            }
            CreateButton("Đóng mục lục", "ĐÓNG · ESC", card.transform,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(200f, 48f),
                new Vector2(0f, 29f), CloseCatalog);
            catalogPanel.SetActive(false);
        }

        private void BuildMenuScreens(Transform parent)
        {
            menuPanel = CreatePanel("Màn hình bắt đầu", parent, Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero, new Color(0.02f, 0.05f, 0.08f, 0.78f));
            var menuCard = CreatePanel("Thẻ bắt đầu", menuPanel.transform, new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(940f, 560f), Vector2.zero,
                new Color(0.04f, 0.13f, 0.16f, 0.98f));
            CreateText("Mở đầu", "CUỘC VƯỢT NGỤC TÀN TÍCH",
                menuCard.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(720f, 30f), new Vector2(0f, -34f), 15,
                new Color(0.75f, 0.91f, 0.82f), TextAnchor.MiddleCenter);
            CreateIcon("Ảnh quả cầu trang mở đầu", "orb", menuCard.transform,
                new Vector2(0.5f, 0.5f), new Vector2(80f, 80f), new Vector2(-270f, 132f));
            CreateText("Tên game", "GHOST", menuCard.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(520f, 78f),
                new Vector2(58f, 148f), 62, new Color(1f, 0.88f, 0.59f), TextAnchor.MiddleLeft);
            CreateText("Tên màn", "MÀN 1  ·  TÀN TÍCH BỊ LÃNG QUÊN", menuCard.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(520f, 34f),
                new Vector2(64f, 104f), 18, new Color(0.73f, 0.89f, 0.79f), TextAnchor.MiddleLeft);

            var objectiveCard = CreatePanel("Mục tiêu màn", menuCard.transform, new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(720f, 86f), new Vector2(0f, 27f),
                new Color(0.07f, 0.19f, 0.20f, 0.96f));
            CreateIcon("Chìa khóa mục tiêu", "key", objectiveCard.transform,
                new Vector2(0f, 0.5f), new Vector2(28f, 42f), new Vector2(36f, 0f));
            CreateText("Mục tiêu", "THU THẬP 3 CHÌA KHÓA\nĐánh bại boss canh cổng để thoát.",
                objectiveCard.transform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-86f, 64f), new Vector2(63f, 0f), 18,
                Color.white, TextAnchor.MiddleLeft);
            CreateText("Điều khiển", "A / D  hoặc  ← / →     DI CHUYỂN     ·     SPACE  NHẢY     ·     SHIFT  LƯỚT",
                menuCard.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(780f, 34f), new Vector2(0f, -60f), 15,
                new Color(0.80f, 0.90f, 0.83f), TextAnchor.MiddleCenter);
            CreateText("Điều khiển chiến đấu", "J / X  ĐÁNH     ·     H  HỒI MÁU     ·     M  BẢN ĐỒ     ·     ESC  TẠM DỪNG",
                menuCard.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(780f, 30f), new Vector2(0f, -96f), 14,
                new Color(0.80f, 0.90f, 0.83f), TextAnchor.MiddleCenter);
            var startButton = CreateButton("Bắt đầu", "BẮT ĐẦU HÀNH TRÌNH",
                menuCard.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(320f, 64f), new Vector2(0f, -178f), BeginGame);
            AddButtonIcon(startButton, "map_icon");
            CreateText("Gợi ý bắt đầu", "Nhấn nút hoặc Enter để lên đường",
                menuCard.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(400f, 24f), new Vector2(0f, 35f), 13,
                new Color(0.70f, 0.82f, 0.76f), TextAnchor.MiddleCenter);

            pausePanel = CreatePanel("Tạm dừng", parent, Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero, new Color(0.02f, 0.05f, 0.08f, 0.78f));
            var pauseCard = CreatePanel("Thẻ tạm dừng", pausePanel.transform, new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(620f, 410f), Vector2.zero,
                new Color(0.04f, 0.13f, 0.16f, 0.99f));
            CreateText("Tạm dừng tiêu đề", "TẠM NGHỈ CHÂN", pauseCard.transform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(540f, 64f),
                new Vector2(0f, -76f), 40, new Color(1f, 0.88f, 0.59f), TextAnchor.MiddleCenter);
            CreateText("Tạm dừng mô tả", "Kẻ canh cổng đang chờ phía trước.",
                pauseCard.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(500f, 32f), new Vector2(0f, -122f), 17,
                new Color(0.78f, 0.91f, 0.82f), TextAnchor.MiddleCenter);
            CreateButton("Tiếp tục", "TIẾP TỤC HÀNH TRÌNH", pauseCard.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(300f, 58f),
                new Vector2(0f, 20f), TogglePause);
            CreateButton("Chơi lại", "CHƠI LẠI TỪ ĐẦU", pauseCard.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(300f, 58f),
                new Vector2(0f, -58f), RestartGame);
            CreateText("Thoát tạm dừng", "ESC  ·  TIẾP TỤC",
                pauseCard.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(320f, 24f), new Vector2(0f, 34f), 13,
                new Color(0.70f, 0.82f, 0.76f), TextAnchor.MiddleCenter);
            pausePanel.SetActive(false);

            endPanel = CreatePanel("Kết thúc màn", parent, Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero, new Color(0.02f, 0.05f, 0.08f, 0.82f));
            var endCard = CreatePanel("Thẻ hoàn thành", endPanel.transform, new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(700f, 430f), Vector2.zero,
                new Color(0.04f, 0.13f, 0.16f, 0.99f));
            CreateIcon("Huy hiệu chìa khóa", "key", endCard.transform,
                new Vector2(0.5f, 0.5f), new Vector2(48f, 68f), new Vector2(0f, 130f));
            CreateText("Kết thúc tiêu đề", "MỞ ĐƯỢC CỔNG RỒI!", endCard.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(620f, 72f),
                new Vector2(0f, 56f), 40, new Color(1f, 0.88f, 0.59f), TextAnchor.MiddleCenter);
            CreateText("Kết thúc mô tả", "Kẻ canh cổng đã gục. Lối thoát đã mở.",
                endCard.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(600f, 48f), new Vector2(0f, 3f), 19, Color.white, TextAnchor.MiddleCenter);
            CreateButton("Thử lại", "CHƠI LẠI MÀN 1", endCard.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(280f, 58f), new Vector2(0f, -78f), RestartGame);
            endPanel.SetActive(false);
        }

        private void BuildEnemies()
        {
            AddEnemy(590f, 160f, 550f, 615f, 2, 26f, EnemyKind.Mud, 0f);
            AddEnemy(825f, 122f, 800f, 866f, 2, 32f, EnemyKind.Glow, 1.7f);
            AddEnemy(965f, 119f, 940f, 995f, 2, 31f, EnemyKind.Crab, 2.8f);
            AddEnemy(1060f, 143f, 1040f, 1088f, 3, 28f, EnemyKind.Glow, 4.1f);
            AddEnemy(1420f, 159f, 1395f, 1458f, 2, 29f, EnemyKind.Mud, 0.8f);
            AddEnemy(1610f, 99f, 1590f, 1645f, 3, 34f, EnemyKind.Crab, 2.2f);
            AddEnemy(1920f, 190f, 1890f, 1950f, 2, 31f, EnemyKind.Mud, 3.4f);
            AddEnemy(2165f, 98f, 2145f, 2208f, 3, 35f, EnemyKind.Glow, 5.2f);
            AddEnemy(2610f, 130f, 2585f, 2650f, 2, 28f, EnemyKind.Crab, 1.1f);
            AddEnemy(2785f, 157f, 2765f, 2820f, 3, 30f, EnemyKind.Mud, 4.7f);
            AddEnemy(3120f, 120f, 2945f, 3270f, 18, 82f, EnemyKind.Boss, 0f);
        }

        private void BuildKeys()
        {
            for (var i = 0; i < keyX.Length; i++)
            {
                keyObjects[i] = AddPixelSprite("Chìa khóa cổ " + (i + 1),
                    keyX[i] - 8f, keyY[i] - 12f, 16f, 24f, "key", 13);
            }
        }

        private void AddEnemy(float x, float topY, float left, float right, int hp, float speed, EnemyKind kind, float phase)
        {
            var enemyObject = new GameObject(kind == EnemyKind.Boss ? "Boss · Kẻ canh cổng" :
                kind == EnemyKind.Glow ? "Đom đóm ma" : kind == EnemyKind.Crab ? "Bọ giáp" : "Bóng bùn");
            enemyObject.layer = 9;
            var width = kind == EnemyKind.Boss ? 64f : 20f;
            var height = kind == EnemyKind.Boss ? 64f : 20f;
            enemyObject.transform.position = PixelPosition(x, topY + height * 0.5f);
            var sprite = enemyObject.AddComponent<SpriteRenderer>();
            sprite.sprite = LoadPixelSprite(kind == EnemyKind.Boss ? "boss" : kind == EnemyKind.Glow ? "firefly" :
                kind == EnemyKind.Crab ? "crab" : "mud_spirit");
            sprite.transform.localScale = new Vector3(width / sprite.sprite.rect.width, height / sprite.sprite.rect.height, 1f);
            sprite.color = Color.white;
            sprite.sortingOrder = 18;
            var collider = enemyObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(width / PixelsPerUnit, height / PixelsPerUnit);
            var body = enemyObject.AddComponent<Rigidbody2D>();
            body.gravityScale = kind == EnemyKind.Glow ? 0f : 1f;
            body.bodyType = kind == EnemyKind.Glow ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            var enemy = enemyObject.AddComponent<GhostEnemy>();
            enemy.Initialize(this, kind, hp, speed, left, right, phase);
            enemies.Add(enemy);
            if (kind == EnemyKind.Boss)
            {
                gateBoss = enemy;
                enemyObject.SetActive(false);
            }
        }

        private void UpdateEnemies(float dt)
        {
            var playerX = player.transform.position.x * PixelsPerUnit;
            var playerY = 240f - player.transform.position.y * PixelsPerUnit;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || !enemy.gameObject.activeInHierarchy)
                    continue;
                if (!enemy.Alive)
                {
                    enemy.TickDeath(dt);
                    continue;
                }

                enemy.TickTimers(dt);
                var enemyX = enemy.transform.position.x * PixelsPerUnit;
                var enemyCenterY = 240f - enemy.transform.position.y * PixelsPerUnit;
                var dx = playerX - enemyX;
                var dy = playerY - enemyCenterY;
                var inRange = Mathf.Abs(dx) < 225f && Mathf.Abs(dy) < 95f;

                if (enemy.Kind != EnemyKind.Boss && !discoveredCreatures[(int)enemy.Kind] &&
                    Mathf.Abs(dx) < 112f && Mathf.Abs(dy) < 76f)
                    DiscoverCreature((int)enemy.Kind);

                if (enemy.Kind == EnemyKind.Boss)
                {
                    UpdateBoss(enemy, dt, dx, dy, inRange);
                }
                else if (enemy.Kind == EnemyKind.Glow)
                {
                    UpdateGlow(enemy, dt, dx, dy, inRange);
                }
                else
                {
                    UpdateGroundEnemy(enemy, dt, dx, dy, inRange);
                }

                var x = enemy.transform.position.x * PixelsPerUnit;
                var topY = 240f - enemy.transform.position.y * PixelsPerUnit - enemy.BodyHeight * 0.5f;
                if (enemy.Kind != EnemyKind.Glow && topY > 300f)
                    enemy.ResetPosition();

                CheckEnemyPlayerContact(enemy, x, topY, playerX, playerY);
            }
        }

        private void UpdateGlow(GhostEnemy enemy, float dt, float dx, float dy, bool inRange)
        {
            if (enemy.State == EnemyState.Windup)
            {
                if (enemy.StateTimer <= 0f)
                {
                    var origin = new Vector2(enemy.transform.position.x * PixelsPerUnit,
                        240f - enemy.transform.position.y * PixelsPerUnit);
                    var target = new Vector2(player.transform.position.x * PixelsPerUnit,
                        240f - player.transform.position.y * PixelsPerUnit);
                    var direction = (target - origin).normalized;
                    AddShot(origin, direction * 106f);
                    enemy.SetState(EnemyState.Recover, 1.65f);
                }
            }
            else if (enemy.State == EnemyState.Recover)
            {
                if (enemy.StateTimer <= 0f)
                    enemy.SetState(EnemyState.Patrol, 0f);
            }
            else
            {
                var targetX = enemy.OriginX + Mathf.Sin(Time.time * 0.62f + enemy.Phase) * 11f;
                var targetY = enemy.BaseTopY + Mathf.Sin(Time.time * 2.4f + enemy.Phase) * 6f;
                var current = enemy.transform.position;
                var currentX = current.x * PixelsPerUnit;
                var currentTopY = 240f - current.y * PixelsPerUnit - 10f;
                var nextX = Mathf.Lerp(currentX, targetX, Mathf.Min(1f, dt * 1.1f));
                var nextTopY = Mathf.Lerp(currentTopY, targetY, Mathf.Min(1f, dt * 6f));
                enemy.transform.position = PixelPosition(nextX, nextTopY + 10f);
                enemy.SetFacing(dx >= 0f ? 1f : -1f);
                if (inRange)
                    enemy.SetState(EnemyState.Windup, 0.55f);
            }
        }

        private void UpdateGroundEnemy(GhostEnemy enemy, float dt, float dx, float dy, bool inRange)
        {
            var body = enemy.Body;
            var isCrab = enemy.Kind == EnemyKind.Crab;
            if (enemy.State == EnemyState.Windup)
            {
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                if (enemy.StateTimer <= 0f)
                {
                    var direction = dx >= 0f ? 1f : -1f;
                    if (isCrab)
                    {
                        enemy.SetState(EnemyState.Pounce, 0.6f);
                        body.linearVelocity = new Vector2(direction * 1.18f, 2.15f);
                    }
                    else
                    {
                        enemy.SetState(EnemyState.Dash, 0.34f);
                        enemy.SetFacing(direction);
                        body.linearVelocity = new Vector2(direction * 1.72f, body.linearVelocity.y);
                    }
                }
            }
            else if (enemy.State == EnemyState.Dash)
            {
                body.linearVelocity = new Vector2(enemy.Facing * 1.72f, body.linearVelocity.y);
                var x = enemy.transform.position.x * PixelsPerUnit;
                if (enemy.StateTimer <= 0f || x < enemy.Left || x > enemy.Right)
                {
                    body.position = new Vector2(Mathf.Clamp(x, enemy.Left, enemy.Right) / PixelsPerUnit, body.position.y);
                    body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                    enemy.SetState(EnemyState.Recover, 0.66f);
                }
            }
            else if (enemy.State == EnemyState.Pounce)
            {
                var clamped = Mathf.Clamp(enemy.transform.position.x * PixelsPerUnit, enemy.Left, enemy.Right);
                var velocity = body.linearVelocity;
                body.position = new Vector2(clamped / PixelsPerUnit, body.position.y);
                if (enemy.StateTimer <= 0f || (enemy.IsGrounded() && velocity.y <= 0f))
                    enemy.SetState(EnemyState.Recover, 0.63f);
            }
            else if (enemy.State == EnemyState.Recover)
            {
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                if (enemy.StateTimer <= 0f)
                    enemy.SetState(EnemyState.Patrol, 0f);
            }
            else
            {
                var x = enemy.transform.position.x * PixelsPerUnit;
                var direction = enemy.Direction;
                if (x < enemy.Left || x > enemy.Right)
                {
                    direction *= -1f;
                    x = Mathf.Clamp(x, enemy.Left, enemy.Right);
                    body.position = new Vector2(x / PixelsPerUnit, body.position.y);
                    enemy.Direction = direction;
                }
                enemy.SetFacing(isCrab ? (dx >= 0f ? 1f : -1f) : direction);
                body.linearVelocity = new Vector2(enemy.Speed * direction / PixelsPerUnit, body.linearVelocity.y);
                if (inRange && Mathf.Abs(dy) < (isCrab ? 38f : 36f))
                    enemy.SetState(EnemyState.Windup, isCrab ? 0.34f : 0.48f);
            }

            if (enemy.Kind == EnemyKind.Crab && enemy.State == EnemyState.Pounce)
            {
                var clampedX = Mathf.Clamp(body.position.x * PixelsPerUnit, enemy.Left, enemy.Right);
                body.position = new Vector2(clampedX / PixelsPerUnit, body.position.y);
            }
        }

        private void UpdateBoss(GhostEnemy boss, float dt, float dx, float dy, bool inRange)
        {
            var body = boss.Body;
            var direction = dx >= 0f ? 1f : -1f;
            var x = body.position.x * PixelsPerUnit;
            var clampedX = Mathf.Clamp(x, boss.Left, boss.Right);
            body.position = new Vector2(clampedX / PixelsPerUnit, body.position.y);
            boss.SetFacing(direction);

            if (boss.State == EnemyState.Windup)
            {
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                if (boss.StateTimer <= 0f)
                {
                    if (boss.NextBossAttack++ % 2 == 0)
                    {
                        boss.SetState(EnemyState.Dash, 0.46f);
                        body.linearVelocity = new Vector2(boss.Facing * 2.65f, body.linearVelocity.y);
                    }
                    else
                    {
                        boss.SetState(EnemyState.Pounce, 1.25f);
                        body.linearVelocity = new Vector2(boss.Facing * 1.45f, 2.7f);
                    }
                }
                return;
            }

            if (boss.State == EnemyState.Dash)
            {
                body.linearVelocity = new Vector2(boss.Facing * 2.65f, body.linearVelocity.y);
                x = body.position.x * PixelsPerUnit;
                if (boss.StateTimer <= 0f || x <= boss.Left || x >= boss.Right)
                {
                    body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                    boss.SetState(EnemyState.Recover, 0.78f);
                }
                return;
            }

            if (boss.State == EnemyState.Pounce)
            {
                body.linearVelocity = new Vector2(boss.Facing * 1.45f, body.linearVelocity.y);
                if (boss.IsGrounded() && body.linearVelocity.y <= 0f)
                {
                    body.linearVelocity = Vector2.zero;
                    boss.SetState(EnemyState.Recover, 0.9f);
                    var playerY = 240f - body.position.y * PixelsPerUnit + 12f;
                    AddBossShot(new Vector2(clampedX - 30f, playerY), new Vector2(-155f, 0f));
                    AddBossShot(new Vector2(clampedX + 30f, playerY), new Vector2(155f, 0f));
                }
                return;
            }

            if (boss.State == EnemyState.Recover)
            {
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                if (boss.StateTimer <= 0f)
                    boss.SetState(EnemyState.Patrol, 0f);
                return;
            }

            if (Mathf.Abs(dx) > 66f)
                body.linearVelocity = new Vector2(direction * 0.55f, body.linearVelocity.y);
            else
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);

            if (inRange && Mathf.Abs(dy) < 92f)
                boss.SetState(EnemyState.Windup, 0.82f);
        }

        private void CheckEnemyPlayerContact(GhostEnemy enemy, float enemyX, float enemyTopY, float playerX, float playerCenterY)
        {
            if (enemy == null || !enemy.Alive)
                return;

            var playerTop = playerCenterY - 10f;
            var playerBottom = playerCenterY + 10f;
            var enemyHalfWidth = enemy.BodyWidth * 0.5f;
            var enemyHeight = enemy.BodyHeight;
            var playerVelocity = player.Velocity;
            if (playerVelocity.y < -0.5f && playerBottom >= enemyTopY && playerTop < enemyTopY + 7f &&
                Mathf.Abs(playerX - enemyX) < enemyHalfWidth + 4f)
            {
                enemy.TakeDamage(1, player.transform.position.x * PixelsPerUnit);
                var body = player.GetComponent<Rigidbody2D>();
                body.linearVelocity = new Vector2(body.linearVelocity.x, 2.1f);
                return;
            }

            if (!player.IsInvulnerable && Mathf.Abs(playerX - enemyX) < enemyHalfWidth + 10f &&
                playerBottom > enemyTopY + 3f && playerTop < enemyTopY + enemyHeight - 3f)
                player.TakeDamage(enemy.transform.position);
        }

        private void UpdateShots(float dt)
        {
            for (var i = shots.Count - 1; i >= 0; i--)
            {
                var shot = shots[i];
                shot.Life -= dt;
                shot.Position += shot.Velocity * dt;
                shot.Visual.transform.position = PixelPosition(shot.Position.x, shot.Position.y);
                if (shot.Life <= 0f || Mathf.Abs(shot.Position.x - player.transform.position.x * PixelsPerUnit) > 500f)
                {
                    Destroy(shot.Visual);
                    shots.RemoveAt(i);
                    continue;
                }

                var playerX = player.transform.position.x * PixelsPerUnit;
                var playerY = 240f - player.transform.position.y * PixelsPerUnit;
                if (!player.IsInvulnerable && Mathf.Abs(playerX - shot.Position.x) < 12f &&
                    Mathf.Abs(playerY - shot.Position.y) < 14f)
                {
                    Destroy(shot.Visual);
                    shots.RemoveAt(i);
                    player.TakeDamage(PixelPosition(shot.Position.x, shot.Position.y));
                }
            }
        }

        private void UpdateCoins(float dt)
        {
            for (var i = coins.Count - 1; i >= 0; i--)
            {
                var coin = coins[i];
                if (!coin.Settled)
                {
                    coin.VelocityY = Mathf.Min(320f, coin.VelocityY + 400f * dt);
                    coin.X += coin.VelocityX * dt;
                    coin.Y += coin.VelocityY * dt;
                    var surface = TerrainHeightAt(coin.X);
                    if (!float.IsPositiveInfinity(surface) && coin.Y + 4f >= surface)
                    {
                        coin.Y = surface - 4f;
                        coin.VelocityY = 0f;
                        coin.VelocityX *= 0.45f;
                        coin.Settled = true;
                    }
                }
                else
                {
                    coin.X += coin.VelocityX * dt;
                    coin.VelocityX *= Mathf.Exp(-dt * 8f);
                }

                coin.Visual.transform.position = PixelPosition(coin.X, coin.Y);
                var playerX = player.transform.position.x * PixelsPerUnit;
                var playerY = 240f - player.transform.position.y * PixelsPerUnit;
                if (Mathf.Abs(playerX - coin.X) < 15f && Mathf.Abs(playerY - coin.Y) < 22f)
                {
                    goldCoins += coin.Value;
                    Destroy(coin.Visual);
                    coins.RemoveAt(i);
                    Say("Nhặt được " + coin.Value + " đồng vàng!", 1.2f);
                }
            }
        }

        private void UpdateCollectibles()
        {
            var playerX = player.transform.position.x * PixelsPerUnit;
            var playerY = 240f - player.transform.position.y * PixelsPerUnit;
            var found = 0;
            for (var i = 0; i < keyX.Length; i++)
            {
                if (collectedKeys[i])
                    continue;
                if (keyObjects[i] != null)
                    keyObjects[i].transform.position = PixelPosition(keyX[i], keyY[i] + Mathf.Sin(Time.time * 3.5f + i) * 2f);
                var dx = playerX - keyX[i];
                var dy = playerY - keyY[i];
                if (Mathf.Abs(dx) < 15f && Mathf.Abs(dy) < 20f)
                {
                    collectedKeys[i] = true;
                    if (keyObjects[i] != null)
                        keyObjects[i].SetActive(false);
                    found++;
                }
            }

            if (found > 0)
            {
                if (CollectedKeyCount == keyX.Length)
                    AwakenBoss();
                UpdateGateVisual();
                Say(CollectedKeyCount == keyX.Length
                    ? "Đủ chìa khóa! Kẻ canh cổng đã thức tỉnh."
                    : "Đã tìm thấy chìa khóa cổ! (" + CollectedKeyCount + "/" + keyX.Length + ")",
                    CollectedKeyCount == keyX.Length ? 2.8f : 1.8f);
            }

            if (CanExit && playerX >= ExitX + 8f)
                Finish();
        }

        private void UpdateCheckpoints()
        {
            var playerX = player.transform.position.x * PixelsPerUnit;
            for (var i = 1; i < checkpointX.Length; i++)
            {
                if (playerX > checkpointX[i] + 16f && player.CheckpointX < checkpointX[i] / PixelsPerUnit)
                {
                    player.SetCheckpoint((checkpointX[i] + 8f) / PixelsPerUnit);
                    Say("Đã lưu điểm kiểm soát.", 1f);
                }
            }
        }

        private void EnforceGate()
        {
            if (CanExit)
                return;

            var body = player.GetComponent<Rigidbody2D>();
            var playerX = body.position.x * PixelsPerUnit;
            if (playerX + 8.5f > ExitX - 10f)
            {
                body.position = new Vector2((ExitX - 18.5f) / PixelsPerUnit, body.position.y);
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                if (gateMessageCooldown <= 0f)
                {
                    Say(CollectedKeyCount < keyX.Length
                        ? "Cổng khóa — hãy tìm đủ 3 chìa khóa."
                        : "Hạ kẻ canh cổng để mở lối thoát.", 1.8f);
                    gateMessageCooldown = 2.2f;
                }
            }
            gateMessageCooldown = Mathf.Max(0f, gateMessageCooldown - Time.deltaTime);
        }

        public void ApplyAttack(GhostPlayerController attacker, int combo)
        {
            var reach = combo == 3 ? 64f : combo == 2 ? 54f : 46f;
            var damage = combo == 3 ? 2 : 1;
            var playerX = attacker.transform.position.x * PixelsPerUnit;
            var playerY = 240f - attacker.transform.position.y * PixelsPerUnit;
            var hits = 0;
            var defeated = 0;

            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || !enemy.Alive || !enemy.gameObject.activeInHierarchy)
                    continue;
                var enemyX = enemy.transform.position.x * PixelsPerUnit;
                var enemyY = 240f - enemy.transform.position.y * PixelsPerUnit;
                var dx = enemyX - playerX;
                var dy = enemyY - playerY;
                var targetReach = reach + (enemy.Kind == EnemyKind.Boss ? enemy.BodyWidth * 0.5f : 0f);
                var verticalReach = enemy.Kind == EnemyKind.Boss ? 78f : 42f;
                if (dx * attacker.Facing < -4f || dx * attacker.Facing > targetReach || Mathf.Abs(dy) > verticalReach)
                    continue;

                hits++;
                if (enemy.TakeDamage(damage, playerX))
                    defeated++;
            }

            if (defeated > 0)
                Say(defeated > 1 ? "Combo hạ gục nhiều kẻ địch!" : "Kẻ địch đã bị hạ!", 1.3f);
            else if (hits > 0)
                Say(combo == 3 ? "Đòn kết liễu trúng đích!" : "Đánh trúng linh vật!", 0.8f);
            RefreshHud();
        }

        public void ShowAttack(GhostPlayerController attacker, int combo)
        {
            var playerX = attacker.transform.position.x * PixelsPerUnit;
            var playerY = 240f - attacker.transform.position.y * PixelsPerUnit;
            var reach = combo == 3 ? 36f : combo == 2 ? 31f : 26f;
            var direction = attacker.Facing;
            var slash = AddPixelRect("Vệt chém", playerX + direction * reach - 11f, playerY - 5f,
                22f + combo * 3f, 4f, new Color(0.90f, 0.88f, 0.72f), 30);
            slash.transform.Rotate(0f, 0f, direction * (combo == 3 ? -18f : 18f));
            Destroy(slash, 0.12f);
            if (combo == 3)
            {
                var glint = AddPixelRect("Ánh chém kết liễu", playerX + direction * (reach + 8f) - 3f,
                    playerY - 11f, 6f, 15f, new Color(1f, 0.83f, 0.47f), 31);
                Destroy(glint, 0.12f);
            }
        }

        public void EnemyDefeated(GhostEnemy enemy)
        {
            var isBoss = enemy.Kind == EnemyKind.Boss;
            var dropCount = isBoss ? 12 : enemy.MaxHp;
            var x = enemy.transform.position.x * PixelsPerUnit;
            var y = 240f - enemy.transform.position.y * PixelsPerUnit;
            for (var i = 0; i < dropCount; i++)
            {
                var offset = i - (dropCount - 1) / 2f;
                var icon = AddPixelSprite("Đồng vàng", x + offset * 6f - 8f, y - 8f,
                    16f, 16f, "coin", 25);
                coins.Add(new CoinDrop
                {
                    X = x + offset * 6f,
                    Y = y,
                    VelocityX = offset * 34f,
                    VelocityY = -115f - i % 2 * 24f,
                    Value = 1,
                    Visual = icon
                });
            }
            if (isBoss)
            {
                UpdateGateVisual();
                Say("Kẻ canh cổng đã gục! Lối thoát đang mở.", 3f);
            }
        }

        private void AddShot(Vector2 position, Vector2 velocity)
        {
            var visual = AddPixelSprite("Đạn sáng ma đèn", position.x - 7f, position.y - 7f,
                14f, 14f, "firefly", 24);
            shots.Add(new EnemyShot { Position = position, Velocity = velocity, Life = 3.2f, Visual = visual });
        }

        private void AddBossShot(Vector2 position, Vector2 velocity)
        {
            var visual = AddPixelSprite("Sóng xung kích", position.x - 6f, position.y - 6f,
                12f, 12f, "boss_bolt", 24);
            shots.Add(new EnemyShot { Position = position, Velocity = velocity, Life = 2.5f, Visual = visual });
        }

        public void Say(string message, float seconds = 2.5f)
        {
            notice = message;
            noticeUntil = Time.unscaledTime + seconds;
            UpdateHud();
        }

        public void Finish()
        {
            if (won)
                return;
            won = true;
            Time.timeScale = 0f;
            endPanel.SetActive(true);
            menuPanel.SetActive(false);
            pausePanel.SetActive(false);
            Say("MÀN 1 HOÀN THÀNH!", 1000f);
        }

        public void RefreshHud()
        {
            UpdateHud();
        }

        private void UpdateHud()
        {
            if (player != null && hpText != null)
            {
                hpText.text = "MÁU  " + player.CurrentHp + " / " + player.MaxHp;
                if (potionText != null)
                    potionText.text = "HỒI MÁU ×" + player.Potions + " · H";
                healthFill.fillAmount = player.CurrentHp / (float)player.MaxHp;
                stressText.text = player.DebuffTimer > 0f
                    ? "CĂNG THẲNG  ·  CHẬM " + Mathf.CeilToInt(player.DebuffTimer) + "s"
                    : "CĂNG THẲNG  ·  " + Mathf.RoundToInt(player.Stress) + "%";
                stressFill.fillAmount = player.Stress / 100f;
                staminaText.text = "THỂ LỰC  ·  " + Mathf.RoundToInt(player.CurrentStamina) + "%";
                staminaFill.fillAmount = player.CurrentStamina / 100f;
                keyText.text = "CHÌA KHÓA  " + CollectedKeyCount + " / 3";
                coinText.text = "VÀNG  " + goldCoins;
                var x = player.transform.position.x * PixelsPerUnit;
                hudStats.text = x < 955f ? "KHU VỰC · TÀN TÍCH NGOÀI" :
                    x < 2095f ? "KHU VỰC · HÀNH LANG ĐÁ" : x < 2875f ? "KHU VỰC · HẺM TỐI" :
                    x < ExitX ? "KHU VỰC · ĐẤU TRƯỜNG BOSS" : "KHU VỰC · CỔNG THOÁT";
            }

            if (bossHud != null)
            {
                var bossVisible = bossAwake && gateBoss != null && gateBoss.Alive;
                bossHud.SetActive(bossVisible);
                if (bossVisible)
                {
                    bossHealthFill.fillAmount = gateBoss.Hp / (float)gateBoss.MaxHp;
                    bossHealthText.text = "KẺ CANH CỔNG  ·  " + gateBoss.Hp + " / " + gateBoss.MaxHp;
                }
            }

            if (noticeText != null)
            {
                var visible = Time.unscaledTime < noticeUntil;
                noticeText.text = visible ? notice : "";
                noticeText.enabled = visible;
                var color = noticeBackground.color;
                color.a = visible ? 0.86f : 0f;
                noticeBackground.color = color;
            }

            if (mapOpen && mapPanel != null)
            {
                mapRefresh -= Time.unscaledDeltaTime;
                if (mapRefresh <= 0f)
                {
                    mapRefresh = 0.18f;
                    DrawMiniMap();
                }
            }
        }

        private void DrawMiniMap()
        {
            var pixels = new Color32[256 * 64];
            var waterColor = new Color32(35, 91, 94, 255);
            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = waterColor;

            for (var i = 0; i < PlatformData.Length; i++)
            {
                var platform = PlatformData[i];
                var x0 = Mathf.Clamp(Mathf.RoundToInt(platform.x / WorldEnd * 255f), 0, 255);
                var x1 = Mathf.Clamp(Mathf.RoundToInt(platform.y / WorldEnd * 255f), 0, 255);
                var y = Mathf.Clamp(52 - Mathf.RoundToInt((platform.z - 140f) / 2f), 8, 59);
                for (var x = x0; x <= x1; x++)
                    SetMapPixel(pixels, x, y, new Color32(175, 149, 98, 255));
            }

            for (var i = 0; i < LedgeData.Length; i++)
            {
                var ledge = LedgeData[i];
                var x0 = Mathf.Clamp(Mathf.RoundToInt(ledge.x / WorldEnd * 255f), 0, 255);
                var x1 = Mathf.Clamp(Mathf.RoundToInt(ledge.y / WorldEnd * 255f), 0, 255);
                var y = Mathf.Clamp(52 - Mathf.RoundToInt((ledge.z - 140f) / 2f), 8, 59);
                for (var x = x0; x <= x1; x++)
                    SetMapPixel(pixels, x, y, new Color32(145, 118, 79, 255));
            }

            for (var i = 0; i < keyX.Length; i++)
            {
                if (!collectedKeys[i])
                    DrawMapDot(pixels, keyX[i], keyY[i], new Color32(255, 227, 138, 255), 2);
            }
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy != null && enemy.Alive)
                {
                    var tint = enemy.Kind == EnemyKind.Glow ? new Color32(245, 216, 117, 255) :
                        enemy.Kind == EnemyKind.Crab ? new Color32(229, 133, 99, 255) : new Color32(121, 181, 141, 255);
                    DrawMapDot(pixels, enemy.transform.position.x * PixelsPerUnit,
                        240f - enemy.transform.position.y * PixelsPerUnit, tint, 1);
                }
            }
            DrawMapDot(pixels, player.transform.position.x * PixelsPerUnit,
                240f - player.transform.position.y * PixelsPerUnit, new Color32(255, 248, 214, 255), 2);
            mapTexture.SetPixels32(pixels);
            mapTexture.Apply(false, false);
        }

        private void DrawMapDot(Color32[] pixels, float worldX, float worldY, Color32 color, int radius)
        {
            var x = Mathf.Clamp(Mathf.RoundToInt(worldX / WorldEnd * 255f), 0, 255);
            var y = Mathf.Clamp(52 - Mathf.RoundToInt((worldY - 140f) / 2f), 0, 63);
            for (var dx = -radius; dx <= radius; dx++)
                for (var dy = -radius; dy <= radius; dy++)
                    SetMapPixel(pixels, Mathf.Clamp(x + dx, 0, 255), Mathf.Clamp(y + dy, 0, 63), color);
        }

        private static void SetMapPixel(Color32[] pixels, int x, int y, Color32 color)
        {
            pixels[y * 256 + x] = color;
        }

        private void UpdateCamera()
        {
            var targetX = Mathf.Clamp(player.transform.position.x + 0.38f, 2.13f, WorldEnd / PixelsPerUnit - 2.13f);
            var playerScreenY = 240f - player.transform.position.y * PixelsPerUnit;
            var cameraOffset = Mathf.Clamp(playerScreenY - 180f, -18f, 70f);
            var targetY = 1.2f - cameraOffset / PixelsPerUnit;
            var position = gameCamera.transform.position;
            position.x = Mathf.SmoothDamp(position.x, targetX, ref cameraVelocityX, 0.12f);
            position.y = Mathf.SmoothDamp(position.y, targetY, ref cameraVelocityY, 0.12f);
            position.z = -10f;
            gameCamera.transform.position = position;
        }

        private void UpdateGateVisual()
        {
            var open = CanExit;
            if (gateDoor != null)
                gateDoor.SetActive(!open);
            if (lockedGateSprite != null)
                lockedGateSprite.color = open ? new Color(0.55f, 0.92f, 0.72f) : Color.white;
        }

        private void AwakenBoss()
        {
            if (bossAwake || gateBoss == null || !gateBoss.Alive)
                return;
            bossAwake = true;
            gateBoss.gameObject.SetActive(true);
            Say("Kẻ canh cổng đã thức tỉnh! Né đòn rồi phản công.", 3f);
        }

        private void AddGateVisual()
        {
            var gateX = ExitX;
            gateDoor = AddPixelSprite("Cổng khóa", gateX - 28f, 127f, 56f, 72f, "gate", 10);
            lockedGateSprite = gateDoor.GetComponent<SpriteRenderer>();
        }

        private void AddStartSign()
        {
            AddPixelSprite("Bảng chỉ dẫn", 38f, 142f, 80f, 56f, "sign", 8);
        }

        private void BuildBossArena()
        {
            AddPixelSprite("Vòm đấu trường", 3038f, 96f, 164f, 88f, "arena_arch", -18);
            AddRuinPillar(2928f, 184f, 1.05f, -8);
            AddRuinPillar(3282f, 184f, 1.05f, -8);
            AddCrystal(2970f, 183f, 0.8f, 8);
            AddCrystal(3240f, 183f, 0.8f, 8);
        }

        private void AddRuinPillar(float x, float baseY, float scale, int order)
        {
            var width = 48f * scale;
            var height = 96f * scale;
            AddPixelSprite("Cột đá đổ", x - width * 0.5f, baseY - height, width, height, "ruin_pillar", order);
        }

        private void AddCrystal(float x, float baseY, float scale, int order)
        {
            var width = 32f * scale;
            var height = 48f * scale;
            AddPixelSprite("Tinh thể xanh", x - width * 0.5f, baseY - height, width, height, "crystal", order);
        }

        private GameObject AddPixelRect(string name, float x, float y, float width, float height, Color color,
            int sortingOrder, bool solid = false, Transform parent = null)
        {
            var item = new GameObject(name);
            if (parent != null)
                item.transform.SetParent(parent, false);
            item.transform.position = PixelPosition(x + width * 0.5f, y + height * 0.5f);
            item.transform.localScale = new Vector3(width / PixelsPerUnit, height / PixelsPerUnit, 1f);
            var renderer = item.AddComponent<SpriteRenderer>();
            renderer.sprite = PixelSprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            if (solid)
            {
                var collider = item.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(1f, 1f);
            }
            return item;
        }

        private GameObject AddPixelSprite(string name, float x, float y, float width, float height,
            string spriteName, int sortingOrder, bool solid = false)
        {
            var item = new GameObject(name);
            var sprite = LoadPixelSprite(spriteName);
            item.transform.position = PixelPosition(x + width * 0.5f, y + height * 0.5f);
            item.transform.localScale = new Vector3(width / sprite.rect.width, height / sprite.rect.height, 1f);
            var renderer = item.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            if (solid)
            {
                var collider = item.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(width / PixelsPerUnit, height / PixelsPerUnit);
            }
            return item;
        }

        private GameObject AddPixelTile(string name, float x, float y, float width, float height,
            string spriteName, int sortingOrder, bool solid)
        {
            var item = new GameObject(name);
            item.transform.position = PixelPosition(x + width * 0.5f, y + height * 0.5f);
            var renderer = item.AddComponent<SpriteRenderer>();
            renderer.sprite = LoadPixelSprite(spriteName);
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = new Vector2(width / PixelsPerUnit, height / PixelsPerUnit);
            renderer.sortingOrder = sortingOrder;
            if (solid)
            {
                var collider = item.AddComponent<BoxCollider2D>();
                collider.size = renderer.size;
            }
            return item;
        }

        private static Sprite LoadPixelSprite(string spriteName)
        {
            if (ArtSprites.TryGetValue(spriteName, out var cached))
                return cached;

            var sprite = Resources.Load<Sprite>("GhostRiverArt/" + spriteName);
            if (sprite == null)
            {
                Debug.LogError("Missing pixel-art sprite: Resources/GhostRiverArt/" + spriteName + ".png");
                return PixelSprite;
            }
            ArtSprites.Add(spriteName, sprite);
            return sprite;
        }

        private static Sprite PixelFrameSprite
        {
            get
            {
                if (frameSprite == null)
                {
                    var source = LoadPixelSprite("ui_frame");
                    frameSprite = Sprite.Create(source.texture, source.rect, new Vector2(0.5f, 0.5f),
                        100f, 0, SpriteMeshType.FullRect, new Vector4(4f, 4f, 4f, 4f));
                }
                return frameSprite;
            }
        }

        private static Sprite PixelIcon(string spriteName)
        {
            return LoadPixelSprite(spriteName);
        }

        private static Sprite PixelSprite
        {
            get
            {
                if (pixelSprite != null)
                    return pixelSprite;
                var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
                {
                    name = "Pixel 1x1",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };
                texture.SetPixel(0, 0, Color.white);
                texture.Apply(false, true);
                pixelSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
                return pixelSprite;
            }
        }

        public Vector2 GetPlayerSpawnPosition()
        {
            return GetSpawnPosition(68f / PixelsPerUnit);
        }

        public Vector2 GetSpawnPosition(float worldX)
        {
            var px = worldX * PixelsPerUnit;
            var surface = TerrainHeightAt(px);
            if (float.IsPositiveInfinity(surface))
                surface = 191f;
            return PixelPosition(px, surface - 10f);
        }

        private static Vector3 PixelPosition(float centerX, float centerY)
        {
            return new Vector3(centerX / PixelsPerUnit, (240f - centerY) / PixelsPerUnit, 0f);
        }

        private static float TerrainHeightAt(float x)
        {
            for (var i = 0; i < PlatformData.Length; i++)
            {
                var platform = PlatformData[i];
                if (x >= platform.x && x <= platform.y)
                    return platform.z;
            }
            return float.PositiveInfinity;
        }

        private int CollectedKeyCount
        {
            get
            {
                var count = 0;
                for (var i = 0; i < collectedKeys.Length; i++)
                    if (collectedKeys[i])
                        count++;
                return count;
            }
        }

        private bool CanExit => CollectedKeyCount == keyX.Length &&
            gateBoss != null && !gateBoss.Alive;

        private GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 size, Vector2 position, Color color, bool pixelFrame = true)
        {
            var item = new GameObject(name, typeof(RectTransform), typeof(Image));
            item.transform.SetParent(parent, false);
            var image = item.GetComponent<Image>();
            image.color = color;
            var rect = item.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(anchorMin.x == anchorMax.x ? anchorMin.x : 0.5f,
                anchorMin.y == anchorMax.y ? anchorMin.y : 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            if (pixelFrame)
            {
                var frameObject = new GameObject("Viền pixel", typeof(RectTransform), typeof(Image));
                frameObject.transform.SetParent(item.transform, false);
                var frameRect = frameObject.GetComponent<RectTransform>();
                frameRect.anchorMin = Vector2.zero;
                frameRect.anchorMax = Vector2.one;
                frameRect.offsetMin = Vector2.zero;
                frameRect.offsetMax = Vector2.zero;
                var frame = frameObject.GetComponent<Image>();
                frame.sprite = PixelFrameSprite;
                frame.type = Image.Type.Sliced;
                frame.raycastTarget = false;
            }
            return item;
        }

        private Text CreateText(string name, string text, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 size, Vector2 position, int fontSize, Color color, TextAnchor alignment)
        {
            var item = new GameObject(name, typeof(RectTransform), typeof(Text));
            item.transform.SetParent(parent, false);
            var uiText = item.GetComponent<Text>();
            uiText.text = text;
            uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            uiText.fontStyle = FontStyle.Bold;
            uiText.fontSize = fontSize;
            uiText.resizeTextForBestFit = true;
            uiText.resizeTextMinSize = Mathf.Max(10, fontSize - 5);
            uiText.resizeTextMaxSize = fontSize;
            uiText.color = color;
            uiText.alignment = alignment;
            uiText.raycastTarget = false;
            uiText.horizontalOverflow = HorizontalWrapMode.Wrap;
            uiText.verticalOverflow = VerticalWrapMode.Overflow;
            var rect = item.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(anchorMin.x == anchorMax.x ? anchorMin.x : 0.5f,
                anchorMin.y == anchorMax.y ? anchorMin.y : 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            var shadow = item.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.02f, 0.04f, 0.05f, 0.9f);
            shadow.effectDistance = new Vector2(1f, -1f);
            return uiText;
        }

        private Image CreateIcon(string name, string spriteName, Transform parent, Vector2 anchor,
            Vector2 size, Vector2 position)
        {
            var item = new GameObject(name, typeof(RectTransform), typeof(Image));
            item.transform.SetParent(parent, false);
            var image = item.GetComponent<Image>();
            image.sprite = PixelIcon(spriteName);
            image.preserveAspect = true;
            image.raycastTarget = false;
            var rect = image.rectTransform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            return image;
        }

        private void AddButtonIcon(Button button, string spriteName)
        {
            var label = button.GetComponentInChildren<Text>();
            label.alignment = TextAnchor.MiddleRight;
            label.rectTransform.offsetMin = new Vector2(44f, -8f);
            label.rectTransform.offsetMax = new Vector2(-12f, 8f);
            CreateIcon("Biểu tượng", spriteName, button.transform, new Vector2(0f, 0.5f),
                new Vector2(26f, 26f), new Vector2(22f, 0f));
        }

        private Image CreateBar(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 size, Vector2 position, Color color)
        {
            var background = CreatePanel(name + " · nền", parent, anchorMin, anchorMax, size, position,
                new Color(0.13f, 0.16f, 0.16f, 1f), false);
            var fill = CreatePanel(name + " · đầy", background.transform, Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero, color, false).GetComponent<Image>();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            return fill;
        }

        private Button CreateButton(string name, string label, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 size, Vector2 position, UnityEngine.Events.UnityAction onClick)
        {
            var item = CreatePanel(name, parent, anchorMin, anchorMax, size, position,
                new Color(0.12f, 0.50f, 0.55f, 0.96f));
            var button = item.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.78f, 1f, 0.94f, 1f);
            colors.pressedColor = new Color(0.72f, 0.86f, 0.80f, 1f);
            colors.selectedColor = new Color(0.88f, 0.96f, 0.83f, 1f);
            button.colors = colors;
            button.onClick.AddListener(onClick);
            var text = CreateText("Nhãn", label, item.transform, Vector2.zero, Vector2.one,
                new Vector2(-12f, -8f), Vector2.zero, 16, Color.white, TextAnchor.MiddleCenter);
            text.raycastTarget = false;
            return button;
        }

        private void ToggleMap()
        {
            mapOpen = !mapOpen;
            mapPanel.SetActive(mapOpen);
            if (mapOpen)
                DrawMiniMap();
        }

        private void OpenCatalog()
        {
            if (!started || paused || won)
                return;
            catalogOpen = true;
            Time.timeScale = 0f;
            catalogPanel.SetActive(true);
            if (mobileControls != null)
                mobileControls.SetActive(false);
            RefreshCatalog();
        }

        private void CloseCatalog()
        {
            catalogOpen = false;
            catalogPanel.SetActive(false);
            if (mobileControls != null)
                mobileControls.SetActive(true);
            Time.timeScale = paused ? 0f : 1f;
        }

        private void SelectCreature(int index)
        {
            if (index < 0 || index >= discoveredCreatures.Length || !discoveredCreatures[index])
                return;
            selectedCreature = index;
            RefreshCatalog();
        }

        private void DiscoverCreature(int index)
        {
            if (discoveredCreatures[index])
                return;
            discoveredCreatures[index] = true;
            selectedCreature = index;
            RefreshCatalog();
            Say("Mục lục mới: " + creatureNames[index] + " · nhấn Mục lục để xem.", 3f);
        }

        private void RefreshCatalog()
        {
            var unlocked = 0;
            for (var i = 0; i < discoveredCreatures.Length; i++)
            {
                if (discoveredCreatures[i])
                    unlocked++;
                if (catalogButtons == null || catalogButtons[i] == null)
                    continue;
                var text = catalogButtons[i].GetComponentInChildren<Text>();
                text.text = discoveredCreatures[i]
                    ? creatureNames[i] + "\n" + creatureHints[i]
                    : "Chưa ghi nhận\nGặp lần đầu để mở khóa";
                catalogButtons[i].interactable = discoveredCreatures[i];
            }

            if (catalogDetail != null)
                catalogDetail.text = selectedCreature >= 0 && discoveredCreatures[selectedCreature]
                    ? creatureDetails[selectedCreature]
                    : "Gặp một linh vật để ghi nhận thông tin về nó.";

            var indexButton = GameObject.Find("Mục lục");
            if (indexButton != null)
            {
                var text = indexButton.GetComponentInChildren<Text>();
                if (text != null)
                    text.text = "MỤC LỤC · " + unlocked + "/3";
            }
        }

        private void BeginGame()
        {
            if (won)
                return;
            started = true;
            paused = false;
            Time.timeScale = 1f;
            menuPanel.SetActive(false);
            pausePanel.SetActive(false);
            endPanel.SetActive(false);
            Say("Tìm đủ 3 chìa khóa rồi đánh bại kẻ canh cổng.", 2.6f);
        }

        private void ResetToMenu()
        {
            started = false;
            paused = false;
            won = false;
            Time.timeScale = 0f;
            menuPanel.SetActive(true);
            pausePanel.SetActive(false);
            endPanel.SetActive(false);
            catalogPanel.SetActive(false);
            mapPanel.SetActive(false);
            Say("Tìm đủ 3 chìa khóa rồi đánh bại kẻ canh cổng.", 5f);
        }

        private void TogglePause()
        {
            if (!started || won)
                return;
            paused = !paused;
            Time.timeScale = paused ? 0f : 1f;
            pausePanel.SetActive(paused);
        }

        private void RestartGame()
        {
            for (var i = 0; i < collectedKeys.Length; i++)
            {
                collectedKeys[i] = false;
                if (keyObjects[i] != null)
                    keyObjects[i].SetActive(true);
            }
            for (var i = 0; i < enemies.Count; i++)
                enemies[i].ResetPosition();
            bossAwake = false;
            if (gateBoss != null)
                gateBoss.gameObject.SetActive(false);
            for (var i = 0; i < coins.Count; i++)
                if (coins[i].Visual != null)
                    Destroy(coins[i].Visual);
            for (var i = 0; i < shots.Count; i++)
                if (shots[i].Visual != null)
                    Destroy(shots[i].Visual);
            coins.Clear();
            shots.Clear();
            goldCoins = 0;
            gateMessageCooldown = 0f;
            won = false;
            paused = false;
            started = true;
            mapOpen = false;
            catalogOpen = false;
            mapPanel.SetActive(false);
            catalogPanel.SetActive(false);
            pausePanel.SetActive(false);
            menuPanel.SetActive(false);
            endPanel.SetActive(false);
            player.ResetState();
            UpdateGateVisual();
            RefreshCatalog();
            Time.timeScale = 1f;
            Say("Tìm đủ 3 chìa khóa rồi đánh bại kẻ canh cổng.", 2.6f);
        }

        private void OnDestroy()
        {
            if (mapTexture != null)
                Destroy(mapTexture);
        }
    }

    public enum EnemyKind
    {
        Mud,
        Crab,
        Glow,
        Boss
    }

    public enum EnemyState
    {
        Patrol,
        Windup,
        Dash,
        Pounce,
        Recover
    }

    public sealed class GhostEnemy : MonoBehaviour
    {
        private GhostRiverGame game;
        private SpriteRenderer sprite;
        private BoxCollider2D hitbox;
        private float hitTimer;
        private float deathTimer;

        public EnemyKind Kind { get; private set; }
        public EnemyState State { get; private set; }
        public Rigidbody2D Body { get; private set; }
        public int Hp { get; private set; }
        public int MaxHp { get; private set; }
        public float Speed { get; private set; }
        public float Left { get; private set; }
        public float Right { get; private set; }
        public float OriginX { get; private set; }
        public float BaseTopY { get; private set; }
        public float BodyWidth { get; private set; }
        public float BodyHeight { get; private set; }
        public float Phase { get; private set; }
        public float StateTimer { get; private set; }
        public int NextBossAttack { get; set; }
        public float Direction { get; set; }
        public float Facing { get; private set; } = 1f;
        public bool Alive { get; private set; } = true;

        public void Initialize(GhostRiverGame owner, EnemyKind kind, int hp, float speed, float left, float right, float phase)
        {
            game = owner;
            Kind = kind;
            Hp = hp;
            MaxHp = hp;
            Speed = speed;
            Left = left;
            Right = right;
            OriginX = transform.position.x * 100f;
            BodyWidth = Kind == EnemyKind.Boss ? 64f : 20f;
            BodyHeight = Kind == EnemyKind.Boss ? 64f : 20f;
            BaseTopY = 240f - transform.position.y * 100f - BodyHeight * 0.5f;
            Phase = phase;
            Direction = 1f;
            StateTimer = phase * 0.2f;
            NextBossAttack = 0;
            sprite = GetComponent<SpriteRenderer>();
            hitbox = GetComponent<BoxCollider2D>();
            Body = GetComponent<Rigidbody2D>();
        }

        public void TickTimers(float dt)
        {
            StateTimer -= dt;
            hitTimer = Mathf.Max(0f, hitTimer - dt);
            if (sprite != null)
            {
                var baseColor = Kind == EnemyKind.Boss ? new Color(0.82f, 0.48f, 0.57f) :
                    Kind == EnemyKind.Glow ? new Color(0.92f, 0.83f, 0.47f) :
                    Kind == EnemyKind.Crab ? new Color(0.91f, 0.57f, 0.34f) :
                    new Color(0.34f, 0.49f, 0.36f);
                sprite.color = hitTimer > 0f ? new Color(1f, 0.45f, 0.43f) : baseColor;
            }
        }

        public void TickDeath(float dt)
        {
            deathTimer -= dt;
            if (deathTimer <= 0f)
                gameObject.SetActive(false);
        }

        public void SetState(EnemyState state, float timer)
        {
            State = state;
            StateTimer = timer;
        }

        public void SetFacing(float direction)
        {
            if (Mathf.Abs(direction) < 0.01f)
                return;
            Facing = Mathf.Sign(direction);
            sprite.flipX = Facing < 0f;
        }

        public bool TakeDamage(int amount, float sourceX)
        {
            if (!Alive)
                return false;
            Hp -= amount;
            hitTimer = 0.2f;
            SetState(EnemyState.Recover, 0.34f);
            if (Body != null && Kind != EnemyKind.Glow)
                Body.linearVelocity = new Vector2((transform.position.x < sourceX / 100f ? -1f : 1f) * 0.7f, 0.8f);
            if (Hp > 0)
                return false;

            Alive = false;
            deathTimer = 0.38f;
            hitbox.enabled = false;
            Body.simulated = false;
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 0.45f);
            game.EnemyDefeated(this);
            return true;
        }

        public bool IsGrounded()
        {
            var filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = GhostRiverGame.GroundMask,
                useTriggers = false
            };
            var hits = new RaycastHit2D[2];
            return Physics2D.BoxCast(hitbox.bounds.center, hitbox.bounds.size * 0.9f, 0f,
                Vector2.down, filter, hits, 0.025f) > 0;
        }

        public void ResetPosition()
        {
            if (Body == null)
                return;
            gameObject.SetActive(true);
            Body.position = new Vector2(OriginX / 100f, (240f - (BaseTopY + BodyHeight * 0.5f)) / 100f);
            Body.linearVelocity = Vector2.zero;
            Direction = 1f;
            Alive = true;
            deathTimer = 0f;
            Hp = MaxHp;
            hitbox.enabled = true;
            Body.simulated = true;
            SetState(EnemyState.Patrol, 0f);
            NextBossAttack = 0;
            sprite.color = Kind == EnemyKind.Boss ? new Color(0.82f, 0.48f, 0.57f) :
                Kind == EnemyKind.Glow ? new Color(0.92f, 0.83f, 0.47f) :
                Kind == EnemyKind.Crab ? new Color(0.91f, 0.57f, 0.34f) :
                new Color(0.34f, 0.49f, 0.36f);
        }
    }

    internal sealed class CoinDrop
    {
        public float X;
        public float Y;
        public float VelocityX;
        public float VelocityY;
        public int Value;
        public bool Settled;
        public GameObject Visual;
    }

    internal sealed class EnemyShot
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Life;
        public GameObject Visual;
    }
}
