using System.Collections.Generic;
using UnityEngine;
using S2RD.Enemy;

namespace S2RD.Managers
{
    /// <summary>
    /// 몬스터 생성/제거/이동 업데이트를 담당합니다.
    /// </summary>
    public class EnemyManager : MonoBehaviour
    {
        private readonly List<Enemy.Enemy> _활성몬스터 = new List<Enemy.Enemy>();
        private EnemyDatabase _enemyDatabase;

        public IReadOnlyList<Enemy.Enemy> 활성몬스터목록 => _활성몬스터;
        public Transform EnemyContainer { get; private set; }

        public event System.Action<Enemy.Enemy> 몬스터사망이벤트;
        public event System.Action<Enemy.Enemy> 몬스터이탈이벤트;

        private void Awake()
        {
            _enemyDatabase = Resources.Load<EnemyDatabase>("Databases/EnemyDatabase");
        }

        public void 초기화(Transform enemyContainer)
        {
            EnemyContainer = enemyContainer;
        }

        public Enemy.Enemy 몬스터생성(bool isBoss, Vector3 spawnPosition, float hp, float speed, int damage, int dropGold, Vector3[] pathPoints)
        {
            GameObject obj = 적오브젝트생성(isBoss);
            obj.transform.SetParent(EnemyContainer, false);
            obj.transform.position = spawnPosition;

            Enemy.Enemy enemy;
            if (isBoss)
            {
                BossEnemy bossEnemy = obj.GetComponent<BossEnemy>();
                if (bossEnemy == null)
                {
                    Enemy.Enemy existingEnemy = obj.GetComponent<Enemy.Enemy>();
                    if (existingEnemy != null)
                        Destroy(existingEnemy);

                    bossEnemy = obj.AddComponent<BossEnemy>();
                }

                enemy = bossEnemy;
            }
            else
            {
                enemy = obj.GetComponent<Enemy.Enemy>();
                if (enemy == null)
                    enemy = obj.AddComponent<Enemy.Enemy>();
            }

            enemy.초기화(hp, speed, damage, dropGold, pathPoints);
            _활성몬스터.Add(enemy);
            return enemy;
        }

        private GameObject 적오브젝트생성(bool isBoss)
        {
            string[] explicitPaths = isBoss
                ? new[] { "Prefabs/Enemies/enemy_BossDragon" }
                : new[] { "Prefabs/Enemies/enemy_goblin" };

            List<GameObject> direct = new List<GameObject>();
            for (int i = 0; i < explicitPaths.Length; i++)
            {
                GameObject p = Resources.Load<GameObject>(explicitPaths[i]);
                if (p != null)
                    direct.Add(p);
            }

            if (direct.Count > 0)
                return Instantiate(direct[Random.Range(0, direct.Count)]);

            if (_enemyDatabase != null && _enemyDatabase.EnemyPrefabs != null && _enemyDatabase.EnemyPrefabs.Count > 0)
            {
                string keyword = "boss";
                List<GameObject> filtered = new List<GameObject>();
                for (int i = 0; i < _enemyDatabase.EnemyPrefabs.Count; i++)
                {
                    GameObject prefab = _enemyDatabase.EnemyPrefabs[i];
                    if (prefab == null)
                        continue;

                    string lower = prefab.name.ToLowerInvariant();
                    bool isUserSpritePrefab = lower.StartsWith("enemy_");
                    if (!isUserSpritePrefab)
                        continue;

                    bool isBossPrefab = prefab.name.ToLowerInvariant().Contains(keyword);
                    if (isBoss == isBossPrefab)
                        filtered.Add(prefab);
                }

                if (filtered.Count > 0)
                    return Instantiate(filtered[Random.Range(0, filtered.Count)]);

                GameObject randomPrefab = _enemyDatabase.랜덤프리팹가져오기();
                if (randomPrefab != null)
                    return Instantiate(randomPrefab);
            }

            return new GameObject(isBoss ? "BossEnemy" : "Enemy");
        }

        private void Update()
        {
            for (int i = _활성몬스터.Count - 1; i >= 0; i--)
            {
                Enemy.Enemy enemy = _활성몬스터[i];
                if (enemy == null)
                {
                    _활성몬스터.RemoveAt(i);
                    continue;
                }

                enemy.이동업데이트();
                if (enemy.사망여부)
                {
                    몬스터사망이벤트?.Invoke(enemy);
                    Destroy(enemy.gameObject);
                    _활성몬스터.RemoveAt(i);
                    continue;
                }

                if (enemy.경로종료도달여부)
                {
                    몬스터이탈이벤트?.Invoke(enemy);
                    Destroy(enemy.gameObject);
                    _활성몬스터.RemoveAt(i);
                }
            }
        }
    }
}
