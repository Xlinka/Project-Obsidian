using System;
using System.Text;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

[NodeCategory("Network/ART-NET")]
public class ArtNetUniverseDataReceiver : ArtNetEvents
{
    public readonly ObjectInput<int> UniverseID;
    public readonly Call Received;
    public readonly ObjectOutput<byte[]> Data;

    private ObjectStore<Action<ArtNetClient, byte[]>> _handler;
    private NodeEventHandler<FrooxEngineContext> _callback;

    public override bool CanBeEvaluated => false;

    protected override void Register(ArtNetClient client, NodeContextPath path, ExecutionEventDispatcher<FrooxEngineContext> dispatcher, FrooxEngineContext context)
    {
        _callback ??= Receive;

        void Value(ArtNetClient c, byte[] d) => dispatcher.ScheduleEvent(path, _callback, d);

        client.PacketReceived += Value;
        _handler.Write(Value, context);
    }

    protected override void Unregister(ArtNetClient client, FrooxEngineContext context) => client.PacketReceived -= _handler.Read(context);

    protected override void Clear(FrooxEngineContext context) => _handler.Clear(context);

    private void Receive(FrooxEngineContext context, object data)
    {
        var receivedData = data as byte[];
        var universeId = UniverseID.Evaluate(context);

        if (IsValidArtNetPacket(receivedData))
        {
            var receivedUniverseId = ParseUniverseID(receivedData);

            if (receivedUniverseId != universeId) return;
            var dmxData = ExtractDMXData(receivedData);
            Data.Write(dmxData, context);
            Received.Execute(context);
        }
        else if (IsValidDMXPacket(receivedData))
        {
            var dmxData = ExtractDMXData(receivedData);
            Data.Write(dmxData, context);
            Received.Execute(context);
        }
    }

    private bool IsValidArtNetPacket(byte[] data) => data.Length >= 8 && Encoding.ASCII.GetString(data, 0, 7) == "Art-Net";

    private bool IsValidDMXPacket(byte[] data) => data.Length >= 1 && data[0] == 0;

    private int ParseUniverseID(byte[] data)
    {
        const int universeIdOffsetLowByte = 14;
        const int universeIdOffsetHighByte = 15;

        var universeId = (data[universeIdOffsetHighByte] << 8) | data[universeIdOffsetLowByte];

        return universeId;
    }

    private byte[] ExtractDMXData(byte[] data)
    {
        const int dmxDataOffset = 18;
        const int dmxDataLength = 512; // fixed size of DMX data

        var dmxData = new byte[dmxDataLength];
        Array.Copy(data, dmxDataOffset, dmxData, 0, dmxDataLength);

        return dmxData;
    }

}
