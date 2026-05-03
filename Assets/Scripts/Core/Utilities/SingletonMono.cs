using UnityEngine;

namespace Core.Utilities
{
    /// <summary>
    /// MonoBehaviour 单例基类。仅 GameManager 使用。
    /// 如果未来有其他需要挂载到 GameObject 的 Manager 也可继承。
    /// 
    /// 自动处理:
    /// - 重复实例销毁
    /// </summary>
    public abstract class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
    {
        public static T Instance { get; private set; }
        public static bool HasInstance => Instance != null;

        protected virtual void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this as T;
            OnAwake();
        }

        /// <summary>
        /// 子类重写此方法替代 Awake，避免忘记调用 base.Awake()。
        /// </summary>
        protected virtual void OnAwake()
        {
        }
    }
}