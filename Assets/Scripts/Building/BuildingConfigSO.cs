using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingConfigSO", menuName = "Config/BuildingConfigSO")]
public class BuildingConfigSO : ScriptableObject
{
    private const string ResourcesPath = "BuildingConfigSO";

    private static BuildingConfigSO _instance;
    public static BuildingConfigSO Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<BuildingConfigSO>(ResourcesPath);
            if (_instance == null)
                Debug.LogError($"[BuildingConfigSO] Asset not found at Resources/{ResourcesPath}");
            return _instance;
        }
    }

    [LabelText("建筑列表")]
    [SerializeField] private List<BuildingConfig> _buildings = new List<BuildingConfig>();

    public BuildingConfig GetById(int id)
    {
        foreach (BuildingConfig config in _buildings)
        {
            if (config.Id == id) return config;
        }
        Debug.LogError($"[BuildingConfigSO] BuildingConfig not found for Id={id}");
        return null;
    }
}

[Serializable]
public class BuildingConfig
{
    [LabelText("ID")]
    public int Id;
    [LabelText("名称")]
    public string Name;
    [LabelText("输入需求")]
    public List<InputRequirement> Inputs;
    [LabelText("输出需求")]
    public List<OutputRequirement> Outputs;
    [Tooltip("每次生产的间隔时间（秒）")]
    [LabelText("生产间隔")]
    public float ProductionInterval;
}

[Serializable]
public class InputRequirement
{
    [LabelText("资源 ID")]
    public int ResourceId;
    [LabelText("数量")]
    public int Count;
}

[Serializable]
public class OutputRequirement
{
    [LabelText("资源 ID")]
    public int ResourceId;
    [LabelText("数量")]
    public int Count;
}