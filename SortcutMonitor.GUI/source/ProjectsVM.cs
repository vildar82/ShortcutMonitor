using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Reactive;
using DynamicData;
using ShortcutMonitor.GUI.Model;

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
        private IObservable<string> filterObs;

        public ProjectsVM(MainVM mainVm)
            : base(mainVm)
        {
            MainVm = mainVm;
            filterObs = this.WhenAnyValue(v => v.Filter);
            OpenDir = CreateCommand<Project>(OpenDirExec);
            OpenSourceFolder = CreateCommand<ShortcutItem>(OpenSourceFolderExec);
            MainVM.AllProjects.Connect().AutoRefreshOnObservable(f => filterObs)
                .Filter(OnFilter)
                .Bind(out var data)
                .Subscribe();
            Projects = data;
            SendEmail = CreateCommand<State>(SendEmailExec);
            FixCmd = CreateCommand<State>(FixExec);
        }

        public MainVM MainVm { get; set; }
        public string Filter { get; set; }
        public ReadOnlyObservableCollection<Project> Projects { get; set; }
        public ReactiveCommand<Project, Unit> OpenDir { get; set; }
        public ReactiveCommand<ShortcutItem, Unit> OpenSourceFolder { get; set; }
        public ReactiveCommand<State, Unit> SendEmail { get; set; }
        public ReactiveCommand<State, Unit> FixCmd { get; set; }

        public async void UpdateProjects()
        {
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
            if (Filter.IsNullOrEmpty() || proj == null) return true;
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
            foreach (var project in MainVM.AllProjects.Items.ToList())
            {
                CheckProject(project);
            }
        }

        private void CheckProject([NotNull] Project project)
        {
            // Поверхность ЧЗ
            var blackSurfaces = project.Shortcuts.Where(w => w.ElementType == "AeccDbSurfaceTin" && w.ElementName.StartsWith("ЧЗ")).ToList();
            CheckSurfaces(blackSurfaces, "ЧЗ", @"1-Исходные данные\2-ЦМР", "ЧЗ", project);

            // Поверхность КЗ
            var redSurfaces = project.Shortcuts.Where(w => w.ElementType == "AeccDbSurfaceTin" && w.ElementName.StartsWith("КЗ")).ToList();
            CheckSurfaces(redSurfaces, "КЗ", @"3-Рабочие Чертежи\ГП", "КЗ", project);
        }

        private void CheckSurfaces(List<ShortcutItem> surfs, string surfPrefix, string relPathFromProjDir, string title,
            Project project)
        {
            if (surfs.Count == 0)
            {
                project.Status.Add(State.Error(null, $"{title} - нет", $"Нет '{title}' поверхности"));
                return;
            }

            Action<State> fix;
            if (surfs.Count > 1)
            {
                foreach (var item in surfs)
                {
                    var errItem = CheckRelSourcePath(item, relPathFromProjDir, out fix);
                    errItem += $" Несколько элементов '{title}' в проекте.";
                    var text = $"{item.ElementName} - (несколько) {(errItem.IsNullOrEmpty() ? "" : item.SourceDwg)}";
                    project.Status.Add(State.Error(item, text, errItem, true, fix));
                }

                return;
            }

            var surf = surfs[0];
            var err  = CheckRelSourcePath(surf, relPathFromProjDir, out fix);
            if (!err.IsNullOrEmpty())
            {
                project.Status.Add(State.Error(surf, $"{surf.ElementName} - {surf.SourceDwg}", err, true, fix));
            }
        }

        private string CheckRelSourcePath(ShortcutItem item, string relPathFromProjDir, out Action<State> fix)
        {
            var sourceDir = System.IO.Path.GetDirectoryName(item.SourceDwg);
            var relDir = System.IO.Path.Combine(item.Project.Dir, relPathFromProjDir);
            if (!System.IO.Path.GetFullPath(sourceDir).EqualsIgnoreCase(System.IO.Path.GetFullPath(relDir)))
            {
                var fixDir = System.IO.Path.Combine(MainVm.ShortcutFolder, item.Project.Name, relPathFromProjDir);
                var fixPath = System.IO.Path.Combine(fixDir, System.IO.Path.GetFileName(item.SourceDwg));
                fix = s => Fix.FixPath(item, fixPath, s);
                return $"Файл должен лежать в папке '{fixDir}'.";
            }

            fix = null;
            return null;
        }

        private void FixExec(State state)
        {
            state.Fix(state);
        }

        private void SendEmailExec(State state)
        {
            var mail  = new MailMessage();
            var login = Helper.GetLogin(state.Item.Author);
            if (login.IsNullOrEmpty()) return;
            mail.IsBodyHtml = true;
            mail.To.Add($"{login}@pik.ru");
            mail.Subject = $"Ошибка в элементе быстрой ссылки '{state.Item.ElementName}' {state.Item.Project.Name}";
            mail.Body = $@"Проект: {state.Item.Project.Name}%0A
Файл быстрой ссылки: {state.Item.SourceDwg}%0A
Исправить ошибку: {state.Msg}";
            mail.MailTo();
        }
    }
}
