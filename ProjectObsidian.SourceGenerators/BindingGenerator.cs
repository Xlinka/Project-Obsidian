using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerators
{
    public static class StringHelpers
    {
        public static string TrimEnds(this string str, int fromStart, int fromEnd) =>
            str.Substring(fromStart, str.Length - fromEnd - fromStart);
    }
    [Generator]
    public class BindingGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            foreach (var tree in context.Compilation.SyntaxTrees)
            {
                var result = new StringBuilder();
                var root = tree.GetCompilationUnitRoot();
                var walker = new BindingGeneratorSyntaxWalker();
                walker.Visit(root);
                result.Append(walker.Result);

                var res = result.ToString();

                if (!string.IsNullOrWhiteSpace(res))
                    context.AddSource($"{walker.BaseName}Bindings.g.cs", result.ToString());
            }
        }
    }

    public class BindingGeneratorSyntaxWalker : CSharpSyntaxWalker
    {
        private class OrderedCount
        {
            private readonly string CountVariableName;
            private readonly string MethodName;
            private readonly string MethodReturnType;
            public string CountOverride => VariableNames.Count == 0
                ? ""
                : $"    public override int {CountVariableName} => base.{CountVariableName} + {VariableNames.Count};\n";

            public void Add(string value) => VariableNames.Add(value);

            public string GetOverride
            {
                get
                {
                    if (VariableNames.Count == 0) return "";
                    var str = $@"
    protected override {MethodReturnType} {MethodName}(ref int index)
    {{
        var item = base.{MethodName}(ref index);
        if (item != null) return item;
        switch (index)
        {{
";
                    for (var index = 0; index < VariableNames.Count; index++)
                    {
                        var order = VariableNames[index];
                        str += $"            case {index}:\n                return this.{order};\n";
                    }
                    str += $@"
            default:
                index -= {VariableNames.Count};
                return null;
        }}
    }}";
                    return str;
                }
            }
            
            public readonly List<string> VariableNames = new();

            public OrderedCount(string countVariableName, string methodName, string methodReturnType)
            {
                CountVariableName = countVariableName;
                MethodName = methodName;
                MethodReturnType = methodReturnType;
            }
        }
        
        public const string BindingPrefix = "FrooxEngine.";
        public const string FluxPrefix = "ProtoFlux.Runtimes.Execution.";

        //TODO: add more, this is not all of the valid node types
        public static readonly string[] ValidNodeTypes =
        {
            "NestedNode",
            "VoidNode", 
            
            "ObjectFunctionNode", 
            "ValueFunctionNode", 

            "ActionNode", 
            "ActionFlowNode",
            "ActionBreakableFlowNode",
            
            "AsyncActionNode", 
            "AsyncActionFlowNode", 
            "AsyncActionBreakableFlowNode",
        };

        private string UsingEnumerate =>
            _usingDeclarations
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Aggregate("", (current, u) => current + $"using {u};\n");

        private readonly OrderedCount _inputCount = new("NodeInputCount", "GetInputInternal", "ISyncRef");
        private readonly OrderedCount _outputCount = new("NodeOutputCount", "GetOutputInternal", "INodeOutput");
        private readonly OrderedCount _impulseCount = new("NodeImpulseCount", "GetImpulseInternal", "ISyncRef");
        private readonly OrderedCount _operationCount = new("NodeOperationCount", "GetOperationInternal", "INodeOperation");
        
        private readonly OrderedCount _inputListCount = new("NodeInputListCount", "GetInputListInternal", "ISyncList");
        private readonly OrderedCount _outputListCount = new("NodeOutputListCount", "GetOutputListInternal", "ISyncList");
        private readonly OrderedCount _impulseListCount = new("NodeImpulseListCount", "GetImpulseListInternal", "ISyncList");
        private readonly OrderedCount _operationListCount = new("NodeOperationListCount", "GetOperationListInternal", "ISyncList");

        private IEnumerable<OrderedCount> _counts => new[]
            { _inputCount, _outputCount, _impulseCount, _operationCount, 
                _inputListCount, _outputListCount, _impulseListCount, _operationListCount };

        private string CountOverride => string.Concat(_counts.Select(i => i.CountOverride));
        private string GetOverride => string.Concat(_counts.Select(i => i.GetOverride));
        
        private List<string> _declarations = [];
        private string Declarations => string.Concat(_declarations);

        public string Result
        {
            get
            {
                if (!_valid) return "";

                var str = @$"//generated
{UsingEnumerate}
using {_currentNameSpace};

namespace {BindingPrefix}{_currentNameSpace};

[Category(new string[] {{""ProtoFlux/Runtimes/Execution/Nodes/{_category}""}})]
public partial class {_fullName} : {_baseType}
{{
{Declarations}
{_nodeNameOverride}
    public override System.Type NodeType => typeof (global::{_currentNameSpace}.{_fullName});
    public global::{_currentNameSpace}.{_fullName} TypedNodeInstance {{ get; private set; }}
    public override INode NodeInstance => (INode)this.TypedNodeInstance;
    public override void ClearInstance() => this.TypedNodeInstance = null;
{CountOverride}
    public override N Instantiate<N>()
    {{
        if (this.TypedNodeInstance != null) throw new System.InvalidOperationException(""Node has already been instantiated"");
        var localVar = new global::{_currentNameSpace}.{_fullName}();
        this.TypedNodeInstance = localVar;
        return localVar as N;
    }}
    protected override void AssociateInstanceInternal(INode node) => this.TypedNodeInstance = node is global::{_currentNameSpace}.{_fullName} localVar ? localVar : throw new System.ArgumentException(""Node instance is not of type "" + typeof (global::{_currentNameSpace}.{_fullName})?.ToString());
{GetOverride}
}}";
                return str;
            }
        }

        private readonly List<string> _usingDeclarations = [""];
        private bool _valid;
        private string _currentNameSpace;
        private string _fullName;
        private string _additionalName = "";
        public string BaseName;
        private string _baseType;
        private string _fullBaseType;
        private string _match;
        private string _category;
        private string _nodeNameOverride = "";

        private bool TypedFieldDetection(string type, string name, string targetTypeName, string declarationFormat, OrderedCount counter)
        {
            if (!type.Contains(targetTypeName)) return false;
            var t = type.TrimEnds((targetTypeName + "<").Length, 1);
            counter.Add(name);
            _declarations.Add(string.Format("    new public readonly " + declarationFormat + " {0};\n", name, t));
            return true;
        }
        private bool UntypedFieldDetection(string type, string name, string targetTypeName, string declarationFormat, OrderedCount counter)
        {
            if (!type.Contains(targetTypeName)) return false;
            counter.Add(name);
            _declarations.Add(string.Format("    new public readonly " + declarationFormat + " {0};\n", name));
            return true;
        }
        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            var type = node.Declaration.Type.ToString();
            var name = node.Declaration.Variables.First().ToString();
            
            //inputs
            TypedFieldDetection(type, name, "ObjectInput", "SyncRef<INodeObjectOutput<{1}>>", _inputCount);
            TypedFieldDetection(type, name, "ObjectArgument", "SyncRef<INodeObjectOutput<{1}>>", _inputCount);
            TypedFieldDetection(type, name, "ValueInput", "SyncRef<INodeValueOutput<{1}>>", _inputCount);
            TypedFieldDetection(type, name, "ValueArgument", "SyncRef<INodeValueOutput<{1}>>", _inputCount);
            
            //outputs
            TypedFieldDetection(type, name, "ObjectOutput", "NodeObjectOutput<{1}>", _outputCount);
            TypedFieldDetection(type, name, "ValueOutput", "NodeValueOutput<{1}>", _outputCount);
            
            //impulses
            if (!UntypedFieldDetection(type, name, "AsyncCall", "SyncRef<INodeOperation>", _impulseCount))
                UntypedFieldDetection(type, name, "Call", "SyncRef<ISyncNodeOperation>", _impulseCount);
            UntypedFieldDetection(type, name, "Continuation", "SyncRef<INodeOperation>", _impulseCount);
            UntypedFieldDetection(type, name, "AsyncResumption", "SyncRef<INodeOperation>", _impulseCount);
            
            //operations
            UntypedFieldDetection(type, name, "Operation", "SyncNodeOperation", _operationCount);
            
            //lists
            
            //input lists
            TypedFieldDetection(type, name, "ValueInputList", "SyncRefList<INodeValueOutput<{1}>>", _inputListCount);
            
            //output lists
            TypedFieldDetection(type, name, "ObjectInputList", "SyncRefList<INodeObjectOutput<{1}>>", _outputListCount);
            
            //impulse lists
            UntypedFieldDetection(type, name, "ContinuationList", "SyncRefList<INodeOperation>", _impulseListCount);
            
            //operation lists
            UntypedFieldDetection(type, name, "SyncOperationList", "SyncList<SyncNodeOperation>", _operationListCount);
            
            base.VisitFieldDeclaration(node);
        }

        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            _currentNameSpace = node.Name.ToString();
            base.VisitNamespaceDeclaration(node);
        }

        public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
        {
            _currentNameSpace = node.Name.ToString();
            base.VisitFileScopedNamespaceDeclaration(node);
        }

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            if (node.Name is not null) _usingDeclarations.Add(node.Name.ToString());
            base.VisitUsingDirective(node);
        }
        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (node.BaseList is null)
            {
                base.VisitClassDeclaration(node);
                return;
            }

            var baseName = node.Identifier.Text;
            var fullName = baseName;

            if (node.TypeParameterList is not null)
            {
                _additionalName += "<";
                _additionalName = node.TypeParameterList.Parameters.Aggregate(_additionalName,
                    (current, p) => current + $"{p.Identifier.Text},");
                _additionalName = _additionalName.Substring(0, _additionalName.Length - 1); //remove last ,
                _additionalName += ">";

                fullName += _additionalName;
            }
            
            BaseName = baseName;
            _fullName = fullName;
            
            var firstBaseType = node.BaseList.Types.First();
            var baseTypeName = firstBaseType.Type.ToString();
            
            _baseType = baseTypeName;

            if (!node.AttributeLists.Any())
            {
                base.VisitClassDeclaration(node);
                return;
            }

            var find = node.AttributeLists.SelectMany(i => i.Attributes)
                .FirstOrDefault(i => i.Name.ToString() == "NodeCategory");

            if (find?.ArgumentList is null)
            {
                base.VisitClassDeclaration(node);
                return;
            }
            
            _category = find.ArgumentList.Arguments.First().ToString().TrimEnds(1,1);
            
            var findName = node.AttributeLists.SelectMany(i => i.Attributes)
                .FirstOrDefault(i => i.Name.ToString() == "NodeName");


            if (findName?.ArgumentList != null)
                _nodeNameOverride =
                    $"    public override string NodeName => {findName.ArgumentList.Arguments.First().ToString()};";
            
            foreach (var u in _usingDeclarations)
            {
                var fullNameSpace = "";
                if (string.IsNullOrEmpty(u))
                    fullNameSpace = baseTypeName;
                else
                    fullNameSpace = u + "." + baseTypeName;

                var match = ValidNodeTypes.FirstOrDefault(i => fullNameSpace.StartsWith(FluxPrefix + i));

                if (match is null) continue;
                
                _match = match;
                _fullBaseType = fullNameSpace;
                _valid = true;
                base.VisitClassDeclaration(node);
                return;
            }
            base.VisitClassDeclaration(node);
        }
    }
}