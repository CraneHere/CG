#version 330 core

uniform vec2 ViewportSize;
uniform mat3 TransformMatrix;      // Добавлено
uniform mat3 TranslationMatrix;    // Добавлено

layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec4 aColor;

out vec4 vColor;

void main()
{
    // Преобразуем положение вершины через матрицы TransformMatrix и TranslationMatrix
    vec3 position = vec3(aPosition, 1.0);
    position = TranslationMatrix * TransformMatrix * position;

    // Преобразуем координаты в нормализованное устройство
    float nx = position.x / ViewportSize.x * 2.0 - 1.0;
    float ny = position.y / ViewportSize.y * 2.0 - 1.0;
    gl_Position = vec4(nx, ny, 0.0, 1.0);

    // Цвет с возможным фактором умножения
    vColor = aColor;
}
