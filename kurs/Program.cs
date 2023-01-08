using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace kurs
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GameWindowSettings gSettings = GameWindowSettings.Default;
            NativeWindowSettings nSettings = new NativeWindowSettings()
            {
                Title = "Acranoid",
                Size = (1280, 1024),
                Flags = ContextFlags.Default,
                Profile = ContextProfile.Compatability,
            };
            Game game = new Game(gSettings, nSettings);
            game.Run();
        }
    }
}