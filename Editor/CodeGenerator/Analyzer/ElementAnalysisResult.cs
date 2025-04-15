using System.Collections.Generic;

namespace NIX.Editor.CodeGenerator
{
    public class ElementAnalysisResult
    {
        public string FieldName;
        public string FieldDeclaration;

        public string AwakeInitialization;

        public Dictionary<string, string> Methods = new();
        // key: method name, value: content
    }
}