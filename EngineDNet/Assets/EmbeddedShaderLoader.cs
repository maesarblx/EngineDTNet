using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EngineDNet.Assets
{
    public static class EmbeddedShaderLoader
    {
        private static readonly string[] ShaderExtensions = new[] { ".dsf", ".dsv", ".glsl", ".hlsl" };

        public static string[] ListResourceNames(Assembly asm = null)
        {
            asm ??= Assembly.GetExecutingAssembly();
            return asm.GetManifestResourceNames();
        }

        public static string LoadShaderByResourceName(string resourceName, Assembly asm = null!)
        {
            asm ??= Assembly.GetExecutingAssembly();
            using var s = asm.GetManifestResourceStream(resourceName)
                     ?? throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");
            using var r = new StreamReader(s);
            return r.ReadToEnd();
        }

        public static string LoadShaderByRelativePath(string relativePath, Assembly asm = null!)
        {
            asm ??= Assembly.GetExecutingAssembly();
            if (string.IsNullOrWhiteSpace(relativePath))
                throw new ArgumentException("relativePath empty", nameof(relativePath));

            var parts = relativePath.TrimStart('\\', '/')
                                    .Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

            var candidateSuffix = string.Join(".", parts);
            var fileName = parts.Last();

            var names = asm.GetManifestResourceNames();

            var match = names.FirstOrDefault(n => n.EndsWith("." + candidateSuffix, StringComparison.OrdinalIgnoreCase)
                                               || n.EndsWith(candidateSuffix, StringComparison.OrdinalIgnoreCase));
            if (match != null) return LoadShaderByResourceName(match, asm);

            var marker = ".Graphics.Shaders.";
            match = names.FirstOrDefault(n => n.IndexOf(marker + candidateSuffix, StringComparison.OrdinalIgnoreCase) >= 0
                                           || n.IndexOf(marker + fileName, StringComparison.OrdinalIgnoreCase) >= 0);
            if (match != null) return LoadShaderByResourceName(match, asm);

            var byFile = names.Where(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (byFile.Length == 1) return LoadShaderByResourceName(byFile[0], asm);

            var sample = string.Join(Environment.NewLine + "  - ", names.Take(50));
            throw new FileNotFoundException(
                $"Embedded shader for '{relativePath}' not found. " +
                $"Searched for candidate suffix '{candidateSuffix}' and file '{fileName}'. " +
                $"Available resources (sample, up to 50):{Environment.NewLine}  - {sample}");
        }

        public static Dictionary<string, string> LoadAllShaders(Assembly asm = null)
        {
            asm ??= Assembly.GetExecutingAssembly();
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var resources = asm.GetManifestResourceNames()
                               .Where(r => ShaderExtensions.Any(ext => r.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                               .ToArray();

            foreach (var res in resources)
            {
                var marker = ".Graphics.Shaders.";
                string rel;
                var idx = res.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    var suffix = res.Substring(idx + marker.Length);
                    var lastDot = suffix.LastIndexOf('.');
                    if (lastDot >= 0)
                    {
                        var before = suffix.Substring(0, lastDot).Replace('.', '/');
                        var after = suffix.Substring(lastDot);
                        rel = before + after;
                    }
                    else rel = suffix.Replace('.', '/');
                }
                else
                {
                    rel = res;
                }

                dict[rel] = LoadShaderByResourceName(res, asm);
            }

            return dict;
        }
    }
}
