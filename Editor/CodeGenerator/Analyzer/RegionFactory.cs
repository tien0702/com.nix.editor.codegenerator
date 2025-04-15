using System.Collections.Generic;

namespace NIX.Editor.CodeGenerator
{
    public enum RegionType
    {
        References,
        Fields,
        Properties,
        UnityMethods,
        Methods
    }

    public static class RegionFactory
    {
        private static readonly Dictionary<RegionType, string> _regionTags = new()
        {
            { RegionType.References, "#region References" },
            { RegionType.Fields, "#region Fields" },
            { RegionType.Properties, "#region Properties" },
            { RegionType.UnityMethods, "#region Unity Methods" },
            { RegionType.Methods, "#region Methods" },
        };

        public static string GetRegionTag(RegionType type)
        {
            return _regionTags.TryGetValue(type, out var tag) ? tag : string.Empty;
        }
    }
}