using Sirenix.OdinInspector;

using UnityEngine;

/// <summary>
/// 临时手动测试入口，挂在场景任意 GameObject 上，通过 Inspector 按钮驱动。
/// 测试完毕后可直接删除此文件。
/// </summary>
public class TestSomething : MonoBehaviour
{
    [Title("背包测试")]

    [LabelText("目标背包")]
    [SerializeField] private BackpackModel _backpackModel;

    [LabelText("资源 Id")]
    [SerializeField] private int _resourceId = 1;

    [LabelText("数量")]
    [SerializeField] private int _amount = 1;

    // ── 操作 ──────────────────────────────────────────

    [Button("TryAdd")]
    private void TestTryAdd()
    {
        bool result = _backpackModel.TryAdd(_resourceId, _amount);
        Debug.Log($"[Test] TryAdd(id={_resourceId}, amount={_amount}) => {result} | Total={_backpackModel.TotalCount}/{_backpackModel.Capacity}");
    }

    [Button("TryRemove")]
    private void TestTryRemove()
    {
        bool result = _backpackModel.TryRemove(_resourceId, _amount);
        Debug.Log($"[Test] TryRemove(id={_resourceId}, amount={_amount}) => {result} | Total={_backpackModel.TotalCount}/{_backpackModel.Capacity}");
    }

    [Button("Print All Items")]
    private void TestPrintAll()
    {
        System.Collections.Generic.IReadOnlyDictionary<int, int> items = _backpackModel.GetAllItems();
        if (items.Count == 0)
        {
            Debug.Log("[Test] 背包为空");
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder("[Test] 背包内容: ");
        foreach (System.Collections.Generic.KeyValuePair<int, int> entry in items)
        {
            ResourceConfig config = ResourceConfigSO.Instance.GetById(entry.Key);
            string name = config != null ? config.Name : entry.Key.ToString();
            sb.Append($"{name}×{entry.Value}  ");
        }
        Debug.Log(sb.ToString());
    }

    [Button("Test Overflow (一次性填满再多加1)")]
    private void TestOverflow()
    {
        int remaining = _backpackModel.Capacity - _backpackModel.TotalCount;
        if (remaining > 0)
        {
            _backpackModel.TryAdd(_resourceId, remaining);
            Debug.Log($"[Test] 已填满背包 Total={_backpackModel.TotalCount}/{_backpackModel.Capacity}");
        }

        bool result = _backpackModel.TryAdd(_resourceId, 1);
        Debug.Log($"[Test] 满载后再 TryAdd(1) => {result}（期望 false）");
    }
}
