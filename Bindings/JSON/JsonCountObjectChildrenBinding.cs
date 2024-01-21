using System;
using Elements.Core;
using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json;
using Newtonsoft.Json.Linq;
using FrooxEngine.ProtoFlux;


    [Category(new string[] { "ProtoFlux/Runtimes/Execution/Nodes/Obsidian/Json" })]
    public class JsonCountObjectChildrenBinding : FrooxEngine.ProtoFlux.Runtimes.Execution.ValueFunctionNode<ExecutionContext, int>
    {
        public readonly SyncRef<INodeObjectOutput<JObject>> Input;

        public override Type NodeType => typeof(JsonCountObjectChildrenNode);

        public JsonCountObjectChildrenNode TypedNodeInstance { get; private set; }

        public override INode NodeInstance => TypedNodeInstance;

        public override int NodeInputCount => base.NodeInputCount + 1;

        public override N Instantiate<N>()
        {
            if (TypedNodeInstance != null)
            {
                throw new InvalidOperationException("Node has already been instantiated");
            }
            JsonCountObjectChildrenNode jsonCountObjectChildrenInstance = (TypedNodeInstance = new JsonCountObjectChildrenNode());
            return jsonCountObjectChildrenInstance as N;
        }

        protected override void AssociateInstanceInternal(INode node)
        {
            if (node is JsonCountObjectChildrenNode typedNodeInstance)
            {
                TypedNodeInstance = typedNodeInstance;
                return;
            }
            throw new ArgumentException("Node instance is not of type " + typeof(JsonCountObjectChildrenNode));
        }

        public override void ClearInstance()
        {
            TypedNodeInstance = null;
        }

        protected override ISyncRef GetInputInternal(ref int index)
        {
            ISyncRef inputInternal = base.GetInputInternal(ref index);
            if (inputInternal != null)
            {
                return inputInternal;
            }
            if (index == 0)
            {
                return Input;
            }
            index -= 1;
            return null;
        }
    }
