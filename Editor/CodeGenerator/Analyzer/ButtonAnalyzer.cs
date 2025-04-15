using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace NIX.Editor.CodeGenerator
{
    public class ButtonAnalyzer : IComponentAnalyzer
    {
        public List<ElementAnalysisResult> Analyze(GameObject go, string existingScript)
        {
            var buttons = go.GetComponentsInChildren<Button>(true);
            var existingLines = new HashSet<string>(existingScript.Split('\n').Select(l => l.Trim()));
            var result = new List<ElementAnalysisResult>();

            foreach (var btn in buttons)
            {
                string name = btn.gameObject.name;
                string fieldName = $"_{char.ToLower(name[0])}{name.Substring(1)}";
                string methodName = $"On{name}";

                string fieldDeclaration = $"[SerializeField] private Button {fieldName};";
                string awakeLine = $"{fieldName}.onClick.AddListener({methodName});";
                string methodBody = $"private void {methodName}()\n{{\n\n}}";

                if (existingLines.Contains(fieldDeclaration) && existingLines.Contains(awakeLine) &&
                    existingLines.Any(l => l.Contains($"void {methodName}(")))
                    continue;

                var element = new ElementAnalysisResult
                {
                    FieldName = fieldName,
                    FieldDeclaration = fieldDeclaration,
                    AwakeInitialization = !existingLines.Contains(awakeLine) ? awakeLine : null,
                };

                if (!existingLines.Any(l => l.Contains($"void {methodName}(")))
                    element.Methods.Add(methodName, methodBody);

                result.Add(element);
            }

            return result;
        }
    }
}