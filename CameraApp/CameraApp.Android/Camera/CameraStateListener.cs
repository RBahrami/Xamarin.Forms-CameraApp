using Android.Hardware.Camera2;
using Android.Util;

namespace Camera2Xam
{
	// A callback objects for receiving updates about the state of a camera device.
	// https://developer.android.com/reference/android/hardware/camera2/CameraDevice.StateCallback
	public class CameraStateListener : CameraDevice.StateCallback
	{
		private const string TAG = "CameraStateListener";
		public CameraHandler Camera;

		public override void OnOpened(CameraDevice camera)
		{
			Log.Info(TAG, $"Camera {camera.Id} OnOpened callback");

			// In this callback we can start to deal with the logic on how to present the camera feed to the user
			if (Camera == null) return;
			Camera.OpenCloseSemaphore.Release();
			Camera.CameraDevice = camera;
			Camera.StartPreview();
			Camera.OpeningCamera = false;
		}

		public override void OnDisconnected(CameraDevice camera)
		{
			Log.Info(TAG, $"Camera {camera.Id} OnDisconnected callback");

			if (Camera == null) return;
			Camera.OpenCloseSemaphore.Release();
			camera.Close();
			Camera.CameraDevice = null;
			Camera.OpeningCamera = false;
		}

		public override void OnError(CameraDevice camera, CameraError error)
		{
			Log.Error(TAG, $"Camera {camera.Id} OnError callback - Error: {error}");

			camera.Close();

			if (Camera == null) return;
			Camera.OpenCloseSemaphore.Release();

			Camera.CameraDevice = null;
			Camera.OpeningCamera = false;
		}
	}
}
