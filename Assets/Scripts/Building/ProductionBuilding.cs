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

    // 复用列表，避免在循环中分配
    private List<ResourceConfig> _animResourceConfigs = new List<ResourceConfig>();
    private List<int> _animCounts = new List<int>();

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
            await UniTask.Delay(TimeSpan.FromSeconds(_config.ProductionInterval), cancellationToken: token);

            await TryProduceAsync(token);
        }
    }

    private async UniTask TryProduceAsync(System.Threading.CancellationToken token)
    {
        // 检查停产条件：输出仓库已满
        if (_outputWarehouse.IsFull)
        {
            NotifyStopped(StopReason.OutputFull);
            return;
        }

        // 检查停产条件：输入资源不足
        if (_config.Inputs != null)
        {
            foreach (InputRequirement req in _config.Inputs)
            {
                if (!_inputWarehouse.HasEnough(req.ResourceId, req.Count))
                {
                    NotifyStopped(StopReason.InputShortage);
                    return;
                }
            }
        }

        // 条件满足，若之前停产则恢复
        if (_isStopped)
        {
            _isStopped = false;
            OnProductionResumed?.Invoke();
            Debug.Log($"[{_config.Name}] Production resumed.");
        }

        // 消耗输入资源并播放「仓库 → 建筑」动画
        if (_config.Inputs != null
         && _config.Inputs.Count > 0)
        {
            _animResourceConfigs.Clear();
            _animCounts.Clear();

            foreach (InputRequirement req in _config.Inputs)
            {
                _inputWarehouse.TryRemove(req.ResourceId, req.Count);
                ResourceConfig resConfig = ResourceConfigSO.Instance.GetById(req.ResourceId);
                if (resConfig != null)
                {
                    _animResourceConfigs.Add(resConfig);
                    _animCounts.Add(req.Count);
                }
            }

            if (_animResourceConfigs.Count > 0)
            {
                await _resourceMover.MoveAsync(_animResourceConfigs, _animCounts, _inputWarehouse.Port, Port, CommonConfigSO.Instance.AnimationDuration);
            }
        }

        // 产出输出资源并播放「建筑 → 仓库」动画
        if (_config.Outputs != null && _config.Outputs.Count > 0)
        {
            _animResourceConfigs.Clear();
            _animCounts.Clear();

            foreach (OutputRequirement req in _config.Outputs)
            {
                _outputWarehouse.TryAdd(req.ResourceId, req.Count);
                ResourceConfig resConfig = ResourceConfigSO.Instance.GetById(req.ResourceId);
                if (resConfig != null)
                {
                    _animResourceConfigs.Add(resConfig);
                    _animCounts.Add(req.Count);
                }
            }

            if (_animResourceConfigs.Count > 0)
            {
                await _resourceMover.MoveAsync(_animResourceConfigs, _animCounts, Port, _outputWarehouse.Port, CommonConfigSO.Instance.AnimationDuration);
            }

            Debug.Log($"[{_config.Name}] Production complete. Output={_outputWarehouse.TotalCount}/{_outputWarehouse.Capacity}");
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