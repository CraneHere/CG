this.animationTime += this.animationSpeed;
if (this.animationTime >= 1f)
{
    this.animationTime = 0f;

    // Меняем начальную и конечную формы местами
    var temp = this.initialShape;
    this.initialShape = this.targetShape;
    this.targetShape = temp;

    // Обновляем конечное положение
    this.positionOffset = this.targetOffset;
    this.targetOffset = -this.targetOffset; // Возврат в исходное положение
}

// Обновляем текущую форму (анимация формы)
for (int i = 0; i < this.currentShape.Length; i++)
{
    this.currentShape[i] = Vector2.Lerp(this.initialShape[i], this.targetShape[i], this.animationTime);
}

// Обновляем позицию (анимация смещения)
Vector2 interpolatedPosition = Vector2.Lerp(this.positionOffset, this.targetOffset, this.animationTime);
this.transformMatrix = Matrix3.CreateTranslation(interpolatedPosition);
base.OnUpdateFrame(args);
