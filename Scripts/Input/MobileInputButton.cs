﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    private float alphaMultiplier = 1f;

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = alphaWhileIdling * alphaMultiplier;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (canvasGroup != null)
            canvasGroup.alpha = alphaWhilePressing * alphaMultiplier;
        InputManager.SetButtonDown(keyName);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (canvasGroup != null)
            canvasGroup.alpha = alphaWhileIdling * alphaMultiplier;
        InputManager.SetButtonUp(keyName);
    }

    public void OnLoadAlpha(float alpha)
    {
        alphaMultiplier = alpha;
    }
}
