using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

public class BackpackView : MonoBehaviour
{
    [LabelText("背包数据")]
    [SerializeField] private BackpackModel _backpackModel;

    [Tooltip("背包显示的世界坐标根节点（角色背后的子 Transform）")]
    [LabelText("背包根节点")]
    [SerializeField] private Transform _backpackRoot;

    [Tooltip("球体间距（建议 0.35，略大于球直径 0.3）")]
    [LabelText("球体间距")]
    [SerializeField] private float _spacing = 0.35f;

    private void Start()
    {
        _backpackModel.OnChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (_backpackModel != null)
        {
            _backpackModel.OnChanged -= Refresh;
        }
    }

    private void Refresh()
    {
        // 销毁所有旧子物体
        for (int i = _backpackRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(_backpackRoot.GetChild(i).gameObject);
        }

        int maxPerColumn = CommonConfigSO.Instance.BackpackMaxPerColumn;
        IReadOnlyDictionary<int, int> items = _backpackModel.GetAllItems();

        // 收集并按 ResourceId 升序排列
        List<int> sortedIds = new List<int>(items.Keys);
        sortedIds.Sort();

        int slotIndex = 0;
        foreach (int resourceId in sortedIds)
        {
            ResourceConfig config = ResourceConfigSO.Instance.GetById(resourceId);
            if (config == null) continue;
            if (config.BackpackItemPrefab == null)
            {
                Debug.LogWarning($"[BackpackView] BackpackItemPrefab not set for ResourceId={resourceId}");
                continue;
            }

            int count = items[resourceId];
            for (int i = 0; i < count; i++)
            {
                int col = slotIndex / maxPerColumn;
                int row = slotIndex % maxPerColumn;

                Vector3 localPos = Vector3.back * col * _spacing + Vector3.up * row * _spacing;

                GameObject item = Object.Instantiate(config.BackpackItemPrefab, _backpackRoot);
                item.transform.localPosition = localPos;

                slotIndex++;
            }
        }
    }
}
