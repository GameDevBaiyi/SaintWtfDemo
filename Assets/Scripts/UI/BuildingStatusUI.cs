using TMPro;

using UnityEngine;

public class BuildingStatusUI
{
    private readonly ProductionBuilding _building;
    private TextMeshProUGUI _text;

    public BuildingStatusUI(ProductionBuilding building, DynamicUIManager uiManager)
    {
        _building = building;

        RectTransform panel = uiManager.RegisterAnchor(building.transform);
        GameObject instance = uiManager.InstantiateInPanel(uiManager.StatusPrefab, panel);
        _text = instance.GetComponentInChildren<TextMeshProUGUI>(true);
        _text.gameObject.SetActive(false);

        _building.OnProductionStopped += HandleStopped;
        _building.OnProductionResumed += HandleResumed;
    }

    public void Dispose()
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
