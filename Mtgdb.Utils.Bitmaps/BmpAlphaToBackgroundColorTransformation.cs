using System.Drawing;

namespace Mtgdb
{
	public class BmpAlphaToBackgroundColorTransformation : BmpProcessor
	{
		public BmpAlphaToBackgroundColorTransformation(Bitmap bmp, Color bg) : base(bmp) =>
			_bg = new[] { bg.R, bg.G, bg.B };

		protected override void ExecuteRaw()
		{
			for (int i = 0; i < Rect.Width; i++)
				for (int j = 0; j < Rect.Height; j++)
				{
					var location = GetLocation(i, j);

					int alphaIndex = location + A;

					byte a = BgraValues[alphaIndex];
					int bgProportion = byte.MaxValue - a;

					if (bgProportion == 0)
						continue;

					ImageChanged = true;

					BgraValues[alphaIndex] = byte.MaxValue;

					for (int c = 0; c < 3; c++)
					{
						BgraValues[location + c] = (byte) ((
								_bg[c] * bgProportion +
								BgraValues[location + c] * (byte.MaxValue - bgProportion)) /
							byte.MaxValue);
					}
				}
		}

		private readonly byte[] _bg;
	}
}