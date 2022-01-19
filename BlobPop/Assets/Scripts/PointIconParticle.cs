using Assets.BlobPopClassic.BlobPopColor;

public class PointIconParticle : PointTextParticle, IPoolObject, IPointText
{
    void IPointText.ChangeValue(int points, BlobColor blobColor)
    {
        _pointsValue = points;
        _textComponent.text = _pointsValue.ToString();
        _currentBlobColor = BlobColor.WHITE;
        _textComponent.color = BlobColorService.GetColorByBlobColor(_currentBlobColor);
    }
}
