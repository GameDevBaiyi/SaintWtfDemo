using Core.Utilities;

namespace Core
{
    public class GameManager : SingletonMono<GameManager>
    {
        protected override void OnAwake() { }

        private void Start()
        {
            BuildingManager.Instance.Init();
            DynamicUIManager.Instance.InitUIs(BuildingManager.Instance.Buildings);
        }
    }
}
