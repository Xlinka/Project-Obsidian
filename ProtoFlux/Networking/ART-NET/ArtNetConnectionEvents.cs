using System;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Networking.ArtNet
{
    [NodeCategory("Obsidian/Network/ArtNet")]
    public class ArtNetConnectionEvents : ArtNetEvents
    {
        public Call OnConnected;
        public Call OnDisconnected;

        private ObjectStore<Action<ArtNetClient>> _connected;
        private ObjectStore<Action<ArtNetClient>> _disconnected;

        protected override void Register(ArtNetClient client, NodeContextPath path, ExecutionEventDispatcher<FrooxEngineContext> dispatcher, FrooxEngineContext context)
        {
            void Value(ArtNetClient obj) => dispatcher.ScheduleEvent(path, Connected);

            void Value2(ArtNetClient obj) => dispatcher.ScheduleEvent(path, Disconnected);

            client.Connected += Value;
            client.Closed += Value2;
            _connected.Write(Value, context);
            _disconnected.Write(Value2, context);
        }

        protected override void Unregister(ArtNetClient client, FrooxEngineContext context)
        {
            client.Connected -= _connected.Read(context);
            client.Closed -= _disconnected.Read(context);
        }

        protected override void Clear(FrooxEngineContext context)
        {
            _connected.Clear(context);
            _disconnected.Clear(context);
        }

        private void Connected(FrooxEngineContext context)
        {
            OnConnected.Execute(context);
        }

        private void Disconnected(FrooxEngineContext context)
        {
            OnDisconnected.Execute(context);
        }
    }


}
