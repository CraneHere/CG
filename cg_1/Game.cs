using System;
using OpenTK;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using CompGraph;

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

        private float colorFactor = 1f;
        private float deltaColorFactor = 1f / 240f;

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

            Random rand = new Random();

            int windowWidth = this.ClientSize.X;
            int windowHeight = this.ClientSize.Y;

            int Count = 1;

            VertexPositionColor[] vertices = new VertexPositionColor[Count * 6];
            this.vertexCount = 0;

            for (int i = 0; i < Count; i++)
            {
                int w = 300;
                int h = 300;
                int x = 600;
                int y = 150;

                float r = (float)rand.NextDouble();
                float g = (float)rand.NextDouble();
                float b = (float)rand.NextDouble();

                vertices[this.vertexCount++] = new VertexPositionColor(new Vector2(x - (w * 0.5f), y + h / 2), new Color4(r, g, b, 1f));
                vertices[this.vertexCount++] = new VertexPositionColor(new Vector2(x, y + h), new Color4(r, g, b, 1f));
                vertices[this.vertexCount++] = new VertexPositionColor(new Vector2(x + w, y + h), new Color4(r, g, b, 1f));
                vertices[this.vertexCount++] = new VertexPositionColor(new Vector2(x + (w * 1.5f), y + h / 2), new Color4(r, g, b, 1f));
                vertices[this.vertexCount++] = new VertexPositionColor(new Vector2(x + (w * 1.5f), y + h / 2), new Color4(r, g, b, 1f));
                vertices[this.vertexCount++] = new VertexPositionColor(new Vector2(x + (w * 0.5f), y), new Color4(r, g, b, 1f));
            }


            int[] indices = new int[Count * 12];
            this.indexCount = 0;
            this.vertexCount = 0;

            for (int i = 0; i < Count; i++)
            {
                indices[this.indexCount++] = 0 + this.vertexCount;
                indices[this.indexCount++] = 1 + this.vertexCount;
                indices[this.indexCount++] = 2 + this.vertexCount;
                indices[this.indexCount++] = 0 + this.vertexCount;
                indices[this.indexCount++] = 2 + this.vertexCount;
                indices[this.indexCount++] = 3 + this.vertexCount;
                indices[this.indexCount++] = 0 + this.vertexCount;
                indices[this.indexCount++] = 3 + this.vertexCount;
                indices[this.indexCount++] = 4 + this.vertexCount;
                indices[this.indexCount++] = 0 + this.vertexCount;
                indices[this.indexCount++] = 4 + this.vertexCount;
                indices[this.indexCount++] = 5 + this.vertexCount;

                this.vertexCount += 6;
            }


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
            this.shaderProgram.SetUniform("ColorFactor", this.colorFactor);


            base.OnLoad();
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

            ште
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.UseProgram(this.shaderProgram.ShaderProgramHandle);
            GL.BindVertexArray(this.vertexArray.VertexArrayHandle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.indexBuffer.IndexBufferHandle);
            GL.DrawElements(PrimitiveType.Triangles, this.indexCount, DrawElementsType.UnsignedInt, 0);

            this.Context.SwapBuffers();
            base.OnRenderFrame(args);
        }
    }
}