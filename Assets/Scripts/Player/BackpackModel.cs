using System;
using System.Collections.Generic;

using UnityEngine;

public class BackpackModel : MonoBehaviour
{
    public event Action OnChanged;

    public int Capacity { get; private set; }
    public int TotalCount { get; private set; }
    public bool IsFull => TotalCount >= Capacity;

    private Dictionary<int, int> _items = new Dictionary<int, int>();

    private void Awake()
    {
        Capacity = CommonConfigSO.Instance.BackpackCapacity;
    }

    public bool TryAdd(int resourceId, int amount)
    {
        if (amount <= 0) return false;
        if (TotalCount + amount > Capacity) return false;

        if (_items.ContainsKey(resourceId))
        {
            _items[resourceId] += amount;
        }
        else
        {
            _items[resourceId] = amount;
        }

        TotalCount += amount;
        OnChanged?.Invoke();
        return true;
    }

    public bool TryRemove(int resourceId, int amount)
    {
        if (amount <= 0) return false;
        if (!_items.ContainsKey(resourceId)) return false;
        if (_items[resourceId] < amount) return false;

        _items[resourceId] -= amount;
        if (_items[resourceId] == 0)
        {
            _items.Remove(resourceId);
        }

        TotalCount -= amount;
        OnChanged?.Invoke();
        return true;
    }

    public IReadOnlyDictionary<int, int> GetAllItems()
    {
        return _items;
    }
}
