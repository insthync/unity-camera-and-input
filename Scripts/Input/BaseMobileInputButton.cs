using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Insthync.CameraAndInput
{
    public abstract class BaseMobileInputButton : MonoBehaviour, IMobileInputArea, IPointerDownHandler, IPointerUpHandler
    {
        [Range(0f, 1f)]
        public float alphaWhileIdling = 1f;
        [Range(0f, 1f)]
        public float alphaWhilePressing = 0.75f;
        public bool interactable = true;

        [Header("Events")]
        public UnityEvent onPointerDown = new UnityEvent();
        public UnityEvent onPointerUp = new UnityEvent();

        public bool Interactable
        {
            get { return interactable; }
            set { interactable = value; }
        }

        private CanvasGroup _canvasGroup;
        private MobileInputConfig _config;
        private float _alphaMultiplier = 1f;
        private PointerEventData _previousPointer;

        private void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                _canvasGroup.alpha = alphaWhileIdling * _alphaMultiplier;
            }
            _config = GetComponent<MobileInputConfig>();
            if (_config != null)
            {
                // Updating default canvas group alpha when loading new config
                _config.onLoadAlpha += OnLoadAlpha;
            }
        }

        private void OnDestroy()
        {
            if (_config != null)
                _config.onLoadAlpha -= OnLoadAlpha;
        }

        private void OnDisable()
        {
            OnPointerUp(null);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Interactable)
                return;
            if (_previousPointer != null)
                return;
            _previousPointer = eventData;
            InputManager.touchedPointerIds[eventData.pointerId] = gameObject;
            OnButtonDown();
            onPointerDown.Invoke();
            SetPressedState();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_previousPointer != null && eventData != null && _previousPointer.pointerId != eventData.pointerId)
                return;
            if (_previousPointer != null)
                InputManager.touchedPointerIds.Remove(_previousPointer.pointerId);
            _previousPointer = null;
            if (eventData != null)
            {
                OnButtonUp();
                onPointerUp.Invoke();
            }
            SetIdleState();
        }

        private void SetIdleState()
        {
            if (_canvasGroup)
                _canvasGroup.alpha = alphaWhileIdling * _alphaMultiplier;
        }

        private void SetPressedState()
        {
            if (_canvasGroup)
                _canvasGroup.alpha = alphaWhilePressing * _alphaMultiplier;
        }

        public void OnLoadAlpha(float alpha)
        {
            _alphaMultiplier = alpha;
        }

        public void SimulateClick()
        {
            StartCoroutine(SimulateClickRoutine());
        }

        private IEnumerator SimulateClickRoutine()
        {
            OnPointerDown(default);
            yield return null;
            OnPointerUp(default);
        }

        protected abstract void OnButtonDown();
        protected abstract void OnButtonUp();
    }
}
