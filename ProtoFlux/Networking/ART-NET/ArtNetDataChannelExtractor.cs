using System;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;

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
            var channel = Channel.Evaluate(context);
            var startIndex = StartIndex.Evaluate(context);

            if (receivedData == null || receivedData.Length <= startIndex + channel - 1) return;
            var extractedValue = receivedData[startIndex + channel - 1];

            // Triggering the OnEvaluationComplete call with the extracted value
            OnEvaluationComplete.Execute(context);
        }
    }
}