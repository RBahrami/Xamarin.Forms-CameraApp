using EE.Camera;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Camera2XamF
{
	public enum CameraOptions
	{
		Rear,
		Front
	}

	public class CameraView : View
	{
		Command takePicture;
		Command initCamera;

		public static readonly BindableProperty AutoInitCameraProperty = BindableProperty.Create(
			propertyName: "AutoInitCamera",
			returnType: typeof(bool),
			declaringType: typeof(CameraView),
			defaultValue: false);

		public bool AutoInitCamera
		{
			get { return (bool)GetValue(AutoInitCameraProperty); }
			set { SetValue(AutoInitCameraProperty, value); }
		}

		public ICameraService CameraService;

		public Command TakePicture
		{
			get { return takePicture; }
			set { takePicture = value; }
		}

		public Command InitCamera
		{
			get { return initCamera; }
			set { initCamera = value; }
		}

		public void PictureTaken(byte[] imgSource)
		{
			PictureFinished?.Invoke(imgSource);
		}

		public event Action<byte[]> PictureFinished;
	}
}
