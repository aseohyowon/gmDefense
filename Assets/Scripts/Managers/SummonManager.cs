using System;
using System.Collections.Generic;
using UnityEngine;
using S2RD.Hero;

namespace S2RD.Managers
{
    /// <summary>
    /// 소환 버튼 기반 영웅 생성과 소환 비용 처리를 담당합니다.
    /// </summary>
    public class SummonManager : MonoBehaviour
    {
        private S2RD.Systems.GoldSystem _goldSystem;
        private S2RD.Systems.RandomHeroSystem _randomHeroSystem;
        private HeroDatabase _heroDatabase;
        private Transform _heroContainer;
        private Vector3[] _heroSlots;
        private int _maxHeroCount;

        public void 초기화(
            S2RD.Systems.GoldSystem goldSystem,
            S2RD.Systems.RandomHeroSystem randomHeroSystem,
            HeroDatabase heroDatabase,
            Transform heroContainer,
            Vector3[] heroSlots,
            int maxHeroCount)
        {
            _goldSystem = goldSystem;
            _randomHeroSystem = randomHeroSystem;
            _heroDatabase = heroDatabase;
            _heroContainer = heroContainer;
            _heroSlots = heroSlots;
            _maxHeroCount = maxHeroCount;
        }

        public bool 소환시도(List<Hero.Hero> heroList, Action<Hero.Hero> onSpawned)
        {
            if (heroList == null || _goldSystem == null || _randomHeroSystem == null || _heroContainer == null || _heroSlots == null || _heroSlots.Length == 0)
                return false;

            if (heroList.Count >= _maxHeroCount)
                return false;

            if (!_goldSystem.소환비용지불())
                return false;

            HeroData data = _randomHeroSystem.랜덤영웅데이터생성();
            data.Type = HeroType.Archer;
            Vector3 spawnPosition = 사용가능영웅위치계산(heroList);

            GameObject heroObj = 영웅오브젝트생성(data);
            heroObj.transform.SetParent(_heroContainer, false);
            heroObj.transform.position = spawnPosition;

            Hero.Hero hero = heroObj.GetComponent<Hero.Hero>();
            if (hero == null)
                hero = heroObj.AddComponent<Hero.Hero>();

            hero.초기화(data);
            heroList.Add(hero);
            onSpawned?.Invoke(hero);
            return true;
        }

        private GameObject 영웅오브젝트생성(HeroData data)
        {
            GameObject typedPrefab = 타입별프리팹로드(data.Type);
            if (typedPrefab != null)
                return Instantiate(typedPrefab);

            if (_heroDatabase != null)
            {
                GameObject prefab = _heroDatabase.랜덤프리팹가져오기();
                if (prefab != null)
                    return Instantiate(prefab);
            }

            return new GameObject("Hero", typeof(SpriteRenderer), typeof(Hero.Hero));
        }

        private static GameObject 타입별프리팹로드(HeroType type)
        {
            _ = type;
            string[] candidates =
            {
                "Prefabs/Heroes/hero_archer"
            };

            for (int i = 0; i < candidates.Length; i++)
            {
                GameObject prefab = Resources.Load<GameObject>(candidates[i]);
                if (prefab != null)
                    return prefab;
            }

            return null;
        }

        private Vector3 사용가능영웅위치계산(List<Hero.Hero> heroList)
        {
            for (int i = 0; i < _heroSlots.Length; i++)
            {
                Vector3 slot = _heroSlots[i];
                bool occupied = false;

                for (int j = 0; j < heroList.Count; j++)
                {
                    Hero.Hero hero = heroList[j];
                    if (hero == null)
                        continue;

                    if (Vector3.Distance(hero.transform.position, slot) < 0.6f)
                    {
                        occupied = true;
                        break;
                    }
                }

                if (!occupied)
                    return slot;
            }

            return _heroSlots[heroList.Count % _heroSlots.Length];
        }
    }
}
