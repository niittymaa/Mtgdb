using Mtgdb.Ui;

namespace Mtgdb.Gui
{
	public interface IDeckFormatter
	{
		string Description { get; }
		string FileNamePattern { get; }

		Deck ImportDeck(string serialized);
		string ExportDeck(string name, Deck current);

		bool ValidateFormat(string serialized);

		bool SupportsExport { get; }
		bool SupportsImport { get; }
		bool SupportsFile { get; }

		bool UseBom { get; }
		string FormatHint { get; }
	}
}