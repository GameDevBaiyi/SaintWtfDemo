using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "CommonConfigSO", menuName = "Config/CommonConfigSO")]
public class CommonConfigSO : ScriptableObject
{
    private const string ResourcesPath = "CommonConfigSO";

    private static CommonConfigSO _instance;
    public static CommonConfigSO Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<CommonConfigSO>(ResourcesPath);
            if (_instance == null)
                Debug.LogError($"[CommonConfigSO] Asset not found at Resources/{ResourcesPath}");
            return _instance;
        }
    }

    [LabelText("仓库容量")]
    [SerializeField] private int _warehouseCapacity = 10;
    public int WarehouseCapacity => _warehouseCapacity;

    [Tooltip("资源 Lerp 动画的持续时间（秒）")]
    [LabelText("动画时长")]
    [SerializeField] private float _animationDuration = 0.5f;
    public float AnimationDuration => _animationDuration;
}
