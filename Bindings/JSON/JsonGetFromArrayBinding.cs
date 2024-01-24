using System;
using Elements.Core;
using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json;
using Newtonsoft.Json.Linq;
using FrooxEngine.ProtoFlux;


    [Category(new string[] { "ProtoFlux/Runtimes/Execution/Nodes/Obsidian/Json" })]
    public class JsonGetFromArray<T> : FrooxEngine.ProtoFlux.Runtimes.Execution.ObjectFunctionNode<ExecutionContext, T>
    {
        public readonly SyncRef<INodeObjectOutput<JArray>> Input;
        public readonly SyncRef<INodeObjectOutput<int>> Index;

        public override Type NodeType => typeof(JsonGetFromArrayNode<T>);

        public JsonGetFromArrayNode<T> TypedNodeInstance { get; private set; }

        public override INode NodeInstance => TypedNodeInstance;

        public override int NodeInputCount => base.NodeInputCount + 2;

        public override N Instantiate<N>()
        {
            if (TypedNodeInstance != null)
            {
                throw new InvalidOperationException("Node has already been instantiated");
            }
            JsonGetFromArrayNode<T> jsonGetFromArrayInstance = (TypedNodeInstance = new JsonGetFromArrayNode<T>());
            return jsonGetFromArrayInstance as N;
        }

        protected override void AssociateInstanceInternal(INode node)
        {
            if (node is JsonGetFromArrayNode<T> typedNodeInstance)
            {
                TypedNodeInstance = typedNodeInstance;
                return;
            }
            throw new ArgumentException("Node instance is not of type " + typeof(JsonGetFromArrayNode<T>));
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
            switch (index)
            {
                case 0:
                    return Input;
                case 1:
                    return Index;
                default:
                    index -= 2;
                    return null;
            }
        }
    }

