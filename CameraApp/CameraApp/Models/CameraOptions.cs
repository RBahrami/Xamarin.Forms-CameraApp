using System;
using System.Collections.Generic;
using System.Text;

namespace EE.Camera
{
	public enum LensFacing
	{
		Front = 0,
		Back,
		External
	}

	public enum AfMode
	{
		Off = 0,
		Auto,
		Macro,
		ContinuousVideo,
		ContinuousPicture,
		Edof
	}

	public enum FlashMode
	{
		Off = 0,
		On,
		OnAlwaysFlash,
		OnAutoFlash,
		OnAutoFlashRedeye,
		OnExternalFlash
	}

	public class ImageSize
	{
		public int Height { get; set; }
		public int Width { get; set; }
		public string AspectRatio { get; set; }
		public static ImageSize GetMaximumSize(ImageSize[] imageSizes, string aspectRatio)
		{
			for(int i = 0; i < imageSizes.Length; i++)
			{
				if (imageSizes[i].AspectRatio == aspectRatio)
					return imageSizes[i];
			}
			return null;
		}
	}

	public class Info
	{
		public string Id { get; set; }
		
		public LensFacing LensFacing { get; set; }

		public ImageSize[] SupportedJpegSizes { get; set; }
		
		public AfMode[] AfModes { get; set; }

		public bool HasFlash { get; set; }
	}

	public class Options
	{
		public Info Info { get; set; }
		public ImageSize PreviewSize { get; set; }
		public ImageSize CaptureSize { get; set; }
		public AfMode AfMode { get; set; }
		public FlashMode FlashMode {get; set;}
	}

}
