# Architecture — SAINT.WTF Unity Test Task

> 项目快照 + 关键决策记录。  
> 每完成一个模块后更新 Folder Structure(和Modules对应) 状态；做出重要决策时在 Key Decisions 追加一行。

---

## Project Overview

**类型**：3D 资源运输模拟（Mobile，参考 Moon Pioneer）  
**引擎**：Unity 6000.3.13f1 (LTS)  
**评分重点**：代码架构 > 画面效果  
**核心循环**：建筑生产资源 → 玩家拾取 → 玩家搬运至目标建筑 → 建筑消耗并生产下一级资源

---

## Folder Structure

```
Assets/
├── Scripts/
│   ├── Resource/          # 资源动画堆叠显示
│   ├── Building/          # 建筑生产循环、仓库数据、资源搬运动画、场景建筑管理
│   ├── Config/            # 全局配置 ScriptableObject 脚本定义
│   ├── Core/              # 游戏入口与生命周期管理
│   │   └── Utilities/     # 通用单例基类
│   ├── UI/                # 浮动世界 UI、建筑状态提示、仓库库存显示
│   └── Player/            # 玩家移动、背包数据与显示、仓库交互、转移动画
├── Resources/             # ConfigSO 数据实例（通过 Resources.Load 懒加载，无需 Inspector 引用）
│   ├── CommonConfigSO.asset
│   ├── ResourceConfigSO.asset
│   └── BuildingConfigSO.asset
└── Plugins/               # 第三方库（UniTask, Joystick Pack, Odin Inspector, Cinemachine）
```

---

## Third-Party Plugins

| 插件 | 用途 |
|---|---|
| **Joystick Pack** | 虚拟摇杆输入（移动端） |
| **Odin Inspector** | Inspector 扩展，简化 ScriptableObject 配置编辑 |
| **UniTask** | async/await 异步方案，替代 Coroutine |
| **Cinemachine** | 虚拟摄像机跟随角色 |

---

## Key Design Decisions

> 只追加，不改写旧条目。

- **资源配置用 ScriptableObject，不引入 Excel + 导表流程。** 导表方案（如 ExcelDataReader / Luban）更适合大型项目多人协作；此项目配置量小、无多人编辑需求，用 SO 直接在 Inspector 编辑即可，选择以匹配规模为优先，避免过度设计。

- **通知用 C# event，不用全局 EventBus / 消息中间件。** EventBus 在需要解耦跨模块通信时有价值，但会带来隐式依赖和调试难度；此项目订阅关系固定且局部，直接 event 更清晰，刻意克制抽象。

- **异步方案用 UniTask（async/await），不用 Unity Coroutine。** Coroutine 无法返回值、异常处理繁琐、取消机制弱；UniTask 是 Unity 现代异步的标准选择。

- **背包用 MVC 分层：`BackpackModel`（纯数据 + event）+ `BackpackView`（订阅重绘）。** 刻意分离关注点：Model 可单独测试，View 可替换（如改为格子 UI 只需换 View），符合单一职责原则。

- **背包 `BackpackView` 用 `UnityEngine.Pool.ObjectPool<GameObject>`，每种 ResourceId 一个独立池。** 原方案每次 `Refresh` 全量 `Destroy + Instantiate`，GC 压力随背包容量线性增长；Pool 方案在容量稳定后 `Refresh` 零 GC，符合移动端性能标准。

- **浮动 UI 用 Screen Space Overlay Canvas + 世界坐标→屏幕坐标转换，由 `DynamicUIManager` 单例统一管理锚点跟踪。** World Space Canvas 方案需处理遮挡和缩放，Overlay 方案渲染开销更低、实现更稳定，`LateUpdate` 驱动位置更新保证跟随无延迟。

- **生产动画时序：输入动画 fire-and-forget 与等待并行；输出动画 await，完成后才写入数据。** 有意区分"纯视觉"和"数据驱动"两类动画——输入动画不影响逻辑，可并行；输出动画决定数据写入时机，必须 await，保证"球体落入仓库"与"数据变化"视觉一致。

- **玩家移动用 `CharacterController`，不用 Rigidbody。** Rigidbody 在移动类游戏中需处理摩擦、碰撞响应等物理副作用；CharacterController 完全代码控制，行为可预期，是 Action/模拟类游戏的常规选择。

- **摄像机用 Cinemachine Virtual Camera，不自写跟随脚本。** 自写跟随两行即可实现，但 Cinemachine 提供阻尼、边界、镜头切换等扩展能力；此处用 Cinemachine 是为了保留扩展空间，非能力限制。

- **`WarehouseInteractor`：进入 Trigger 后先等一个 `TransferInterval` 再开始；临时无法转移时 `UniTask.Yield()` 让出一帧重试，不重置 interval。** 避免"刚进入就立即转移"的体验问题；重试不重置 interval 使节奏稳定，不因库存波动导致节奏抖动。

- **生产动画时序精确化：等待时长 = `ProductionInterval - outputAnimDuration`，总周期精确等于 `ProductionInterval`。** 若直接 `await 动画` 再 `await ProductionInterval`，总时长会超出；此设计提前扣除动画耗时，保证视觉节奏与配置值严格一致，并在 `Init()` 时校验，防止配置错误导致死锁。

- **转移动画用二次贝塞尔抛物线，弧高可配置；目标位置每帧实时取 world position。** 固定弧顶的线性插值在玩家移动时轨迹会漂移；实时取终点位置使动画在玩家移动时自然跟随，无需任何额外逻辑。

- **初始化链在 `Awake` 阶段完成（`GameManager → BuildingManager → ProductionBuilding → Warehouse → DynamicUIManager`），不依赖 Script Execution Order 设置。** SEO 是隐式配置，多人协作时易遗漏；显式初始化链将顺序依赖变为代码中可读的调用顺序，更可维护，也彻底消除了 `Start` 阶段的时序竞争问题。



