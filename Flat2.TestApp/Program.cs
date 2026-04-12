using Flat2.Core.Nodes;
using Flat2.Core.Platform;
using Flat2.Core.Comps;
using Silk.NET.Input;
using System.Numerics;

// 1. 初始化窗口
using var app = new GameWindow(new Vector2(800, 800), "Flat2 GC & Sprite Test");

// 2. 创建一个带有资源的场景
void SetupGameScene()
{
    var gameScene = new Scene { SceneName = "GameScene" };
    gameScene.AddChild(new TestNode());
    // 添加相机

    app.ChangeScene(gameScene);
}

// 3. 初始加载
app._window.Load += () => {
    SetupGameScene();
};

// 4. 输入控制：按下 R 键重启场景（触发旧场景回收）
app._window.Load += () => {
    var input = new InputAction(app._inputMgr).AddKey(Key.R);
    input.Pressed += (act, frames) => {
        Console.WriteLine("--- Manual Trigger: Reloading Scene (GC Old One) ---");
        SetupGameScene();
    };

    // 按下 Space 动态移除一个节点
    var space = new InputAction(app._inputMgr).AddKey(Key.Space);
    space.Pressed += (act, frames) => {
        var player = app.ActiveScene.Children.FirstOrDefault(c => c.Name == "Player");
        if (player != null)
        {
            // 手动调用 Dispose 模拟删除并回收显存
            player.Dispose();
            // 注意：你应该在引擎里实现一个 RemoveChild 并在内部调用 Dispose
            Console.WriteLine("Player node and its texture destroyed.");
        }
    };
};

// 5. 运行
app.Run();