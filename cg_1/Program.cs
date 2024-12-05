using System;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using CompGraph;

class Program
{
    static void Main(string[] args)
    {
        var settings = new GameWindowSettings
        {
            
        };

        var nativeWindowSettings = new NativeWindowSettings
        {
            Size = new Vector2i(800, 600)
        };

        using (Hexagon window = new Hexagon(settings, nativeWindowSettings))
        {
            window.Run();
        }
    }
}