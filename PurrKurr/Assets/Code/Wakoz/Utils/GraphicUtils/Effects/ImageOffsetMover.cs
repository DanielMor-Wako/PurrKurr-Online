using UnityEngine;
using UnityEngine.UI;

namespace Code.Wakoz.Utils.GraphicUtils.Effects {

    [RequireComponent(typeof(Image))]
    public class ImageOffsetMover : MonoBehaviour {

        [SerializeField] private Vector2 _speed;

        [SerializeField] private Vector2 _repeatOffset;
        private Vector2 _offset;

        private void Start() {
            var image = GetComponent<Image>();
            _repeatOffset = new Vector2(image.sprite.rect.width, image.sprite.rect.height);
            _repeatOffset /= image.pixelsPerUnitMultiplier;
            _repeatOffset *= 2;
        }

        private void LateUpdate() {
            _offset += _speed * Time.deltaTime;

            if (Mathf.Abs(_offset.x) > _repeatOffset.x) {
                transform.localPosition -= new Vector3(_repeatOffset.x, 0, 0);
                _offset.x -= _repeatOffset.x * Mathf.Sign(_speed.x);
            }

            if (Mathf.Abs(_offset.y) > _repeatOffset.y) {
                transform.localPosition -= new Vector3(0, _repeatOffset.y, 0);
                _offset.y -= _repeatOffset.y * Mathf.Sign(_speed.y);
                ;
            }

            transform.localPosition += (Vector3) _speed * Time.deltaTime;
        }

    }
}