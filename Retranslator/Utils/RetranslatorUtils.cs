using System.Collections.Generic;
using System.Linq;
using Retranslator.Model;

namespace Retranslator.Utils
{
	internal static class RetranslatorUtils
	{
		/// <summary>
		/// Добавляет все переводы в словарь.
		/// </summary>
		public static int AppendToPhraseBook(this Translation translation)
		{
			var count = 0;
			foreach (var entry in translation.Sections.SelectMany(section => section.Entries))
			{
				if (PhraseBook.Instance.AddTranslation(entry.Source, entry.Target, translation.Properties.TargetLang))
					count++;
			}
			return count;
		}

		static public bool DifferFrom(this IList<TranslationEntry.Warning> w1, IList<TranslationEntry.Warning> w2)
		{
			if (w1 == null && w2 == null) return false;
			if (w1 == null) return true;
			if (w2 == null) return true;
			return w1.Distinct().Any();
		}
	}
}
