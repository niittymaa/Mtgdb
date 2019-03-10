using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Mtgdb.Controls
{
	public static class Dpi
	{
		public static event Action BeforeChanged;
		public static event Action Changed;
		public static event Action AfterChanged;

		public static void Set(int uiScalePercent = 100)
		{
			if (!_initialized && Environment.OSVersion.Version.Major >= 6)
				SetProcessDPIAware();

			if (_initialized && _uiScalePercent == uiScalePercent)
				return;

			_initialized = true;

			_uiScalePercent = uiScalePercent;
			_scale = getDpiScale().MultiplyBy(_uiScalePercent / 100f);

			BeforeChanged?.Invoke();
			Changed?.Invoke();
			AfterChanged?.Invoke();
		}

		public static Font ByDpi(this Font font)
		{
			if (_uiScalePercent == 100)
				return font;

			return new Font(
				font.FontFamily,
				font.Size * _uiScalePercent / 100f,
				font.Style,
				font.Unit,
				font.GdiCharSet,
				font.GdiVerticalFont);
		}

		public static float ByDpiWidth(this float width) =>
			width * _scale.Width;

		public static int ByDpiWidth(this int width) =>
			(width * _scale.Width).Round();

		public static int ByDpiHeight(this int height) =>
			(height * _scale.Height).Round();

		public static Size ByDpi(this Size original) =>
			original.MultiplyBy(_scale).Round();

		public static Padding ByDpi(this Padding original) =>
			new Padding(original.Left.ByDpiWidth(), original.Top.ByDpiHeight(), original.Right.ByDpiWidth(), original.Bottom.ByDpiHeight());

		public static SizeF ByDpi(this SizeF original) =>
			original.MultiplyBy(_scale);

		private static SizeF getDpiScale()
		{
			var g = Graphics.FromHwnd(IntPtr.Zero);
			var desktopPtr = g.GetHdc();

			//http://pinvoke.net/default.aspx/gdi32/GetDeviceCaps.html
			int dpiX = GetDeviceCaps(desktopPtr, 88);
			int dpiY = GetDeviceCaps(desktopPtr, 90);
			
			return new SizeF((float) dpiX / 96, (float) dpiY / 96);
		}

		public static int ScalePercent => (int)Math.Ceiling(100 * Math.Max(_scale.Width, _scale.Height));
		private static int _uiScalePercent;

		[DllImport("user32.dll")]
		private static extern bool SetProcessDPIAware();

		[DllImport("gdi32.dll")]
		private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

		private static SizeF _scale = new SizeF(1f, 1f);
		private static bool _initialized;
	}
}