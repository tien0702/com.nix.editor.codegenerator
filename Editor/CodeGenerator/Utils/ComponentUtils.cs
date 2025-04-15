using System;
using System.Collections.Generic;
using UnityEngine;

namespace NIX.Editor.CodeGenerator
{
    public static class ComponentUtils
    {
        /// <summary>
        /// Get all components in children, skipping any GameObject that contains a component in the 'skips' list.
        /// </summary>
        public static List<T> GetComponentsInChildren<T>(GameObject go, List<Type> skips) where T : Component
        {
            var result = new List<T>();
            Traverse(go.transform, skips, result);
            return result;
        }

        private static void Traverse<T>(Transform current, List<Type> skips, List<T> result) where T : Component
        {
            if (current.TryGetComponent<T>(out T component))
            {
                result.Add(component);
            }

            // Skip this GameObject if it contains any component in skips list
            foreach (var type in skips)
            {
                if (current.GetComponent(type) != null)
                    return;
            }

            // Recurse children
            for (int i = 0; i < current.childCount; i++)
            {
                Traverse(current.GetChild(i), skips, result);
            }
        }
    }
}