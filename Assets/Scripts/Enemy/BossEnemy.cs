using UnityEngine;

namespace S2RD.Enemy
{
    /// <summary>
    /// 보스 몬스터 규칙을 가진 특수 몬스터입니다.
    /// </summary>
    public class BossEnemy : Enemy
    {
        public override void 초기화(float hp, float speed, int damage, int dropGold, Vector3[] pathPoints)
        {
            // 요구사항: 체력 5배, 크기 2배, 속도 느림
            float bossHp = hp * 5f;
            float bossSpeed = Mathf.Max(0.4f, speed * 0.55f);
            int bossDamage = Mathf.Max(2, damage * 3);
            int bossDrop = Mathf.Max(30, dropGold * 3);

            base.초기화(bossHp, bossSpeed, bossDamage, bossDrop, pathPoints);

            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer != null)
                renderer.color = new Color(0.52f, 0.11f, 0.11f, 1f);

            transform.localScale = new Vector3(2f, 2f, 1f);
        }
    }
}
