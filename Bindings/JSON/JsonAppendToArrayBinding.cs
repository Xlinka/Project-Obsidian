﻿using System;
using Elements.Core;
using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json;
using Newtonsoft.Json.Linq;
using FrooxEngine.ProtoFlux;

    [Category(new string[] { "ProtoFlux/Runtimes/Execution/Nodes/Obsidian/Json" })]
    public class JsonAppendToArray<T> : FrooxEngine.ProtoFlux.Runtimes.Execution.ObjectFunctionNode<ExecutionContext, JArray>
    {
        public readonly SyncRef<INodeObjectOutput<JArray>> Array;
        public readonly SyncRef<INodeObjectOutput<T>> Object;

        public override Type NodeType => typeof(JsonAppendToArrayNode<T>);

        public JsonAppendToArrayNode<T> TypedNodeInstance { get; private set; }

        public override INode NodeInstance => TypedNodeInstance;

        public override int NodeInputCount => base.NodeInputCount + 2;

        public override N Instantiate<N>()
        {
            if (TypedNodeInstance != null)
            {
                throw new InvalidOperationException("Node has already been instantiated");
            }
        JsonAppendToArrayNode<T> jsonAppendToArrayInstance = (TypedNodeInstance = new JsonAppendToArrayNode<T>());
            return jsonAppendToArrayInstance as N;
        }

        protected override void AssociateInstanceInternal(INode node)
        {
            if (node is JsonAppendToArrayNode<T> typedNodeInstance)
            {
                TypedNodeInstance = typedNodeInstance;
                return;
            }
            throw new ArgumentException("Node instance is not of type " + typeof(JsonAppendToArrayNode<T>));
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
                    return Array;
                case 1:
                    return Object;
                default:
                    index -= 2;
                    return null;
            }
        }
    }

