using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using S2RD.Hero;
using S2RD.Enemy;
using S2RD.Systems;
using S2RD.UI;

namespace S2RD.Managers
{
    /// <summary>
    /// 모바일 세로형 디펜스 게임의 핵심 시스템을 조립하는 메인 매니저입니다.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static readonly string[] 대상씬이름목록 = { "GameScene", "Game", "test", "Test" };
        private const int 최대영웅수 = 20;
        private const int 시작라이프 = 20;
        private static readonly Vector3[] 경로포인트 =
        {
            new Vector3(-4.4f, 4f, 0f),
            new Vector3(4.4f, 4f, 0f),
            new Vector3(4.4f, 1.6f, 0f),
            new Vector3(-4.4f, 1.6f, 0f),
            new Vector3(-4.4f, -0.9f, 0f),
            new Vector3(4.4f, -0.9f, 0f),
            new Vector3(4.4f, -3.5f, 0f)
        };
        private static readonly Vector3[] 기본영웅슬롯 =
        {
            new Vector3(-4.9f, 4.8f, 0f),
            new Vector3(-4.9f, 2.9f, 0f),
            new Vector3(4.9f, 2.9f, 0f),
            new Vector3(4.9f, 1.0f, 0f),
            new Vector3(-4.9f, 1.0f, 0f),
            new Vector3(-4.9f, -1.3f, 0f),
            new Vector3(4.9f, -1.3f, 0f),
            new Vector3(4.9f, -3.2f, 0f),
            new Vector3(-4.9f, -3.2f, 0f),
            new Vector3(0f, 5.0f, 0f)
        };

        private readonly List<Hero.Hero> _영웅목록 = new List<Hero.Hero>();

        private S2RD.Systems.GoldSystem _goldSystem;
        private RandomHeroSystem _randomHeroSystem;
        private WaveManager _waveManager;
        private EnemyManager _enemyManager;
        private MonsterSpawner _spawnManager;
        private MergeManager _mergeManager;
        private SummonManager _summonManager;
        private UIManager _uiManager;
        private HeroDatabase _heroDatabase;

        private Transform _heroContainer;
        private Transform _enemyContainer;
        private Transform _gameArea;
        private int _현재라이프 = 시작라이프;
        private bool _게임오버;
        private Hero.Hero _드래그중영웅;
        private int _활성터치아이디 = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void 씬로드시초기화()
        {
            string activeSceneName = SceneManager.GetActiveScene().name;
            if (!게임씬여부(activeSceneName))
                return;

            if (FindObjectOfType<GameManager>() != null)
                return;

            GameObject root = new GameObject("GameManager");
            root.AddComponent<GameManager>();
        }

        private static bool 게임씬여부(string sceneName)
        {
            for (int i = 0; i < 대상씬이름목록.Length; i++)
            {
                if (대상씬이름목록[i] == sceneName)
                    return true;
            }

            return false;
        }

        private void Awake()
        {
            세로형카메라설정();
            기존오브젝트정리();
            씬구조생성();

            _goldSystem = gameObject.AddComponent<S2RD.Systems.GoldSystem>();
            _randomHeroSystem = gameObject.AddComponent<RandomHeroSystem>();
            GameObject waveManagerObject = new GameObject("WaveManager");
            _waveManager = waveManagerObject.AddComponent<WaveManager>();
            _enemyManager = gameObject.AddComponent<EnemyManager>();
            GameObject monsterSpawnerObject = new GameObject("MonsterSpawner");
            _spawnManager = monsterSpawnerObject.AddComponent<MonsterSpawner>();
            _mergeManager = gameObject.AddComponent<MergeManager>();
            _summonManager = gameObject.AddComponent<SummonManager>();
            _uiManager = gameObject.AddComponent<UIManager>();
            _heroDatabase = Resources.Load<HeroDatabase>("Databases/HeroDatabase");

            _enemyManager.초기화(_enemyContainer);
            _enemyManager.몬스터사망이벤트 += 몬스터사망처리;
            _enemyManager.몬스터이탈이벤트 += 몬스터이탈처리;
            _spawnManager.초기화(_waveManager, _enemyManager);
            _summonManager.초기화(_goldSystem, _randomHeroSystem, _heroDatabase, _heroContainer, 기본영웅슬롯, 최대영웅수);
            _goldSystem.골드변경이벤트 += 골드표시갱신;
            _waveManager.웨이브변경이벤트 += 웨이브표시갱신;
            _uiManager.소환버튼클릭이벤트 += 소환버튼처리;
            _uiManager.메인메뉴버튼클릭이벤트 += 메인메뉴이동;
        }

        private void Start()
        {
            골드표시갱신(_goldSystem.현재골드);
            웨이브표시갱신(_waveManager.현재웨이브);
            _uiManager.라이프갱신(_현재라이프);
        }

        private void Update()
        {
            if (_게임오버)
                return;

            드래그입력처리();

            IReadOnlyList<Enemy.Enemy> enemies = _enemyManager.활성몬스터목록;
            List<Enemy.Enemy> enemyList = new List<Enemy.Enemy>(enemies);
            for (int i = _영웅목록.Count - 1; i >= 0; i--)
            {
                if (_영웅목록[i] == null)
                {
                    _영웅목록.RemoveAt(i);
                    continue;
                }

                _영웅목록[i].자동공격업데이트(enemyList);
            }
        }

        private void 소환버튼처리()
        {
            if (_게임오버 || _summonManager == null)
                return;

            _summonManager.소환시도(_영웅목록, hero =>
            {
                if (hero != null)
                    hero.드래그해제이벤트 += 영웅드래그해제처리;
            });
        }

        private void 영웅드래그해제처리(Hero.Hero source, Hero.Hero target)
        {
            Hero.Hero merged = _mergeManager.드래그합성시도(_영웅목록, source, target);
            if (merged != null)
                merged.드래그해제이벤트 += 영웅드래그해제처리;
        }

        private void 몬스터사망처리(Enemy.Enemy enemy)
        {
            if (_게임오버)
                return;

            _goldSystem.골드추가(enemy.골드드랍);
            _waveManager.처치카운트증가();
        }

        private void 몬스터이탈처리(Enemy.Enemy enemy)
        {
            if (_게임오버)
                return;

            _현재라이프 = Mathf.Max(0, _현재라이프 - 1);
            _uiManager.라이프갱신(_현재라이프);

            if (_현재라이프 <= 0)
                게임오버처리();
        }

        private void 게임오버처리()
        {
            _게임오버 = true;
            _spawnManager.enabled = false;
            _waveManager.enabled = false;
            _enemyManager.enabled = false;
            _summonManager.enabled = false;
            _uiManager.소환버튼활성화(false);
            _uiManager.게임오버표시();
        }

        private void 메인메뉴이동()
        {
            string targetScene = SceneUtility.GetBuildIndexByScenePath("Assets/MainMenu.unity") >= 0 ? "MainMenu" : "Menu";
            SceneManager.LoadScene(targetScene);
        }

        private void 골드표시갱신(int gold)
        {
            if (_uiManager != null)
                _uiManager.골드갱신(gold);
        }

        private void 웨이브표시갱신(int wave)
        {
            if (_uiManager != null)
                _uiManager.웨이브갱신(wave);
        }

        private void 세로형카메라설정()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera", typeof(Camera));
                cam = camObj.GetComponent<Camera>();
                camObj.tag = "MainCamera";
            }

            cam.orthographic = true;
            cam.orthographicSize = 9.4f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.backgroundColor = new Color(0.08f, 0.11f, 0.18f, 1f);
        }

        private void 기존오브젝트정리()
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];
                if (root == gameObject)
                    continue;

                if (root.CompareTag("MainCamera"))
                {
                    카메라자식정리(root.transform);
                    continue;
                }

                Destroy(root);
            }

            Hero.Hero[] existingHeroes = FindObjectsOfType<Hero.Hero>(true);
            for (int i = 0; i < existingHeroes.Length; i++)
            {
                if (existingHeroes[i] != null)
                    Destroy(existingHeroes[i].gameObject);
            }

            Enemy.Enemy[] existingEnemies = FindObjectsOfType<Enemy.Enemy>(true);
            for (int i = 0; i < existingEnemies.Length; i++)
            {
                if (existingEnemies[i] != null)
                    Destroy(existingEnemies[i].gameObject);
            }
        }

        private void 카메라자식정리(Transform cameraTransform)
        {
            for (int i = cameraTransform.childCount - 1; i >= 0; i--)
            {
                Transform child = cameraTransform.GetChild(i);
                Destroy(child.gameObject);
            }
        }

        private void 씬구조생성()
        {
            _gameArea = new GameObject("GameArea").transform;
            _gameArea.position = Vector3.zero;

            맵배경생성(_gameArea);
            도로생성(_gameArea);

            _enemyContainer = new GameObject("EnemyContainer").transform;
            _enemyContainer.SetParent(_gameArea, false);

            _heroContainer = new GameObject("HeroContainer").transform;
            _heroContainer.SetParent(_gameArea, false);
        }

        private void 맵배경생성(Transform parent)
        {
            GameObject bg = new GameObject("MapBackground", typeof(SpriteRenderer));
            bg.transform.SetParent(parent, false);

            SpriteRenderer renderer = bg.GetComponent<SpriteRenderer>();
            renderer.sprite = 기본사각스프라이트생성();
            renderer.color = new Color(0.30f, 0.60f, 0.26f, 1f);
            renderer.sortingOrder = 0;
            bg.transform.position = Vector3.zero;
            bg.transform.localScale = new Vector3(18.5f, 18f, 1f);
        }

        private void 도로생성(Transform parent)
        {
            GameObject roadRoot = new GameObject("RoadPath");
            roadRoot.transform.SetParent(parent, false);

            for (int i = 0; i < 경로포인트.Length - 1; i++)
            {
                Vector3 start = 경로포인트[i];
                Vector3 end = 경로포인트[i + 1];

                GameObject segment = new GameObject($"RoadSegment_{i}", typeof(SpriteRenderer));
                segment.transform.SetParent(roadRoot.transform, false);
                segment.transform.position = (start + end) * 0.5f;

                Vector3 dir = end - start;
                float length = dir.magnitude;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                segment.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                segment.transform.localScale = new Vector3(length, 1.35f, 1f);

                SpriteRenderer renderer = segment.GetComponent<SpriteRenderer>();
                renderer.sprite = 기본사각스프라이트생성();
                renderer.color = new Color(0.47f, 0.30f, 0.14f, 1f);
                renderer.sortingOrder = 1;
            }
        }

        private void 드래그입력처리()
        {
            if (Input.touchSupported && Application.isMobilePlatform)
            {
                터치드래그처리();
                return;
            }

            마우스드래그처리();
        }

        private void 터치드래그처리()
        {
            if (_드래그중영웅 == null)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    if (touch.phase != TouchPhase.Began)
                        continue;

                    Hero.Hero hero = 화면좌표영웅찾기(touch.position);
                    if (hero == null)
                        continue;

                    _드래그중영웅 = hero;
                    _활성터치아이디 = touch.fingerId;
                    _드래그중영웅.드래그시작();
                    break;
                }
            }

            if (_드래그중영웅 == null)
                return;

            bool found = false;
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.fingerId != _활성터치아이디)
                    continue;

                found = true;
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    _드래그중영웅.드래그종료();
                    _드래그중영웅 = null;
                    _활성터치아이디 = -1;
                    return;
                }

                Vector3 worldPos = 스크린좌표월드좌표변환(touch.position);
                _드래그중영웅.드래그이동(worldPos);
                return;
            }

            if (!found)
            {
                _드래그중영웅.드래그종료();
                _드래그중영웅 = null;
                _활성터치아이디 = -1;
            }
        }

        private void 마우스드래그처리()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _드래그중영웅 = 화면좌표영웅찾기(Input.mousePosition);
                if (_드래그중영웅 != null)
                    _드래그중영웅.드래그시작();
            }

            if (_드래그중영웅 == null)
                return;

            if (Input.GetMouseButton(0))
            {
                Vector3 worldPos = 스크린좌표월드좌표변환(Input.mousePosition);
                _드래그중영웅.드래그이동(worldPos);
            }

            if (Input.GetMouseButtonUp(0))
            {
                _드래그중영웅.드래그종료();
                _드래그중영웅 = null;
            }
        }

        private Hero.Hero 화면좌표영웅찾기(Vector2 screenPos)
        {
            Vector3 worldPos = 스크린좌표월드좌표변환(screenPos);
            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            if (hit == null)
                return null;

            return hit.GetComponent<Hero.Hero>();
        }

        private Vector3 스크린좌표월드좌표변환(Vector2 screenPos)
        {
            Camera cam = Camera.main;
            if (cam == null)
                return Vector3.zero;

            Vector3 screen = new Vector3(screenPos.x, screenPos.y, Mathf.Abs(cam.transform.position.z));
            Vector3 world = cam.ScreenToWorldPoint(screen);
            world.z = 0f;
            return world;
        }

        private static Sprite 기본사각스프라이트생성()
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}
