using Android.Hardware.Camera2;
using Android.Util;
using System;

namespace Camera2Xam
{
	public class CameraCaptureSessionStateCallback : CameraCaptureSession.StateCallback
	{
		const string TAG = "CameraCaptureStateListener";

		public Action<CameraCaptureSession> OnConfigureFailedAction;

		public Action<CameraCaptureSession> OnConfiguredAction;

		public override void OnConfigureFailed(CameraCaptureSession session)
		{
			Log.Error(TAG, "'OnConfigureFailed' is called");
			OnConfigureFailedAction?.Invoke(session);
		}

		public override void OnConfigured(CameraCaptureSession session)
		{
			Log.Error(TAG, "'OnConfigured' is called");
			OnConfiguredAction?.Invoke(session);
		}
	}
}
