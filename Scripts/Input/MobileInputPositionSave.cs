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


    private Vector2 lastMousePosition;

    private void Start()
    {
        LoadPosition();
    }

    public void ResetPosition()
    {
        transform.localPosition = defaultPosition;
    }

    public void SavePosition()
    {
        SavedPosition = transform.localPosition;
    }

    public void LoadPosition()
    {
        transform.localPosition = SavedPosition;
    }

#if UNITY_EDITOR
    [ContextMenu("Set Default Position By Current Position")]
    public void SetDefaultPositionByCurrentPosition()
    {
        defaultPosition = transform.localPosition;
        Debug.Log("[MobileInputPositionSave] Set default position to: " + defaultPosition);
        EditorUtility.SetDirty(this);
    }
#endif

    private bool IsRectTransformInsideSreen(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        int visibleCorners = 0;
        Rect rect = new Rect(0, 0, Screen.width, Screen.height);
        foreach (Vector3 corner in corners)
        {
            if (rect.Contains(corner))
            {
                visibleCorners++;
            }
        }
        return visibleCorners == 4;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        lastMousePosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 currentMousePosition = eventData.position;
        Vector2 diff = currentMousePosition - lastMousePosition;
        RectTransform rect = GetComponent<RectTransform>();

        Vector3 newPosition = rect.position + new Vector3(diff.x, diff.y, transform.position.z);
        Vector3 oldPos = rect.position;
        rect.position = newPosition;
        if (!IsRectTransformInsideSreen(rect))
        {
            rect.position = oldPos;
        }
        lastMousePosition = currentMousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (autoSave)
            SavePosition();
    }
}
