namespace NIX.Editor.CodeGenerator
{
    public class ElementBuilder
    {
        private readonly ElementAnalysisResult _result = new();

        public ElementBuilder WithField(string fieldType, string name)
        {
            _result.FieldName = name;
            _result.FieldDeclaration = $"[SerializeField] private {fieldType} {name};";
            return this;
        }

        public ElementBuilder WithMethods(string name, string body)
        {
            _result.Methods[name] = body;
            return this;
        }

        public ElementAnalysisResult Build() => _result;
    }
}