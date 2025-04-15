using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace NIX.Editor.CodeGenerator
{
    public class DropdownAnalyzer : IComponentAnalyzer
    {
        public List<ElementAnalysisResult> Analyze(GameObject go, string existingScript)
        {
            var dropdowns = go.GetComponentsInChildren<TMP_Dropdown>(true);
            var existingLines = new HashSet<string>(existingScript.Split('\n').Select(l => l.Trim()));
            var result = new List<ElementAnalysisResult>();

            foreach (var dropdown in dropdowns)
            {
                string name = dropdown.gameObject.name;
                string fieldName = $"_{char.ToLower(name[0])}{name.Substring(1)}";
                string fieldDeclaration = $"[SerializeField] private TMP_Dropdown {fieldName};";

                if (existingLines.Contains(fieldDeclaration)) continue;

                var builder = new ElementBuilder()
                    .WithField("TMP_Dropdown", fieldName);

                result.Add(builder.Build());
            }

            return result;
        }
    }
}