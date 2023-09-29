using System;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

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
            Action<ArtNetClient> value = delegate
            {
                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
                {
                    Connected(c);
                });
            };
            Action<ArtNetClient> value2 = delegate
            {
                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
                {
                    Disconnected(c);
                });
            };
            client.Connected += value;
            client.Closed += value2;
            _connected.Write(value, context);
            _disconnected.Write(value2, context);
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
