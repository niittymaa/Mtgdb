﻿using System.IO;
using System.Linq;
using Mtgdb.Data;
using Mtgdb.Gui;
using Mtgdb.Test;
using Ninject;
using NUnit.Framework;

namespace Mtgdb.Util
{
	[TestFixture]
	public class DeckFormattersTests : TestsBase
	{
		[OneTimeSetUp]
		public void Setup()
		{
			LoadCards();
		}

		[TestCase(@"D:\Games\xmage\mage-client\sample-decks")]
		public void XMage(string decksLocation)
		{
			findCards(decksLocation, new XMageDeckFormatter(Repo));
		}

		[TestCase(@"D:\Games\Forge\res\quest\world")]
		// ReSharper disable once StringLiteralTypo
		[TestCase(@"C:\Users\Kolia\AppData\Roaming\Forge\decks")]
		public void Forge(string decksLocation)
		{
			findCards(decksLocation, new ForgeDeckFormatter(Repo, Kernel.Get<ForgeSetRepository>()));
		}

		[TestCase(@"D:\games\Magarena-1.81\Magarena\decks")]
		public void Magarena(string decksLocation)
		{
			findCards(decksLocation, new MagarenaDeckFormatter(Repo));
		}

		[Test]
		public void Mtgo()
		{
			var mtgoCardsFile = TestContext.CurrentContext.TestDirectory.AddPath("Resources\\Mtgo\\cards.txt");
			var mtgoCardNames = File.ReadAllLines(mtgoCardsFile).Distinct().OrderBy(Str.Comparer);

			Log.Debug("Unmatched mtgo cards");
			
			var cardsByMtgoName = Repo.Cards.GroupBy(MtgoDeckFormatter.ToMtgoName)
				.ToDictionary(_ => _.Key, _ => _.ToList());

			foreach (string name in mtgoCardNames)
				if (!cardsByMtgoName.ContainsKey(name))
					Log.Debug(name);
		}

		private void findCards(string decksLocation, RegexDeckFormatter formatter)
		{
			var matches = Directory.GetFiles(decksLocation, formatter.FileNamePattern, SearchOption.AllDirectories)
				.SelectMany(file => File.ReadAllLines(file).Select(line => new { line, file }))
				.GroupBy(_ => _.line)
				.Select(gr =>
				{
					string line = gr.Key;
					var match = formatter.LineRegex.Match(line);

					var card = match.Success 
						? formatter.GetCard(match) 
						: null;

					return new { match, card, files = gr.Select(_ => _.file).ToArray() };
				})
				.Where(_ => _.match.Success)
				.ToArray();

			if (matches.Length == 0)
				Assert.Fail("Decks not found");

			foreach (var match in matches)
			{
				if (match.card == null)
					Log.Debug("NOT FOUND {0} in\r\n{1}", match.match.Value, string.Join("\r\n", match.files.Select(_ => '\t' + _)));
			}
		}
	}
}
