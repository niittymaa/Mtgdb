using System;
using System.Collections.Generic;
using System.Linq;
using Mtgdb.Data;
using NLog;

namespace Mtgdb.Ui
{
	public class DeckMigrator
	{
		public DeckMigrator(
			CardRepository repo,
			CardRepository42 repo42,
			CardRepositoryLegacy repoLegacy)
		{
			_repo = repo;
			_repo42 = repo42;
			_repoLegacy = repoLegacy;
		}

		public Deck ConvertLegacyDeck(Deck deck) =>
			DeckConverter.ConvertDeck(deck, legacyId =>
			{
				var cardLegacy = _repoLegacy.CardsById.TryGet(legacyId);
				if (cardLegacy == null)
				{
					_log.Error($"No legacy card found by id {legacyId}");
					return null;
				}

				var card = MatchLegacyCard(cardLegacy);
				if (card == null)
				{
					_log.Warn($"No matching card found for legacy card {cardLegacy.SetCode} {cardLegacy.NameEn} {cardLegacy.Number} {cardLegacy.MultiverseId} {cardLegacy.Id}");
					return null;
				}

				return card.Id;
			});

		public Deck ConvertV2Deck(Deck deck) =>
			DeckConverter.ConvertDeck(deck, v2Id =>
			{
				var card = _repo.CardsByScryfallId.TryGet(v2Id) ??
					_repo.CardsById.TryGet(v2Id);

				if (card == null)
				{
					_log.Error($"No v2 card found by id {v2Id}");
					return null;
				}

				return card.Id;
			});

		public Deck ConvertV3Deck(Deck deck) =>
			DeckConverter.ConvertDeck(deck, MatchIdV3);

		public string MatchIdV3(string v3Id)
		{
			(string scryfallId, string name) = _repo42.GetById(v3Id);

			if (scryfallId == default && name == default)
			{
				_log.Error($"No v3 card found by id {v3Id}");
				return null;
			}

			var id = CardId.Generate(scryfallId, name);

			if (!_repo.CardsById.ContainsKey(id))
			{
				if (_repo.CardsByName.TryGetValue(name, out var cards))
					return cards[0].Id;

				_log.Error($"No card found by id {id}");
				return null;
			}

			return id;
		}

		public Card MatchLegacyCard(CardLegacy cardLegacy)
		{
			IEnumerable<string> setCodes;

			if (_setCodeMap.TryGetValue(cardLegacy.SetCode, out var setsArray))
				setCodes = setsArray;
			else
			{
				if (_repo.SetsByCode.TryGetValue(cardLegacy.SetCode, out var set))
					setCodes = Sequence.From(set.Code);
				else
					return null;
			}

			var candidates = setCodes.SelectMany(s =>
				(IEnumerable<Card>) _repo.SetsByCode[s].CardsByName.TryGet(cardLegacy.NameNormalized) ??
				Empty<Card>.Sequence);

			var namesakes = cardLegacy.Set.CardsByName[cardLegacy.NameNormalized];

			if (namesakes.Count == 1)
				return candidates.FirstOrDefault();

			var namesakesCopy = namesakes.ToList();

			for (int i = 0; i < _matchers.Length; i++)
			{
				namesakesCopy.RemoveAll(c=> !_matchers[i].Item1(cardLegacy, c));
				if (namesakesCopy.Count == 1)
					return candidates.FirstOrDefault(c => _matchers.Take(i + 1).All(m => m.Item2(cardLegacy, c)));
			}

			if (cardLegacy.MultiverseId.HasValue)
				return _repo.CardsByMultiverseId.TryGet(cardLegacy.MultiverseId.Value)?.FirstOrDefault();

			return null;
		}



		private static readonly Dictionary<string, string[]> _setCodeMap =
			new Dictionary<string, string[]>(Str.Comparer)
			{
				["pFNM"] = new[] { "fnm", "f01", "f02", "f03", "f04", "f05", "f06", "f07", "f08", "f09", "f10", "f11", "f12", "f13", "f14", "f15", "f16", "f17", "f18" },
				["pMPR"] = new[] { "mpr", "pr2", "p03", "p04", "p05", "p06", "p07", "p08", "p09", "p10", "p11" },
				["pJGP"] = new[] { "jgp", "g99", "g00", "g01", "g02", "g03", "g04", "g05", "g06", "g07", "g08", "g09", "g10", "g11", "j12", "j13", "j14", "j15", "j16", "j17", "j18" },
				["parl"] = new[] { "parl", "pal99", "pal00", "pal01", "pal02", "pal03", "pal04", "pal05", "pal06" },
				["pWPN"] = new[] { "pwpn", "pwp09", "pwp10", "pwp11", "pwp12" },
				["NMS"] = new[] { "nem" },
				["VAN"] = new[] { "pvan" },
				["pHHO"] = new[] { "hho" },
				["FRF_UGIN"] = new[] { "ugin" },
				["PO2"] = new[] { "p02" },
				["MPS_AKH"] = new[] { "mp2" },
				["DD3_JVC"] = new[] { "jvc" },
				["DD3_GVL"] = new[] { "gvl" },
				["DD3_EVG"] = new[] { "dd1" },
				["DD3_DVD"] = new[] { "dvd" },
				["MED"] = new[] { "me1" },
				["pGTW"] = new[] { "pgtw", "pg07", "pg08" },
				["pSUS"] = new[] { "psus", "pjas", "pjse" }
			};

		private static readonly (Func<CardLegacy, CardLegacy, bool>, Func<CardLegacy, Card, bool>)[] _matchers =
		{
			(
				(c1, c2) => Str.Equals(c1.Number, c2.Number) || c1.MultiverseId == c2.MultiverseId,
				(c1, c2) => Str.Equals(c1.Number, c2.Number) || c1.MultiverseId == c2.MultiverseId
			),
			(
				(c1, c2) => Str.Equals(c1.Number, c2.Number),
				(c1, c2) => Str.Equals(c1.Number, c2.Number)
			),
			(
				(c1, c2) => c1.MultiverseId == c2.MultiverseId,
				(c1, c2) => c1.MultiverseId == c2.MultiverseId
			),
			(
				(c1, c2) => Str.Equals(c1.ImageNameOriginal, c2.ImageNameOriginal),
				(c1, c2) => Str.Equals(c1.ImageNameOriginal, c2.ImageName)
			)
		};

		private readonly CardRepository _repo;
		private readonly CardRepository42 _repo42;
		private readonly CardRepositoryLegacy _repoLegacy;

		private static readonly Logger _log = LogManager.GetCurrentClassLogger();
	}
}