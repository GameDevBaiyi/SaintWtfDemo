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

    [Tooltip("资源运送速度（单位/秒），时长由距离÷速度自动计算")]
    [LabelText("运送速度")]
    [SerializeField] private float _resourceMoveSpeed = 5f;
    public float ResourceMoveSpeed => _resourceMoveSpeed;

    [Tooltip("玩家移动速度（单位/秒）")]
    [LabelText("玩家移动速度")]
    [SerializeField] private float _playerMoveSpeed = 5f;
    public float PlayerMoveSpeed => _playerMoveSpeed;

    [Tooltip("玩家朝向旋转速度（度/秒，Slerp t 系数）")]
    [LabelText("玩家旋转速度")]
    [SerializeField] private float _playerRotateSpeed = 10f;
    public float PlayerRotateSpeed => _playerRotateSpeed;

    [LabelText("背包容量")]
    [SerializeField] private int _backpackCapacity = 10;
    public int BackpackCapacity => _backpackCapacity;

    [Tooltip("背包每列最大 Y 轴堆叠数量")]
    [LabelText("背包每列容量")]
    [SerializeField] private int _backpackMaxPerColumn = 3;
    public int BackpackMaxPerColumn => _backpackMaxPerColumn;
}
