using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

[NodeCategory("Obsidian/Network/ArtNet")]
public abstract class ArtNetEvents : VoidNode<FrooxEngineContext>
{
    public readonly GlobalRef<ArtNetClient> Client;

    private ObjectStore<ArtNetClient> _current;

    private void OnClientChanged(ArtNetClient client, FrooxEngineContext context)
    {
        ArtNetClient artNetClient = _current.Read(context);
        if (client != artNetClient)
        {
            if (artNetClient != null)
            {
                Unregister(artNetClient, context);
            }
            if (client != null)
            {
                NodeContextPath path = context.CaptureContextPath();
                context.GetEventDispatcher(out var eventDispatcher);
                Register(client, path, eventDispatcher, context);
                _current.Write(client, context);
            }
            else
            {
                _current.Clear(context);
                Clear(context);
            }
        }
    }

    protected abstract void Register(ArtNetClient client, NodeContextPath path, ExecutionEventDispatcher<FrooxEngineContext> dispatcher, FrooxEngineContext context);

    protected abstract void Unregister(ArtNetClient client, FrooxEngineContext context);

    protected abstract void Clear(FrooxEngineContext context);

    protected ArtNetEvents()
    {
        Client = new GlobalRef<ArtNetClient>(this, 0);
    }
}
