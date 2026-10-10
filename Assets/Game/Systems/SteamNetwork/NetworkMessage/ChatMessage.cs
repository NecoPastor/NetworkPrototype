using Game.Systems.SteamNetwork;
using System.IO;

public struct ChatMessage : INetworkMessage
{
    // ”никальный ID вашего сетевого сообщени€
    public ushort MessageId => 100;

    public string Content;

    public ChatMessage(string content)
    {
        Content = content;
    }

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(Content ?? string.Empty);
    }

    public static ChatMessage Deserialize(BinaryReader reader)
    {
        return new ChatMessage(reader.ReadString());
    }
}