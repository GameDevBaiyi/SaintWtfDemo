using System.Text;

using TMPro;

using UnityEngine;

public class WarehouseStockUI
{
    private readonly Warehouse _warehouse;
    private TextMeshProUGUI _text;
    private StringBuilder _sb = new StringBuilder();

    public WarehouseStockUI(Warehouse warehouse, DynamicUIManager uiManager)
    {
        _warehouse = warehouse;

        RectTransform panel = uiManager.RegisterAnchor(warehouse.transform);
        GameObject instance = uiManager.InstantiateInPanel(uiManager.StockPrefab, panel);
        _text = instance.GetComponentInChildren<TextMeshProUGUI>(true);

        _warehouse.OnChanged += RefreshText;
        RefreshText();
    }

    public void Dispose()
    {
        if (_warehouse != null)
        {
            _warehouse.OnChanged -= RefreshText;
        }
    }

    private void RefreshText()
    {
        _sb.Clear();
        bool first = true;
        foreach (int resourceId in _warehouse.AcceptedResourceIds)
        {
            if (!first)
            {
                _sb.Append(" ; ");
            }
            first = false;

            ResourceConfig config = ResourceConfigSO.Instance.GetById(resourceId);
            string resourceName = config != null ? config.Name : resourceId.ToString();
            _sb.Append(resourceName);
            _sb.Append(':');
            _sb.Append(_warehouse.GetCount(resourceId));
        }

        _text.text = _sb.ToString();
    }
}