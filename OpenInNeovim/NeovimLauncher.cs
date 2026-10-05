using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenInNeovim
{
    internal static class NeovimLauncher
    {
        internal static string QuoteArgument(string value)
        {
            var result = new StringBuilder("\"");
            int slashes = 0;
            foreach (char c in value)
            {
                if (c == '\\') { slashes++; continue; }
                result.Append('\\', c == '"' ? slashes * 2 + 1 : slashes);
                result.Append(c);
                slashes = 0;
            }
            return result.Append('\\', slashes * 2).Append('"').ToString();
        }

        internal static string FindExecutable(string configured)
        {
            if (!string.IsNullOrWhiteSpace(configured))
            {
                var fullPath = Environment.ExpandEnvironmentVariables(configured.Trim().Trim('"'));
                if (File.Exists(fullPath)) return Path.GetFullPath(fullPath);
                throw new FileNotFoundException("The configured Neovim executable does not exist: " + fullPath);
            }
            var paths = (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator)
                .Select(p => Path.Combine(p.Trim().Trim('"'), "nvim.exe"))
                .Concat(new[] {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Neovim", "bin", "nvim.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Neovim", "bin", "nvim.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "scoop", "apps", "neovim", "current", "bin", "nvim.exe")
                });
            return paths.FirstOrDefault(File.Exists) ?? throw new FileNotFoundException(
                "Neovim was not found. Install Neovim or set its executable in Tools > Options > Open in NeoVim > General.");
        }

        internal static void Open(string target, string executable)
        {
            var start = new ProcessStartInfo
            {
                FileName = FindExecutable(executable),
                Arguments = "-- " + QuoteArgument(target),
                WorkingDirectory = Directory.Exists(target) ? target : Path.GetDirectoryName(target),
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Normal
            };
            using (Process.Start(start)) { }
        }
    }
}
