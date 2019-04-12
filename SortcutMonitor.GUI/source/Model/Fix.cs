using System.Text.RegularExpressions;
using NetLib.WPF.Controls;

namespace ShortcutMonitor.GUI.Model
{
	using System;
	using System.IO;
	using Data;
	using NetLib;

	public static class Fix
	{
		public static void FixPath(ShortcutItem item, string fixPath, State state)
		{
			if (!File.Exists(item.SourceDwg))
				throw new FileNotFoundException("Не найден исходный файл элемента внешней ссылки.", item.SourceDwg);
			item.ProjectXml.Shortcuts.Shortcut.Criteria.File.Name = fixPath;
			var fixDir = Path.GetDirectoryName(fixPath);
			Directory.CreateDirectory(fixDir);
			File.Move(item.SourceDwg, fixPath);
			BackUp(item.XmlFile);
			item.ProjectXml.ToXml(item.XmlFile);
			item.SourceDwg = fixPath;
			state.Fix = null;
			state.Msg = null;
			state.Status = "OK";
			state.IsEmailErr = false;
			state.Color = State.colorOk;
		}

		private static void BackUp(string file)
		{
			var name = Path.GetFileName(file);
			var bName = $"{name}_{DateTime.Now:dd.MM.yyyy hh.mm}.bak";
			var fName = Path.Combine(Path.GetDirectoryName(file), bName);
			File.Move(file, fName);
		}

		public static void FixProjectDesc(State state)
		{
			var textVm = new TextVM("Примечание", $"Примечание к проекту быстрой ссылки '{state.Project.Name}'",
				"", s => null);
			var textView = new TextView(textVm);
			if (textView.ShowDialog() != true) throw new OperationCanceledException();
			state.Project.Description = textVm.Value;
			var his = Path.Combine(state.Project.Dir, @"_Shortcuts\ShortcutsHistory.xml");
			var hisText = File.ReadAllText(his);
			var match = Regex.Match(hisText, @"desc="".*""");
			if (match.Success)
			{
				var g = match.Groups[0];
				hisText = hisText.Substring(0, g.Index) + $@"desc=""{textVm.Value}""" + hisText.Substring(g.Index + g.Length);
				File.WriteAllText(his, hisText);
			}

			foreach (var shortcut in state.Project.Shortcuts)
			{
				var text = File.ReadAllText(shortcut.XmlFile);
				match = Regex.Match(text, @"<ProjectDesc>.*<\/ProjectDesc>");
				if (!match.Success) continue;
				var g = match.Groups[0];
				text = text.Substring(0, g.Index) + $"<ProjectDesc>{textVm.Value}</ProjectDesc>" +
				       text.Substring(g.Index + g.Length);
				File.WriteAllText(shortcut.XmlFile, text);
			}
		}
	}
}
