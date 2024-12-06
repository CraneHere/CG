if (!Shader.CompileVertexShader(vertexShaderCode, out this.VertexShaderHandle, out string vertexShaderCompileError))
{
    Console.WriteLine("Vertex shader compilation error: " + vertexShaderCompileError);
    throw new ArgumentException(vertexShaderCompileError);
}
