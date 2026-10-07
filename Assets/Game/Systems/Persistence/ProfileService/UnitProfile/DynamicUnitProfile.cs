using Game.Systems.Persistence;

using System;

/// <summary>
/// Профиль конкретного юнита.
/// </summary>
[Serializable]
public class DynamicUnitProfile : ProfileData
{
    public string ConfigId; // Ссылка на базовый шаблон/SO
}