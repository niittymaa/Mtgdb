using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;
using ReadOnlyCollectionsExtensions;

namespace Mtgdb.Controls
{
	public class ColorSchemeController
	{
		public static event Action SystemColorsChanging;
		public static event Action SystemColorsChanged;

		public ColorSchemeController()
		{
			// force init color table
			byte unused = SystemColors.Window.R;

			var systemDrawingAssembly = typeof(Color).Assembly;

			_colorTableField = systemDrawingAssembly.GetType("System.Drawing.KnownColors")
				.GetField("ArgbValues", BindingFlags.Static | BindingFlags.NonPublic);

			_colorTable = readColorTable();
			SystemEvents.UserPreferenceChanging += userPreferenceChanging;

			OriginalColors = _colorTable.ToArray();
			KnownOriginalColors = KnownColors.Cast<int>()
				.ToDictionary(i => i, i => unchecked((int)OriginalColors[i]))
				.AsReadOnlyDictionary();

			_threadDataProperty = systemDrawingAssembly.GetType("System.Drawing.SafeNativeMethods")
				.GetNestedType("Gdip", BindingFlags.NonPublic)
				.GetProperty("ThreadData", BindingFlags.Static | BindingFlags.NonPublic);

			SystemBrushesKey = typeof(SystemBrushes)
				.GetField("s_systemBrushesKey", BindingFlags.Static | BindingFlags.NonPublic)
				.GetValue(null);
		}

		private void userPreferenceChanging(object sender, UserPreferenceChangingEventArgs e)
		{
			if (e.Category != UserPreferenceCategory.Color)
				return;

			_colorTable = readColorTable();
			fireColorsChangedEvents();
		}

		private static void fireColorsChangedEvents()
		{
			SystemColorsChanging?.Invoke();
			SystemColorsChanged?.Invoke();
		}

		private uint[] readColorTable()
		{
			var result = (uint[]) _colorTableField.GetValue(null);
			return result;
		}

		public void SetColor(KnownColor knownColor, int argb)
		{
			setColor(knownColor, argb);

			ThreadData[SystemBrushesKey] = null;
			fireColorsChangedEvents();
		}

		private void setColor(KnownColor knownColor, int argb) =>
			_colorTable[(int) knownColor] = unchecked((uint)argb);

		public int GetOriginalColor(KnownColor knownColor) =>
			unchecked((int) OriginalColors[(int) knownColor]);

		public int GetColor(KnownColor knownColor)
		{
			if (!KnownColors.Contains(knownColor))
				throw new ArgumentException();

			return unchecked((int) _colorTable[(int) knownColor]);
		}

		public IReadOnlyDictionary<int, int> Save() =>
			KnownColors.Cast<int>()
				.ToDictionary(i => i, i => unchecked((int) _colorTable[i]))
				.AsReadOnlyDictionary();

		public void Load(IReadOnlyDictionary<int, int> saved)
		{
			foreach (var color in KnownColors)
			{
				var value = saved.TryGet((int) color, unchecked((int) KnownOriginalColors[(int) color]));
				setColor(color, value);
			}

			ThreadData[SystemBrushesKey] = null;
			fireColorsChangedEvents();
		}

		public void Reset(KnownColor color) =>
			SetColor(color, unchecked((int)OriginalColors[(int) color]));

		public void ResetAll() =>
			Load(KnownOriginalColors);

		private IDictionary ThreadData =>
			(IDictionary) _threadDataProperty.GetValue(null, null);

		private object SystemBrushesKey { get; }

		public readonly HashSet<KnownColor> KnownColors = new HashSet<KnownColor>(
			new[]
			{
				SystemColors.Control,
				SystemColors.ControlText,

				SystemColors.ButtonFace, // menu gradient
				SystemColors.ButtonShadow, // menu border

				SystemColors.Window,
				SystemColors.WindowText,
				SystemColors.GrayText,

				SystemColors.HotTrack,
				SystemColors.Highlight,
				SystemColors.HighlightText,

				SystemColors.ActiveCaption,
				SystemColors.GradientActiveCaption,

				SystemColors.InactiveCaption,
				SystemColors.GradientInactiveCaption,

				SystemColors.ActiveBorder
			}.Select(_ => _.ToKnownColor())
		);

		private uint[] OriginalColors { get; }
		private IReadOnlyDictionary<int, int> KnownOriginalColors { get; }

		private uint[] _colorTable;
		private readonly FieldInfo _colorTableField;
		private readonly PropertyInfo _threadDataProperty;
	}
}