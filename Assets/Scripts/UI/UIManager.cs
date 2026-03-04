// UIManager.cs
// 게임 내 모든 UI 요소를 관리하는 클래스.
// 골드, HP, 웨이브, 소환 버튼, 특성 시너지 표시를 담당한다.

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GMDefense.Core;
using GMDefense.Systems;
using GMDefense.Heroes;

namespace GMDefense.UI
{
    /// <summary>
    /// UI 매니저.
    /// 각 매니저의 이벤트를 구독하여 UI를 갱신한다.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("골드 UI")]
        [Tooltip("현재 골드 텍스트")]
        [SerializeField] private TextMeshProUGUI 골드텍스트;

        [Tooltip("소환 비용 텍스트")]
        [SerializeField] private TextMeshProUGUI 소환비용텍스트;

        [Header("플레이어 HP UI")]
        [Tooltip("플레이어 HP 슬라이더")]
        [SerializeField] private Slider HP슬라이더;

        [Tooltip("플레이어 HP 텍스트 (숫자 표시)")]
        [SerializeField] private TextMeshProUGUI HP텍스트;

        [Header("웨이브 UI")]
        [Tooltip("현재 웨이브 번호 텍스트")]
        [SerializeField] private TextMeshProUGUI 웨이브텍스트;

        [Tooltip("다음 웨이브까지 남은 시간 텍스트")]
        [SerializeField] private TextMeshProUGUI 웨이브타이머텍스트;

        [Header("특성 시너지 UI")]
        [Tooltip("시너지 상태 텍스트")]
        [SerializeField] private TextMeshProUGUI 시너지텍스트;

        [Header("게임 상태 UI")]
        [Tooltip("게임 오버 패널")]
        [SerializeField] private GameObject 게임오버패널;

        [Tooltip("일시정지 패널")]
        [SerializeField] private GameObject 일시정지패널;

        [Header("버튼")]
        [Tooltip("영웅 소환 버튼")]
        [SerializeField] private Button 소환버튼;

        [Tooltip("일시정지 버튼")]
        [SerializeField] private Button 일시정지버튼;

        [Tooltip("게임 시작 버튼")]
        [SerializeField] private Button 게임시작버튼;

        private void Start()
        {
            이벤트구독();
            초기UI설정();
        }

        private void OnDestroy()
        {
            이벤트해제();
        }

        private void Update()
        {
            웨이브타이머갱신();
        }

        /// <summary>
        /// 각 시스템의 이벤트를 구독한다.
        /// </summary>
        private void 이벤트구독()
        {
            if (GoldSystem.인스턴스 != null)
                GoldSystem.인스턴스.골드변경이벤트 += 골드UI갱신;

            if (GameManager.인스턴스 != null)
            {
                GameManager.인스턴스.HP변경이벤트 += HPUI갱신;
                GameManager.인스턴스.게임상태변경이벤트 += 게임상태UI갱신;
            }

            if (WaveManager.인스턴스 != null)
                WaveManager.인스턴스.웨이브변경이벤트 += 웨이브UI갱신;

            if (TraitSystem.인스턴스 != null)
                TraitSystem.인스턴스.시너지변경이벤트 += 시너지UI갱신;

            // 버튼 이벤트 연결
            소환버튼?.onClick.AddListener(() => GameManager.인스턴스?.영웅소환());
            일시정지버튼?.onClick.AddListener(일시정지버튼클릭);
            게임시작버튼?.onClick.AddListener(() => GameManager.인스턴스?.게임시작());
        }

        /// <summary>
        /// 이벤트 구독을 해제한다.
        /// </summary>
        private void 이벤트해제()
        {
            if (GoldSystem.인스턴스 != null)
                GoldSystem.인스턴스.골드변경이벤트 -= 골드UI갱신;

            if (GameManager.인스턴스 != null)
            {
                GameManager.인스턴스.HP변경이벤트 -= HPUI갱신;
                GameManager.인스턴스.게임상태변경이벤트 -= 게임상태UI갱신;
            }

            if (WaveManager.인스턴스 != null)
                WaveManager.인스턴스.웨이브변경이벤트 -= 웨이브UI갱신;

            if (TraitSystem.인스턴스 != null)
                TraitSystem.인스턴스.시너지변경이벤트 -= 시너지UI갱신;
        }

        /// <summary>
        /// 초기 UI 값을 설정한다.
        /// </summary>
        private void 초기UI설정()
        {
            게임오버패널?.SetActive(false);
            일시정지패널?.SetActive(false);

            if (GoldSystem.인스턴스 != null)
            {
                골드UI갱신(GoldSystem.인스턴스.현재골드);
                소환비용텍스트?.SetText($"소환 비용: {GoldSystem.인스턴스.현재소환비용}G");
            }

            if (GameManager.인스턴스 != null)
                HPUI갱신(GameManager.인스턴스.현재플레이어HP, GameManager.인스턴스.최대플레이어HP);
        }

        /// <summary>골드 UI를 갱신한다.</summary>
        private void 골드UI갱신(int 골드)
        {
            골드텍스트?.SetText($"골드: {골드}G");

            if (GoldSystem.인스턴스 != null)
                소환비용텍스트?.SetText($"소환 비용: {GoldSystem.인스턴스.현재소환비용}G");
        }

        /// <summary>HP UI를 갱신한다.</summary>
        private void HPUI갱신(int 현재, int 최대)
        {
            if (HP슬라이더 != null)
            {
                HP슬라이더.maxValue = 최대;
                HP슬라이더.value = 현재;
            }
            HP텍스트?.SetText($"{현재} / {최대}");
        }

        /// <summary>웨이브 UI를 갱신한다.</summary>
        private void 웨이브UI갱신(int 웨이브)
        {
            웨이브텍스트?.SetText($"웨이브 {웨이브}");
        }

        /// <summary>시너지 UI를 갱신한다.</summary>
        private void 시너지UI갱신(System.Collections.Generic.Dictionary<TraitType, int> 시너지목록)
        {
            if (TraitSystem.인스턴스 != null)
                시너지텍스트?.SetText(TraitSystem.인스턴스.시너지상태문자열가져오기());
        }

        /// <summary>게임 상태에 따라 UI 패널을 갱신한다.</summary>
        private void 게임상태UI갱신(게임상태 상태)
        {
            게임오버패널?.SetActive(상태 == 게임상태.게임오버);
            일시정지패널?.SetActive(상태 == 게임상태.일시정지);
        }

        /// <summary>매 프레임 웨이브 타이머를 갱신한다.</summary>
        private void 웨이브타이머갱신()
        {
            if (WaveManager.인스턴스 == null) return;
            웨이브타이머텍스트?.SetText($"다음 웨이브: {Mathf.CeilToInt(WaveManager.인스턴스.남은대기시간)}초");
        }

        /// <summary>일시정지 버튼 클릭 처리.</summary>
        private void 일시정지버튼클릭()
        {
            if (GameManager.인스턴스 == null) return;

            if (GameManager.인스턴스.현재상태 == 게임상태.진행중)
                GameManager.인스턴스.일시정지();
            else if (GameManager.인스턴스.현재상태 == 게임상태.일시정지)
                GameManager.인스턴스.일시정지해제();
        }
    }
}
