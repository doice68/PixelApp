class App
{
    string projectName = "untitled";
    public static List<Popup> popups = new();
    ConfirmPopup exitPopup = new("Warning", "Are u sure u want to exit?");
    Popup settingsPopup = new("Settings", 500, 500);
    OpenDialog openFile = new("Open");
    Popup newFile = new("New", 500, 170);
    string name = "";
    int w = 24;
    int h = 24;
    float zoom = 10;
    RenderTexture2D canvas;
    RenderTexture2D outline;
    RenderTexture2D grid;
    bool showGrid = true;
    Vector3 color = Vector3.One;
    int currentTheme = 0;
    float roundness = 0;
    string[] themes = { "Purple", "Blue", "Gray" };
    public Vector3 bgColor = new(0, 0, 0);
    public App()
    {
        openFile.ok = OpenFile;
        newFile.onRenderer = NewFile;
        
        exitPopup.onConfirm = () =>
        {
            Program.exit = true;
        };
        Init();
    }

    void NewFile()
    {
        ImGui.PushItemWidth(Helpers.GetWindowWidth() - 80);
        Helpers.WithText("Width ", "width", () =>
        {
            ImGui.InputInt("", ref w);
        });
        Helpers.WithText("Height", "height", () =>
        {
            ImGui.InputInt("", ref h);
        });
        ImGui.PopItemWidth();
        if (ImGui.Button("Create", new Vector2(Helpers.GetWindowWidth(), 0)))
        {
            newFile.Hide();
            Init();
        }
    }

    void Init()
    {
        Outline();
        Grid();
        Settings();
        canvas = LoadRenderTexture(w, h);
    }

    void OpenFile(string path)
    {
        var t = LoadTexture(path);
        w = t.width;
        h = t.height;
        Init();
        Draw(() => 
        {
            ClearBackground(Color.BLANK);
            DrawTexture(t, 0, 0, Color.WHITE);
        });
    }

    void Settings()
    {
        settingsPopup.onRenderer = () =>
        {
            ImGui.PushItemWidth(450);

            ImGui.Text("Theme:");
            Helpers.WithText("", "theme", () =>
            {
                if (ImGui.Combo("", ref currentTheme, themes, themes.Length))
                {
                    MyStyle.styles[currentTheme]();
                }
            });

            ImGui.Text("Roundness:");
            Helpers.WithText("", "roundness", () =>
            {
                ImGui.SliderFloat("", ref roundness, 0, 20);
            });

            // ImGui.Text("Background Color");
            // Helpers.WithText("", "bgcolor", () => 
            // {
            //     ImGui.ColorPicker3("", ref bgColor);
            // });
            ImGui.PopItemWidth();

            var style = ImGui.GetStyle();
            style.WindowRounding = roundness;
            style.TabRounding = roundness;
            style.FrameRounding = roundness;
            style.ScrollbarRounding = roundness;
            style.ChildRounding = roundness;
            style.GrabRounding = roundness;

        };
    }

    void Grid()
    {
        var scale = 25;
        var lineThickness = 2;
        grid = LoadRenderTexture(w * scale, h * scale);
        BeginTextureMode(grid);
        var c = new Color(80, 80, 80, 250);
        for (int i = 0; i < w; i++)
        {
            DrawRectangle(i * scale - lineThickness / 2, 0, lineThickness, h * scale, c);
        }
        for (int i = 0; i < h; i++)
        {
            DrawRectangle(0, i * scale - lineThickness / 2, w * scale, lineThickness, c);
        }
        DrawRectangleLines(0, 0, w * scale, h * scale, Color.WHITE);
        EndTextureMode();
    }

    void Outline()
    {
        outline = LoadRenderTexture(w * 2, h * 2);
        BeginTextureMode(outline);
        ClearBackground(Color.WHITE);
        var k = 0;
        for (int i = 0; i < w * 2; i++)
        {
            for (int j = 0; j < h * 2; j++)
            {
                if (k % 2 == 0)
                    DrawPixel(i, j, Color.GRAY);
                k++;
            }
            k++;
        }
        EndTextureMode();
    }

    void Draw(Action action)
    {
        BeginTextureMode(canvas);
        action();
        EndTextureMode();
    }
    public void LateUpdate()
    {
        foreach (var p in popups)
        {
            p.LateUpdate();
        }
    }
    public void Update()
    {

        if (WindowShouldClose())
        {
            exitPopup.Show();
        }
        foreach (var p in popups)
        {
            p.Update();
        }
        ImGui.DockSpaceOverViewport();
        MenuBar();
        //layers
        Layers();
        //canvas
        Canvas();
        //colors
        Colors();
    }

    void Colors()
    {
        ImGui.Begin("Colors");
        ImGui.ColorPicker3("color", ref color);
        ImGui.End();
    }

    void Canvas()
    {
        ImGui.Begin(projectName, ImGuiWindowFlags.NoScrollbar);
        if (ImGui.IsWindowHovered())
        {
            zoom += GetMouseWheelMove();
            if (zoom < 0) zoom = 0;
        }
        // var x = ImGui.GetWindowWidth() / 2 - (canvas.texture.width / 2);
        // var y = Helpers.GetWindowHeight() / 2 - canvas.texture.height / 2;

        var cw = (canvas.texture.width) * zoom;
        var ch = (canvas.texture.height) * zoom;

        var x = Helpers.GetWindowWidth() / 2 - cw / 2;
        var y = Helpers.GetWindowHeight() / 2 - ch / 2;
        DrawToCanvas(x, y);

        ImGui.SetCursorPos(new Vector2(x, y));
        ImGui.Image((IntPtr)outline.texture.id, new Vector2(cw, ch));
        
        //this is horrible :D
        ImGui.SetCursorPos(new Vector2(x, y));
        
        var img = LoadImageFromTexture(canvas.texture);
        ImageFlipVertical(ref img);

        var t = LoadTextureFromImage(img);
        ImGui.Image((IntPtr)t.id, new Vector2(cw, ch));

        if (showGrid)
        {
            ImGui.SetCursorPos(new Vector2(x, y));
            ImGui.Image((IntPtr)grid.texture.id, new Vector2(cw, ch));
        }
        ImGui.End();
    }

    void Layers()
    {
        if (ImGui.Begin("Layers"))
        {
            ImGui.PushID("layers listbox");
            ImGui.BeginListBox("", new Vector2(Helpers.GetWindowWidth(), ImGui.GetWindowHeight() - 130));
            ImGui.PopID();

            for (int i = 0; i < 10; i++)
            {
                ImGui.Selectable($"layer {i}");
            }
            ImGui.EndListBox();

            ImGui.PushID("layer name");
            ImGui.SetNextItemWidth(Helpers.GetWindowWidth());
            ImGui.InputText("", ref name, 20);
            ImGui.PopID();
            if (ImGui.Button("Add Layer", new Vector2(Helpers.GetWindowWidth(), 0)))
            {
                name = "";
            }
            ImGui.End();
        }
    }

    void MenuBar()
    {
        ImGui.BeginMainMenuBar();
        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem("New"))
            {
                newFile.Show();
            }
            if (ImGui.MenuItem("Open"))
            {
                openFile.Show();
            }
            ImGui.MenuItem("Exit");

            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Edit"))
        {
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Debug"))
        {
            ImGui.Text($"fps: {GetFPS()}");
            ImGui.Text($"zoom: {zoom}");
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Settings"))
        {
            if (ImGui.MenuItem("Style"))
            {
                settingsPopup.Show();
            }
            if (ImGui.MenuItem("Show Grid", "", ref showGrid))
            {
                // showGrid = !showGrid;
            }
            ImGui.EndMenu();
        }
        ImGui.EndMainMenuBar();
    }

    void DrawToCanvas(float x, float y)
    {
        if (ImGui.IsWindowFocused() == false)
            return;

        Draw(() => 
        {
            var mx = (GetMouseX() - x - ImGui.GetWindowPos().X - zoom / 2) / zoom;
            var my = (GetMouseY() - y - ImGui.GetWindowPos().Y - zoom / 2) / zoom;
            
            if (IsMouseButtonDown(0))
            {
                DrawPixel(
                    (int)Math.Round(mx), 
                    (int)Math.Round(my), 
                    new Color((int)(color.X * 255), (int)(color.Y * 255), (int)(color.Z * 255), 255));
            }
            else if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                BeginScissorMode((int)Math.Round(mx), (int)Math.Round(my), 1, 1);
                ClearBackground(Color.BLANK);
                EndScissorMode();
            }
        });
    }

}