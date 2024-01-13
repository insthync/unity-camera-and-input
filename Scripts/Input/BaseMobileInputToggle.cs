using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public abstract class BaseMobileInputToggle : MonoBehaviour, IMobileInputArea, IPointerDownHandler, IPointerUpHandler
{
    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    [Range(0f, 1f)]
    public float alphaWhileOff = 0.75f;
    [Range(0f, 1f)]
    public float alphaWhileOn = 1f;
    public BoolEvent onToggle = new BoolEvent();
    [SerializeField]
    private bool isOn = false;

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
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = GetAlphaByCurrentState() * _alphaMultiplier;
        }
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

    private float GetAlphaByCurrentState()
    {
        return IsOn ? alphaWhileOn : alphaWhileOff;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        IsOn = !IsOn;
        if (_canvasGroup != null)
            _canvasGroup.alpha = GetAlphaByCurrentState() * _alphaMultiplier;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // TODO: May have setting to toggle when pointer up
    }

    public void OnLoadAlpha(float alpha)
    {
        _alphaMultiplier = alpha;
    }

    protected abstract void OnToggle(bool isOn);
}
