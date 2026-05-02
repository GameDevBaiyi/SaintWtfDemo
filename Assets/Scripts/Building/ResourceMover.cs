using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using UnityEngine;

public class ResourceMover : MonoBehaviour
{
    private const float StackZSpacing = 0.3f;

    // 将一批资源堆（可并排多种）从 from 移动到 to，完成后回调 onComplete
    // resourceConfigs: 每种资源的配置（决定 Prefab）
    // counts: 与 resourceConfigs 一一对应，每种资源的数量
    // moveSpeed: 运动速度（单位/秒），时长由距离÷速度自动计算
    public async UniTask MoveAsync(List<ResourceConfig> resourceConfigs, List<int> counts, Transform from,
                                   Transform to, float moveSpeed, Action onComplete = null)
    {
        List<GameObject> spawnedObjects = new List<GameObject>();
        List<Vector3> spawnedOffsets = new List<Vector3>();

        // 多堆之间沿 Z 轴偏移 0.3，避免重叠；单堆无偏移
        int totalStacks = 0;
        for (int i = 0; i < counts.Count; i++)
        {
            totalStacks += counts[i];
        }

        float totalZSpan = (totalStacks - 1) * StackZSpacing;
        float startZ = -totalZSpan / 2f;

        int stackIndex = 0;
        for (int i = 0; i < resourceConfigs.Count; i++)
        {
            for (int j = 0; j < counts[i]; j++)
            {
                float localZ = startZ + stackIndex * StackZSpacing;
                Vector3 localOffset = new Vector3(0f, 0f, localZ);

                GameObject obj = Instantiate(resourceConfigs[i].StackPrefab, from.position + localOffset, Quaternion.identity);
                spawnedObjects.Add(obj);
                spawnedOffsets.Add(localOffset);
                stackIndex++;
            }
        }

        // 根据距离和速度计算时长，保证匀速
        Vector3 fromPos = from.position;
        float distance = Vector3.Distance(fromPos, to.position);
        float duration = (moveSpeed > 0f) ? (distance / moveSpeed) : 0f;

        // 每个对象各自从 (from + offset) lerp 到 (to + offset)，保持 Z 偏移全程不变
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 toPos = to.position;
            for (int i = 0; i < spawnedObjects.Count; i++)
            {
                if (spawnedObjects[i] == null) continue;
                spawnedObjects[i].transform.position = Vector3.Lerp(fromPos + spawnedOffsets[i], toPos + spawnedOffsets[i], t);
            }

            await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
        }

        // 销毁动画对象
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            if (spawnedObjects[i] != null)
            {
                Destroy(spawnedObjects[i]);
            }
        }

        onComplete?.Invoke();
    }
}