using System;
using FrooxEngine;
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
        if (_callback == null)
        {
            _callback = Receive;
        }
        Action<ArtNetClient, byte[]> value = delegate (ArtNetClient c, byte[] d)
        {
            dispatcher.ScheduleEvent(path, _callback, d);
        };
        client.PacketReceived += value;
        _handler.Write(value, context);
    }

    protected override void Unregister(ArtNetClient client, FrooxEngineContext context)
    {
        client.PacketReceived -= _handler.Read(context);
    }

    protected override void Clear(FrooxEngineContext context)
    {
        _handler.Clear(context);
    }

    private void Receive(FrooxEngineContext context, object data)
    {
        byte[] receivedData = data as byte[];
        int universeID = UniverseID.Evaluate(context);

        if (IsValidArtNetPacket(receivedData))
        {
            int receivedUniverseID = ParseUniverseID(receivedData);

            if (receivedUniverseID == universeID)
            {
                byte[] dmxData = ExtractDMXData(receivedData);
                Data.Write(dmxData, context);
                Received.Execute(context);
            }
        }
        else if (IsValidDMXPacket(receivedData))
        {
            byte[] dmxData = ExtractDMXData(receivedData);
            Data.Write(dmxData, context);
            Received.Execute(context);
        }
    }

    private bool IsValidArtNetPacket(byte[] data)
    {
        return data.Length >= 8 && System.Text.Encoding.ASCII.GetString(data, 0, 7) == "Art-Net";
    }

    private bool IsValidDMXPacket(byte[] data)
    {
        return data.Length >= 1 && data[0] == 0;
    }

    private int ParseUniverseID(byte[] data)
    {
        int universeIDOffsetLowByte = 14;
        int universeIDOffsetHighByte = 15;

        int universeID = (data[universeIDOffsetHighByte] << 8) | data[universeIDOffsetLowByte];

        return universeID;
    }

    private byte[] ExtractDMXData(byte[] data)
    {
        int dmxDataOffset = 18;
        int dmxDataLength = 512; // fixed size of DMX data

        byte[] dmxData = new byte[dmxDataLength];
        Array.Copy(data, dmxDataOffset, dmxData, 0, dmxDataLength);

        return dmxData;
    }

}
