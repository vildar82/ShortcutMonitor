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

        private async Task<List<Project>> CheckOtherProjects()
        {
            return await Task.Run(async () =>
            {
                var projects = new List<Project>();
                var dir      = new DirectoryInfo(MainVm.ShortcutFolder);
                foreach (var projDir in dir.EnumerateDirectories())
                {
                    if (Project.GetProject(projDir, false) == null)
                    {
                        var proj      = Project.GetProject(projDir, true);
                        var itemFiles = await MainVM.GetShortcutFiles(projDir.FullName);
                        proj.Shortcuts = await MainVM.GetShortcutItems(itemFiles);
                        foreach (var i in proj.Shortcuts) Checks.CheckElement(i);
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
            var surfaces = project.Shortcuts.Where(w => w.ElementType == "AeccDbSurfaceTin").ToList();

            // Поверхность ЧЗ
            var blackSurfaces = surfaces.Where(w => w.ElementName.StartsWith("ЧЗ")).ToList();
            CheckSurfaces(blackSurfaces, "ЧЗ", @"1-Исходные данные\2-ЦМР", "ЧЗ", project);

            // Поверхность КЗ
            var redSurfaces = surfaces.Where(w => w.ElementName.StartsWith("КЗ")).ToList();
            CheckSurfaces(redSurfaces, "КЗ", @"3-Рабочие Чертежи\ГП", "КЗ", project);

            // Примечание
            if (project.Description.IsNullOrEmpty())
            {
                project.Status.Add(State.Error(null, project, "Нет описания проекта.", "Добавить описание к проекту.",
                    true, author: project.Author, fix: Fix.FixProjectDesc));
            }
            else
            {
                project.Status.Add(State.Ok(null, project, $"Описание: {project.Description}", project.Description, fix: Fix.FixProjectDesc));
            }
        }

        private void CheckSurfaces(List<ShortcutItem> surfs, string surfPrefix, string relPathFromProjDir, string title,
            Project project)
        {
            if (surfs.Count == 0)
            {
                project.Status.Add(State.Error(null, project, $"{title} - нет", $"Нет '{title}' поверхности"));
                return;
            }

            if (surfs.Count > 1)
            {
                foreach (var item in surfs)
                {
                    var msg = $"Несколько элементов '{title}' в проекте.";
                    item.Project.Status.Add(State.Error(item, project, msg, msg, true));
                }
            }
        }

        private void FixExec(State state)
        {
            state.Fix(state);
        }

        private void SendEmailExec(State state)
        {
            var mail  = new MailMessage();
            var login = Helper.GetLogin(state.Author);
            if (login.IsNullOrEmpty()) return;
            mail.IsBodyHtml = true;
            mail.To.Add($"{login}@pik.ru");
            mail.Subject = state.Item != null
                ? $"Ошибка в элементе быстрой ссылки '{state.Item.ElementName}' {state.Item.Project.Name}"
                : $"Ошибка в проекте быстрой ссылки '{state.Project.Name}'";
            mail.Body = $@"Проект: {state.Project.Name}%0A
{(state.Item == null ? string.Empty : "Файл быстрой ссылки: {state.Item.SourceDwg}%0A")}
Исправить ошибку: {state.Msg}";
            mail.MailTo();
        }
    }
}
