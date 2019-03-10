using Mtgdb.Data;
using NUnit.Framework;

namespace Mtgdb.Test
{
	[TestFixture]
	public class ImageLoaderTests : TestsBase
	{
		[OneTimeSetUp]
		public void Setup()
		{
			LoadCards();

			ImgRepo.LoadFilesSmall();
			ImgRepo.LoadSmall();
		}

		[TestCase("V17")]
		[TestCase("RIX")]
		[TestCase("A25")]
		[TestCase("DDU")]
		[TestCase("DOM")]
		public void Small_images_have_transparent_corners(string setCode)
		{
			int removedCornersCount = 0;

			bool isCornerRemoved;
			bool isFoundInCache;

			var imageLoader = new ImageLoader(new UiConfigRepository());
			imageLoader.CornerRemoved += () => { isCornerRemoved = true; };
			imageLoader.FoundInCache += () => { isFoundInCache = true; };

			var cards = Repo.SetsByCode[setCode].Cards;

			foreach (var card in cards)
			{
				isCornerRemoved = false;
				isFoundInCache = false;

				var model = Ui.GetSmallImage(card);
				imageLoader.GetSmallImage(model);

				string message = $"{card.SetCode} {model.ImageFile.FullPath}";

				if (!isFoundInCache)
				{
					Assert.That(isCornerRemoved, message);
					removedCornersCount++;
				}
			}

			Assert.That(removedCornersCount, Is.GreaterThan(0));
		}
	}
}