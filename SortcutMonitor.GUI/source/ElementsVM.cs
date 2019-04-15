using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mail;
using System.Reactive;
using System.Xaml.Schema;
using DynamicData;
using ShortcutMonitor.GUI.Model;

namespace ShortcutMonitor.GUI
{
    using System;
    using System.Reactive.Linq;
    using System.Text.RegularExpressions;
    using Data;
    using NetLib;
    using NetLib.IO;
    using NetLib.WPF;
    using ReactiveUI;

    public class ElementsVM : BaseModel
    {
        private IObservable<string> filterObs;

        public ElementsVM(MainVM mainVm)
            : base(mainVm)
        {
            filterObs = this.WhenAnyValue(v => v.Filter);
            OpenProjectFolder = CreateCommand<ShortcutItem>(OpenProjectFolderExec);
            OpenSourceDwg = CreateCommand<ShortcutItem>(OpenSourceDwgExec);
            OpenAutor = CreateCommand<string>(OpenAutorExec);
            MainVM.AllElements.Connect().AutoRefreshOnObservable(f => filterObs)
                .Filter(OnFilter)
                .Bind(out var data)
                .Subscribe();
            Elements = data;
            Email = CreateCommand<ShortcutItem>(EmailExec);
            FixCmd = CreateCommand<ShortcutItem>(FixCmdExec);
        }

        public string Filter { get; set; }
        public ReadOnlyObservableCollection<ShortcutItem> Elements { get; set; }
        public ReactiveCommand<ShortcutItem, Unit> OpenProjectFolder { get; set; }
        public ReactiveCommand<ShortcutItem, Unit> OpenSourceDwg { get; set; }
        public ReactiveCommand<Unit, Unit> OpenElementFolder { get; set; }
        public ReactiveCommand<string, Unit> OpenAutor { get; set; }
        public ReactiveCommand<ShortcutItem, Unit> Email { get; set; }
        public ReactiveCommand<ShortcutItem, Unit> FixCmd { get; set; }

        private bool OnFilter(ShortcutItem item)
        {
            if (Filter.IsNullOrEmpty() || item == null) return true;
            return Regex.IsMatch(item.ToString(), Filter, RegexOptions.IgnoreCase);
        }

        private void OpenProjectFolderExec(ShortcutItem item)
        {
            var dir = item.Project.Dir;
            dir.StartExplorer();
        }

        private void OpenSourceDwgExec(ShortcutItem item)
        {
            var file = item.SourceDwg;
            file.StartExplorer();
        }

        private void OpenAutorExec(string autor)
        {
            var login = Helper.GetLogin(autor);
            if (login.IsNullOrEmpty()) return;
            System.Diagnostics.Process.Start($"https://home.pik.ru/search?k={login}");
        }

        private void EmailExec(ShortcutItem item)
        {
            var mail = new MailMessage();
            var login = Helper.GetLogin(item.Author);
            if (login.IsNullOrEmpty()) return;
            mail.IsBodyHtml = true;
            mail.To.Add($"{login}@pik.ru");
            mail.Subject = $"Ошибки в быстрой ссылке '{item.ElementName}' {item.Project.Name}";
            mail.Body = $@"Название проекта быстрых ссылок: {item.Project.Name}%0A
Название элемента быстрой ссылки: {item.ElementName}%0A
Файл быстрой ссылки: {item.SourceDwg}%0A
Исправить ошибки:%0A{item.Status.Select((s, i) => $"{i + 1}. {s.Msg}").JoinToString("%0A")}";
            mail.MailTo();
        }

        private void FixCmdExec(ShortcutItem item)
        {
            var msg = string.Empty;
            foreach (var state in item.Status.Where(w => w.Fix != null))
            {
                var s = state.Msg;
                state.Fix(state);
                msg += $"Исправлено: {s}\n\r";
            }

            ShowMessage(msg.IsNullOrEmpty() ? "Ничего не исправлено." : msg);
        }
    }
}
