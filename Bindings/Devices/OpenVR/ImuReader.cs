using System;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using static OpenvrDataGetter.ImuReader;
using Valve.VR;
using ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Utility;

namespace OpenvrDataGetter.ProtoFluxBindings
{
    [Category(new string[] { "ProtoFlux/Runtimes/Execution/Nodes/Obsidian/OpenvrDataGetter" })]

    public class IMUDATA : FrooxEngine.ProtoFlux.Runtimes.Execution.AsyncActionNode<ExecutionContext>
    {
        public readonly SyncRef<INodeObjectOutput<string>> DevicePath;

        public readonly SyncRef<INodeOperation> OnOpened;
        public readonly SyncRef<INodeOperation> OnClosed;
        public readonly SyncRef<INodeOperation> OnFail;
        public readonly SyncRef<INodeOperation> OnData;

        public readonly NodeValueOutput<bool> IsOpened;
        public readonly NodeValueOutput<ErrorCode> FailReason;
        public readonly NodeValueOutput<double> FSampleTime;
        public readonly NodeValueOutput<double3> VAccel;
        public readonly NodeValueOutput<double3> VGyro;
        public readonly NodeValueOutput<Imu_OffScaleFlags> UnOffScaleFlags;
        public override Type NodeType => typeof(ImuReader);

        public ImuReader TypedNodeInstance { get; private set; }

        public override INode NodeInstance => TypedNodeInstance;

        public override int NodeInputCount => base.NodeInputCount + 1;

        public override int NodeOutputCount => base.NodeOutputCount + 6;

        public override int NodeImpulseCount => base.NodeImpulseCount + 4;

        public override N Instantiate<N>()
        {
            if (TypedNodeInstance != null)
            {
                throw new InvalidOperationException("Node has already been instantiated");
            }
            ImuReader ImuReaderNode = (TypedNodeInstance = new ImuReader());
            return ImuReaderNode as N;
        }


        protected override void AssociateInstanceInternal(INode node)
        {
            if (node is ImuReader typedNodeInstance)
            {
                TypedNodeInstance = typedNodeInstance;
                return;
            }
            throw new ArgumentException("Node instance is not of type ImuReader.");
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
                return DevicePath;
            }
            index -= 1;
            return null;
        }
        protected override INodeOutput GetOutputInternal(ref int index)
        {
            INodeOutput OutputInternal = base.GetOutputInternal(ref index);
            if (OutputInternal != null)
            {
                return OutputInternal;
            }
            switch (index)
            {
                case 0:
                    return IsOpened;
                case 1:
                    return FailReason;
                case 2:
                    return FSampleTime;
                case 3:
                    return VAccel;
                case 4:
                    return VGyro;
                case 5: 
                    return UnOffScaleFlags;
                default:
                    index -= 6;
                    return null;
            }
        }

        protected override ISyncRef GetImpulseInternal(ref int index)
        {
            ISyncRef impulseInternal = base.GetImpulseInternal(ref index);
            if (impulseInternal != null)
            {
                return impulseInternal;
            }
            switch (index)
            {
                case 0:
                    return OnOpened;
                case 1: 
                    return OnClosed;
                case 2: 
                    return OnFail;
                case 3: 
                    return OnData;
                default:
                    index -= 4;
                    return null;
            }
        }
      
    }
}
