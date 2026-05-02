using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceConfigSO", menuName = "Config/ResourceConfigSO")]
public class ResourceConfigSO : ScriptableObject
{
    private const string ResourcesPath = "ResourceConfigSO";

    private static ResourceConfigSO _instance;
    public static ResourceConfigSO Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<ResourceConfigSO>(ResourcesPath);
            if (_instance == null)
                Debug.LogError($"[ResourceConfigSO] Asset not found at Resources/{ResourcesPath}");
            return _instance;
        }
    }

    [LabelText("资源列表")]
    [SerializeField] private List<ResourceConfig> _resources = new List<ResourceConfig>();

    public ResourceConfig GetById(int id)
    {
        foreach (ResourceConfig config in _resources)
        {
            if (config.Id == id) return config;
        }
        Debug.LogError($"[ResourceConfigSO] ResourceConfig not found for Id={id}");
        return null;
    }
}

[Serializable]
public class ResourceConfig
{
    [LabelText("ID")]
    public int Id;
    [LabelText("名称")]
    public string Name;
    [LabelText("堆叠 Prefab")]
    public GameObject StackPrefab;
    [Tooltip("并排动画时每个堆的占位尺寸（米）")]
    [LabelText("堆叠尺寸")]
    public Vector3 StackSize;
    [Tooltip("背包显示用球形 Prefab")]
    [LabelText("背包球 Prefab")]
    public GameObject BackpackItemPrefab;
}
