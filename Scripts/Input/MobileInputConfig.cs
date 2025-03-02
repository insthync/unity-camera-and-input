﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Insthync.CameraAndInput
{
    [RequireComponent(typeof(CanvasGroup))]
    public class MobileInputConfig : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
    {
        public const string KEY_X = "_X";
        public const string KEY_Y = "_Y";
        public const string KEY_SCALE = "_SCALE";
        public const string KEY_ALPHA = "_ALPHA";
        [Tooltip("This is key prefix which will follows by _X,_Y")]
        public string saveKey = "_ANY_KEY_NAME_";
        [Tooltip("Default position will be used when there is no saved position")]
        public Vector2 defaultPosition;
        [Tooltip("Default scale will be used when there is no saved scale")]
        public float defaultScale = 1f;
        [Tooltip("Min value for scale settings")]
        public float minScale = 0.1f;
        [Tooltip("Max value for scale settings")]
        public float maxScale = 2f;
        [Tooltip("Default alpha will be used when there is no saved alpha")]
        public float defaultAlpha = 1f;
        [Tooltip("Min value for alpha settings")]
        [Range(0f, 1f)]
        public float minAlpha = 0.1f;
        [Tooltip("Max value for alpha settings")]
        [Range(0f, 1f)]
        public float maxAlpha = 1f;
        [Tooltip("If this is `TRUE` it will be able to move and save position")]
        public bool isEditMode;
        [Tooltip("If this is `TRUE` it will save to player prefs when end drag")]
        public bool autoSave;

        #region Save keys
        public string SaveKeyX { get { return saveKey + KEY_X; } }
        public string SaveKeyY { get { return saveKey + KEY_Y; } }
        public string SaveKeyScale { get { return saveKey + KEY_SCALE; } }
        public string SaveKeyAlpha { get { return saveKey + KEY_ALPHA; } }
        #endregion

        #region Saved data
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

        public Vector2 CurrentPosition
        {
            get { return RectTransform.anchoredPosition; }
            set { RectTransform.anchoredPosition = value; }
        }

        public float SavedScale
        {
            get
            {
                return PlayerPrefs.GetFloat(SaveKeyScale, defaultScale);
            }
            set
            {
                PlayerPrefs.SetFloat(SaveKeyScale, value);
                PlayerPrefs.Save();
            }
        }

        public float CurrentScale
        {
            get { return RectTransform.localScale.x; }
            set { RectTransform.localScale = Vector3.one * value; }
        }

        public float SavedAlpha
        {
            get
            {
                return PlayerPrefs.GetFloat(SaveKeyAlpha, defaultAlpha);
            }
            set
            {
                PlayerPrefs.SetFloat(SaveKeyAlpha, value);
                PlayerPrefs.Save();
            }
        }

        public float CurrentAlpha
        {
            get { return CanvasGroup.alpha; }
            set { CanvasGroup.alpha = value; }
        }
        #endregion

        #region Events
        public System.Action<Vector3> onLoadPosition;
        public System.Action<float> onLoadScale;
        public System.Action<float> onLoadAlpha;
        public System.Action<Vector3> onSavePosition;
        public System.Action<float> onSaveScale;
        public System.Action<float> onSaveAlpha;
        #endregion

        public RectTransform RectTransform
        {
            get { return transform as RectTransform; }
        }

        private CanvasGroup canvasGroup;
        public CanvasGroup CanvasGroup
        {
            get
            {
                if (!canvasGroup)
                    canvasGroup = GetComponent<CanvasGroup>();
                return canvasGroup;
            }
        }

        private Vector2 lastMousePosition;

        private void OnEnable()
        {
            LoadPosition();
            LoadScale();
            LoadAlpha();
        }

        public void ResetPosition()
        {
            if (!isEditMode)
                return;
            CurrentPosition = defaultPosition;
        }

        public void SavePosition()
        {
            if (!isEditMode)
                return;
            SavedPosition = CurrentPosition;
            onSavePosition?.Invoke(CurrentPosition);
        }

        public void LoadPosition()
        {
            CurrentPosition = SavedPosition;
            onLoadPosition?.Invoke(CurrentPosition);
        }

        public void ResetScale()
        {
            if (!isEditMode)
                return;
            CurrentScale = defaultScale;
        }

        public void SaveScale()
        {
            if (!isEditMode)
                return;
            SavedScale = CurrentScale;
            onSaveScale?.Invoke(CurrentScale);
        }

        public void LoadScale()
        {
            CanvasGroup.alpha = SavedAlpha;
            CurrentScale = SavedScale;
            onLoadScale?.Invoke(CurrentScale);
        }

        public void SetScale(float amount)
        {
            amount = amount < minScale ? minScale : amount;
            amount = amount > maxScale ? maxScale : amount;
            CurrentScale = amount;
        }

        public void ResetAlpha()
        {
            if (!isEditMode)
                return;
            CurrentAlpha = defaultAlpha;
        }

        public void SaveAlpha()
        {
            if (!isEditMode)
                return;
            SavedAlpha = CurrentAlpha;
            onSaveAlpha?.Invoke(CurrentAlpha);
        }

        public void LoadAlpha()
        {
            CurrentAlpha = SavedAlpha;
            onLoadAlpha?.Invoke(CurrentAlpha);
        }

        public void SetAlpha(float amount)
        {
            amount = amount < minAlpha ? minAlpha : amount;
            amount = amount > maxAlpha ? maxAlpha : amount;
            CurrentAlpha = amount;
        }

#if UNITY_EDITOR
        [ContextMenu("Set Default Position By Current Position To All Objects", false, 1000000)]
        public void SetDefaultPositionByCurrentPositionToAllObjects()
        {
            var objs = FindObjectsOfType<MobileInputConfig>();
            foreach (var obj in objs)
            {
                obj.SetDefaultPositionByCurrentPosition();
            }
        }

        [ContextMenu("Set Default Scale By Current Scale To All Objects", false, 1000001)]
        public void SetDefaultScaleByCurrentScaleToAllObjects()
        {
            var objs = FindObjectsOfType<MobileInputConfig>();
            foreach (var obj in objs)
            {
                obj.SetDefaultScaleByCurrentScale();
            }
        }

        [ContextMenu("Set Default Alpha By Current Alpha To All Objects", false, 1000002)]
        public void SetDefaultAlphaByCurrentAlphaToAllObjects()
        {
            var objs = FindObjectsOfType<MobileInputConfig>();
            foreach (var obj in objs)
            {
                obj.SetDefaultAlphaByCurrentAlpha();
            }
        }

        [ContextMenu("Set Default Position By Current Position", false, 1000100)]
        public void SetDefaultPositionByCurrentPosition()
        {
            defaultPosition = CurrentPosition;
            Debug.Log("[MobileInputPositionSave] Set default position to: " + defaultPosition);
            EditorUtility.SetDirty(this);
        }

        [ContextMenu("Set Default Scale By Current Scale", false, 1000101)]
        public void SetDefaultScaleByCurrentScale()
        {
            defaultScale = CurrentScale;
            Debug.Log("[MobileInputScaleSave] Set default scale to: " + defaultScale);
            EditorUtility.SetDirty(this);
        }

        [ContextMenu("Set Default Alpha By Current Alpha", false, 1000102)]
        public void SetDefaultAlphaByCurrentAlpha()
        {
            defaultAlpha = CurrentAlpha;
            Debug.Log("[MobileInputAlphaSave] Set default alpha to: " + defaultAlpha);
            EditorUtility.SetDirty(this);
        }

        [ContextMenu("Set Current Position By Default Position", false, 1000200)]
        public void SetCurrentPositionByDefaultPosition()
        {
            CurrentPosition = defaultPosition;
            Debug.Log("[MobileInputPositionSave] Set current position to: " + CurrentPosition);
            EditorUtility.SetDirty(this);
        }

        [ContextMenu("Set Current Scale By Default Scale", false, 1000201)]
        public void SetCurrentScaleByDefaultScale()
        {
            CurrentScale = defaultScale;
            Debug.Log("[MobileInputScaleSave] Set current scale to: " + CurrentScale);
            EditorUtility.SetDirty(this);
        }

        [ContextMenu("Set Current Alpha By Default Alpha", false, 1000202)]
        public void SetCurrentAlphaByDefaultAlpha()
        {
            CurrentAlpha = defaultAlpha;
            Debug.Log("[MobileInputAlphaSave] Set current alpha to: " + CurrentAlpha);
            EditorUtility.SetDirty(this);
        }

        [ContextMenu("Find And Remove Child Buttons", false, 1000300)]
        public void FindAndRemoveChildButtons()
        {
            Button[] components = GetComponentsInChildren<Button>(true);
            for (int i = components.Length - 1; i >= 0; --i)
            {
                Destroy(components[i]);
                Debug.Log(components[i] + " removed");
            }
            EditorUtility.SetDirty(this);
        }

        [ContextMenu("Find And Remove Child Toggles", false, 1000301)]
        public void FindAndRemoveChildToggles()
        {
            Toggle[] components = GetComponentsInChildren<Toggle>(true);
            for (int i = components.Length - 1; i >= 0; --i)
            {
                Destroy(components[i]);
                Debug.Log(components[i] + " removed");
            }
            EditorUtility.SetDirty(this);
        }

        [ContextMenu("Find And Remove Child Dropdowns", false, 1000302)]
        public void FindAndRemoveChildDropdowns()
        {
            Dropdown[] components = GetComponentsInChildren<Dropdown>(true);
            for (int i = components.Length - 1; i >= 0; --i)
            {
                Destroy(components[i]);
                Debug.Log(components[i] + " removed");
            }
            EditorUtility.SetDirty(this);
        }

        [ContextMenu("Find And Disable Graphic Raycasting", false, 1000303)]
        public void FindAndDisableGraphicRaycasting()
        {
            Graphic[] components = GetComponentsInChildren<Graphic>(true);
            for (int i = components.Length - 1; i >= 0; --i)
            {
                if (components[i].gameObject != gameObject)
                {
                    components[i].raycastTarget = false;
                    Debug.Log(components[i] + " raycastTarget = false");
                }
                else
                {
                    components[i].raycastTarget = true;
                    Debug.Log(components[i] + " raycastTarget = true");
                }
            }
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

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isEditMode)
                return;
            // Tell manager to edit this
            MobileInputConfigManager.Instance.SelectMobileInput(this);
        }
    }
}
