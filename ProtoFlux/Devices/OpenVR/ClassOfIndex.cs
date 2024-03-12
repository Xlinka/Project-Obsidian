using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using Valve.VR;

namespace OpenvrDataGetter.Nodes;

public class ClassOfIndex : ValueFunctionNode<ExecutionContext, ETrackedDeviceClass>
{
    public ValueInput<uint> Index;

    protected override ETrackedDeviceClass Compute(ExecutionContext context)
    {
        uint index = Index.Evaluate(context);
        return OpenVR.System?.GetTrackedDeviceClass(index) ?? ETrackedDeviceClass.Invalid;
    }
}