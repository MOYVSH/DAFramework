#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 专门用于从池配置列表生成枚举的工具类。
/// 常量定义了枚举名、命名空间。
/// 生成的文件将直接放在该脚本的同一目录下。
/// </summary>
public static class PoolEnumGeneratorTool
{
    /// <summary>
    /// 根据指定的池名称列表生成枚举文件。
    /// </summary>
    /// <param name="poolNames">池名称集合（会自动过滤空值、去重、转为合法标识符）</param>
    public static void Generate(IEnumerable<string> poolNames, string enumName, string nameSpace)
    {
        // 获取当前脚本所在路径
        string scriptPath = GetCurrentScriptPath();
        if (string.IsNullOrEmpty(scriptPath))
        {
            Debug.LogError("无法获取 PoolEnumGeneratorTool 脚本的路径，生成取消。");
            return;
        }

        string scriptDir = Path.GetDirectoryName(scriptPath);
        // 直接放在脚本同级目录
        string filePath = Path.Combine(scriptDir, $"{enumName}.cs");

        // 清洗名称
        var cleanNames = poolNames
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => MakeValidIdentifier(n.Trim()))
            .Distinct()
            .ToList();

        if (cleanNames.Count == 0)
        {
            Debug.LogWarning("没有有效的池名称，未生成枚举。");
            return;
        }

        // 拼接枚举成员
        string members = string.Join(",\n", cleanNames.Select(name => $"        {name}"));

        // 构建完整代码
        var sb = new StringBuilder();
        sb.AppendLine("// 自动生成的枚举，请勿手动修改");
        sb.AppendLine($"// 生成时间：{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        bool hasNamespace = !string.IsNullOrWhiteSpace(nameSpace);
        if (hasNamespace)
        {
            sb.AppendLine($"namespace {nameSpace}");
            sb.AppendLine("{");
        }

        sb.AppendLine($"    public enum {enumName}");
        sb.AppendLine("    {");
        sb.Append(members);
        sb.AppendLine();
        sb.AppendLine("    }");

        if (hasNamespace)
        {
            sb.AppendLine("}");
        }

        // 写入文件并刷新资源数据库
        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh();

        Debug.Log($"枚举 {enumName} 已生成（共 {cleanNames.Count} 个成员），保存路径：{filePath}");
    }

    /// <summary>
    /// 通过 MonoScript 获取当前工具类的脚本路径。
    /// </summary>
    private static string GetCurrentScriptPath()
    {
        string[] guids = AssetDatabase.FindAssets("t:MonoScript PoolEnumGeneratorTool");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.EndsWith("PoolEnumGeneratorTool.cs"))
                return path;
        }
        return null;
    }

    /// <summary>
    /// 将字符串转换为合法的 C# 标识符。
    /// </summary>
    private static string MakeValidIdentifier(string rawName)
    {
        if (string.IsNullOrEmpty(rawName))
            return "_Empty";

        var sb = new StringBuilder();
        foreach (char c in rawName)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
                sb.Append(c);
            else
                sb.Append('_');
        }

        string result = sb.ToString();
        if (result.Length > 0 && char.IsDigit(result[0]))
            result = "_" + result;

        return string.IsNullOrEmpty(result) ? "_Unknown" : result;
    }
}
#endif