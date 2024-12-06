public void SetUniform(string name, Matrix3 matrix)
{
    if (!this.GetShaderUniform(name, out ShaderUniform uniform))
    {
        throw new ArgumentException($"Uniform '{name}' was not found.");
    }

    if (uniform.Type != ActiveUniformType.FloatMat3)
    {
        throw new ArgumentException($"Uniform '{name}' is not of type Matrix3.");
    }

    // Преобразуем Matrix3 в массив float[]
    float[] matrixData = {
        matrix.M11, matrix.M21, matrix.M31,
        matrix.M12, matrix.M22, matrix.M32,
        matrix.M13, matrix.M23, matrix.M33
    };

    // Передаем данные в шейдер
    GL.UseProgram(this.ShaderProgramHandle);
    GL.UniformMatrix3(uniform.Location, false, matrixData);
    GL.UseProgram(0);
}
