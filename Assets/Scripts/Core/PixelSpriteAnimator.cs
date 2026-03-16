using UnityEngine;

namespace S2RD.Core
{
    /// <summary>
    /// 간단한 프레임 기반 픽셀 스프라이트 애니메이션 컴포넌트입니다.
    /// idle, move, attack 상태를 지원합니다.
    /// </summary>
    public class PixelSpriteAnimator : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        private Sprite[] _idleFrames;
        private Sprite[] _moveFrames;
        private Sprite[] _attackFrames;

        private float _frameTimer;
        private int _frameIndex;
        private float _frameInterval = 0.14f;
        private float _attackFrameInterval = 0.07f;  // 공격 애니메이션은 2배 빠르게
        private AnimState _currentState = AnimState.Idle;

        private enum AnimState
        {
            Idle,
            Move,
            Attack
        }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        public void 프레임설정(Sprite[] idleFrames, Sprite[] moveFrames, Sprite[] attackFrames)
        {
            _idleFrames = idleFrames;
            _moveFrames = moveFrames;
            _attackFrames = attackFrames;
            _frameIndex = 0;
            _frameTimer = 0f;
            _currentState = AnimState.Idle;
            현재프레임적용();
        }

        public void 상태설정이동(bool isMoving)
        {
            if (_currentState == AnimState.Attack)
                return;

            AnimState target = isMoving ? AnimState.Move : AnimState.Idle;
            if (_currentState == target)
                return;

            _currentState = target;
            _frameIndex = 0;
            _frameTimer = 0f;
            현재프레임적용();
        }

        // 핑퐁 공격 전용 상태
        private bool _attackForward = true;

        public void 공격애니메이션재생()
        {
            _currentState = AnimState.Attack;
            _attackForward = true;
            _frameIndex = 0;
            _frameTimer = 0f;
            현재프레임적용();
        }

        private void Update()
        {
            Sprite[] frames = 현재상태프레임가져오기();
            if (frames == null || frames.Length == 0 || _renderer == null)
                return;

            float currentInterval = _currentState == AnimState.Attack ? _attackFrameInterval : _frameInterval;
            _frameTimer += Time.deltaTime;
            if (_frameTimer < currentInterval)
                return;

            _frameTimer = 0f;

            if (_currentState == AnimState.Attack)
            {
                // 핑퐁: 끝까지 갔다가 역방향으로 돌아온 뒤 Idle
                if (_attackForward)
                {
                    _frameIndex++;
                    if (_frameIndex >= frames.Length)
                    {
                        _frameIndex = frames.Length - 2;  // 마지막 프레임 바로 전으로
                        _attackForward = false;
                    }
                }
                else
                {
                    _frameIndex--;
                    if (_frameIndex < 0)
                    {
                        _currentState = AnimState.Idle;
                        _frameIndex = 0;
                    }
                }
            }
            else
            {
                _frameIndex++;
                if (_frameIndex >= frames.Length)
                    _frameIndex = 0;
            }

            현재프레임적용();
        }

        private void 현재프레임적용()
        {
            Sprite[] frames = 현재상태프레임가져오기();
            if (_renderer == null || frames == null || frames.Length == 0)
                return;

            _renderer.sprite = frames[Mathf.Clamp(_frameIndex, 0, frames.Length - 1)];
        }

        private Sprite[] 현재상태프레임가져오기()
        {
            return _currentState switch
            {
                AnimState.Move => _moveFrames,
                AnimState.Attack => _attackFrames,
                _ => _idleFrames
            };
        }
    }
}
