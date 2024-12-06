protected override void OnUpdateFrame(FrameEventArgs args)
{
    this.animationTime += this.animationSpeed;
    if (this.animationTime >= 1f)
    {
        this.animationTime = 0f;

        // Смена начальной и целевой позиции
        var tempShape = this.initialShape;
        this.initialShape = this.targetShape;
        this.targetShape = tempShape;

        var tempOffset = this.positionOffset;
        this.positionOffset = this.targetOffset;
        this.targetOffset = tempOffset;
    }

    // Линейная интерполяция формы
    for (int i = 0; i < this.currentShape.Length; i++)
    {
        this.currentShape[i] = Vector2.Lerp(this.initialShape[i], this.targetShape[i], this.animationTime);
    }

    // Линейная интерполяция позиции
    var interpolatedOffset = Vector2.Lerp(this.positionOffset, this.targetOffset, this.animationTime);

    // Создание трансляционной матрицы
    var translationMatrix = CreateTranslationMatrix(interpolatedOffset.X, interpolatedOffset.Y);

    // Передача матрицы в шейдер
    this.shaderProgram.SetUniform("TranslationMatrix", translationMatrix);
}
