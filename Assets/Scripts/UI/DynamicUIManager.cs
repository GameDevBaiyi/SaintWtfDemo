using System.Collections.Generic;

using Core.Utilities;

using Sirenix.OdinInspector;

using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class DynamicUIManager : SingletonMono<DynamicUIManager>
{
    [LabelText("停产提示 Prefab")]
    [SerializeField] private GameObject _statusPrefab;

    [LabelText("库存显示 Prefab")]
    [SerializeField] private GameObject _stockPrefab;

    public GameObject StatusPrefab => _statusPrefab;
    public GameObject StockPrefab => _stockPrefab;

    private Canvas _canvas;
    private Camera _camera;

    private struct AnchoredPanel
    {
        public Transform WorldAnchor;
        public RectTransform Panel;
    }

    private List<AnchoredPanel> _panels = new List<AnchoredPanel>();
    private List<BuildingStatusUI> _buildingStatusHandlers = new List<BuildingStatusUI>();
    private List<WarehouseStockUI> _warehouseStockHandlers = new List<WarehouseStockUI>();

    protected override void OnAwake()
    {
        _canvas = GetComponent<Canvas>();
        _camera = Camera.main;
    }

    public void InitUIs(IReadOnlyList<ProductionBuilding> buildings)
    {
        foreach (ProductionBuilding building in buildings)
        {
            _buildingStatusHandlers.Add(new BuildingStatusUI(building, this));
            _warehouseStockHandlers.Add(new WarehouseStockUI(building.InputWarehouse, this));
            _warehouseStockHandlers.Add(new WarehouseStockUI(building.OutputWarehouse, this));
        }
    }

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

    public GameObject InstantiateInPanel(GameObject prefab, RectTransform panel)
    {
        return Object.Instantiate(prefab, panel);
    }

    private void OnDestroy()
    {
        foreach (BuildingStatusUI handler in _buildingStatusHandlers)
        {
            handler.Dispose();
        }
        foreach (WarehouseStockUI handler in _warehouseStockHandlers)
        {
            handler.Dispose();
        }
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
