using System.Collections.Generic;
using UnityEngine;

namespace S2RD.Managers
{
    /// <summary>
    /// Hero 프리팹 목록을 보관하는 데이터베이스입니다.
    /// 자동 생성 도구가 목록을 갱신하며 런타임 소환에서 사용합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "HeroDatabase", menuName = "S2RD/Database/HeroDatabase")]
    public class HeroDatabase : ScriptableObject
    {
        [SerializeField] private List<GameObject> heroPrefabs = new List<GameObject>();

        public IReadOnlyList<GameObject> HeroPrefabs => heroPrefabs;

        public GameObject 랜덤프리팹가져오기()
        {
            if (heroPrefabs == null || heroPrefabs.Count == 0)
                return null;

            int index = Random.Range(0, heroPrefabs.Count);
            return heroPrefabs[index];
        }

        public void 프리팹목록설정(List<GameObject> prefabs)
        {
            heroPrefabs = prefabs ?? new List<GameObject>();
        }
    }
}
