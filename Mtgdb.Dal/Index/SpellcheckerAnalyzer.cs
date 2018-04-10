﻿using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Util;

namespace Mtgdb.Dal.Index
{
	public class SpellcheckerAnalyzer : Analyzer
	{
		public SpellcheckerAnalyzer()
			:base(GLOBAL_REUSE_STRATEGY)
		{
		}

		protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
		{
			var tokenizer = new KeywordTokenizer(reader);
			var filter = new LowerCaseFilter(LuceneVersion.LUCENE_48, tokenizer);
			return new TokenStreamComponents(tokenizer, filter);
		}
	}
}