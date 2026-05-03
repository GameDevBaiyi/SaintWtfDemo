using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.Pool;

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

    // 每种 ResourceId 对应一个独立的对象池
    private Dictionary<int, ObjectPool<GameObject>> _pools = new Dictionary<int, ObjectPool<GameObject>>();
    // 当前激活中的球体，Refresh 时先全部归还池
    private List<(int resourceId, GameObject obj)> _activeItems = new List<(int, GameObject)>();
    // 排序复用列表
    private List<int> _sortedIds = new List<int>();

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

        foreach (ObjectPool<GameObject> pool in _pools.Values)
        {
            pool.Dispose();
        }
        _pools.Clear();
    }

    private void Refresh()
    {
        // 将所有激活球归还对应池（SetActive(false) 由 Release 回调处理）
        for (int i = _activeItems.Count - 1; i >= 0; i--)
        {
            (int resourceId, GameObject obj) = _activeItems[i];
            _pools[resourceId].Release(obj);
        }
        _activeItems.Clear();

        int maxPerColumn = CommonConfigSO.Instance.BackpackMaxPerColumn;
        IReadOnlyDictionary<int, int> items = _backpackModel.GetAllItems();

        _sortedIds.Clear();
        foreach (int id in items.Keys)
        {
            _sortedIds.Add(id);
        }
        _sortedIds.Sort();

        int slotIndex = 0;
        foreach (int resourceId in _sortedIds)
        {
            ResourceConfig config = ResourceConfigSO.Instance.GetById(resourceId);
            if (config == null) continue;
            if (config.BackpackItemPrefab == null)
            {
                Debug.LogWarning($"[BackpackView] BackpackItemPrefab not set for ResourceId={resourceId}");
                continue;
            }

            EnsurePool(resourceId, config.BackpackItemPrefab);

            int count = items[resourceId];
            for (int i = 0; i < count; i++)
            {
                int col = slotIndex / maxPerColumn;
                int row = slotIndex % maxPerColumn;

                Vector3 localPos = Vector3.back * col * _spacing + Vector3.up * row * _spacing;

                GameObject item = _pools[resourceId].Get();
                item.transform.localPosition = localPos;

                _activeItems.Add((resourceId, item));
                slotIndex++;
            }
        }
    }

    private void EnsurePool(int resourceId, GameObject prefab)
    {
        if (_pools.ContainsKey(resourceId)) return;

        _pools[resourceId] = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                GameObject go = Object.Instantiate(prefab, _backpackRoot);
                return go;
            },
            actionOnGet: go => go.SetActive(true),
            actionOnRelease: go => go.SetActive(false),
            actionOnDestroy: go => Object.Destroy(go),
            collectionCheck: false
        );
    }
}

