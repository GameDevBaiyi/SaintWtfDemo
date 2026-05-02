using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

// 在场景中以三轴网格方式生成资源堆
// X / Y / Z 分别控制三个轴的堆叠数量
// 挂在任意 GameObject 上，以该 Transform 为原点居中排列
public class ResourceStackDisplay : MonoBehaviour
{
    [LabelText("资源 Prefab")]
    [SerializeField] private GameObject _resourcePrefab;

    [Title("堆叠数量")]
    [LabelText("X 轴数量")]
    [SerializeField] private int _countX = 5;
    [LabelText("Y 轴数量")]
    [SerializeField] private int _countY = 2;
    [LabelText("Z 轴数量")]
    [SerializeField] private int _countZ = 1;

    [Title("尺寸")]
    [LabelText("单个尺寸（米）")]
    [SerializeField] private float _itemSize = 0.3f;
    [Tooltip("相邻资源之间的间隔距离（米）")]
    [LabelText("间距（米）")]
    [SerializeField] private float _spacing = 0.05f;

    private List<GameObject> _spawnedItems = new List<GameObject>();
    private GameObject _stackRoot;

    [Button("生成资源堆")]
    public void Generate()
    {
        Clear();

        if (_resourcePrefab == null)
        {
            Debug.LogWarning("[ResourceStackDisplay] Resource Prefab is not assigned.");
            return;
        }

        // 创建 ResourceStack 父节点，Pivot 在 XZ 中心、Y 轴最底部
        _stackRoot = new GameObject("ResourceStack");
        _stackRoot.transform.SetParent(transform, false);
        _stackRoot.transform.localPosition = Vector3.zero;
        _stackRoot.transform.localRotation = Quaternion.identity;
        _stackRoot.transform.localScale = Vector3.one;

        float step = _itemSize + _spacing;
        // XZ 居中，Y 从 _itemSize/2 开始（Sphere/Cube pivot 在中心，底面贴 y=0）
        float xOffset = -(_countX - 1) * step / 2f;
        float yStart  = _itemSize / 2f;
        float zOffset = -(_countZ - 1) * step / 2f;

        for (int y = 0; y < _countY; y++)
        {
            for (int z = 0; z < _countZ; z++)
            {
                for (int x = 0; x < _countX; x++)
                {
                    Vector3 localPos = new Vector3(
                        xOffset + x * step,
                        yStart  + y * step,
                        zOffset + z * step);

                    GameObject obj = Instantiate(_resourcePrefab, _stackRoot.transform);
                    obj.transform.localPosition = localPos;
                    obj.transform.localScale = Vector3.one * _itemSize;
                    _spawnedItems.Add(obj);
                }
            }
        }
    }

    [Button("清除资源堆")]
    public void Clear()
    {
        foreach (GameObject obj in _spawnedItems)
        {
            if (obj != null) DestroyImmediate(obj);
        }
        _spawnedItems.Clear();

        if (_stackRoot != null)
        {
            DestroyImmediate(_stackRoot);
            _stackRoot = null;
        }
    }

    public int TotalCount => _countX * _countY * _countZ;
}
