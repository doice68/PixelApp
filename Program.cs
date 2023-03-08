using System.Text;
using ImGuiDemo;

public class Program
{
    public static bool exit = false;
    static void Main(string[] args)
    {
        unsafe
        {
            Raylib.SetTraceLogCallback(&Logging.LogConsole);
        }
        Raylib.SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT | ConfigFlags.FLAG_VSYNC_HINT | ConfigFlags.FLAG_WINDOW_RESIZABLE);
        Raylib.SetTraceLogLevel(TraceLogLevel.LOG_FATAL | TraceLogLevel.LOG_ERROR | TraceLogLevel.LOG_WARNING);
        
        Raylib.InitWindow(1200, 700, "APP");
        Raylib.SetTargetFPS(60);
        
        Raylib.InitAudioDevice();
        
        ImguiController controller = new ImguiController();
        ImGui.GetIO().ConfigFlags = ImGuiConfigFlags.DockingEnable | ImGuiConfigFlags.ViewportsEnable;
        var path = @"Inter-Medium.ttf";
        ImGui.GetIO().Fonts.AddFontFromFileTTF(path, 17);
        
        MyStyle.SetupImGuiStyle();
        ImGui.GetStyle().ScaleAllSizes(1.5f);
        
        controller.Load(GetScreenWidth(), GetScreenHeight());
        var app = new App();
        while (exit == false)
        {
            float dt = Raylib.GetFrameTime();
            controller.Update(dt);
            
            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color((int)(app.bgColor.X * 255), (int)(app.bgColor.Y * 255), (int)(app.bgColor.Z * 255), 255));
            app.Update();
            controller.Draw();
            app.LateUpdate();
            Raylib.EndDrawing();
        }
        controller.Dispose();
        Raylib.CloseAudioDevice();
        Raylib.CloseWindow();
    }
}

