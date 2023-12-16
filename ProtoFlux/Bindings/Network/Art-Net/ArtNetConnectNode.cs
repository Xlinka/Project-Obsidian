using System;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Network.ArtNet;
using Obsidian;

[Category(new string[] { "ProtoFlux/Runtimes/Execution/Nodes/Obsidian/Network/ArtNet" })]
public class ArtNetClientConnectNode : FrooxEngine.ProtoFlux.Runtimes.Execution.ActionBreakableFlowNode<FrooxEngineContext>
{
    public readonly SyncRef<INodeObjectOutput<ArtNetClient>> Client;
    public readonly SyncRef<INodeObjectOutput<Uri>> URL;
    public readonly SyncRef<INodeObjectOutput<User>> HandlingUser;

    public override Type NodeType => typeof(ArtNetClientConnect);

    public ArtNetClientConnect TypedNodeInstance { get; private set; }

    public override INode NodeInstance => TypedNodeInstance;

    public override int NodeInputCount => 3;

    public override N Instantiate<N>()
    {
        if (TypedNodeInstance != null)
        {
            throw new InvalidOperationException("Node has already been instantiated");
        }
        TypedNodeInstance = new ArtNetClientConnect();
        return TypedNodeInstance as N;
    }

    protected override void AssociateInstanceInternal(INode node)
    {
        TypedNodeInstance = node as ArtNetClientConnect ?? throw new ArgumentException("Node instance is not of type ArtNetClientConnect");
    }

    public override void ClearInstance()
    {
        TypedNodeInstance = null;
    }

    protected override ISyncRef GetInputInternal(ref int index)
    {
        switch (index)
        {
            case 0: return Client;
            case 1: return URL;
            case 2: return HandlingUser;
            default: index -= 3; return null;
        }
    }
}
