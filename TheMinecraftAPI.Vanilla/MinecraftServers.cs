using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace TheMinecraftAPI.Vanilla;

public class MinecraftServers
{
    private string Address { get; set; }
    private int Port { get; set; }

    public MinecraftServers(string address, int port = 25565)
    {
        Address = address;
        Port = port;

        if (port is < IPEndPoint.MinPort or > IPEndPoint.MaxPort)
        {
            throw new ArgumentOutOfRangeException($"Port is out of range (must be between {IPEndPoint.MinPort} and {IPEndPoint.MaxPort})");
        }
    }

    /// <summary>
    /// Retrieves the status of a Minecraft server asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation. The task result is an object containing the server status information.</returns>
    public async Task<object> GetServerStatusAsync()
    {
        using TcpClient tcpClient = InitialiseConnection(1, TimeSpan.FromSeconds(5));
        await using NetworkStream stream = tcpClient.GetStream();
        SendStatusRequest(stream);
        return await ReceiveStatusRequestAsync(stream);
    }

    /// <summary>
    /// Asynchronously receives the status request from a Minecraft server.
    /// </summary>
    /// <param name="stream">The network stream used for communication with the server.</param>
    /// <returns>A task representing the asynchronous operation. The task result is an object containing the server status information.</returns>
    private async Task<object> ReceiveStatusRequestAsync(NetworkStream stream)
    {
        object failedResponse = new
        {
            Status = "Unknown",
            Ip = Address,
            Name = "Unknown response"
        };
        using MemoryStream packet = new();
        using MemoryStream data = new();
        _ = ReadVarInt(stream);
        int packetId = ReadVarInt(stream);
        if (packetId != 0)
            return failedResponse;
        int jsonLength = ReadVarInt(stream);
        byte[] jsonBytes = new byte[jsonLength];
        _ = await stream.ReadAsync(jsonBytes.AsMemory(0, jsonLength));
        string json = Encoding.UTF8.GetString(jsonBytes).ReplaceLineEndings();
        return JsonConvert.DeserializeObject<object>(json) ?? failedResponse;
    }

    /// <summary>
    /// Sends a status request to a Minecraft server.
    /// </summary>
    /// <param name="stream">The network stream used for communication with the server.</param>
    private static void SendStatusRequest(NetworkStream stream)
    {
        using MemoryStream packet = new();
        using MemoryStream data = new();
        SendConstructedPacket(stream, packet, data, 0);
    }

    /// <summary>
    /// Establishes a connection to the specified Minecraft server with the given state and timeout.
    /// </summary>
    /// <param name="state">The state of the connection.</param>
    /// <param name="timeout">The timeout duration for the connection.</param>
    /// <returns>A TcpClient object representing the established connection.</returns>
    private TcpClient InitialiseConnection(int state, TimeSpan timeout)
    {
        TcpClient client = new();

        client.Client.Blocking = true;
        var result = client.BeginConnect(Address, Port, null, null);
        var success = result.AsyncWaitHandle.WaitOne(timeout);
        if (success && client.Connected)
        {
            Handshake(client.GetStream(), state);
            return client;
        }

        client.Close();
        throw new TimeoutException("Connection to host timed out");
    }

    /// <summary>
    /// This will generate the session id and send the handshake packet to the server.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="state"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private void Handshake(NetworkStream stream, int state)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanWrite)
        {
            throw new InvalidOperationException("The stream is not writable.");
        }

        using MemoryStream packet = new();
        using MemoryStream data = new();
        WriteVarInt(data, -1);
        data.WriteByte((byte)Encoding.ASCII.GetByteCount(Address));
        byte[] addressBytes = Encoding.ASCII.GetBytes(Address);
        data.Write(addressBytes, 0, addressBytes.Length);
        byte[] portBytes = BitConverter.GetBytes((ushort)Port);
        data.Write(portBytes, 0, portBytes.Length);
        WriteVarInt(data, state);
        SendConstructedPacket(stream, packet, data, 0);
    }

    /// <summary>
    /// Sends a constructed packet to the Minecraft server.
    /// </summary>
    /// <param name="stream">The network stream used for communication with the server.</param>
    /// <param name="packet">The memory stream that holds the constructed packet.</param>
    /// <param name="data">The memory stream that holds the packet data.</param>
    /// <param name="packetId">The ID of the packet.</param>
    private static void SendConstructedPacket(NetworkStream stream, MemoryStream packet, MemoryStream data, int packetId)
    {
        WriteVarInt(packet, (int)data.Length + 1);
        WriteVarInt(packet, packetId);
        data.Seek(0, SeekOrigin.Begin);
        data.CopyTo(packet);
        packet.Seek(0, SeekOrigin.Begin);
        packet.CopyTo(stream);
    }

    /// <summary>
    /// Writes an integer value to the provided stream as a variable-length encoded value.
    /// </summary>
    /// <param name="stream">The stream to write the value to.</param>
    /// <param name="value">The integer value to encode and write.</param>
    private static void WriteVarInt(Stream stream, int value)
    {
        uint actualValue = (uint)value;

        do
        {
            byte temp = (byte)(actualValue & 0b01111111);
            actualValue >>= 7;

            if (actualValue != 0)
            {
                temp |= 0b10000000;
            }

            stream.WriteByte(temp);
        } while (actualValue != 0);
    }

    /// <summary>
    /// Reads a variable-length integer from the specified stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <returns>The variable-length integer read from the stream.</returns>
    private static int ReadVarInt(Stream stream)
    {
        int numberRead = 0;
        int result = 0;
        byte read;
        do
        {
            int value = stream.ReadByte();
            if (value == -1) break;
            read = (byte)value;
            int temp = (read & 0b01111111);
            result |= (temp << (7 * numberRead));
            numberRead++;
            if (numberRead > 5)
            {
                throw new InvalidDataException("VarInt is too big");
            }
        } while ((read & 0b10000000) != 0);

        if (numberRead == 0)
        {
            throw new InvalidDataException("No data read");
        }

        return result;
    }
}