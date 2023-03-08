public static class Helpers
{
    static int trimLength = 45;
    public static float GetWindowWidth()
    {
        return ImGui.GetWindowWidth() - ImGui.GetStyle().WindowPadding.X * 2;
    }
    public static float GetWindowHeight()
    {
        return ImGui.GetWindowHeight() - ImGui.GetStyle().WindowPadding.Y * 2;
    }
    public static void WithText(string text, string id, Action action)
    {
        ImGui.SetNextItemWidth(ImGui.CalcTextSize("xd").X - 17f);

        ImGui.LabelText(text, "");
        ImGui.SameLine();
        ImGui.PushID(id);
        action();
        ImGui.PopID();
    }
    public static void CenteredText(string text)
    {
        var txt = text.Length > trimLength ? text.Substring(0, trimLength) + "..." : text;
        var WindowW = ImGui.GetWindowWidth();
        var textW = ImGui.CalcTextSize(txt).X;
        ImGui.SetCursorPosX((WindowW - textW) * 0.5f);
        ImGui.Text(txt);
    }
    public static string TimeFormat(int time) {
        string hr, min;
        min = Convert.ToString(time % 60);
        hr = Convert.ToString(time / 60);
        if (hr.Length == 1) hr = "0" + hr;
        if (min.Length == 1) min = "0" + min;
        return hr + ":" + min;
    }
    public static void Popup(string name, ref bool open, Action action, int w = 300, int h = 200)
    {
        var flags = 
            ImGuiWindowFlags.NoResize | 
            ImGuiWindowFlags.NoCollapse | 
            ImGuiWindowFlags.NoDocking |
            ImGuiWindowFlags.NoMove;
        ImGui.Begin(name, ref open, flags);
        ImGui.SetWindowSize(new Vector2(w, h));
        
        var x = GetScreenWidth() / 2 - ImGui.GetWindowWidth() / 2;
        var y = GetScreenHeight() / 2 - ImGui.GetWindowHeight() / 2;
        ImGui.SetWindowPos(new Vector2(x, y));

        action();
        ImGui.End();
        // DrawRectangle(0, 0, GetScreenWidth(), GetScreenHeight(), new Color(0, 0, 0, 50));
    }
    // public static bool 
}
public class Popup
{
    string name;
    protected int w, h;
    public Action onRenderer;
    public Popup(string name, int w, int h)
    {
        this.name = name;
        this.w = w;
        this.h = h;
        App.popups.Add(this);
    }

    protected bool open = false;
    public void Show() => open = true;
    public void Hide() => open = false;

    public void LateUpdate()
    {
        if (open == false) return;
        // DrawRectangle(0, 0, GetScreenWidth(), GetScreenHeight(), new Color(0, 0, 0, 50));
    }
    public void Update()
    {
        if (open == false) return;
        var flags = 
            ImGuiWindowFlags.NoResize | 
            ImGuiWindowFlags.NoCollapse | 
            ImGuiWindowFlags.NoDocking |
            ImGuiWindowFlags.NoMove;

        ImGui.Begin(name, ref open, flags);
        ImGui.SetWindowSize(new Vector2(w, h));
        
        var x = GetScreenWidth() / 2 - ImGui.GetWindowWidth() / 2;
        var y = GetScreenHeight() / 2 - ImGui.GetWindowHeight() / 2;
        ImGui.SetWindowPos(new Vector2(x, y));

        onRenderer();
        ImGui.End();
    }
}
public class FileDialog : Popup
{
    protected string[] drives;
    protected int currentDrive = 0;
    protected string[] dir;
    protected string currentDir = "";

    public Action<string> ok = (dir) => {};
    public FileDialog(string name, int w, int h) : base(name, w, h)
    {
        drives = Directory.GetLogicalDrives();
        dir = Directory.GetDirectories(drives[currentDrive]);
        onRenderer = () => 
        {
            ImGui.SetNextItemWidth(Helpers.GetWindowWidth());
            ImGui.PushID("Drive");
            if (ImGui.Combo("", ref currentDrive, drives, drives.Length))
            {
                dir = Directory.GetDirectories(drives[currentDrive]);            
            }
            ImGui.PopID();

            ImGui.SetNextItemWidth(Helpers.GetWindowWidth());
            ImGui.PushID("Files");
            ImGui.BeginListBox("");
            ImGui.PopID();
            if (ImGui.Selectable(".."))
            {
                if (currentDir != "")
                {
                    var path = Directory.GetParent(currentDir);
                    if (path != null)
                        GoToPath(path.FullName);
                }            
            }
            for (int i = 0; i < dir.Count(); i++)
            {
                if (ImGui.Selectable($"{dir[i]}"))
                {
                    GoToPath(dir[i]);
                }
            }
            ListRender();
            ImGui.EndListBox();

            Render();
        };
    }
    protected virtual void Render(){}
    protected virtual void ListRender(){}
    protected virtual void PathChanged(){}
    protected virtual void DriveChanged(){}


    void GoToPath(string name)
    {
        try
        {
            currentDir = name;
            dir = Directory.GetDirectories(name);
            PathChanged();
        }
        catch (UnauthorizedAccessException)
        {            
        }
    }
}
public class OpenDialog : FileDialog
{
    string[] files;
    public OpenDialog(string name) : base(name, 500, 280)
    {
        files = Directory.GetFiles(drives[currentDrive], ".png");
    }
    protected override void ListRender()
    {
        for (int i = 0; i < files.Count(); i++)
        {
            if (ImGui.Selectable($"{files[i]}"))
            {
                ok(files[i]);
                open = false;
            }
        }
    }
    protected override void PathChanged()
    {
        files = Directory.GetFiles(currentDir, "*.png");
    }
    protected override void DriveChanged()
    {
        files = Directory.GetFiles(drives[currentDrive], ".png");
    }
}
public class SaveDialog : FileDialog
{
    string fileName = "";
    public SaveDialog(string name, int w) : base(name, w, 400)
    {

    }
    protected override void Render()
    {
        if (ImGui.Button("Ok"))
        {
            ok(currentDir + fileName + ".png");
            open = false;
        }
        ImGui.SameLine();
        ImGui.InputText("", ref fileName, 20);
    }
}
public class ConfirmPopup : Popup
{
    string msg;
    public Action onConfirm;
    public ConfirmPopup(string name, string msg) : base(name, 250, 110)
    {
        onRenderer = () => 
        {
            this.msg = msg;
            ImGui.Text(msg);
            if (ImGui.Button("Confirm", new Vector2(Helpers.GetWindowWidth() / 2 - 5, 0)))
            {
                onConfirm();
                open = false;
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel", new Vector2(Helpers.GetWindowWidth() / 2 - 5, 0)))
            {
                open = false;
            }
 
        };
    }
}