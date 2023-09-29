using System;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;


namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Networking.ArtNet
{
    [NodeCategory("Network/ART-NET")]
    public class ArtNetDataChannelExtractor : ArtNetEvents
    {
        public readonly ObjectInput<byte[]> Data;
        public readonly ObjectInput<int> Channel;
        public readonly ObjectInput<int> StartIndex;

        public readonly Call OnEvaluationComplete;

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
            int channel = Channel.Evaluate(context);
            int startIndex = StartIndex.Evaluate(context);

            if (receivedData != null && receivedData.Length > startIndex + channel - 1)
            {
                byte extractedValue = receivedData[startIndex + channel - 1];

                // Triggering the OnEvaluationComplete call with the extracted value
                OnEvaluationComplete.Execute(context);
            }
        }
    }
}