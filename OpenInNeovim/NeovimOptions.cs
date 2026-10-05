using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace OpenInNeovim
{
    public sealed class NeovimOptions : DialogPage
    {
        [Category("Neovim"), DisplayName("Executable path")]
        [Description("Full path to nvim.exe or a Neovim GUI executable. Leave empty to find nvim.exe on PATH or in standard installation folders.")]
        public string ExecutablePath { get; set; } = "";
    }
}
