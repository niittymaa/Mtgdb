﻿using System;
using System.Drawing;

namespace Mtgdb
{
	public class BitmapTransformationChain : TransformationChain<Bitmap>
	{
		public BitmapTransformationChain(Bitmap original, Action<Exception> logger) : base(original, bmp => (Bitmap) bmp.Clone()) => 
			TransformationException += logger;
	}
}