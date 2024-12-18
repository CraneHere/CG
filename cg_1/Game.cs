using System;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace CompGraph
{
    public class Game : GameWindow
    {
        private VertexArray[] vertexArrays;
        private Shader shaderProgram;

        private Matrix4 projectionMatrix;
        private Matrix4 viewMatrix;
        private Matrix4[] modelMatrices;

        public Game(int width = 1280, int height = 768, string title = "3D Scene")
            : base(
                GameWindowSettings.Default,
                new NativeWindowSettings()
                {
                    Title = title,
                    Size = new Vector2i(width, height),
                    WindowBorder = WindowBorder.Fixed,
                    StartVisible = false,
                    StartFocused = true,
                    API = ContextAPI.OpenGL,
                    Profile = ContextProfile.Core,
                    APIVersion = new Version(3, 3)
                })
        {
            this.CenterWindow();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(45f),
                Size.X / (float)Size.Y,
                0.1f,
                100f);
            base.OnResize(e);
        }

        protected override void OnLoad()
        {
            this.IsVisible = true;

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1f);
            GL.Enable(EnableCap.DepthTest);

            string vertexShaderCode = @"
            #version 330 core

            layout (location = 0) in vec3 aPosition;

            uniform mat4 projection;
            uniform mat4 view;
            uniform mat4 model;

            void main()
            {
                gl_Position = projection * view * model * vec4(aPosition, 1.0);
            }";

            string fragmentShaderCode = @"
            #version 330 core

            out vec4 FragColor;

            void main()
            {
                FragColor = vec4(1.0, 0.5, 0.2, 1.0);
            }";

            shaderProgram = new Shader(vertexShaderCode, fragmentShaderCode);

            // Создание геометрии
            vertexArrays = new[]
            {
                CreateCube(),
                CreatePyramid(),
                CreateCylinder(1f, 0.5f, 16)
            };

            // Матрицы
            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(45f),
                Size.X / (float)Size.Y,
                0.1f,
                100f);

            viewMatrix = Matrix4.LookAt(
                new Vector3(3f, 3f, 3f),
                Vector3.Zero,
                Vector3.UnitY);

            modelMatrices = new[]
            {
                Matrix4.Identity, // Куб
                Matrix4.CreateTranslation(2f, 0f, 0f), // Пирамида
                Matrix4.CreateTranslation(-2f, 0f, 0f) // Цилиндр
            };

            base.OnLoad();
        }

        protected override void OnUnload()
        {
            foreach (var va in vertexArrays)
            {
                va.Dispose();
            }

            shaderProgram.Dispose();
            base.OnUnload();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            shaderProgram.Use();

            shaderProgram.SetUniform("projection", projectionMatrix);
            shaderProgram.SetUniform("view", viewMatrix);

            foreach (var (vertexArray, modelMatrix) in vertexArrays.Zip(modelMatrices))
            {
                shaderProgram.SetUniform("model", modelMatrix);
                vertexArray.Bind();
                GL.DrawElements(PrimitiveType.Triangles, vertexArray.IndexCount, DrawElementsType.UnsignedInt, 0);
            }

            SwapBuffers();
            base.OnRenderFrame(args);
        }

        private VertexArray CreateCube()
        {
            // Куб: 8 вершин, 12 треугольников
            float[] vertices =
            {
                // Front face
                -0.5f, -0.5f,  0.5f,
                 0.5f, -0.5f,  0.5f,
                 0.5f,  0.5f,  0.5f,
                -0.5f,  0.5f,  0.5f,
                // Back face
                -0.5f, -0.5f, -0.5f,
                 0.5f, -0.5f, -0.5f,
                 0.5f,  0.5f, -0.5f,
                -0.5f,  0.5f, -0.5f,
            };

            int[] indices =
            {
                // Front
                0, 1, 2, 0, 2, 3,
                // Back
                4, 5, 6, 4, 6, 7,
                // Left
                0, 3, 7, 0, 7, 4,
                // Right
                1, 2, 6, 1, 6, 5,
                // Top
                2, 3, 7, 2, 7, 6,
                // Bottom
                0, 1, 5, 0, 5, 4
            };

            return new VertexArray(vertices, indices);
        }

        private VertexArray CreatePyramid()
        {
            // Пирамида с квадратным основанием
            float[] vertices =
            {
                // Основание
                -0.5f, 0f,  0.5f,
                 0.5f, 0f,  0.5f,
                 0.5f, 0f, -0.5f,
                -0.5f, 0f, -0.5f,
                // Вершина
                 0f,   1f,  0f,
            };

            int[] indices =
            {
                // Основание
                0, 1, 2, 0, 2, 3,
                // Стороны
                0, 1, 4,
                1, 2, 4,
                2, 3, 4,
                3, 0, 4
            };

            return new VertexArray(vertices, indices);
        }

        private VertexArray CreateCylinder(float height, float radius, int segments)
        {
            // Цилиндр: точки оснований и боковые поверхности
            var vertices = new List<float>();
            var indices = new List<int>();

            // Верхнее и нижнее основание
            for (int i = 0; i <= segments; i++)
            {
                float angle = MathHelper.TwoPi / segments * i;
                float x = radius * MathF.Cos(angle);
                float z = radius * MathF.Sin(angle);
                // Верхняя точка
                vertices.Add(x);
                vertices.Add(height / 2);
                vertices.Add(z);
                // Нижняя точка
                vertices.Add(x);
                vertices.Add(-height / 2);
                vertices.Add(z);
            }

            // Индексы
            for (int i = 0; i < segments; i++)
            {
                indices.Add(i * 2);
                indices.Add((i + 1) * 2);
                indices.Add(i * 2 + 1);

                indices.Add(i * 2 + 1);
                indices.Add((i + 1) * 2);
                indices.Add((i + 1) * 2 + 1);
            }

            return new VertexArray(vertices.ToArray(), indices.ToArray());
        }
    }
}
