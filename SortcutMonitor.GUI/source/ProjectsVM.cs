using System.IO;

namespace ShortcutMonitor.GUI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Data;
    using JetBrains.Annotations;
    using NetLib;
    using NetLib.IO;
    using NetLib.WPF;
    using ReactiveUI;

    public class ProjectsVM : BaseModel
    {
        public ProjectsVM(MainVM mainVm)
            : base(mainVm)
        {
            MainVm = mainVm;
            OpenDir = CreateCommand<Project>(OpenDirExec);
            OpenSourceFolder = CreateCommand<ShortcutItem>(OpenSourceFolderExec);
            this.WhenAnyValue(v => v.Filter).Skip(1).Subscribe(s => Projects?.Reset());
        }

        public MainVM MainVm { get; set; }
        public string Filter { get; set; }
        public IReactiveDerivedList<Project> Projects { get; set; }

        public ReactiveCommand OpenDir { get; set; }
        public ReactiveCommand OpenSourceFolder { get; set; }

        public async void UpdateProjects()
        {
            Projects = MainVM.AllProjects.CreateDerivedCollection(s => s, OnFilter);
            CheckProjects();
            var otherProjects = await CheckOtherProjects();
            MainVM.AllProjects.AddRange(otherProjects);
        }

        private Task<List<Project>> CheckOtherProjects()
        {
            return Task.Run(() =>
            {
                var projects = new List<Project>();
                var dir = new DirectoryInfo(MainVm.ShortcutFolder);
                foreach (var projDir in dir.EnumerateDirectories())
                {
                    if (Project.GetProject(projDir, false) == null)
                    {
                        var proj = Project.GetProject(projDir, true);
                        var itemFiles = MainVM.GetShortcutFiles(projDir.FullName).Result;
                        proj.Shortcuts = MainVM.GetShortcutItems(itemFiles).Result;
                        projects.Add(proj);
                    }
                }
                return projects;
            });
        }

        private bool OnFilter(Project proj)
        {
            if (Filter.IsNullOrEmpty() || proj == null)
                return true;
            return Regex.IsMatch(proj.ToString(), Filter, RegexOptions.IgnoreCase);
        }

        private void OpenDirExec(Project proj)
        {
            proj.Dir.StartExplorer();
        }

        private void OpenSourceFolderExec([NotNull] ShortcutItem item)
        {
            item.SourceDwg.StartExplorer();
        }

        private void CheckProjects()
        {
            foreach (var project in MainVM.AllProjects)
            {
                CheckProject(project);
            }
        }

        private void CheckProject([NotNull] Project project)
        {
            // Поверхность ЧЗ
            var blackSurfaces = project.Shortcuts.Where(w => w.ElementType == "AeccDbSurfaceTin" &&  w.ElementName.StartsWith("ЧЗ")).ToList();
            CheckSurfaces(blackSurfaces, "ЧЗ", "1-Исходные данные", "ЧЗ", project);

            // Поверхность КЗ
            var redSurfaces = project.Shortcuts.Where(w => w.ElementType == "AeccDbSurfaceTin" && w.ElementName.StartsWith("КЗ")).ToList();
            CheckSurfaces(redSurfaces, "КЗ", @"3-Рабочие Чертежи\ГП", "КЗ", project);
        }

        private void CheckSurfaces(List<ShortcutItem> surfs, string surfPrefix, string relPathFromProjDir, string title,
            Project project)
        {
            if (surfs.Count == 0)
            {
                project.Status.Add(State.Error(null, $"{title} - нет", "Нет поверхности"));
                return;
            }

            if (surfs.Count > 1)
            {
                foreach (var item in surfs)
                {
                    var errItem = CheckRelSourcePath(item, relPathFromProjDir);
                    var text = $"{item.ElementName} - (несколько) {(errItem.IsNullOrEmpty() ? "" : item.SourceDwg)}";
                    project.Status.Add(State.Error(item, text, errItem));
                }

                return;
            }

            var surf = surfs[0];
            var err = CheckRelSourcePath(surf, relPathFromProjDir);
            if (!err.IsNullOrEmpty())
                project.Status.Add(State.Error(surf, $"{surf.ElementName} - {surf.SourceDwg}", err));
        }

        private string CheckRelSourcePath(ShortcutItem item, string relPathFromProjDir)
        {
            var sourceDir = System.IO.Path.GetDirectoryName(item.SourceDwg);
            var relDir = System.IO.Path.Combine(item.Project.Dir, relPathFromProjDir);
            return !System.IO.Path.GetFullPath(sourceDir).Equals(System.IO.Path.GetFullPath(relDir))
                ? $"Исходный файл должен лежать в папке '{relPathFromProjDir}'."
                : null;
        }
    }
}