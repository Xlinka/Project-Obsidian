using System.Linq;
using Elements.Core;
using FrooxEngine;

namespace Obsidian;

[DataModelType]
public enum ArithmeticMode
{
	Addition,
	Subtraction,
	Multiplication,
	Division
}

[Category(new string[] { "Obsidian/Transform/Drivers" })]
[GenericTypes(GenericTypesAttribute.Group.EnginePrimitives)]
public class MultiValueArithmeticDriver<T> : Component
{
	public static bool IsValidGenericType => Coder<T>.SupportsAddSub;

	public readonly FieldDrive<T> Target;

	public readonly Sync<ArithmeticMode> Mode;

	public readonly SyncList<Sync<T>> Values;

	protected override void OnChanges()
	{
		if (!Target.IsLinkValid || Values.Count == 0)
		{
			return;
		}
		if (Values.Contains(Target.Target))
		{
			// don't let the component drive itself, don't want a feedback loop
			Target.ReleaseLink();
			return;
		}
		T value = Values[0].Value;
		switch (Mode.Value)
		{
			case ArithmeticMode.Addition:
				foreach (Sync<T> sync in Values.Skip(1))
				{
					value = Coder<T>.Add(value, sync.Value);
				}
				break;
			case ArithmeticMode.Subtraction:
				foreach (Sync<T> sync in Values.Skip(1))
				{
					value = Coder<T>.Sub(value, sync.Value);
				}
				break;
			case ArithmeticMode.Multiplication:
				foreach (Sync<T> sync in Values.Skip(1))
				{
					value = Coder<T>.Mul(value, sync.Value);
				}
				break;
			case ArithmeticMode.Division:
				foreach (Sync<T> sync in Values.Skip(1))
				{
					value = Coder<T>.Div(value, sync.Value);
				}
				break;
		}
		Target.Target.Value = value;
	}
}