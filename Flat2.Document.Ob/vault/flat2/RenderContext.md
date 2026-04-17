重构后的引擎架构现在像是一场由**导演（FPL）**指挥、**演员（Node）**提供数据、**法则（Layer）**进行时空转换的电影拍摄。

以下是重构后的**关系树**与**渲染全流程**：

### 1. 核心关系树 (The Architecture Hierarchy)

这棵树展示了数据（Scene）与逻辑（FPL）是如何解耦并最终汇聚到硬件（Context）的。

Plaintext

```
Engine (Global Control)
├── RenderContext (硬件资产与指令池)
│   ├── MainOffscreenBuffer (唯一的离屏画布)
│   └── SharedVPLs (通用的渲染状态配方库)
│
└── Scene (当前激活的场景数据)
    ├── RenderPipelineConfig (FPL 的配置/引用)
    ├── LayerMap (ID -> ISpatialProvider: 空间转换规则)
    └── NodeTree (层级数据容器)
        └── Node (Transform, Renderer, LayerID)

FPL (执行策略库 - 独立于场景)
└── Pass List
    ├── SpritePass (提取 World 节点，用 WorldProvider 转换)
    ├── SkiaPass (提取 UI 节点，用 ScreenSpaceProvider 转换)
    └── PostProcessPass (对全屏纹理应用着色器)
```

---

### 2. 一次渲染的完整流程 (The Frame Lifecycle)

我们将一次渲染分为四个阶段：**聚合、转换、绘制、合成。**

#### **第一阶段：准备与聚合 (Gathering)**

1. `Engine` 从 `Scene` 中获取当前的 `NodeTree`。
    
2. **筛选 (Culling)**：只留下在 `Camera` 视野内且激活的节点。
    
3. **排序 (Sorting)**：根据 `LayerID` 和 `ZIndex` 将节点分类。
    

#### **第二阶段：规则转换 (Spatial Transformation)**

1. `FPL` 开始按顺序执行 `Passes`。
    
2. 当 `Pass` 迭代一个节点时，它会向 `Scene.LayerMap` 询问：“这个节点所属的 Layer 使用哪个空间规则？”
    
3. **ISpatialProvider 介入**：
    
    - 如果是 **World Layer**：节点坐标 + 相机矩阵 $\rightarrow$ $MVP$。
        
    - 如果是 **UI Layer**：节点坐标 + 屏幕正交矩阵 $\rightarrow$ $MVP$。
        
4. `RenderContext` 将算出的 $MVP$ 更新到 GPU 常量缓冲。
    

#### **第三阶段：指令录制 (Recording)**

1. 节点的 `Renderer` 被触发（例如 `SpriteBatcher.Draw()`）。
    
2. `Renderer` 本身**完全不关心**它在哪个空间，它只管把纹理坐标和顶点数据塞进 `CommandList`。
    
3. 所有的绘制结果都被画入 `RenderContext.MainOffscreenBuffer`。
    

#### **第四阶段：最终合成 (Composition & Presentation)**

1. 所有的 Pass 完成后，`MainOffscreenBuffer` 里已经存有了完整的画面（游戏画面 + Skia 渲染的 UI）。
    
2. **Present 操作**：
    
    - FPL 自动切换渲染目标为 `Veldrid.Swapchain`（物理屏幕）。
        
    - 运行一个极简的 **Blit VPL**，将离屏纹理作为全屏贴图画出。
        
3. `context.Submit()`：将一整包命令发给 GPU，画面在显示器上刷新。
    

---

### 3. 数据流向图

|**步骤**|**数据状态**|**处理主体**|
|---|---|---|
|**1**|`Node.LocalTransform`|**Node** (数据源)|
|**2**|`Node.GlobalTransform`|**Scene** (变换级联)|
|**3**|`Matrix4x4 MVP`|**ISpatialProvider** (法则应用)|
|**4**|`Draw Commands`|**Renderer** (指令录制)|
|**5**|`MainOffscreenTexture`|**RenderContext** (资源存储)|
|**6**|`Physical Screen Pixels`|**PresentPass** (最终呈现)|

---

### 4. 关键点总结

- **Scene** 是纯粹的**数据状态**，它持有规则（Layer）和内容（Node）。
    
- **FPL** 是**逻辑引擎**，它决定了数据以什么顺序、什么规则经过什么样的 VPL。
    
- **Renderer** 变得**极简且可移植**，它只负责录制绘图指令，空间感完全由外部注入。
    

你现在的架构已经从“写死一个渲染循环”变成了“搭建一个渲染工厂”。当你想加一个特殊的渲染层（比如扭曲层）时，只需在 Scene 里加一个特殊的 `ISpatialProvider` 并在 FPL 里加一个对应的 Pass，整个引擎的骨架不需要任何变动。