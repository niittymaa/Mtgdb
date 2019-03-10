﻿using System.Windows.Forms;
using Mtgdb.Controls;
using Mtgdb.Data;
using Mtgdb.Data.Index;
using Mtgdb.Ui;

namespace Mtgdb.Gui
{
	public class CardSearchSubsystem : SearchSubsystem<int, Card>
	{
		public CardSearchSubsystem(
			Control parent,
			SearchBar searchBar,
			CardSearcher searcher,
			CardDocumentAdapter adapter,
			params LayoutViewControl[] layoutViews)
			: base(
				parent,
				searchBar,
				searcher,
				adapter,
				layoutViews)
		{
		}

		protected override string GetLanguage() =>
			Ui.LanguageController.Language;

		protected override IntellisenseSuggest CycleValue(TextInputState currentState, bool backward) =>
			((CardSearcher) Searcher).Spellchecker.CycleValue(currentState, backward, ((CardSuggestModel) SuggestModel).Language);

		protected override SearchResult<int> Search(string query) =>
			Searcher.Search(query, GetLanguage());

		public override string GetFieldValueQuery(string fieldName, string fieldValue, bool useAndOperator = false)
		{
			if (fieldName == nameof(Card.Image))
				fieldName = CardQueryParser.Like;

			return base.GetFieldValueQuery(fieldName, fieldValue, useAndOperator);
		}

		public UiModel Ui { get; set; }
	}
}