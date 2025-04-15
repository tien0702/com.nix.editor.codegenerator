using System.Collections.Generic;
using UnityEngine;

namespace NIX.Editor.CodeGenerator
{
    public interface IComponentAnalyzer
    {
        List<ElementAnalysisResult> Analyze(GameObject go, string script);
    }
}