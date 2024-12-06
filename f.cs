public void SetUniform(string name, Matrix3 matrix)
{
    // Получаем локацию uniform-переменной в шейдере
    int location = GL.GetUniformLocation(this.Handle, name);
    if (location == -1)
        throw new Exception($"Uniform '{name}' not found in shader.");

    // Преобразуем Matrix3 в массив float[]
    float[] matrixData = {
        matrix.M11, matrix.M21, matrix.M31,
        matrix.M12, matrix.M22, matrix.M32,
        matrix.M13, matrix.M23, matrix.M33
    };

    // Передаём данные в шейдер
    GL.UniformMatrix3fv(location, 1, false, matrixData);
}
