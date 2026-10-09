using System.IO;

namespace Game.Systems.SteamNetwork
{
    public interface INetworkMessage
    {
        // ”никальный ID типа сообщени€ (например, 1 = Movement, 2 = Chat, 3 = Inventory)
        ushort MessageId { get; }

        // «апись полей структуры/класса в бинарный поток
        void Serialize(BinaryWriter writer);

        // „тение полей из бинарного потока
        void Deserialize(BinaryReader reader);
    }
}