using System.Collections.Generic;

namespace NIX.Editor.CodeGenerator
{
    public class ClassAnalysisResult
    {
        public string ClassName;
        public List<ElementAnalysisResult> Elements = new();
    }
}