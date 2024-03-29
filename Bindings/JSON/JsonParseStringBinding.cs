﻿using System;
using Elements.Core;
using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json;
using Newtonsoft.Json.Linq;
using FrooxEngine.ProtoFlux;


[Category(new string[] { "ProtoFlux/Runtimes/Execution/Nodes/Obsidian/Json" })]
public class JsonParseString : FrooxEngine.ProtoFlux.Runtimes.Execution.ObjectFunctionNode<ExecutionContext, JArray>
{
    public readonly SyncRef<INodeObjectOutput<string>> Input;

    public override Type NodeType => typeof(JsonParseStringNode);

    public JsonParseStringNode TypedNodeInstance { get; private set; }

    public override INode NodeInstance => TypedNodeInstance;

    public override int NodeInputCount => base.NodeInputCount + 1;

    public override N Instantiate<N>()
    {
        if (TypedNodeInstance != null)
        {
            throw new InvalidOperationException("Node has already been instantiated");
        }
        JsonParseStringNode jsonParseStringArrayInstance = (TypedNodeInstance = new JsonParseStringNode());
        return jsonParseStringArrayInstance as N;
    }

    protected override void AssociateInstanceInternal(INode node)
    {
        if (node is JsonParseStringNode typedNodeInstance)
        {
            TypedNodeInstance = typedNodeInstance;
            return;
        }
        throw new ArgumentException("Node instance is not of type " + typeof(JsonParseStringNode));
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
