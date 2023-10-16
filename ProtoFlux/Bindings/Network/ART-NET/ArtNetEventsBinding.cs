using System;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Networking.ArtNet
{
    [NodeCategory("Obsidian/Network/ArtNet")]
    public abstract class ArtNetEventsNode : VoidNode<FrooxEngineContext>
    {
        public readonly GlobalRef<ArtNetClient> Client;

        public override Type NodeType => typeof(ArtNetEvents);

        public ArtNetEvents TypedNodeInstance { get; private set; }

        public override INode NodeInstance => TypedNodeInstance;

        public override int NodeInputCount => base.NodeInputCount + 1;

        public override TN Instantiate<TN>()
        {
            if (TypedNodeInstance != null)
                throw new InvalidOperationException("Node has already been instantiated");
            var instance = (TypedNodeInstance = new ArtNetEvents());
            return instance as TN;
        }

        protected override void AssociateInstanceInternal(INode node)
        {
            if (node is not ArtNetEvents typedNodeInstance)
                throw new ArgumentException("Node instance is not of type " + typeof(ArtNetEvents));
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
            if (index == 0)
            {
                return Client;
            }
            index -= 1;
            return null;
        }
    }
}
