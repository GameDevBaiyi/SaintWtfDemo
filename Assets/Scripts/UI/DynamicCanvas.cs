using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class DynamicCanvas : MonoBehaviour
{
    public static DynamicCanvas Instance { get; private set; }

    private Canvas _canvas;
    private Camera _camera;

    private struct AnchoredPanel
    {
        public Transform WorldAnchor;
        public RectTransform Panel;
    }

    private List<AnchoredPanel> _panels = new List<AnchoredPanel>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _canvas = GetComponent<Canvas>();
        _camera = Camera.main;
    }

    // 注册一个世界坐标锚点，返回跟随该锚点的 RectTransform 容器
    // 调用方在容器内自由添加 UI 子元素
    public RectTransform RegisterAnchor(Transform worldAnchor)
    {
        GameObject panelGo = new GameObject("AnchoredPanel", typeof(RectTransform));
        panelGo.transform.SetParent(_canvas.transform, false);

        RectTransform panel = panelGo.GetComponent<RectTransform>();
        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.zero;
        panel.pivot = new Vector2(0.5f, 0f);
        panel.sizeDelta = new Vector2(200f, 80f);

        _panels.Add(new AnchoredPanel { WorldAnchor = worldAnchor, Panel = panel });
        return panel;
    }

    private void LateUpdate()
    {
        RectTransform canvasRect = (RectTransform)_canvas.transform;

        for (int i = _panels.Count - 1; i >= 0; i--)
        {
            AnchoredPanel entry = _panels[i];

            if (entry.WorldAnchor == null || entry.Panel == null)
            {
                _panels.RemoveAt(i);
                continue;
            }

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(_camera, entry.WorldAnchor.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out Vector2 localPoint);
            entry.Panel.localPosition = localPoint;
        }
    }
}
