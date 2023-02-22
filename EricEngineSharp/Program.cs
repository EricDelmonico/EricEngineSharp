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
            App app = new App();
            app.Run();
        }
    }
}