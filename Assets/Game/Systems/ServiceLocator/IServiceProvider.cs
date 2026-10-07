
using Game.Systems.Service;

namespace Game.Systems
{
    public interface IServiceProvider<T> : IService where T : class
    {
        T Get();
    }
}