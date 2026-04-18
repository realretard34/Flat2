
using Flat2.Core.Nodes;
using Flat2.Core.Platform;
using Flat2.Core.Renderer;
using Silk.NET.Input;
using System.Numerics;

// 1. 初始化窗口
using var app = new GameWindow(new Vector2(800, 800), "Flat2 GC & Sprite Test");

// Prepare tri pass and FPL
var triPass = new TriPass();
var fpl = new RenderFPL { Passes = { triPass,new BlitPass() } };

// Prepare scene with a node that has TriComp
var scene = new Scene();
var triNode = new Node1();
var triComp = new Flat2.Core.Comps.TriComp();
triNode.AddComp(triComp);
scene.Nodes.Add(triNode);

// 3. 初始加载
app._window.Load += () => {
    app.ChangeScene(scene);
    app.ChangeFPLTemp(fpl);
    // Load render pass resources
    fpl.OnLoad(app.RenderContext);
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
class Node1 : NodeUI
{
    public override void OnDestroy()
    {
        Console.WriteLine("Node1 destroyed");
    }

    public override void OnLoad()
    {
        Console.WriteLine("Node1 loaded");
    }

    public override void OnRender(double deltaTime, RenderContext renderContext)
    {
        Console.WriteLine("Node1 rendering");
    }

    public override void OnUpdate(double deltaTime)
    {
        Console.WriteLine("Node1 updating");
    }
}