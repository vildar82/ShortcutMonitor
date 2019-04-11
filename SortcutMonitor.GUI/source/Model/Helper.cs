namespace ShortcutMonitor.GUI.Model
{
	using NetLib;

	public static class Helper
	{
		public static string GetLogin(string autor)
		{
			if (autor.IsNullOrEmpty()) return null;
			var indexSlash = autor.IndexOf('\\');
			return indexSlash > 0 ? autor.Substring(indexSlash + 1) : autor;
		}
	}
}
