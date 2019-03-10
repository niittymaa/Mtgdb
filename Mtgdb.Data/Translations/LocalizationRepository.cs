﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Mtgdb.Data
{
	public class LocalizationRepository
	{
		private const string TranslationsFile = @"translations.json";

		private Dictionary<int, Dictionary<string, Translation>> _cardsByMultiverseIdByNumber =
			new Dictionary<int, Dictionary<string, Translation>>();

		private string _serialized;

		public void LoadFile()
		{
			_serialized = File.ReadAllText(AppDir.Data.AddPath(TranslationsFile));
		}

		public void Load()
		{
			var translations = JsonConvert.DeserializeObject<List<Translation>>(_serialized);

			_cardsByMultiverseIdByNumber = translations.GroupBy(_ => _.Id)
				.ToDictionary(
					gr => gr.Key,
					gr => gr.ToDictionary(_ => _.CardNumber ?? string.Empty));

			// release RAM
			_serialized = null;
		}

		/// <summary>
		/// release RAM
		/// </summary>
		public void Clear()
		{
			_cardsByMultiverseIdByNumber.Clear();
		}

		public CardLocalization GetLocalization(Card card)
		{
			if (card.ForeignNames == null)
				return null;

			var result = new CardLocalization();

			foreach (var name in card.ForeignNames)
			{
				var translation = _cardsByMultiverseIdByNumber.TryGet(name.MultiverseId)
					?.TryGet(card.Number ?? string.Empty);

				if (translation != null)
				{
					if (Str.Equals(translation.Type, card.TypeEn))
						translation.Type = null;

					if (Str.Equals(translation.Flavor, card.FlavorEn))
						translation.Flavor = null;

					if (Str.Equals(translation.Text, card.TextEn))
						translation.Text = null;
					
					if (translation.Text != null || translation.Type != null || translation.Flavor != null)
						result.Add(card, name, translation);
				}
			}

			return result;
		}

		public static readonly Regex IncompleteChaosPattern = new Regex("(?<!{)CHAOS(?!})");
	}
}