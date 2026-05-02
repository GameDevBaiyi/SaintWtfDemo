using System.Collections.Generic;
using System.Text;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;

public class WarehouseStockUI : MonoBehaviour
{
    [LabelText("仓库")]
    [SerializeField] private Warehouse _warehouse;

    [Tooltip("头顶 UI 的世界坐标锚点（仓库正上方的子 Transform）")]
    [LabelText("头顶锚点")]
    [SerializeField] private Transform _worldAnchor;

    [Tooltip("库存显示的 UI Prefab，需包含 TextMeshProUGUI 组件")]
    [LabelText("库存显示 Prefab")]
    [SerializeField] private GameObject _stockPrefab;

    private TextMeshProUGUI _text;
    private StringBuilder _sb = new StringBuilder();

    private void Start()
    {
        RectTransform panel = DynamicCanvas.Instance.RegisterAnchor(_worldAnchor);

        GameObject instance = Instantiate(_stockPrefab, panel);
        _text = instance.GetComponentInChildren<TextMeshProUGUI>(true);

        _warehouse.OnChanged += RefreshText;
        RefreshText();
    }

    private void OnDestroy()
    {
        if (_warehouse != null)
        {
            _warehouse.OnChanged -= RefreshText;
        }
    }

    private void RefreshText()
    {
        IReadOnlyDictionary<int, int> stocks = _warehouse.GetAllStocks();

        _sb.Clear();
        bool first = true;
        foreach (KeyValuePair<int, int> entry in stocks)
        {
            if (!first)
            {
                _sb.Append(" ; ");
            }
            first = false;

            ResourceConfig config = ResourceConfigSO.Instance.GetById(entry.Key);
            string resourceName = config != null ? config.Name : entry.Key.ToString();
            _sb.Append(resourceName);
            _sb.Append(':');
            _sb.Append(entry.Value);
        }

        _text.text = _sb.ToString();
    }
}