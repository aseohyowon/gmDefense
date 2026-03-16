using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace S2RD.Systems
{
    /// <summary>
    /// MainMenu 씬의 기본 UI(타이틀/게임 시작 버튼)를 런타임에 구성합니다.
    /// </summary>
    public class MainMenuBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void 씬로드후초기화()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name != "MainMenu")
                return;

            if (FindObjectOfType<MainMenuBootstrap>() != null)
                return;

            GameObject root = new GameObject("MainMenuRoot");
            root.AddComponent<MainMenuBootstrap>();
        }

        private void Awake()
        {
            씬정리();
            카메라보장();
            메뉴UI생성();
        }

        private void 씬정리()
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];
                if (root == gameObject)
                    continue;

                if (root.CompareTag("MainCamera"))
                    continue;

                Destroy(root);
            }
        }

        private static void 카메라보장()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera", typeof(Camera));
                cam = camObj.GetComponent<Camera>();
                camObj.tag = "MainCamera";
            }

            cam.orthographic = true;
            cam.backgroundColor = new Color(0.08f, 0.11f, 0.18f, 1f);
            cam.transform.position = new Vector3(0f, 0f, -10f);
        }

        private void 메뉴UI생성()
        {
            GameObject canvasObj = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
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

            Text title = 텍스트생성(canvas.transform, "Title", "Defense Game", 92, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.72f), new Vector2(900f, 160f), Color.white);
            _ = title;

            GameObject buttonObj = new GameObject("StartButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObj.transform.SetParent(canvas.transform, false);

            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.42f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.42f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(420f, 118f);

            Image buttonImage = buttonObj.GetComponent<Image>();
            buttonImage.color = new Color(0.95f, 0.76f, 0.20f, 1f);

            Text label = 텍스트생성(buttonObj.transform, "Label", "게임 시작", 44, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(260f, 70f), Color.black);
            _ = label;

            Button button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(게임시작);
        }

        private void 게임시작()
        {
            string nextScene = SceneUtility.GetBuildIndexByScenePath("Assets/Game.unity") >= 0 ? "Game" : "GameScene";
            SceneManager.LoadScene(nextScene);
        }

        private static Text 텍스트생성(Transform parent, string name, string value, int fontSize, TextAnchor alignment, Vector2 anchor, Vector2 size, Color? color = null)
        {
            GameObject textObj = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObj.transform.SetParent(parent, false);

            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.sizeDelta = size;

            Text text = textObj.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color ?? Color.white;
            text.text = value;
            return text;
        }
    }
}
