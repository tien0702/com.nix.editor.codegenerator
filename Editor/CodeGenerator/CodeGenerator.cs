using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace NIX.Editor.CodeGenerator
{
    public static class CodeGenerator
    {
        [MenuItem("GameObject/NIX/GenCode", false, 10)]
        public static void GenCode()
        {
            var go = Selection.activeGameObject;
            if (go == null)
            {
                Debug.LogWarning("No GameObject selected.");
                return;
            }

            string className = go.name;
            string scriptType = GetScriptTypeFromName(className);
            string templatePath = GetTemplatePath(scriptType);

            if (!File.Exists(templatePath))
            {
                Debug.LogError($"Template not found: {templatePath}");
                return;
            }

            string outputPath = $"Assets/Scripts/Game/{className}/{className}.cs";
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            string finalScript = "";

            if (File.Exists(outputPath))
            {
                string existingContent = File.ReadAllText(outputPath);
                finalScript = InsertNewFieldsAndMethods(existingContent, go);
            }
            else
            {
                string template = File.ReadAllText(templatePath).Replace("#CLASSNAME#", className);
                finalScript = InsertNewFieldsAndMethods(template, go);
            }

            File.WriteAllText(outputPath, finalScript);
            AssetDatabase.Refresh();

            MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(outputPath);
            AutoAssignReferences(go, monoScript);

            Debug.Log($"Script {(File.Exists(outputPath) ? "updated" : "created")}: {outputPath}");
        }

        private static string GetScriptTypeFromName(string name)
        {
            if (name.EndsWith("Panel")) return "Panel";
            if (name.EndsWith("Popup")) return "Popup";
            if (name.EndsWith("Ctrl")) return "Ctrl";
            return "Ctrl";
        }

        private static string GetTemplatePath(string type)
        {
            // Find the path to this script file
            string[] guids = UnityEditor.AssetDatabase.FindAssets("CodeGenerator t:Script");
            foreach (string guid in guids)
            {
                string scriptPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(scriptPath) == "CodeGenerator")
                {
                    // Build path to Templates folder
                    string baseFolder = System.IO.Path.GetDirectoryName(scriptPath); // .../Editor/CodeGen
                    string templatesFolder = System.IO.Path.Combine(baseFolder, "Templates");

                    // Try .txt first
                    string txtPath =
                        System.IO.Path.GetFullPath(System.IO.Path.Combine(templatesFolder, $"{type}Template.txt"));
                    if (System.IO.File.Exists(txtPath))
                        return txtPath;

                    // Try .json fallback
                    string jsonPath =
                        System.IO.Path.GetFullPath(System.IO.Path.Combine(templatesFolder, $"{type}Template.json"));
                    if (System.IO.File.Exists(jsonPath))
                        return jsonPath;

                    Debug.LogError(
                        $"[CodeGen] Neither .txt nor .json template found for type: {type} in path: {templatesFolder}");
                    return null;
                }
            }

            Debug.LogError("[CodeGen] Cannot locate CodeGenerator script to resolve template path.");
            return null;
        }

        private static string InsertNewFieldsAndMethods(string script, GameObject go)
        {
            var result = Analyze(go, script, new List<IComponentAnalyzer>()
            {
                new ButtonAnalyzer(),
                new ScrollRectAnalyzer(),
                new DropdownAnalyzer(),
                new InputFieldAnalyzer()
            });

            var updatedScript = ApplyAnalysisResult(script, result);

            return CleanUpEmptyRegions(updatedScript);
        }

        public static ClassAnalysisResult Analyze(GameObject go, string existingScript,
            List<IComponentAnalyzer> analyzers)
        {
            var result = new ClassAnalysisResult
            {
                ClassName = go.name
            };

            foreach (var analyzer in analyzers)
            {
                var elements = analyzer.Analyze(go, existingScript);
                result.Elements.AddRange(elements);
            }

            return result;
        }

        public static string ApplyAnalysisResult(string script, ClassAnalysisResult result)
        {
            var fields = new StringBuilder();
            var awakes = new StringBuilder();
            var methods = new StringBuilder();

            foreach (var element in result.Elements)
            {
                if (!string.IsNullOrEmpty(element.FieldDeclaration))
                    fields.AppendLine(element.FieldDeclaration);

                if (!string.IsNullOrEmpty(element.AwakeInitialization))
                    awakes.AppendLine("        " + element.AwakeInitialization);

                foreach (var method in element.Methods.Values)
                {
                    methods.AppendLine(method);
                    methods.AppendLine(); // spacing
                }
            }

            script = CodeGenerator.InsertIntoRegion(script, RegionFactory.GetRegionTag(RegionType.References),
                fields.ToString());
            script = CodeGenerator.InsertIntoRegion(script, RegionFactory.GetRegionTag(RegionType.Methods),
                methods.ToString());
            script = CodeGenerator.InsertIntoAwakeFunction(script, awakes.ToString());

            return script;
        }


        public static string InsertIntoRegion(string script, string regionTag, string content)
        {
            if (string.IsNullOrEmpty(content)) return script;

            int regionStart = script.IndexOf(regionTag);
            if (regionStart == -1) return script;

            int insertPoint = script.IndexOf("#endregion", regionStart);
            if (insertPoint == -1) return script;

            return script.Insert(insertPoint, "\n" + content);
        }

        public static string InsertIntoAwakeFunction(string script, string contentToAdd)
        {
            if (string.IsNullOrEmpty(contentToAdd)) return script;

            const string awakeHeader = "void Awake()";
            int awakeIndex = script.IndexOf(awakeHeader);

            if (awakeIndex == -1)
            {
                // Add new Awake if it doesn't exist
                string awakeBlock = $"\n    private void Awake()\n    {{\n{contentToAdd}    }}\n";
                return InsertIntoRegion(script, "#region Unity Methods", awakeBlock);
            }

            // Insert into existing Awake
            int braceStart = script.IndexOf('{', awakeIndex);
            int insertPos = script.IndexOf('\n', braceStart) + 1;

            return script.Insert(insertPos, contentToAdd);
        }

        public static void AutoAssignReferences(GameObject go, MonoScript monoScript)
        {
            if (monoScript == null) return;

            var scriptType = monoScript.GetClass();
            if (scriptType == null) return;

            var component = go.GetComponent(scriptType);
            if (component == null) component = go.AddComponent(scriptType);

            var so = new SerializedObject(component);
            var buttons = go.GetComponentsInChildren<Button>(true);

            foreach (var btn in buttons)
            {
                string fieldName = $"_{char.ToLower(btn.name[0])}{btn.name.Substring(1)}";
                var property = so.FindProperty(fieldName);
                if (property != null && property.objectReferenceValue == null)
                {
                    Undo.RecordObject(component, "Auto Assign Button");
                    property.objectReferenceValue = btn;
                }
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(component);
        }

        public static string CleanUpEmptyRegions(string script)
        {
            var lines = script.Split('\n').ToList();
            var cleanedLines = new List<string>();

            int i = 0;
            while (i < lines.Count)
            {
                if (lines[i].TrimStart().StartsWith("#region"))
                {
                    int regionStart = i;
                    int regionEnd = -1;
                    for (int j = i + 1; j < lines.Count; j++)
                    {
                        if (lines[j].TrimStart().StartsWith("#endregion"))
                        {
                            regionEnd = j;
                            break;
                        }
                    }

                    if (regionEnd != -1)
                    {
                        // Check if region is empty (only whitespace lines)
                        bool isEmpty = true;
                        for (int k = regionStart + 1; k < regionEnd; k++)
                        {
                            if (!string.IsNullOrWhiteSpace(lines[k]))
                            {
                                isEmpty = false;
                                break;
                            }
                        }

                        if (isEmpty)
                        {
                            i = regionEnd + 1; // skip this region
                            continue;
                        }
                    }
                }

                cleanedLines.Add(lines[i]);
                i++;
            }

            return string.Join("\n", cleanedLines);
        }
    }
}