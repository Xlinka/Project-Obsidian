using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using Valve.VR;

namespace OpenvrDataGetter.ProtoFlux
{
    [NodeName("IndexOfRole")]
    [Category("ProtoFlux/OpenVR")]
    public class IndexOfRole : ValueFunctionNode<FrooxEngineContext, uint>
    {
        public readonly ValueInput<ETrackedControllerRole> Role;
        protected override uint Compute(FrooxEngineContext context)
        {
            var role = Role.Evaluate(context);
            return OpenVR.System.GetTrackedDeviceIndexForControllerRole(role);
        }
    }
}
