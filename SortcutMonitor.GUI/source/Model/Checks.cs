namespace ShortcutMonitor.GUI.Model
{
	using System;
	using System.IO;
	using NetLib;

	public class Checks
	{
		private readonly MainVM _mainVm;

		public Checks(MainVM mainVm)
		{
			_mainVm = mainVm;
		}

		public bool CheckElementFile(string elemFile, out string err)
		{
			if (elemFile.IsNullOrEmpty())
			{
				err = "Пустой путь файла";
				return false;
			}

			if (!elemFile.Contains(_mainVm.ShortcutFolder, StringComparison.OrdinalIgnoreCase))
			{
				err = $"Файл элемента не лежит в папке быстрых ссылок - {_mainVm.ShortcutFolder}. Переместить файл в папку проекта быстрых ссылок.";
				return false;
			}

			if (!File.Exists(elemFile))
			{
				err = "Файл не найден";
				return false;
			}

			err = null;
			return true;
		}
	}
}
