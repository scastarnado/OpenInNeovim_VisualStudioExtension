using System;
using System.IO;
using OpenInNeovim;

static void Equal(string expected, string actual)
{
    if (expected != actual) throw new Exception($"Expected {expected}, received {actual}");
}
Equal("\"\"", NeovimLauncher.QuoteArgument(""));
Equal("\"C:\\folder with spaces\\file.cs\"", NeovimLauncher.QuoteArgument(@"C:\folder with spaces\file.cs"));
Equal("\"C:\\folder\\\\\"", NeovimLauncher.QuoteArgument(@"C:\folder\"));
Equal("\"a\\\"b\"", NeovimLauncher.QuoteArgument("a\"b"));
Equal("\"a\\\\\\\"b\"", NeovimLauncher.QuoteArgument("a\\\"b"));
Equal("\"C:\\a & b\\file.cs\"", NeovimLauncher.QuoteArgument(@"C:\a & b\file.cs"));
var current = Environment.ProcessPath;
Equal(Path.GetFullPath(current), NeovimLauncher.FindExecutable(current));
try { NeovimLauncher.FindExecutable(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".exe")); throw new Exception("Missing executable was accepted"); }
catch (FileNotFoundException) { }
Console.WriteLine("All 8 launcher checks passed.");
