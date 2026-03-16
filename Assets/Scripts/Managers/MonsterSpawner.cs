using UnityEngine;

namespace S2RD.Managers
{
    /// <summary>
    /// 웨이브 규칙에 따라 경로 시작점에서 몬스터를 생성하는 스포너입니다.
    /// </summary>
    public class MonsterSpawner : MonoBehaviour
    {
        private static readonly Vector3[] _경로포인트 =
        {
            new Vector3(-4.4f, 4f, 0f),
            new Vector3(4.4f, 4f, 0f),
            new Vector3(4.4f, 1.6f, 0f),
            new Vector3(-4.4f, 1.6f, 0f),
            new Vector3(-4.4f, -0.9f, 0f),
            new Vector3(4.4f, -0.9f, 0f),
            new Vector3(4.4f, -3.5f, 0f)
        };

        private WaveManager _waveManager;
        private EnemyManager _enemyManager;
        private float _다음스폰시간;
        private int _현재웨이브남은스폰수;
        private int _현재웨이브번호 = -1;
        private bool _보스스폰완료;

        public void 초기화(WaveManager waveManager, EnemyManager enemyManager)
        {
            _waveManager = waveManager;
            _enemyManager = enemyManager;
        }

        public Vector3[] 경로포인트 => _경로포인트;

        private void Update()
        {
            if (_waveManager == null || _enemyManager == null)
                return;

            웨이브동기화();

            if (Time.time < _다음스폰시간)
                return;

            if (_현재웨이브남은스폰수 > 0)
            {
                일반몬스터스폰();
                _현재웨이브남은스폰수--;
                _다음스폰시간 = Time.time + 0.7f;
                return;
            }

            if (_waveManager.보스웨이브여부() && !_보스스폰완료)
            {
                보스몬스터스폰();
                _보스스폰완료 = true;
                _다음스폰시간 = Time.time + 1.5f;
            }
        }

        private void 웨이브동기화()
        {
            if (_현재웨이브번호 == _waveManager.현재웨이브)
                return;

            _현재웨이브번호 = _waveManager.현재웨이브;
            _현재웨이브남은스폰수 = _waveManager.현재웨이브스폰수();
            _보스스폰완료 = false;
            _다음스폰시간 = Time.time + 0.3f;
        }

        private void 일반몬스터스폰()
        {
            Vector3 spawn = 랜덤스폰위치();
            float hp = 100f * _waveManager.몬스터체력배율();
            float speed = 1.5f * _waveManager.몬스터이동속도배율();
            _enemyManager.몬스터생성(false, spawn, hp, speed, 1, 8, _경로포인트);
        }

        private void 보스몬스터스폰()
        {
            Vector3 spawn = 랜덤스폰위치();
            float hp = 100f * _waveManager.몬스터체력배율();
            float speed = 1.5f * _waveManager.몬스터이동속도배율();
            _enemyManager.몬스터생성(true, spawn, hp, speed, 1, 8, _경로포인트);
        }

        private Vector3 랜덤스폰위치()
        {
            Vector3 start = _경로포인트[0];
            Vector3 jitter = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.2f, 0.2f), 0f);
            return start + jitter;
        }
    }
}
