using System;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;


namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Networking.ArtNet
{
    [NodeCategory("Obsidian/Network/ArtNet")]
    public class ArtNetConnectNode : ActionBreakableFlowNode<FrooxEngineContext>
    {
        public ObjectInput<ArtNetClient> Client;
        public ObjectInput<Uri> URL;
        public ObjectInput<User> HandlingUser;

        protected override bool Do(FrooxEngineContext context)
        {
            ArtNetClient artNetClient = Client.Evaluate(context);
            if (artNetClient == null)
            {
                return false;
            }
            Uri uri = URL.Evaluate(context);
            if (uri != null)
            {
                artNetClient.URL.Value = uri;
            }
            artNetClient.HandlingUser.Target = HandlingUser.Evaluate(context, context.LocalUser);
            return true;
        }
    }
}

