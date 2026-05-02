using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using UnityEngine;

public class ResourceMover : MonoBehaviour
{
    // 将一批资源堆（可并排多种）从 from 移动到 to，完成后回调 onComplete
    // resourceConfigs: 每种资源的配置（决定 Prefab 和 StackSize）
    // counts: 与 resourceConfigs 一一对应，每种资源的数量（用于计算并排偏移）
    public async UniTask MoveAsync(List<ResourceConfig> resourceConfigs, List<int> counts, Transform from,
                                   Transform to, float duration, Action onComplete = null)
    {
        List<GameObject> spawnedObjects = new List<GameObject>();

        // 计算所有堆的总宽度，居中排列
        float totalWidth = 0f;
        for (int i = 0; i < resourceConfigs.Count; i++)
        {
            totalWidth += resourceConfigs[i].StackSize.x * counts[i];
        }

        float offsetX = -totalWidth / 2f;
        for (int i = 0; i < resourceConfigs.Count; i++)
        {
            ResourceConfig config = resourceConfigs[i];
            float stackWidth = config.StackSize.x;

            for (int j = 0; j < counts[i]; j++)
            {
                float localX = offsetX + stackWidth * j + stackWidth / 2f;
                Vector3 localOffset = new Vector3(localX, 0f, 0f);

                GameObject obj = Instantiate(config.StackPrefab, from.position + localOffset, Quaternion.identity);
                spawnedObjects.Add(obj);
            }

            offsetX += stackWidth * counts[i];
        }

        // Lerp 所有对象从 from 到 to
        float elapsed = 0f;
        Vector3 startPos = from.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 currentTo = to.position;
            for (int i = 0; i < spawnedObjects.Count; i++)
            {
                if (spawnedObjects[i] == null) continue;
                spawnedObjects[i].transform.position = Vector3.Lerp(startPos, currentTo, t);
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