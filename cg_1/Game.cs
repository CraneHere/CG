using System;
using OpenTK;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace CompGraph
{
    public class Game : GameWindow
    {
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private VertexArray vertexArray;
        private Shader shaderProgram;

        private int vertexCount;
        private int indexCount;

        private float animationTime = 0f;
        private float animationSpeed = 1f / 240f;
        private Vector2 positionOffset = Vector2.Zero;
        private Vector2 targetOffset = new Vector2(100, 100);
        private Vector2[] initialShape;
        private Vector2[] targetShape;
        private Vector2[] currentShape;

        private Matrix3 transformMatrix = Matrix3.Identity;

        public Game(int width = 1280, int height = 768, string title = "Game1")
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
            base.OnResize(e);
        }

        protected override void OnLoad()
        {
            this.IsVisible = true;

            GL.ClearColor(0.8f, 0.8f, 0.8f, 1f);

            this.initialShape = CreateHexagon(200, 200, 100);
            this.targetShape = CreateHexagon(200, 200, 150); // Slightly larger hexagon
            this.currentShape = new Vector2[this.initialShape.Length];
            Array.Copy(this.initialShape, this.currentShape, this.initialShape.Length);

            VertexPositionColor[] vertices = new VertexPositionColor[this.currentShape.Length];
            for (int i = 0; i < this.currentShape.Length; i++)
            {
                vertices[i] = new VertexPositionColor(this.currentShape[i], new Color4(0.2f, 0.4f, 0.8f, 1f));
            }


            this.vertexCount = vertices.Length;

            int[] indices = new int[(this.vertexCount - 2) * 3];
            for (int i = 1; i < this.vertexCount - 1; i++)
            {
                indices[(i - 1) * 3] = 0;
                indices[(i - 1) * 3 + 1] = i;
                indices[(i - 1) * 3 + 2] = i + 1;
            }

            this.indexCount = indices.Length;


            this.vertexBuffer = new VertexBuffer(VertexPositionColor.VertexInfo, vertices.Length, true);
            this.vertexBuffer.SetData(vertices, vertices.Length);

            this.indexBuffer = new IndexBuffer(indices.Length, true);
            this.indexBuffer.SetData(indices, indices.Length);

            this.vertexArray = new VertexArray(this.vertexBuffer);


            string vertexShaderCode =
                @"
                #version 330 core

                uniform vec2 ViewportSize;
                uniform float ColorFactor;

                layout (location = 0) in vec2 aPosition;
                layout (location = 1) in vec4 aColor;

                out vec4 vColor;

                void main()
                {
                    float nx = aPosition.x / ViewportSize.x * 2f - 1f;
                    float ny = aPosition.y / ViewportSize.y * 2f - 1f;
                    gl_Position = vec4(nx, ny, 0f, 1f);

                    vColor = aColor * ColorFactor;
                }
                ";

            string pixelShaderCode =
                @"
                #version 330 core

                in vec4 vColor;

                out vec4 pixelColor;

                void main()
                {
                    pixelColor = vColor;
                }
                ";

            this.shaderProgram = new Shader(vertexShaderCode, pixelShaderCode);

            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);

            this.shaderProgram.SetUniform("ViewportSize", (float)viewport[2], (float)viewport[3]);

            base.OnLoad();
        }

        private Vector2[] CreateHexagon(float centerX, float centerY, float radius)
        {
            Vector2[] vertices = new Vector2[6];
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi / 6 * i;
                vertices[i] = new Vector2(
                    centerX + radius * MathF.Cos(angle),
                    centerY + radius * MathF.Sin(angle));
            }
            return vertices;
        }

        protected override void OnUnload()
        {
            this.vertexArray?.Dispose();
            this.indexBuffer?.Dispose();
            this.vertexBuffer?.Dispose();

            base.OnUnload();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            this.animationTime += this.animationSpeed;
            if (this.animationTime >= 1f)
            {
                this.animationTime = 0f;

                var tempShape = this.initialShape;
                this.initialShape = this.targetShape;
                this.targetShape = tempShape;

                var tempOffset = this.positionOffset;
                this.positionOffset = this.targetOffset;
                this.targetOffset = tempOffset;
            }

            for (int i = 0; i < this.currentShape.Length; i++)
            {
                this.currentShape[i] = Vector2.Lerp(this.initialShape[i], this.targetShape[i], this.animationTime);
            }

            var interpolatedOffset = Vector2.Lerp(this.positionOffset, this.targetOffset, this.animationTime);

            var translationMatrix = CreateTranslationMatrix(interpolatedOffset.X, interpolatedOffset.Y);

            this.shaderProgram.SetUniform("TranslationMatrix", translationMatrix);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.UseProgram(this.shaderProgram.ShaderProgramHandle);

            this.shaderProgram.SetUniform("TransformMatrix", this.transformMatrix);

            GL.BindVertexArray(this.vertexArray.VertexArrayHandle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.indexBuffer.IndexBufferHandle);
            GL.DrawElements(PrimitiveType.Triangles, this.indexCount, DrawElementsType.UnsignedInt, 0);

            this.Context.SwapBuffers();
            base.OnRenderFrame(args);
        }

        public static Matrix3 CreateTranslationMatrix(float x, float y)
        {
            return new Matrix3(
                1f, 0f, 0f,
                0f, 1f, 0f,
                x, y, 1f
            );
        }
    }
}