using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace OpenInNeovim
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuid)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(NeovimOptions), "Open in NeoVim", "General", 0, 0, true)]
    public sealed class OpenInNeovimPackage : AsyncPackage
    {
        public const string PackageGuid = "430e78d6-124a-4d8e-9c05-3c1c6c877ec3";
        private static readonly Guid CommandSet = new Guid("5710f306-2fa0-433e-b239-fd01263fb712");
        private EnvDTE80.DTE2 dte;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            dte = await GetServiceAsync(typeof(SDTE)) as EnvDTE80.DTE2
                ?? throw new InvalidOperationException("Visual Studio automation is unavailable.");
            var service = (OleMenuCommandService)await GetServiceAsync(typeof(IMenuCommandService));
            if (service == null) throw new InvalidOperationException("Visual Studio command service is unavailable.");
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            AddCommand(service, 0x0100, false);
            AddCommand(service, 0x0101, true);
        }

        private void AddCommand(OleMenuCommandService service, int id, bool editor)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var command = new OleMenuCommand((s, e) => Execute(editor), new CommandID(CommandSet, id));
            command.BeforeQueryStatus += (s, e) =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                try { command.Visible = command.Enabled = GetTargets(editor).Count > 0; }
                catch (COMException) { command.Visible = command.Enabled = false; }
                catch (ArgumentException) { command.Visible = command.Enabled = false; }
            };
            service.AddCommand(command);
        }

        private List<string> GetTargets(bool editor)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var targets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (editor)
            {
                AddPath(targets, dte.ActiveDocument?.FullName, false);
            }
            else if (dte.ToolWindows.SolutionExplorer.SelectedItems is Array items)
            {
                foreach (UIHierarchyItem item in items)
                {
                    try
                    {
                        if (item.Object is Project project)
                            AddPath(targets, project.FullName, true);
                        else if (item.Object is ProjectItem projectItem)
                        {
                            for (short i = 1; i <= projectItem.FileCount; i++)
                                AddPath(targets, projectItem.FileNames[i], false);
                        }
                        else if (item.Object is Solution solution)
                            AddPath(targets, solution.FullName, true);
                    }
                    // Some project systems expose virtual nodes without file properties.
                    catch (COMException) { }
                    catch (ArgumentException) { }
                }
            }
            return new List<string>(targets);
        }

        private static void AddPath(HashSet<string> targets, string path, bool containingFolder)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            if (containingFolder && File.Exists(path)) path = Path.GetDirectoryName(path);
            if (File.Exists(path) || Directory.Exists(path)) targets.Add(Path.GetFullPath(path));
        }

        private void Execute(bool editor)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                var options = (NeovimOptions)GetDialogPage(typeof(NeovimOptions));
                foreach (var target in GetTargets(editor)) NeovimLauncher.Open(target, options.ExecutablePath);
            }
            catch (Exception ex)
            {
                VsShellUtilities.ShowMessageBox(this, ex.Message, "Open in NeoVim",
                    OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }
    }
}
