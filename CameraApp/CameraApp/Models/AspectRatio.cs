using System;

namespace EE.Camera
{
	public static class AspectRatio
	{
		static double margin = 0.05;

		// https://stackoverflow.com/questions/10070296/c-sharp-how-to-calculate-aspect-ratio
		private static int GCD(int a, int b)
		{
			int Remainder;

			while (b != 0)
			{
				Remainder = a % b;
				a = b;
				b = Remainder;
			}

			return a;
		}

		public static string Calculate(int width, int height)
		{
			double ratio = (double)width / (double)height;

			if (ratio == 1)
				return "1:1";
			else if (Math.Abs(ratio - 1.25) <= margin)
				return "5:4";
			else if (Math.Abs(ratio - 1.33) <= margin)
				return "4:3";
			else if (Math.Abs(ratio - 1.50) <= margin)
				return "3:2";
			else if (Math.Abs(ratio - 1.60) <= margin)
				return "16:10";
			else if (Math.Abs(ratio - 1.78) <= margin)
				return "16:9";
			else
				return string.Format("{0}:{1}", width / GCD(width, height), height / GCD(width, height));
		}
	}
}
