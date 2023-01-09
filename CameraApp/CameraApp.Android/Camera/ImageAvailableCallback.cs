using Android.Media;
using Android.Util;
using System;

namespace Camera2Xam
{
	public class ImageAvailableCallback : Java.Lang.Object, ImageReader.IOnImageAvailableListener
	{
		public event EventHandler<byte[]> OnPhotoTakenEvent;

		public void OnImageAvailable(ImageReader reader)
		{
			Log.Info("ImageAvailableListener", "'OnImageAvailable' is called");

			Image image = null;

			try
			{
				image = reader.AcquireLatestImage();
				var buffer = image.GetPlanes()[0].Buffer;
				var imageData = new byte[buffer.Capacity()];
				buffer.Get(imageData);

				// Call the OnPhotoTaken callback
				OnPhotoTakenEvent?.Invoke(this, imageData);
			}
			catch (Exception)
			{
				// ignored
			}
			finally
			{
				image?.Close();
			}
		}
	}
}
