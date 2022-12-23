using Silk.NET.Maths;
using Silk.NET.Windowing;
using NLog;

namespace EricEngineSharp
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            int windowWidth = 1280, windowHeight = 720;

            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(windowWidth, windowHeight);
            options.Title = "Eric Engine Sharp";

            // Inject dependencies
            IWindow window = Window.Create(options);
            Renderer renderer = new Renderer(window);
            App app = new App(window, renderer);
            app.Run();
        }
    }
}