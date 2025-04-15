using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace NIX.Editor.CodeGenerator
{
    public class InputFieldAnalyzer : IComponentAnalyzer
    {
        public List<ElementAnalysisResult> Analyze(GameObject go, string existingScript)
        {
            var inputFields = go.GetComponentsInChildren<TMP_InputField>(true);
            var existingLines = new HashSet<string>(existingScript.Split('\n').Select(l => l.Trim()));
            var result = new List<ElementAnalysisResult>();

            foreach (var field in inputFields)
            {
                string name = field.gameObject.name;
                string fieldName = $"_{char.ToLower(name[0])}{name.Substring(1)}";
                string fieldDeclaration = $"[SerializeField] private TMP_InputField {fieldName};";

                if (existingLines.Contains(fieldDeclaration)) continue;

                var builder = new ElementBuilder()
                    .WithField("TMP_InputField", fieldName);

                result.Add(builder.Build());
            }

            return result;
        }
    }
}