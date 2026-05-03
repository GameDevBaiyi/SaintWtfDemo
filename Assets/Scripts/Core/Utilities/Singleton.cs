namespace Core.Utilities
{
    /// <summary>
    /// 纯 C# 单例基类。所有非 MonoBehaviour 的 Manager 继承此类。
    /// </summary>
    public abstract class Singleton<T> where T : class, new()
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                _instance ??= new T();
                return _instance;
            }
        }

        public static bool HasInstance => _instance != null;

        public static void SetInstance(T instance)
        {
            _instance = instance;
        }
    }
}