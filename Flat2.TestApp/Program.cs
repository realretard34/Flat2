
using Flat2.Core.Nodes;
using Flat2.Core.Platform;
using Flat2.Core.Renderer;
using Silk.NET.Input;
using System.Numerics;

// 1. 初始化窗口
using var app = new GameWindow(new Vector2(800, 800), "Flat2 GC & Sprite Test");
// 3. 初始加载
app._window.Load += () => {
    app.ChangeScene(new Scene());
    app.ChangeFPLTemp(new RenderFPL());
};

// 4. 输入控制：按下 R 键重启场景（触发旧场景回收）
app._window.Load += () => {
    var input = new InputAction(app.InputMgr).AddKey(Key.R);
    input.Pressed += (act, frames) => {
        Console.WriteLine("--- Manual Trigger: Reloading Scene (GC Old One) ---");
    };
};

// 5. 运行
app.Run();