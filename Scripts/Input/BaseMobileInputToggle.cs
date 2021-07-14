using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public abstract class BaseMobileInputToggle : MonoBehaviour, IMobileInputArea, IPointerDownHandler, IPointerUpHandler
{
    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    [SerializeField]
    [Range(0f, 1f)]
    private float alphaWhileOff = 0.75f;
    [SerializeField]
    [Range(0f, 1f)]
    private float alphaWhileOn = 1f;
    [SerializeField]
    private bool isOn = false;
    [SerializeField]
    private BoolEvent onToggle = new BoolEvent();

    public bool IsOn
    {
        get { return isOn; }
        set
        {
            isOn = value;
            OnToggle(value);
            if (onToggle != null)
                onToggle.Invoke(value);
        }
    }

    private CanvasGroup canvasGroup;
    private MobileInputConfig config;
    private float alphaMultiplier = 1f;

    protected virtual void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = GetAlphaByCurrentState() * alphaMultiplier;
        }
        config = GetComponent<MobileInputConfig>();
        if (config != null)
        {
            // Updating default canvas group alpha when loading new config
            config.onLoadAlpha += OnLoadAlpha;
        }
    }

    protected virtual void OnDestroy()
    {
        if (config != null)
            config.onLoadAlpha -= OnLoadAlpha;
    }

    private float GetAlphaByCurrentState()
    {
        return IsOn ? alphaWhileOn : alphaWhileOff;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        IsOn = !IsOn;
        if (canvasGroup != null)
            canvasGroup.alpha = GetAlphaByCurrentState() * alphaMultiplier;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // TODO: May have setting to toggle when pointer up
    }

    public void OnLoadAlpha(float alpha)
    {
        alphaMultiplier = alpha;
    }

    protected abstract void OnToggle(bool isOn);
}
