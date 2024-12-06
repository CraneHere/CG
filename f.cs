public static Matrix3 CreateTranslationMatrix(float x, float y)
{
    return new Matrix3(
        1f, 0f, 0f, // Первый столбец
        0f, 1f, 0f, // Второй столбец
        x,  y,  1f  // Третий столбец
    );
}
