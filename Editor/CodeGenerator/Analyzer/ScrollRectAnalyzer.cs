using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace NIX.Editor.CodeGenerator
{
    public class ScrollRectAnalyzer : IComponentAnalyzer
    {
        public List<ElementAnalysisResult> Analyze(GameObject go, string existingScript)
        {
            var scrollRects = ComponentUtils.GetComponentsInChildren<ScrollRect>(go, new List<Type>()
            {
                typeof(Dropdown)
            });
            var existingLines = new HashSet<string>(existingScript.Split('\n').Select(l => l.Trim()));
            var result = new List<ElementAnalysisResult>();

            foreach (var scroll in scrollRects)
            {
                string name = scroll.gameObject.name;
                string fieldName = $"_{char.ToLower(name[0])}{name.Substring(1)}";
                string fieldDeclaration = $"[SerializeField] private ScrollRect {fieldName};";

                if (existingLines.Contains(fieldDeclaration)) continue;

                var builder = new ElementBuilder()
                    .WithField("ScrollRect", fieldName);

                result.Add(builder.Build());
            }

            return result;
        }
    }
}