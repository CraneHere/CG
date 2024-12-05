using System;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace CompGraph
{
    class Hexagon : GameWindow
    {
        private readonly float[] vertices = new float[12];
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private VertexArray vertexArray;
        private Shader shader;

        private int vertexCount;
        private int indexCount;

        private float time = 0.0f;
        private float duration = 2.0f;
        private Vector2 startPosition = new Vector2(-0.5f, 0.0f);
        private Vector2 endPosition = new Vector2(0.5f, 0.0f);

        public Hexagon(GameWindowSettings settings, NativeWindowSettings nativeWindowSettings) : base(settings, nativeWindowSettings)
        {

        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.8f, 0.8f, 0.8f, 1f);

            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.DegreesToRadians(60 * i);
                vertices[i * 2] = MathF.Cos(angle) * 0.5f;
                vertices[i * 2 + 1] = MathF.Sin(angle) * 0.5f;
            }

            this.vertexBuffer = new VertexBuffer(VertexPositionColor.VertexInfo, vertices.Length, true);
            this.vertexBuffer.SetData(vertices, vertices.Length);

            //this.indexBuffer = new IndexBuffer(indices.Length, true);
            //this.indexBuffer.SetData(indices, indices.Length);

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

            this.shader = new Shader(vertexShaderCode, pixelShaderCode);

            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);

            GL.UseProgram(this.shader.ShaderProgramHandle);
            int viewportSizeUniformLocation = GL.GetUniformLocation(this.shader.ShaderProgramHandle, "ViewportSize");
            GL.Uniform2(viewportSizeUniformLocation, (float)viewport[2], (float)viewport[3]);
            GL.UseProgram(0);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            float t = Math.Min(time / duration, 1.0f);
            Vector2 interpolatedPosition = Vector2.Lerp(startPosition, endPosition, t);

            GL.UseProgram(this.shader.ShaderProgramHandle);
            int translationLocation = GL.GetUniformLocation(this.shader.ShaderProgramHandle, "uTranslation");
            GL.Uniform2(translationLocation, interpolatedPosition);

            GL.BindVertexArray(this.vertexArray.VertexArrayHandle);
            GL.DrawArrays(PrimitiveType.Points, 0, 6);

            time += (float)args.Time;
            if (time >= duration)
            {
                time = 0.0f;
                (startPosition, endPosition) = (endPosition, startPosition);
            }

            this.Context.SwapBuffers();
        }
    }
}
