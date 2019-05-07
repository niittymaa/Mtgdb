using System.Globalization;
using System.Reflection;
using System.Threading;

namespace Mtgdb
{
	public static class ApplicationCulture
	{
		public static void SetCulture(CultureInfo culture)
		{
			var userDefaultCultureProperty = typeof(CultureInfo).GetField("default_current_culture", BindingFlags.Static | BindingFlags.NonPublic);
			userDefaultCultureProperty.SetValue(null, culture);

			Thread.CurrentThread.CurrentCulture = culture;
			Thread.CurrentThread.CurrentUICulture = culture;
		}
	}
}