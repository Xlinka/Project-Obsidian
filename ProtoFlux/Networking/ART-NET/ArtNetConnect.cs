using System;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;

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
            var artNetClient = Client.Evaluate(context);
            if (artNetClient == null)
                return false;
            var uri = URL.Evaluate(context);
            if (uri != null) artNetClient.URL.Value = uri;
            artNetClient.HandlingUser.Target = HandlingUser.Evaluate(context, context.LocalUser);
            return true;
        }
    }
}

