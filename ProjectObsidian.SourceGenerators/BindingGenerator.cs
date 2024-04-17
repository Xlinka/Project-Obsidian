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
            var index = 0;
            foreach (var tree in context.Compilation.SyntaxTrees)
            {
                var result = new StringBuilder();
                var root = tree.GetCompilationUnitRoot();
                var walker = new BindingGeneratorSyntaxWalker();
                walker.Visit(root);
                result.Append(walker.Result);

                var res = result.ToString();

                if (!string.IsNullOrWhiteSpace(res))
                {
                    context.AddSource($"{walker.BaseName}Bindings.g.cs", result.ToString());
                    index++;
                }
            }
        }
    }

    public class BindingGeneratorSyntaxWalker : CSharpSyntaxWalker
    {
        public const string BindingPrefix = "FrooxEngine.";
        public const string FluxPrefix = "ProtoFlux.Runtimes.Execution.";

        //TODO: add more, this is not all of the valid node types
        public static readonly string[] ValidNodeTypes = { "VoidNode", "ObjectFunctionNode", "ValueFunctionNode" };

        private string UsingEnumerate =>
            _usingDeclarations
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Aggregate("", (current, u) => current + $"using {u};\n");

        private string ObjectInputDeclaration => _objectInputs.Aggregate("",
            (current, pair) =>
                current + $"    new public readonly SyncRef<INodeObjectOutput<{pair.Value}>> {pair.Key};\n");

        private string ObjectOutputDeclaration => _objectOutputs.Aggregate("",
            (current, pair) => 
                current + $"    new public readonly NodeObjectOutput<{pair.Value}> {pair.Key};\n");
        
        private string ValueInputDeclaration => _valueInputs.Aggregate("",
            (current, pair) =>
                current + $"    new public readonly SyncRef<INodeValueOutput<{pair.Value}>> {pair.Key};\n");

        private string ValueOutputDeclaration => _valueOutputs.Aggregate("",
            (current, pair) => 
                current + $"    new public readonly NodeValueOutput<{pair.Value}> {pair.Key};\n");
        
        private string ImpulseDeclaration => _impulseOrder.Aggregate("",
            (current, name) =>
                current + $"    new public readonly SyncRef<INodeOperation> {name};\n");

        private string InputCountOverride => _inputOrder.Count == 0
            ? ""
            : $"    public override int NodeInputCount => base.NodeInputCount + {_inputOrder.Count};";

        private string OutputCountOverride => _outputOrder.Count == 0
            ? ""
            : $"    public override int NodeOutputCount => base.NodeOutputCount + {_outputOrder.Count};";
        
        private string ImpulseCountOverride => _impulseOrder.Count == 0
            ? ""
            : $"    public override int NodeImpulseCount => base.NodeImpulseCount + {_impulseOrder.Count};";

        private string GetInputInternalOverride
        {
            get
            {
                if (_inputOrder.Count == 0) return "";
                var str = $$"""
                            
                                protected override ISyncRef GetInputInternal(ref int index)
                                {
                                    var inputInternal = base.GetInputInternal(ref index);
                                    if (inputInternal != null) return inputInternal;
                                    switch (index)
                                    {

                            """;
                for (var index = 0; index < _inputOrder.Count; index++)
                {
                    var order = _inputOrder[index];
                    str += $"            case {index}:\n                return this.{order};\n";
                }
                str += $$"""
                         
                                     default:
                                         index -= {{_inputOrder.Count}};
                                         return null;
                                 }
                             }
                         """;
                return str;
            }
        }
        
        private string GetImpulseInternalOverride
        {
            get
            {
                if (_impulseOrder.Count == 0) return "";
                var str = $$"""
                            
                                protected override ISyncRef GetImpulseInternal(ref int index)
                                {
                                    var impulseInternal = base.GetImpulseInternal(ref index);
                                    if (impulseInternal != null) return impulseInternal;
                                    switch (index)
                                    {

                            """;
                for (var index = 0; index < _impulseOrder.Count; index++)
                {
                    var order = _impulseOrder[index];
                    str += $"            case {index}:\n                return this.{order};\n";
                }
                str += $$"""
                         
                                     default:
                                         index -= {{_impulseOrder.Count}};
                                         return null;
                                 }
                             }
                         """;
                return str;
            }
        }
        private string GetOutputInternalOverride
        {
            get
            {
                if (_outputOrder.Count == 0) return "";
                var str = $$"""
                            
                                protected override INodeOutput GetOutputInternal(ref int index)
                                {
                                    var outputInternal = base.GetOutputInternal(ref index);
                                    if (outputInternal != null) return outputInternal;
                                    switch (index)
                                    {

                            """;
                for (var index = 0; index < _outputOrder.Count; index++)
                {
                    var order = _outputOrder[index];
                    str += $"            case {index}:\n                return this.{order};\n";
                }
                str += $$"""
                         
                                     default:
                                         index -= {{_outputOrder.Count}};
                                         return null;
                                 }
                             }
                         """;
                return str;
            }
        }

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
{ObjectInputDeclaration}
{ObjectOutputDeclaration}
{ValueInputDeclaration}
{ValueOutputDeclaration}
{ImpulseDeclaration}
    public override System.Type NodeType => typeof ({_currentNameSpace}.{_fullName});
    public {_currentNameSpace}.{_fullName} TypedNodeInstance {{ get; private set; }}
    public override INode NodeInstance => (INode)this.TypedNodeInstance;
    public override void ClearInstance() => this.TypedNodeInstance = null;
{InputCountOverride}
{OutputCountOverride}
{ImpulseCountOverride}
    public override N Instantiate<N>()
    {{
        if (this.TypedNodeInstance != null) throw new System.InvalidOperationException(""Node has already been instantiated"");
        var localVar = new {_currentNameSpace}.{_fullName}();
        this.TypedNodeInstance = localVar;
        return localVar as N;
    }}
    protected override void AssociateInstanceInternal(INode node) => this.TypedNodeInstance = node is {_currentNameSpace}.{_fullName} localVar ? localVar : throw new System.ArgumentException(""Node instance is not of type "" + typeof ({_currentNameSpace}.{_fullName})?.ToString());
{GetInputInternalOverride}
{GetOutputInternalOverride}
{GetImpulseInternalOverride}
}}";
                return str;
            }
        }

        private readonly List<string> _usingDeclarations = [""];
        private bool _valid;
        private string _currentNameSpace;
        private string _fullName;
        public string BaseName;
        private string _baseType;
        private string _fullBaseType;
        private string _match;
        private string _category;

        private readonly Dictionary<string, string> _objectInputs = new();
        private readonly Dictionary<string, string> _valueInputs = new();
        
        private readonly Dictionary<string, string> _objectArguments = new();
        private readonly Dictionary<string, string> _valueArguments = new();
        
        private readonly Dictionary<string, string> _objectOutputs = new();
        private readonly Dictionary<string, string> _valueOutputs = new();

        private readonly List<string> _inputOrder = new();
        private readonly List<string> _outputOrder = new();

        private readonly List<string> _impulseOrder = new();

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            var type = node.Declaration.Type.ToString();
            var name = node.Declaration.Variables.First().ToString();
            if (type.Contains("ObjectInput<"))
            {
                _objectInputs.Add(name, type.TrimEnds("ObjectInput<".Length, 1));
                _inputOrder.Add(name);
            }
            if (type.Contains("ObjectOutput<"))
            {
                _objectOutputs.Add(name, type.TrimEnds("ObjectOutput<".Length, 1));
                _outputOrder.Add(name);
            }
            
            if (type.Contains("ValueInput<"))
            {
                _valueInputs.Add(name, type.TrimEnds("ValueInput<".Length, 1));
                _inputOrder.Add(name);
            }
            if (type.Contains("ValueOutput<"))
            {
                _valueOutputs.Add(name, type.TrimEnds("ValueOutput<".Length, 1));
                _outputOrder.Add(name);
            }

            if (type.EndsWith("Continuation"))
            {
                _impulseOrder.Add(name);
            }
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
                fullName += "<";
                fullName = node.TypeParameterList.Parameters.Aggregate(fullName,
                    (current, p) => current + $"{p.Identifier.Text},");
                fullName = fullName.Substring(0, fullName.Length - 1); //remove last ,
                fullName += ">";
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

            if (find is null || find.ArgumentList is null)
            {
                base.VisitClassDeclaration(node);
                return;
            }

            _category = find.ArgumentList.Arguments.First().ToString();
            _category = _category.Substring(1, _category.Length - 2);
            
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