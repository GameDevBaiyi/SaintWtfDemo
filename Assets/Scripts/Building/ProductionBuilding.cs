using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using Sirenix.OdinInspector;

using UnityEngine;

public class ProductionBuilding : MonoBehaviour
{
    [LabelText("出入口")]
    [SerializeField] public Transform Port;

    [LabelText("建筑配置 ID")]
    [SerializeField] private int _configId;
    [LabelText("输入仓库")]
    [SerializeField] private Warehouse _inputWarehouse;
    [LabelText("输出仓库")]
    [SerializeField] private Warehouse _outputWarehouse;
    [LabelText("资源移动器")]
    [SerializeField] private ResourceMover _resourceMover;

    public event Action<StopReason> OnProductionStopped;
    public event Action OnProductionResumed;

    private BuildingConfig _config;
    private bool _isStopped;
    private StopReason _currentStopReason;

    // 复用列表，避免在循环中分配（输入/输出动画各持独立列表，防止并发覆盖）
    private List<ResourceConfig> _inputAnimConfigs = new List<ResourceConfig>();
    private List<int> _inputAnimCounts = new List<int>();
    private List<ResourceConfig> _outputAnimConfigs = new List<ResourceConfig>();
    private List<int> _outputAnimCounts = new List<int>();

    private void Start()
    {
        _config = BuildingConfigSO.Instance.GetById(_configId);
        if (_config == null)
        {
            Debug.LogError($"[ProductionBuilding] Config not found for Id={_configId}, stopping.", this);
            return;
        }

        // 初始化仓库（方案B：仓库支持多种资源类型，不绑定单一 ResourceId）
        int capacity = CommonConfigSO.Instance.WarehouseCapacity;
        _inputWarehouse.Init(capacity);
        _outputWarehouse.Init(capacity);

        RunProductionLoopAsync().Forget();
    }

    private async UniTaskVoid RunProductionLoopAsync()
    {
        System.Threading.CancellationToken token = this.GetCancellationTokenOnDestroy();

        while (!token.IsCancellationRequested)
        {
            // 周期开头：检查能否生产
            if (!CanProduce(out StopReason stopReason))
            {
                NotifyStopped(stopReason);
                await UniTask.Delay(TimeSpan.FromSeconds(_config.ProductionInterval), cancellationToken: token);
                continue;
            }

            // 条件满足，若之前停产则恢复
            if (_isStopped)
            {
                _isStopped = false;
                OnProductionResumed?.Invoke();
                Debug.Log($"[{_config.Name}] Production resumed.");
            }

            // 周期开头：播放「仓库 → 建筑」动画（纯视觉，与生产等待并行，暂不扣资源）
            PlayInputAnimAsync().Forget();

            // 等待整个生产周期
            await UniTask.Delay(TimeSpan.FromSeconds(_config.ProductionInterval), cancellationToken: token);

            // 周期结尾：实际扣除输入资源
            if (_config.Inputs != null)
            {
                foreach (InputRequirement req in _config.Inputs)
                {
                    _inputWarehouse.TryRemove(req.ResourceId, req.Count);
                }
            }

            // 周期结尾：写入输出资源
            if (_config.Outputs != null)
            {
                foreach (OutputRequirement req in _config.Outputs)
                {
                    _outputWarehouse.TryAdd(req.ResourceId, req.Count);
                }
            }

            Debug.Log($"[{_config.Name}] Production complete. Output={_outputWarehouse.TotalCount}/{_outputWarehouse.Capacity}");

            // 周期结尾：播放「建筑 → 仓库」动画（fire-and-forget，不阻塞下一周期）
            PlayOutputAnimAsync().Forget();
        }
    }

    private bool CanProduce(out StopReason stopReason)
    {
        if (_outputWarehouse.IsFull)
        {
            stopReason = StopReason.OutputFull;
            return false;
        }

        if (_config.Inputs != null)
        {
            foreach (InputRequirement req in _config.Inputs)
            {
                if (!_inputWarehouse.HasEnough(req.ResourceId, req.Count))
                {
                    stopReason = StopReason.InputShortage;
                    return false;
                }
            }
        }

        stopReason = default;
        return true;
    }

    // 周期开头动画：仓库 → 建筑（fire-and-forget，与生产等待并行）
    private async UniTaskVoid PlayInputAnimAsync()
    {
        if (_config.Inputs == null
         || _config.Inputs.Count == 0) return;

        _inputAnimConfigs.Clear();
        _inputAnimCounts.Clear();

        foreach (InputRequirement req in _config.Inputs)
        {
            ResourceConfig resConfig = ResourceConfigSO.Instance.GetById(req.ResourceId);
            if (resConfig != null)
            {
                _inputAnimConfigs.Add(resConfig);
                _inputAnimCounts.Add(req.Count);
            }
        }

        if (_inputAnimConfigs.Count > 0)
        {
            await _resourceMover.MoveAsync(_inputAnimConfigs, _inputAnimCounts, _inputWarehouse.Port, Port, CommonConfigSO.Instance.ResourceMoveSpeed);
        }
    }

    // 周期结尾动画：建筑 → 仓库（fire-and-forget，不阻塞下一周期）
    private async UniTaskVoid PlayOutputAnimAsync()
    {
        if (_config.Outputs == null
         || _config.Outputs.Count == 0) return;

        _outputAnimConfigs.Clear();
        _outputAnimCounts.Clear();

        foreach (OutputRequirement req in _config.Outputs)
        {
            ResourceConfig resConfig = ResourceConfigSO.Instance.GetById(req.ResourceId);
            if (resConfig != null)
            {
                _outputAnimConfigs.Add(resConfig);
                _outputAnimCounts.Add(req.Count);
            }
        }

        if (_outputAnimConfigs.Count > 0)
        {
            await _resourceMover.MoveAsync(_outputAnimConfigs, _outputAnimCounts, Port, _outputWarehouse.Port,
                                           CommonConfigSO.Instance.ResourceMoveSpeed);
        }
    }

    private void NotifyStopped(StopReason reason)
    {
        if (_isStopped && _currentStopReason == reason) return;

        _isStopped = true;
        _currentStopReason = reason;
        OnProductionStopped?.Invoke(reason);
        Debug.Log($"[{_config.Name}] Production stopped: {reason}");
    }
}

public enum StopReason
{
    InputShortage,
    OutputFull
}