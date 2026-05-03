using Core.Utilities;

namespace Core
{
    public class GameManager : SingletonMono<GameManager>
    {
        protected override void OnAwake()
        {
            BuildingManager.Instance.Init();
            DynamicUIManager.Instance.InitUIs(BuildingManager.Instance.Buildings);
        }
    }
}
