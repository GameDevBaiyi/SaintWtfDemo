using System.Collections.Generic;

using Core.Utilities;

using Sirenix.OdinInspector;

using UnityEngine;

public class BuildingManager : SingletonMono<BuildingManager>
{
    [LabelText("场景内所有生产建筑")]
    [SerializeField] private List<ProductionBuilding> _buildings = new List<ProductionBuilding>();

    public IReadOnlyList<ProductionBuilding> Buildings => _buildings;

    public void Init()
    {
        foreach (ProductionBuilding building in _buildings)
        {
            building.Init();
        }
    }
}
