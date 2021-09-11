using UnityEngine.UI;

public interface IPointPool
{
    int Id { get; }
    bool IsUsed { get; }
    void Init(int id, Text text);
    void Show();
    void Hide();
}