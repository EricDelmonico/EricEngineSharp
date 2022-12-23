using NLog;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace EricEngineSharp
{
    internal class App
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        IWindow window;
        IRenderer renderer;

        internal App(IWindow window, IRenderer renderer)
        {
            this.window = window;
            this.renderer= renderer;
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
            renderer.Render(obj);
        }

        private void OnUpdate(double obj)
        {

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
