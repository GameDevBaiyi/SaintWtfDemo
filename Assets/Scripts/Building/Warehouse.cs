using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;
using UnityEngine;

public class Warehouse : MonoBehaviour
{
    [LabelText("出入口")]
    [SerializeField] public Transform Port;

    public int Capacity { get; private set; }
    public int TotalCount { get; private set; }

    public bool IsFull => TotalCount >= Capacity;

    private Dictionary<int, int> _stocks = new Dictionary<int, int>();
    public IReadOnlyDictionary<int, int> GetAllStocks() => _stocks;

    public event Action OnChanged;

    public void Init(int capacity)
    {
        Capacity = capacity;
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