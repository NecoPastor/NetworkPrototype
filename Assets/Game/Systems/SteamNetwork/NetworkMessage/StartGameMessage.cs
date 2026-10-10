using System.IO;

namespace Game.Systems.SteamNetwork
{
    public struct StartGameMessage : INetworkMessage
    {
        // Уникальный идентификатор сообщения для диспетчера
        public ushort MessageId => 101; // Укажи свой свободный ID

        public int MapId;
        public int RandomSeed;

        // Сериализация для отправки через лобби (BinaryWriter)
        public static void Serialize(StartGameMessage message, BinaryWriter writer)
        {
            writer.Write(message.MapId);
            writer.Write(message.RandomSeed);
        }

        // Десериализация при получении из лобби (BinaryReader)
        public static StartGameMessage Deserialize(BinaryReader reader)
        {
            return new StartGameMessage
            {
                MapId = reader.ReadInt32(),
                RandomSeed = reader.ReadInt32()
            };
        }
    }
}