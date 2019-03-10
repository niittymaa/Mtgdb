using System.Collections.Generic;
using JetBrains.Annotations;
using Ninject;

namespace Mtgdb.Data
{
	public class UiModel
	{
		[UsedImplicitly]
		public UiModel(
			ImageLoader imageLoader,
			ImageRepository imageRepo,
			CardRepository cardRepo,
			UiConfigRepository uiConfigRepository,
			[Optional, Named("collection")] ICardCollection collection)
		{
			UIConfigRepository = uiConfigRepository;
			CardRepo = cardRepo;
			Collection = collection;
			ImageLoader = imageLoader;
			ImageRepo = imageRepo;

			LanguageController = new LanguageController(CardLocalization.DefaultLanguage);
		}

		public UiModel(CardRepository repo, ICardCollection collection, ICardCollection deck)
		{
			CardRepo = repo;
			Collection = collection;
			Deck = deck;
		}

		public UiModel(CardRepository repo, ICardCollection collection)
		{
			CardRepo = repo;
			Collection = collection;
		}

		public ImageModel GetSmallImage(Card card) =>
			ImageRepo.GetSmallImage(card, CardRepo.GetReleaseDateSimilarity);

		public IReadOnlyList<ImageModel> GetZoomImages(Card card) =>
			ImageRepo.GetZooms(card, CardRepo.GetReleaseDateSimilarity);

		public IReadOnlyList<ImageModel> GetImagesArt(Card card) =>
			ImageRepo.GetArts(card, CardRepo.GetReleaseDateSimilarity);

		public LanguageController LanguageController { get; }
		public CardRepository CardRepo { get; }

		public ICardCollection Collection { get; }
		public ImageLoader ImageLoader { get; }
		public ImageRepository ImageRepo { get; }

		public ICardCollection Deck { get; set; }
		public UiConfig Config => UIConfigRepository.Config;

		private UiConfigRepository UIConfigRepository { get; }
	}
}