using UnityEngine;

namespace S2RD.Core
{
    public class CharacterAssembler : MonoBehaviour
    {
        [Header("Character Parts")]
        [SerializeField] private Sprite headSprite;
        [SerializeField] private Sprite bodySprite;
        [SerializeField] private Sprite weaponSprite;

        [Header("Part Offsets")]
        [SerializeField] private Vector3 headOffset = new Vector3(0f, 0.2f, 0f);
        [SerializeField] private Vector3 bodyOffset = Vector3.zero;
        [SerializeField] private Vector3 weaponOffset = new Vector3(0.2f, 0f, 0f);

        private Transform _head;
        private Transform _body;
        private Transform _weapon;

        private SpriteRenderer _headRenderer;
        private SpriteRenderer _bodyRenderer;
        private SpriteRenderer _weaponRenderer;

        private void Awake()
        {
            EnsureParts();
        }

        private void Start()
        {
            ApplySprites();
        }

        private void OnValidate()
        {
            ApplySprites();
        }

        private void EnsureParts()
        {
            _head = EnsureChild("Head");
            _body = EnsureChild("Body");
            _weapon = EnsureChild("Weapon");

            _headRenderer = EnsureRenderer(_head, 2);
            _bodyRenderer = EnsureRenderer(_body, 0);
            _weaponRenderer = EnsureRenderer(_weapon, 1);
        }

        private Transform EnsureChild(string childName)
        {
            Transform child = transform.Find(childName);
            if (child != null)
                return child;

            GameObject childObject = new GameObject(childName);
            childObject.transform.SetParent(transform, false);
            return childObject.transform;
        }

        private SpriteRenderer EnsureRenderer(Transform target, int order)
        {
            SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
            if (renderer == null)
                renderer = target.gameObject.AddComponent<SpriteRenderer>();

            renderer.sortingOrder = order;
            return renderer;
        }

        public void ApplySprites()
        {
            if (_headRenderer == null || _bodyRenderer == null || _weaponRenderer == null)
                EnsureParts();

            _headRenderer.sprite = headSprite;
            _bodyRenderer.sprite = bodySprite;
            _weaponRenderer.sprite = weaponSprite;

            _head.localPosition = headOffset;
            _body.localPosition = bodyOffset;
            _weapon.localPosition = weaponOffset;
        }
    }
}