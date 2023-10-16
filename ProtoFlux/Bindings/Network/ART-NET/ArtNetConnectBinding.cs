using System;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Networking.ArtNet
{
    [NodeCategory("Obsidian/Network/ArtNet")]
    public class ArtNetConnect : ActionBreakableFlowNode<FrooxEngineContext>
    {
        public readonly ObjectInput<ArtNetClient> Client;
        public readonly ObjectInput<Uri> URL;
        public readonly ObjectInput<User> HandlingUser;

        public override Type NodeType => typeof(ArtNetConnectNode);

        public ArtNetConnectNode TypedNodeInstance { get; private set; }

        public override INode NodeInstance => TypedNodeInstance;

        public override int NodeInputCount => base.NodeInputCount + 3;

        public override TN Instantiate<TN>()
        {
            if (TypedNodeInstance != null)
                throw new InvalidOperationException("Node has already been instantiated");
            var instance = (TypedNodeInstance = new ArtNetConnectNode());
            return instance as TN;
        }

        protected override void AssociateInstanceInternal(INode node)
        {
            if (node is not ArtNetConnectNode typedNodeInstance)
                throw new ArgumentException("Node instance is not of type " + typeof(ArtNetConnectNode));
            TypedNodeInstance = typedNodeInstance;
        }

        public override void ClearInstance() => TypedNodeInstance = null;

        protected override ISyncRef GetInputInternal(ref int index)
        {
            var inputInternal = base.GetInputInternal(ref index);
            if (inputInternal != null)
            {
                return inputInternal;
            }
            switch (index)
            {
                case 0:
                    return Client;
                case 1:
                    return URL;
                case 2:
                    return HandlingUser;
                default:
                    index -= 3;
                    return null;
            }
        }
    }
}
