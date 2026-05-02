using System.Collections.Generic;
using System.Threading;

using Cysharp.Threading.Tasks;

using Sirenix.OdinInspector;

using UnityEngine;

public class WarehouseInteractor : MonoBehaviour
{
    [LabelText("玩家背包")]
    [SerializeField] private BackpackModel _backpackModel;

    [LabelText("转移动画")]
    [SerializeField] private TransferAnimator _transferAnimator;

    private CancellationTokenSource _cts;

    private void OnTriggerEnter(Collider other)
    {
        PickupZone pickupZone = other.GetComponent<PickupZone>();
        if (pickupZone != null)
        {
            Warehouse warehouse = other.GetComponentInParent<Warehouse>();
            if (warehouse == null) return;

            StartTransfer();
            PickupLoopAsync(warehouse, _cts.Token).Forget();
            return;
        }

        DeliveryZone deliveryZone = other.GetComponent<DeliveryZone>();
        if (deliveryZone != null)
        {
            Warehouse warehouse = other.GetComponentInParent<Warehouse>();
            if (warehouse == null) return;

            StartTransfer();
            DeliveryLoopAsync(warehouse, _cts.Token).Forget();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PickupZone>() != null || other.GetComponent<DeliveryZone>() != null)
        {
            CancelTransfer();
        }
    }

    private void OnDestroy()
    {
        CancelTransfer();
    }

    // ── 私有方法 ──────────────────────────────────────────

    private void StartTransfer()
    {
        // 取消前一个（多区域冲突）
        CancelTransfer();
        _cts = new CancellationTokenSource();
    }

    private void CancelTransfer()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }

    private async UniTaskVoid PickupLoopAsync(Warehouse warehouse, CancellationToken token)
    {
        IReadOnlyList<int> acceptedIds = warehouse.AcceptedResourceIds;

        while (!token.IsCancellationRequested)
        {
            if (_backpackModel.IsFull) break;                 // 背包满，终止

            // 找到仓库中有库存的第一种资源
            int resourceId = -1;
            foreach (int id in acceptedIds)
            {
                if (warehouse.HasEnough(id, 1)) { resourceId = id; break; }
            }

            if (resourceId == -1)                            // 仓库暂无库存，下帧重试
            {
                await UniTask.Yield(token);
                continue;
            }

            ResourceConfig config = ResourceConfigSO.Instance.GetById(resourceId);

            // 动画先行：飞行结束后才写入数据；token 取消时球体销毁，异常退出循环
            await _transferAnimator.PlayPickupAsync(config, warehouse.Port, token);

            warehouse.TryRemove(resourceId, 1);
            _backpackModel.TryAdd(resourceId, 1);
        }
    }

    private async UniTaskVoid DeliveryLoopAsync(Warehouse warehouse, CancellationToken token)
    {
        IReadOnlyList<int> acceptedIds = warehouse.AcceptedResourceIds;

        while (!token.IsCancellationRequested)
        {
            if (_backpackModel.IsEmpty) break;                // 背包空，终止

            // 找到背包中第一种仓库接受的资源
            IReadOnlyDictionary<int, int> items = _backpackModel.GetAllItems();
            int deliverId = -1;
            foreach (int id in acceptedIds)
            {
                if (items.TryGetValue(id, out int cnt) && cnt > 0) { deliverId = id; break; }
            }

            if (deliverId == -1) break;                      // 背包没有仓库接受的资源，终止

            if (warehouse.IsFull)                            // 仓库满，下帧重试
            {
                await UniTask.Yield(token);
                continue;
            }

            ResourceConfig config = ResourceConfigSO.Instance.GetById(deliverId);

            // 动画先行：飞行结束后才写入数据；token 取消时球体销毁，异常退出循环
            await _transferAnimator.PlayDeliveryAsync(config, warehouse.Port, token);

            _backpackModel.TryRemove(deliverId, 1);
            warehouse.TryAdd(deliverId, 1);
        }
    }
}
