using System.Diagnostics;
using Mtgdb.Data;
using Mtgdb.Data.Index;
using Mtgdb.Downloader;
using Ninject;
using NLog;
using NUnit.Framework;

namespace Mtgdb.Test
{
	public class TestsBase
	{
		[SetUp]
		public void BaseSetup()
		{
			ApplicationCulture.SetCulture(Str.Culture);
		}

		[TearDown]
		public void TearDown()
		{
			LogManager.Flush();
		}

		public static void LoadModules()
		{
			if (_loadedModules)
				return;

			Kernel = new StandardKernel();
			Kernel.Load<DalModule>();
			Kernel.Load<CoreModule>();
			Kernel.Load<DownloaderModule>();
			Kernel.Load<DeckListModule>();

			Repo = Kernel.Get<CardRepository>();
			ImgRepo = Kernel.Get<ImageRepository>();
			Ui = Kernel.Get<UiModel>();

			_loadedModules = true;
		}

		protected static void LoadCards()
		{
			if (_loadedCards)
				return;

			LoadModules();

			var sw = new Stopwatch();
			sw.Start();

			Repo.LoadFile();
			Repo.Load();

			sw.Stop();
			_log.Info($"Cards loaded in {sw.ElapsedMilliseconds} ms");

			LogManager.Flush();

			_loadedCards = true;
		}

		protected static void LoadTranslations()
		{
			if (_loadedTranslations)
				return;

			LoadCards();

			var sw = new Stopwatch();
			sw.Start();

			var locRepo = Kernel.Get<LocalizationRepository>();
			locRepo.LoadFile();
			locRepo.Load();

			Repo.FillLocalizations(locRepo);

			sw.Stop();
			_log.Info($"Translations loaded in {sw.ElapsedMilliseconds} ms");

			LogManager.Flush();

			_loadedTranslations = true;
		}

		protected static void LoadIndexes()
		{
			if (_loadedIndexes)
				return;

			LoadTranslations();

			Searcher = Kernel.Get<CardSearcher>();

			var sw = new Stopwatch();

			sw.Start();
			Searcher.LoadIndexes();
			sw.Stop();
			_log.Info($"Searcher indexes loaded in {sw.ElapsedMilliseconds} ms");

			Spellchecker = Searcher.Spellchecker;

			sw.Start();
			KeywordSearcher = Kernel.Get<KeywordSearcher>();
			KeywordSearcher.Load();
			sw.Stop();
			_log.Info($"Keyword searcher index loaded in {sw.ElapsedMilliseconds} ms");

			LogManager.Flush();

			_loadedIndexes = true;
		}

		protected static void LoadSmallAndZoomImages()
		{
			if (_loadedSmallAndZoomImages)
				return;

			ImgRepo.LoadFiles();
			ImgRepo.LoadZoom();

			_loadedSmallAndZoomImages = true;
		}

		protected static IKernel Kernel;
		protected static CardRepository Repo { get; set; }
		protected static ImageRepository ImgRepo { get; set; }
		protected static UiModel Ui { get; set; }

		protected static CardSearcher Searcher { get; set; }
		protected static CardSpellchecker Spellchecker { get; set; }
		protected static KeywordSearcher KeywordSearcher { get; set; }

		private static bool _loadedModules;
		private static bool _loadedCards;
		private static bool _loadedTranslations;
		private static bool _loadedIndexes;
		private static bool _loadedSmallAndZoomImages;

		private static readonly Logger _log = LogManager.GetCurrentClassLogger();

		protected Logger Log => LogManager.GetLogger(GetType().FullName);
	}
}