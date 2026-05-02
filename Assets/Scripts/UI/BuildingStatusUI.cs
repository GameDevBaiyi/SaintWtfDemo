using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;

public class BuildingStatusUI : MonoBehaviour
{
    [LabelText("生产建筑")]
    [SerializeField] private ProductionBuilding _building;

    [Tooltip("头顶 UI 的世界坐标锚点（建筑正上方的子 Transform）")]
    [LabelText("头顶锚点")]
    [SerializeField] private Transform _worldAnchor;

    [Tooltip("停产提示的 UI Prefab，需包含 TextMeshProUGUI 组件")]
    [LabelText("停产提示 Prefab")]
    [SerializeField] private GameObject _statusPrefab;

    private TextMeshProUGUI _text;

    private void Start()
    {
        RectTransform panel = DynamicCanvas.Instance.RegisterAnchor(_worldAnchor);

        GameObject instance = Instantiate(_statusPrefab, panel);
        _text = instance.GetComponentInChildren<TextMeshProUGUI>(true);
        _text.gameObject.SetActive(false);

        _building.OnProductionStopped += HandleStopped;
        _building.OnProductionResumed += HandleResumed;
    }

    private void OnDestroy()
    {
        if (_building != null)
        {
            _building.OnProductionStopped -= HandleStopped;
            _building.OnProductionResumed -= HandleResumed;
        }
    }

    private void HandleStopped(StopReason reason)
    {
        _text.text = reason == StopReason.InputShortage ? "缺少输入资源" : "输出仓库已满";
        _text.gameObject.SetActive(true);
    }

    private void HandleResumed()
    {
        _text.gameObject.SetActive(false);
    }
}
