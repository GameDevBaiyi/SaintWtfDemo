using System;
using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;
using UnityEngine;

public class Warehouse : MonoBehaviour
{
    [Serializable]
    public struct InitialStock
    {
        [LabelText("资源 ID")]  public int ResourceId;
        [LabelText("初始数量")] public int Amount;
    }

    [LabelText("出入口")]
    [SerializeField] public Transform Port;

    public int Capacity { get; private set; }
    public int TotalCount { get; private set; }
    public bool IsInput { get; private set; }                        // true = 接收投递（输入仓库），false = 提供拾取（输出仓库）
    public IReadOnlyList<int> AcceptedResourceIds { get; private set; } = new List<int>(); // 本仓库接受的资源类型列表

    public bool IsFull => TotalCount >= Capacity;

    private Dictionary<int, int> _stocks = new Dictionary<int, int>();
    public IReadOnlyDictionary<int, int> GetAllStocks() => _stocks;

    public event Action OnChanged;

    public void Init(int capacity, bool isInput, List<int> acceptedResourceIds)
    {
        Capacity = capacity;
        IsInput = isInput;
        AcceptedResourceIds = acceptedResourceIds ?? new List<int>();
        TotalCount = 0;
        _stocks.Clear();
    }

    public int GetCount(int resourceId)
    {
        _stocks.TryGetValue(resourceId, out int count);
        return count;
    }

    public bool HasEnough(int resourceId, int amount)
    {
        return GetCount(resourceId) >= amount;
    }

    public bool TryAdd(int resourceId, int amount)
    {
        if (!AcceptedResourceIds.Contains(resourceId))
        {
            Debug.LogWarning($"[Warehouse] '{name}' 拒绝资源 ID={resourceId}，不在接受列表中。", this);
            return false;
        }
        if (TotalCount + amount > Capacity) return false;
        if (!_stocks.ContainsKey(resourceId)) _stocks[resourceId] = 0;
        _stocks[resourceId] += amount;
        TotalCount += amount;
        OnChanged?.Invoke();
        return true;
    }

    public bool TryRemove(int resourceId, int amount)
    {
        if (!HasEnough(resourceId, amount)) return false;
        _stocks[resourceId] -= amount;
        TotalCount -= amount;
        OnChanged?.Invoke();
        return true;
    }
}