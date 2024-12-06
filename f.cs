using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

class Program
{
    static void Main()
    {
        var game = new GameWindow(GameWindowSettings.Default, NativeWindowSettings.Default);
        game.Run();
    }
}

class GameWindow : GameWindow
{
    private readonly float[] vertices =
    {
        // Шестиугольник, нормализованные координаты
        0.0f,  0.5f,   // Вершина 1
        -0.43f,  0.25f, // Вершина 2
        -0.43f, -0.25f, // Вершина 3
        0.0f, -0.5f,    // Вершина 4
        0.43f, -0.25f,  // Вершина 5
        0.43f,  0.25f   // Вершина 6
    };

    private int vbo, vao;
    private Shader shader;

    private Matrix4 transformation = Matrix4.Identity;
    private Vector2 targetPosition = Vector2.Zero;
    private Vector2 startPosition = Vector2.Zero;
    private float interpolationFactor = 0.0f;
    private float shapeFactor = 0.0f; // Для анимации формы

    public GameWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        : base(gameWindowSettings, nativeWindowSettings)
    {
        CenterWindow();
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

        // Инициализация VBO и VAO
        vao = GL.GenVertexArray();
        vbo = GL.GenBuffer();

        GL.BindVertexArray(vao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        shader = new Shader("vertex.glsl", "fragment.glsl");
        shader.Use();

        // Установка начального положения и цели
        targetPosition = new Vector2(0.5f, 0.5f);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        // Интерполяция позиции
        interpolationFactor += (float)args.Time * 0.5f; // Скорость интерполяции
        if (interpolationFactor > 1.0f)
        {
            interpolationFactor = 0.0f;
            startPosition = targetPosition;
            targetPosition = new Vector2(-startPosition.X, -startPosition.Y); // Меняем направление
        }

        Vector2 currentPosition = Vector2.Lerp(startPosition, targetPosition, interpolationFactor);

        // Анимация изменения формы
        shapeFactor += (float)args.Time * 0.5f;
        if (shapeFactor > 1.0f) shapeFactor = 0.0f;

        float deformFactor = MathF.Sin(shapeFactor * MathF.PI * 2.0f) * 0.2f; // Синусоидальная деформация

        // Создание трансформационной матрицы
        transformation = Matrix4.CreateTranslation(currentPosition.X, currentPosition.Y, 0.0f);

        // Применение к форме шестиугольника
        transformation *= Matrix4.CreateScale(1.0f + deformFactor);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        shader.Use();
        shader.SetMatrix4("transform", transformation);

        GL.BindVertexArray(vao);
        GL.DrawArrays(PrimitiveType.TriangleFan, 0, 6);

        SwapBuffers();
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        GL.DeleteBuffer(vbo);
        GL.DeleteVertexArray(vao);
        shader.Dispose();
    }
}

class Shader
{
    private readonly int handle;

    public Shader(string vertexPath, string fragmentPath)
    {
        string vertexShaderSource = File.ReadAllText(vertexPath);
        string fragmentShaderSource = File.ReadAllText(fragmentPath);

        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexShaderSource);
        GL.CompileShader(vertexShader);

        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentShaderSource);
        GL.CompileShader(fragmentShader);

        handle = GL.CreateProgram();
        GL.AttachShader(handle, vertexShader);
        GL.AttachShader(handle, fragmentShader);
        GL.LinkProgram(handle);

        GL.DetachShader(handle, vertexShader);
        GL.DetachShader(handle, fragmentShader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
    }

    public void Use() => GL.UseProgram(handle);

    public void SetMatrix4(string name, Matrix4 matrix)
    {
        int location = GL.GetUniformLocation(handle, name);
        GL.UniformMatrix4(location, true, ref matrix);
    }

    public void Dispose() => GL.DeleteProgram(handle);
}
