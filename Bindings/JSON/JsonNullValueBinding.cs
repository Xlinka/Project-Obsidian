using System;
using Elements.Core;
using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using Newtonsoft.Json.Linq;
using ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json;

[Category(new string[] { "ProtoFlux/Runtimes/Execution/Nodes/Obsidian/Json" })]
    public class JsonNullValue : FrooxEngine.ProtoFlux.Runtimes.Execution.ObjectFunctionNode<ExecutionContext, JToken>
    {
        public override Type NodeType => typeof(JsonNullValueNode);

        public JsonNullValueNode TypedNodeInstance { get; private set; }

        public override INode NodeInstance => TypedNodeInstance;

        public override int NodeInputCount => 0;

        public override N Instantiate<N>()
        {
            if (TypedNodeInstance != null)
                throw new InvalidOperationException("Node has already been instantiated");

            TypedNodeInstance = new JsonNullValueNode();
            return TypedNodeInstance as N;
        }

        protected override void AssociateInstanceInternal(INode node)
        {
            if (node is JsonNullValueNode typedNodeInstance)
                TypedNodeInstance = typedNodeInstance;
            else
                throw new ArgumentException("Node instance is not of type " + typeof(JsonNullValueNode));
        }

        public override void ClearInstance()
        {
            TypedNodeInstance = null;
        }
    }

