using System.Threading;

using Cysharp.Threading.Tasks;

using Sirenix.OdinInspector;

using UnityEngine;

public class TransferAnimator : MonoBehaviour
{
    [LabelText("背包根节点")]
    [SerializeField] private Transform _backpackRoot;

    // ── 公开 API ─────────────────────────────────────────

    /// <summary>
    /// 播放从仓库 Port 飞向背包根节点的抛物线动画。
    /// 动画完成后 UniTask 正常结束；token 取消时球体立即销毁并抛出 OperationCanceledException。
    /// </summary>
    public UniTask PlayPickupAsync(ResourceConfig resourceConfig, Transform fromPort, CancellationToken token)
    {
        return FlyAsync(
            resourceConfig.BackpackItemPrefab,
            () => fromPort.position,
            () => _backpackRoot.position,
            CommonConfigSO.Instance.TransferInterval,
            token);
    }

    /// <summary>
    /// 播放从背包根节点飞向仓库 Port 的抛物线动画。
    /// </summary>
    public UniTask PlayDeliveryAsync(ResourceConfig resourceConfig, Transform toPort, CancellationToken token)
    {
        return FlyAsync(
            resourceConfig.BackpackItemPrefab,
            () => _backpackRoot.position,
            () => toPort.position,
            CommonConfigSO.Instance.TransferInterval,
            token);
    }

    // ── 私有方法 ──────────────────────────────────────────

    private async UniTask FlyAsync(
        GameObject prefab,
        System.Func<Vector3> getStart,
        System.Func<Vector3> getEnd,
        float duration,
        CancellationToken token)
    {
        GameObject go = Instantiate(prefab);   // 场景根下，不跟随任何父节点
        float arcHeight = CommonConfigSO.Instance.TransferArcHeight;

        try
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                token.ThrowIfCancellationRequested();

                float t = elapsed / duration;

                Vector3 p0 = getStart();
                Vector3 p2 = getEnd();
                Vector3 mid = (p0 + p2) * 0.5f + Vector3.up * arcHeight;

                // 二次贝塞尔：P(t) = (1-t)²·P0 + 2(1-t)t·P1 + t²·P2
                float u = 1f - t;
                go.transform.position = u * u * p0 + 2f * u * t * mid + t * t * p2;

                await UniTask.Yield(PlayerLoopTiming.Update, token);
                elapsed += Time.deltaTime;
            }

            // 最终落点精确对齐终点
            go.transform.position = getEnd();
        }
        finally
        {
            // 无论正常结束还是取消，都销毁球体
            if (go != null)
                Destroy(go);
        }
    }
}
