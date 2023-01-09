using Android.Content;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Camera2Xam;
using EE.Camera;
using Java.Lang;
using Java.Nio.Channels;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

[assembly: Dependency(typeof(CameraService))]
namespace EE.Camera
{
	public class CameraService : ICameraService
	{
		Context context;
		CameraManager cameraManager;
		CameraHandler cameraHandler;
		public CameraService()
		{
			context = Forms.Context;
			cameraManager = (CameraManager)context.GetSystemService(Context.CameraService);
		}

		#region Interfaces

		public List<Info> GetCameras()
		{
			List<Info> Info = new List<Info>();

			string[] cameraIds = cameraManager.GetCameraIdList();

			foreach (var id in cameraIds)
			{
				CameraCharacteristics characteristics = cameraManager.GetCameraCharacteristics(id);
				var lensFacing =  (LensFacing) (int)characteristics.Get(CameraCharacteristics.LensFacing);

				var supportedJpegSizes_ = ((StreamConfigurationMap)characteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap)).GetOutputSizes((int)ImageFormatType.Jpeg); // TODO: ImageType

				ImageSize[] supportedJpegSizes = new ImageSize[supportedJpegSizes_.Length];

				for(int i = 0; i < supportedJpegSizes.Length; i++)
				{
					supportedJpegSizes[i] = GetImageSize(supportedJpegSizes_[i]);
				}

				var supportedAfModes_int = (int[])characteristics.Get(CameraCharacteristics.ControlAfAvailableModes);
				
				var supportedAfModes = supportedAfModes_int.Cast<AfMode>().ToArray();

				var hasFlash_ = (Java.Lang.Boolean)characteristics.Get(CameraCharacteristics.FlashInfoAvailable);
				var hasFlash = hasFlash_ == null ? false : (bool)hasFlash_;

				Info.Add(new Info { Id = id, LensFacing = lensFacing, SupportedJpegSizes=supportedJpegSizes, AfModes = supportedAfModes, HasFlash = hasFlash });
			}

			return Info;
		}

		public void SelectCamera(Options options)
		{
			cameraHandler = new CameraHandler(cameraManager, options);
		}
		#endregion

		public CameraHandler GetCameraHandler()
		{
			return cameraHandler;
		}

		public ImageSize GetImageSize(Android.Util.Size size)
		{
			return new ImageSize { Height = size.Height, Width = size.Width, AspectRatio = AspectRatio.Calculate(size.Width, size.Height) };
		}
	}
}