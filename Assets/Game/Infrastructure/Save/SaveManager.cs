using UnityEngine;

public sealed class SaveManager : MonoBehaviour, IGameSystem
{
    public void Initialize()
    {
        Load();
    }

    public void Shutdown()
    {
        Save();
    }

    private void Load()
    {
        Debug.Log("Загрузка сохранения...");
        // Реализация
    }

    private void Save()
    {
        Debug.Log("Сохранение игры...");
        // Реализация
    }
}