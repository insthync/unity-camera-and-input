using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MobileInputPositionSave : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public const string KEY_X = "_X";
    public const string KEY_Y = "_Y";
    [Tooltip("This is key prefix which will follows by _X,_Y")]
    public string saveKey = "_ANY_KEY_NAME_";
    [Tooltip("Default position will be used when there is no saved position")]
    public Vector2 defaultPosition;
    [Tooltip("If this is `TRUE` it will be able to move and save position")]
    public bool isEditMode;
    [Tooltip("If this is `TRUE` it will save to player prefs when end drag")]
    public bool autoSave;

    public string SaveKeyX { get { return saveKey + KEY_X; } }
    public string SaveKeyY { get { return saveKey + KEY_Y; } }
    public Vector2 SavedPosition
    {
        get
        {
            return new Vector2(
                PlayerPrefs.GetFloat(SaveKeyX, defaultPosition.x),
                PlayerPrefs.GetFloat(SaveKeyY, defaultPosition.y));
        }
        set
        {
            PlayerPrefs.SetFloat(SaveKeyX, value.x);
            PlayerPrefs.SetFloat(SaveKeyY, value.y);
            PlayerPrefs.Save();
        }
    }

    public RectTransform RectTransform
    {
        get { return transform as RectTransform; }
    }

    private Vector2 lastMousePosition;

    private void Start()
    {
        LoadPosition();
    }

    public void ResetPosition()
    {
        if (!isEditMode)
            return;
        RectTransform.anchoredPosition = defaultPosition;
    }

    public void SavePosition()
    {
        if (!isEditMode)
            return;
        SavedPosition = RectTransform.anchoredPosition;
    }

    public void LoadPosition()
    {
        RectTransform.anchoredPosition = SavedPosition;
    }

#if UNITY_EDITOR
    [ContextMenu("Set Default Position By Current Position")]
    public void SetDefaultPositionByCurrentPosition()
    {
        defaultPosition = RectTransform.anchoredPosition;
        Debug.Log("[MobileInputPositionSave] Set default position to: " + defaultPosition);
        EditorUtility.SetDirty(this);
    }
#endif

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isEditMode)
            return;
        lastMousePosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isEditMode)
            return;
        Vector2 currentMousePosition = eventData.position;
        Vector2 diff = currentMousePosition - lastMousePosition;
        Vector3 newPosition = RectTransform.position + new Vector3(diff.x, diff.y);
        RectTransform.position = newPosition;
        lastMousePosition = currentMousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isEditMode)
            return;
        if (autoSave)
            SavePosition();
    }
}
