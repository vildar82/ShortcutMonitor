using System.Collections.Generic;
using ShortcutMonitor.GUI.Data;

namespace ShortcutMonitor.GUI.Model
{
	using System;
	using System.IO;
	using NetLib;

	public static class Checks
	{
		public static void CheckElement(ShortcutItem elem)
		{
			elem.Status = new List<State>();
			var elemFile = elem.SourceDwg;
			if (elemFile.IsNullOrEmpty())
			{
				const string msg = "Не задан файл быстрой ссылки";
				elem.Status.Add(State.Error(elem, elem.Project, msg, msg, true));
				return;
			}

			if (!elemFile.Contains(MainVM.MainVm.ShortcutFolder, StringComparison.OrdinalIgnoreCase))
			{
				var msg = $"Файл не лежит в папке быстрых ссылок - {MainVM.MainVm.ShortcutFolder}. Переместить файл в папку проекта быстрых ссылок.";
				elem.Status.Add(State.Error(elem, elem.Project, msg, msg, true));
			}

			if (!File.Exists(elemFile))
			{
				var err = "Файл не найден";
				elem.Status.Add(State.Error(elem, elem.Project, err, err, true));
			}

			if (elem.ElementName.StartsWith("ЧЗ_"))
			{
				CheckRelSourcePath(elem, @"1-Исходные данные\2-ЦМР", "ЧЗ");
			}

			if (IsBlackSurfPath(elemFile) && !elem.ElementName.StartsWith("ЧЗ_"))
			{
				elem.Status.Add(State.Error(elem, elem.Project, "Неправильное имя быстрой ссылки.", $@"Быстрая ссылка '{elem.ElementName}' лежит в папке '1-Исходные данные\2-ЦМР' для черной поверхности! Имя быстрой ссылки черной поверхности должно начинаться с 'ЧЗ_'. Исправить имя быстрой ссылки!", true));
			}

			if (elem.ElementName.StartsWith("КЗ_"))
			{
				CheckRelSourcePath(elem, @"3-Рабочие Чертежи\ГП", "КЗ");
			}

			if (IsRedSurfPath(elemFile) && !elem.ElementName.StartsWith("КЗ_"))
			{
				elem.Status.Add(State.Error(elem, elem.Project, "Неправильное имя быстрой ссылки.", $@"Быстрая ссылка '{elem.ElementName}' лежит в папке '3-Рабочие Чертежи\ГП' для красной поверхности! Имя быстрой ссылки красной поверхности должно начинаться с 'КЗ_'. Исправить имя быстрой ссылки!", true));
			}
		}

		public static void CheckRelSourcePath(ShortcutItem item, string relPathFromProjDir, string prefix)
		{
			var sourceDir = Path.GetDirectoryName(item.SourceDwg);
			var relDir    = Path.Combine(item.Project.Dir, relPathFromProjDir);
			if (!Path.GetFullPath(sourceDir).EqualsIgnoreCase(Path.GetFullPath(relDir)))
			{
				var fixDir  = Path.Combine(MainVM.MainVm.ShortcutFolder, item.Project.Name, relPathFromProjDir);
				var fixPath = Path.Combine(fixDir, Path.GetFileName(item.SourceDwg));
				var msg = $"{prefix} должна лежать в папке '{fixDir}'.";
				item.Status.Add(State.Error(item, item.Project, msg, msg, true, s => Fix.FixPath(item, fixPath, s)));
			}
		}

		public static bool IsBlackSurfPath(string dwgPath)
		{
			return dwgPath.Contains(@"1-Исходные данные\2-ЦМР", StringComparison.OrdinalIgnoreCase);
		}

		public static bool IsRedSurfPath(string dwgPath)
		{
			return dwgPath.Contains(@"3-Рабочие Чертежи\ГП", StringComparison.OrdinalIgnoreCase);
		}
	}
}
