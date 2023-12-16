using System;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using Obsidian;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

[NodeCategory("Obsidian/Network/Websockets")]
public class ArtNetConnect : ActionBreakableFlowNode<FrooxEngineContext>
{
    public ObjectInput<ArtNetClient> Client;

    public ObjectInput<Uri> URL;

    public ObjectInput<User> HandlingUser;

    protected override bool Do(FrooxEngineContext context)
    {
        ArtNetClient ArtnetClient = Client.Evaluate(context);
        if (ArtnetClient == null)
        {
            return false;
        }
        Uri uri = URL.Evaluate(context);
        if (uri != null)
        {
            ArtnetClient.URL.Value = uri;
        }
        ArtnetClient.HandlingUser.Target = HandlingUser.Evaluate(context, context.LocalUser);
        return true;
    }
}
