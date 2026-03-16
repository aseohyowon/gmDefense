using System.Collections.Generic;
using UnityEngine;
using S2RD.Hero;

namespace S2RD.Managers
{
    /// <summary>
    /// 드래그로 겹친 동일 타입/동일 레벨 영웅을 합성하는 매니저입니다.
    /// </summary>
    public class MergeManager : MonoBehaviour
    {
        public Hero.Hero 드래그합성시도(List<Hero.Hero> heroes, Hero.Hero source, Hero.Hero target)
        {
            if (heroes == null || source == null || target == null)
                return null;

            if (!heroes.Contains(source) || !heroes.Contains(target))
                return null;

            HeroData sourceData = source.데이터;
            HeroData targetData = target.데이터;

            if (sourceData.Type != targetData.Type)
                return null;

            if (sourceData.Level != targetData.Level)
                return null;

            Vector3 spawnPos = target.transform.position;
            Transform parent = target.transform.parent;
            int nextLevel = targetData.Level + 1;

            heroes.Remove(source);
            heroes.Remove(target);
            Destroy(source.gameObject);
            Destroy(target.gameObject);

            GameObject mergedObj = new GameObject("MergedHero", typeof(SpriteRenderer), typeof(Hero.Hero));
            if (parent != null)
                mergedObj.transform.SetParent(parent, false);
            mergedObj.transform.position = spawnPos;

            Hero.Hero merged = mergedObj.GetComponent<Hero.Hero>();
            merged.초기화(HeroData.생성(targetData.Type, nextLevel));
            heroes.Add(merged);
            return merged;
        }
    }
}
