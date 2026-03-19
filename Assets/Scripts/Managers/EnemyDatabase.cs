using System.Collections.Generic;
using UnityEngine;

namespace S2RD.Managers
{
    /// <summary>
    /// Enemy 프리팹 목록을 보관하는 데이터베이스입니다.
    /// 자동 생성 도구가 목록을 갱신하며 런타임 스폰에서 사용합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyDatabase", menuName = "S2RD/Database/EnemyDatabase")]
    public class EnemyDatabase : ScriptableObject
    {
        [SerializeField] private List<GameObject> enemyPrefabs = new List<GameObject>();

        public IReadOnlyList<GameObject> EnemyPrefabs => enemyPrefabs;

        public GameObject 랜덤프리팹가져오기()
        {
            if (enemyPrefabs == null || enemyPrefabs.Count == 0)
                return null;

            int index = Random.Range(0, enemyPrefabs.Count);
            return enemyPrefabs[index];
        }

        public void 프리팹목록설정(List<GameObject> prefabs)
        {
            enemyPrefabs = prefabs ?? new List<GameObject>();
        }
    }
}
