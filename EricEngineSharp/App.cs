using EricEngineSharp.Components;
using NLog;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace EricEngineSharp
{
    internal class App
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IWindow window;
        private IRenderer renderer;

        private EntityComponentManager ecm;

        internal App()
        {
            int windowWidth = 1920, windowHeight = 1080;

            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(windowWidth, windowHeight);
            options.Title = "Eric Engine Sharp";

            window = Window.Create(options);
            renderer = new Renderer(window);

            ecm = EntityComponentManager.Instance;
            for (int i = 0; i < 4000; i++)
            {
                Entity e = ecm.AddEntity();
                e.AddComponent(new RenderComponent(AssetLoader.Instance.LoadOrGetMeshList("cube.obj"), e));
            }
        }

        public void Run()
        {
            window.Load += OnLoad;
            window.Render += OnRender;
            window.Update += OnUpdate;
            window.Closing += OnClosing;

            window.Run();
        }

        private void OnLoad()
        {
            // Set up input context
            IInputContext input = window.CreateInput();
            for (int i = 0; i < input.Keyboards.Count; i++)
            {
                input.Keyboards[i].KeyDown += KeyDown;
            }

            renderer.Init();
        }

        private void OnRender(double obj)
        {
            renderer.Render(obj, ecm.Entities);
        }

        private void OnUpdate(double obj)
        {
            ecm.Entities.ForEach(e => { e.Update(obj); });
        }

        private void OnClosing()
        {
            renderer.OnClose();
        }

        private void KeyDown(IKeyboard arg1, Key arg2, int arg3)
        {
            if (arg2 == Key.Escape)
            {
                window.Close();
            }
        }
    }
}
