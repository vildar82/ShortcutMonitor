using NetLib;

namespace ShortcutMonitor.GUI.Model
{
	using System;
	using System.IO;
	using Data;

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
	}
}
