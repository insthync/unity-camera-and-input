using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Insthync.CameraAndInput
{
    public abstract class BaseMobileInputToggle : MonoBehaviour, IMobileInputArea, IPointerDownHandler, IPointerUpHandler
    {
        [System.Serializable]
        public class BoolEvent : UnityEvent<bool> { }

        [System.Serializable]
        public class ToggleColorSetting
        {
            public Graphic targetGraphic;
            public Color colorWhileToggled = Color.white;
            public Color colorWhileUntoggled = Color.white;
        }

        [Range(0f, 1f)]
        public float alphaWhileOff = 0.75f;
        [Range(0f, 1f)]
        public float alphaWhileOn = 1f;
        public bool interactable = true;
        public ToggleColorSetting[] toggleColorSettings = new ToggleColorSetting[0];

        [Header("Events")]
        public BoolEvent onToggle = new BoolEvent();
        public UnityEvent onPointerDown = new UnityEvent();
        public UnityEvent onPointerUp = new UnityEvent();
        [SerializeField]
        private bool isOn = false;

        public bool Interactable
        {
            get { return interactable; }
            set { interactable = value; }
        }

        private bool _dirtyIsOn = false;
        public bool IsOn
        {
            get { return isOn; }
            set
            {
                isOn = value;
                if (_dirtyIsOn != value)
                {
                    _dirtyIsOn = value;
                    UpdateGraphics();
                    OnToggle(value);
                    if (onToggle != null)
                        onToggle.Invoke(value);
                }
            }
        }

        private CanvasGroup _canvasGroup;
        private MobileInputConfig _config;
        private float _alphaMultiplier = 1f;

        protected virtual void Start()
        {
            _dirtyIsOn = IsOn;
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            UpdateGraphics();
            _config = GetComponent<MobileInputConfig>();
            if (_config != null)
            {
                // Updating default canvas group alpha when loading new config
                _config.onLoadAlpha += OnLoadAlpha;
            }
        }

        protected virtual void OnDestroy()
        {
            if (_config != null)
                _config.onLoadAlpha -= OnLoadAlpha;
        }

        public void SetIsOnWithoutNotify(bool value)
        {
            isOn = value;
            if (_dirtyIsOn != value)
            {
                _dirtyIsOn = value;
                UpdateGraphics();
            }
        }

        private float GetAlphaByCurrentState()
        {
            return IsOn ? alphaWhileOn : alphaWhileOff;
        }

        private void UpdateGraphics()
        {
            if (_canvasGroup != null)
                _canvasGroup.alpha = GetAlphaByCurrentState() * _alphaMultiplier;
            for (int i = 0; i < toggleColorSettings.Length; ++i)
            {
                toggleColorSettings[i].targetGraphic.color = IsOn ? toggleColorSettings[i].colorWhileToggled : toggleColorSettings[i].colorWhileUntoggled;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Interactable)
                return;
            IsOn = !IsOn;
            if (eventData != null)
                onPointerDown?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // TODO: May have setting to toggle when pointer up
            if (eventData != null)
                onPointerUp?.Invoke();
        }

        public void OnLoadAlpha(float alpha)
        {
            _alphaMultiplier = alpha;
        }

        protected abstract void OnToggle(bool isOn);
    }
}
