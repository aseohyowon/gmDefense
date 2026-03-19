using UnityEngine;
using UnityEngine.UI;

namespace S2RD.UI
{
    /// <summary>
    /// 캔버스 기반 UI를 생성하고 게임 UI 이벤트를 전달합니다.
    /// TopBar, BottomBar 구조로 확장 가능한 HUD를 제공합니다.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private Text _waveText;
        private Text _goldText;
        private Text _lifeText;
        private GameObject _gameOverPanel;
        private Text _gameOverText;
        private Button _mainMenuButton;
        private Button _summonButton;

        public event System.Action 소환버튼클릭이벤트;
        public event System.Action 메인메뉴버튼클릭이벤트;

        private void Awake()
        {
            Canvas canvas = 게임캔버스생성();
            RectTransform topBar = TopBar생성(canvas.transform);
            RectTransform bottomBar = BottomUI생성(canvas.transform);

            _waveText = 텍스트생성(topBar, "WaveText", "Wave: 1", 34, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f), new Vector2(300f, 60f), Color.white, new Vector2(26f, 0f));
            _lifeText = 텍스트생성(topBar, "LifeText", "Life: 20", 34, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(300f, 60f), Color.white);
            _goldText = 텍스트생성(topBar, "GoldText", "Gold: 0", 34, TextAnchor.MiddleRight, new Vector2(1f, 0.5f), new Vector2(360f, 60f), Color.white, new Vector2(-26f, 0f));
            _summonButton = SummonButton생성(bottomBar).GetComponent<Button>();
            게임오버패널생성(canvas.transform);
        }

        private Canvas 게임캔버스생성()
        {
            GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

            // 화면 방향에 따라 기준 해상도를 맞춰 비율 찌그러짐을 줄입니다.
            bool isLandscape = Screen.width >= Screen.height;
            scaler.referenceResolution = isLandscape
                ? new Vector2(1920f, 1080f)
                : new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                new GameObject("EventSystem",
                    typeof(UnityEngine.EventSystems.EventSystem),
                    typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }

            return canvas;
        }

        private RectTransform TopBar생성(Transform parent)
        {
            GameObject topBar = 패널생성(parent, "TopBar", new Color(0.10f, 0.18f, 0.40f, 0.90f), 1080f, 140f, new Vector2(0.5f, 1f), new Vector2(0f, 0f));
            return topBar.GetComponent<RectTransform>();
        }

        private RectTransform BottomUI생성(Transform parent)
        {
            GameObject bar = 패널생성(parent, "BottomUI", new Color(0.08f, 0.13f, 0.24f, 0.88f), 1080f, 200f, new Vector2(0.5f, 0f), new Vector2(0f, 0f));
            return bar.GetComponent<RectTransform>();
        }

        private RectTransform SummonButton생성(Transform parent)
        {
            GameObject buttonObj = new GameObject("SummonButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObj.transform.SetParent(parent, false);

            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(460f, 118f);
            rect.anchoredPosition = new Vector2(0f, 100f);

            Image image = buttonObj.GetComponent<Image>();
            image.color = new Color(0.95f, 0.76f, 0.20f, 0.98f);

            Text label = 텍스트생성(rect, "Label", "소환", 42, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(280f, 70f), Color.black);
            _ = label;

            Button button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(() => 소환버튼클릭이벤트?.Invoke());
            return rect;
        }

        private void 게임오버패널생성(Transform parent)
        {
            _gameOverPanel = new GameObject("GameOverPanel", typeof(RectTransform), typeof(Image));
            _gameOverPanel.transform.SetParent(parent, false);

            RectTransform panelRect = _gameOverPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(820f, 540f);

            Image panelImage = _gameOverPanel.GetComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.72f);

            _gameOverText = 텍스트생성(_gameOverPanel.transform, "GameOverText", "GAME OVER", 96, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(760f, 180f), new Color(1f, 0.32f, 0.32f, 1f), new Vector2(0f, 90f));

            GameObject buttonObj = new GameObject("MainMenuButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObj.transform.SetParent(_gameOverPanel.transform, false);

            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(360f, 108f);
            buttonRect.anchoredPosition = new Vector2(0f, -110f);

            Image buttonImage = buttonObj.GetComponent<Image>();
            buttonImage.color = new Color(0.96f, 0.82f, 0.25f, 1f);

            텍스트생성(buttonObj.transform, "Label", "메인 메뉴", 42, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(280f, 70f), Color.black);

            _mainMenuButton = buttonObj.GetComponent<Button>();
            _mainMenuButton.onClick.AddListener(() => 메인메뉴버튼클릭이벤트?.Invoke());

            _gameOverPanel.SetActive(false);
        }

        private GameObject 패널생성(Transform parent, string name, Color color, float width, float height, Vector2 anchor, Vector2 anchoredPosition)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = anchoredPosition;

            Image image = obj.GetComponent<Image>();
            image.color = color;
            return obj;
        }

        private Text 텍스트생성(Transform parent, string name, string value, int fontSize, TextAnchor alignment, Vector2 anchor, Vector2 size, Color? color = null, Vector2? anchoredPos = null)
        {
            GameObject textObj = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObj.transform.SetParent(parent, false);

            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPos ?? Vector2.zero;

            Text text = textObj.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color ?? Color.white;
            text.text = value;
            return text;
        }

        public void 골드갱신(int gold)
        {
            if (_goldText != null)
                _goldText.text = $"Gold: {gold}";
        }

        public void 웨이브갱신(int wave)
        {
            if (_waveText != null)
                _waveText.text = $"Wave: {wave}";
        }

        public void 라이프갱신(int life)
        {
            if (_lifeText != null)
                _lifeText.text = $"Life: {life}";
        }

        public void 소환버튼활성화(bool isEnabled)
        {
            if (_summonButton != null)
                _summonButton.interactable = isEnabled;
        }

        public void 게임오버표시()
        {
            if (_gameOverPanel != null)
                _gameOverPanel.SetActive(true);

            if (_waveText != null)
                _waveText.text = "Game Over";

            if (_summonButton != null)
                _summonButton.interactable = false;
        }
    }
}
