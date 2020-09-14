using UnityEngine;
using UnityEngine.EventSystems;

public class MobileInputButton : MonoBehaviour, IMobileInputArea, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private string keyName;
    [SerializeField]
    [Range(0f, 1f)]
    private float alphaWhileIdling = 1f;
    [SerializeField]
    [Range(0f, 1f)]
    private float alphaWhilePressing = 0.75f;

    private CanvasGroup canvasGroup;
    private MobileInputConfig config;
    private float alphaMultiplier = 1f;
    private bool buttonAlreadyDown;

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = alphaWhileIdling * alphaMultiplier;
        }
        config = GetComponent<MobileInputConfig>();
        if (config != null)
        {
            // Updating default canvas group alpha when loading new config
            config.onLoadAlpha += OnLoadAlpha;
        }
    }

    private void OnDestroy()
    {
        if (config != null)
            config.onLoadAlpha -= OnLoadAlpha;
    }

    private void OnDisable()
    {
        if (buttonAlreadyDown)
            InputManager.SetButtonUp(keyName);
        SetIdleState();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SetPressedState();
        InputManager.SetButtonDown(keyName);
        buttonAlreadyDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        SetIdleState();
        InputManager.SetButtonUp(keyName);
        buttonAlreadyDown = false;
    }

    private void SetIdleState()
    {
        canvasGroup.alpha = alphaWhileIdling * alphaMultiplier;
    }

    private void SetPressedState()
    {
        canvasGroup.alpha = alphaWhilePressing * alphaMultiplier;
    }

    public void OnLoadAlpha(float alpha)
    {
        alphaMultiplier = alpha;
    }
}
