# C# 代码规范
---

## 1. 命名速查表

| 类别 | 风格 | 示例 |
|------|------|------|
| 命名空间 | PascalCase,与文件夹路径一致 | `Framework.Event` |
| 类 / 结构体 | PascalCase | `EnemySpawner`, `DamageInfo` |
| 接口 | I + PascalCase | `IDamageable` |
| 抽象类 | Base 后缀(可选) | `WeaponBase` |
| 枚举类型 | PascalCase + Enum 后缀(单数) | `WeaponEnum` |
| 异步方法 | Async 后缀 | `SpawnWaveAsync()` |
| 私有/受保护字段 | _camelCase | `_currentHealth` |
| 布尔值 | is/has/can 前缀 | `_isDead`, `HasShield` |
| 事件(C# event) | On + PascalCase | `OnDeath` |
| 事件结构体(EventManager) | PascalCase + Event 后缀 | `PlayerDeathEvent` |
| 委托类型 | PascalCase + Handler | `DamageHandler` |
| ScriptableObject | SO 后缀 | `WeaponConfigSO` |
| Dictionary | _camelCase_camelCase 或 PascalCase_PascalCase | `_key_value`, `Item_Count` |

其余(公共方法、属性、常量、局部变量、参数等)遵循 C# 通用约定,不再赘述。

---

## 2. 代码风格

- 不使用 `var`,始终写明类型。
- `if` / `for` / `foreach` / `while` 单行体也必须加 `{}`，唯一例外：`if (xxx) return;` 可同行。
- 始终显式写出访问修饰符。

---

## 3. 类成员排列顺序

```
1. 常量
2. 静态字段
3. 事件/委托
4. 序列化字段
5. 私有字段
6. 属性
7. Unity 生命周期(按调用顺序)
8. 公共方法(Init / Dispose 在最前)
9. 私有方法
```

---

## 4. 异步编程

- 异步操作优先使用 **UniTask**(`UniTask`, `UniTaskVoid`),不使用 Unity Coroutine
- 异步方法以 `Async` 后缀结尾(见命名速查表)
- 不要用 `async void`,除非是 Unity 事件回调(如 `OnPointerClick`)
- 长生命周期的异步操作传入 `CancellationToken`,组件销毁时取消

---

## 5. 性能关键路径

- 不要在 `Update` / `FixedUpdate` / `LateUpdate` 中分配引用类型
  - 禁止:`new` 任何 class、string 拼接、LINQ 链、lambda 闭包
- 需要临时集合时,用成员变量复用或从对象池取

---

## 6. 属性与字段

### 默认规则
- **内部状态**:`private` 字段,`_camelCase`
- **需要外部可读的值**:自动属性 `public T X { get; private set; }`
- **派生 / 计算值**:expression-bodied property `public bool IsDead => _hp <= 0;`

### 不要使用 public field
Unity 自动序列化所有 `public` 字段,会污染 Inspector 并引入副作用;
外部可写的 field 未来想加逻辑时无法平滑扩展。统一用自动属性代替。

### 示例

推荐:
```csharp
public float MaxHealth { get; private set; } = 100;
public bool IsDead => _currentHealth <= 0;
public int EnemyCount => _enemies.Count;
```

避免:
```csharp
public float MaxHealth = 100;                    // ❌ public field
public float MaxHealth { get { return _hp; } }   // ❌ 完整 getter 对简单值过度
```

### 完整 property 块什么时候才写
只在 setter 需要额外逻辑时:
```csharp
private float _maxHealth = 100;
public float MaxHealth
{
    get => _maxHealth;
    private set
    {
        if (Mathf.Approximately(_maxHealth, value)) return;
        _maxHealth = value;
        OnMaxHealthChanged?.Invoke(value);
    }
}
```

---

## 7. Odin Inspector 标注规则

所有 `[SerializeField]` 字段**必须**配合 Odin 标注，规则如下：

| 情况 | 用法 |
|---|---|
| 字段含义一目了然 | `[LabelText("中文名")]` |
| 字段需要补充说明（单位、作用、注意事项） | `[LabelText("中文名")]` + `[Tooltip("说明")]` |

```csharp
// 简单字段 — 只用 LabelText
[LabelText("仓库容量")]
[SerializeField] private int _warehouseCapacity = 10;

// 需要说明的字段 — LabelText + Tooltip
[Tooltip("资源 Lerp 动画的持续时间（秒）")]
[LabelText("动画时长")]
[SerializeField] private float _animationDuration = 0.5f;
```

- `[LabelText]` 写在 `[SerializeField]` **正上方**紧邻行
- `[Tooltip]` 如有，写在 `[LabelText]` **正上方**紧邻行
- `[Serializable]` 数据类的 `public` 字段同样适用此规则
