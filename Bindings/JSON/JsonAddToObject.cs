using System;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json;
using Newtonsoft.Json.Linq;


    [Category(new string[] { "ProtoFlux/Runtimes/Execution/Nodes/Obsidian/Json" })]
    public class JsonAddToObject<T> : FrooxEngine.ProtoFlux.Runtimes.Execution.ObjectFunctionNode<FrooxEngineContext, JObject>
    {
        public readonly SyncRef<INodeObjectOutput<JObject>> Input;
        public readonly SyncRef<INodeObjectOutput<string>> Tag;
        public readonly SyncRef<INodeObjectOutput<T>> Object;

        public override Type NodeType => typeof(JsonAddToObjectNode<T>);

        public JsonAddToObjectNode<T> TypedNodeInstance { get; private set; }

        public override INode NodeInstance => TypedNodeInstance;

        public override int NodeInputCount => base.NodeInputCount + 3;

        public override N Instantiate<N>()
        {
            if (TypedNodeInstance != null)
            {
                throw new InvalidOperationException("Node has already been instantiated");
            }
        JsonAddToObjectNode<T> jsonAddToObjectInstance = (TypedNodeInstance = new JsonAddToObjectNode<T>());
            return jsonAddToObjectInstance as N;
        }

        protected override void AssociateInstanceInternal(INode node)
        {
            if (node is JsonAddToObjectNode<T> typedNodeInstance)
            {
                TypedNodeInstance = typedNodeInstance;
                return;
            }
            throw new ArgumentException("Node instance is not of type " + typeof(JsonAddToObjectNode<T>));
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
                    return Tag;
                case 2:
                    return Object;
                default:
                    index -= 3;
                    return null;
            }
        }
    }
