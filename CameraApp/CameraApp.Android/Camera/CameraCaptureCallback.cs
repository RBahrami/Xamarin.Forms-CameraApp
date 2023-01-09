using Android.Hardware.Camera2;
using Android.Util;
using Java.Lang;

namespace Camera2Xam
{
	public class CameraCaptureCallback : CameraCaptureSession.CaptureCallback
	{
		const string TAG = "Camera2Xam.CameraCaptureCallbacks";
		private readonly CameraHandler owner;

		public CameraCaptureCallback(CameraHandler owner)
		{
			this.owner = owner ?? throw new System.ArgumentNullException("owner");
		}

		public override void OnCaptureCompleted(CameraCaptureSession session, CaptureRequest request, TotalCaptureResult result)
		{
			Log.Info(TAG, "OnCaptureCompleted");
			Process(result);
		}

		public override void OnCaptureProgressed(CameraCaptureSession session, CaptureRequest request, CaptureResult partialResult)
		{
			Log.Info(TAG, "OnCaptureProgressed");
			Process(partialResult);
		}

		private void Process(CaptureResult result)
		{
			switch (owner.FsmState)
			{
				case CameraHandler.FsmStates.WAITING_LOCK:
					{
						Integer afState = (Integer)result.Get(CaptureResult.ControlAfState);
						if (afState.IntValue() == 0) //if (afState == null) // REZA
						{
							// If Auto-Focus is disabled (or not exist)
							owner.FsmState = CameraHandler.FsmStates.PICTURE_TAKEN; // avoids multiple picture callbacks
							owner.CaptureStillPicture();
						}
						else if ((((int)ControlAFState.FocusedLocked) == afState.IntValue()) || (((int)ControlAFState.NotFocusedLocked) == afState.IntValue()))
						{
							// ControlAeState can be null on some devices  // TODO: Probably null is wrong, it values must be 0
							Integer aeState = (Integer)result.Get(CaptureResult.ControlAeState);
							if (aeState == null || aeState.IntValue() == ((int)ControlAEState.Converged))
							{
								owner.FsmState = CameraHandler.FsmStates.PICTURE_TAKEN;
								owner.CaptureStillPicture();
							}
							else
							{
								owner.RunPrecaptureSequence();
							}
						}
						break;
					}
				case CameraHandler.FsmStates.WAITING_PRECAPTURE:
					{
						// ControlAeState can be null on some devices
						Integer aeState = (Integer)result.Get(CaptureResult.ControlAeState);
						if (aeState == null || // TODO: Probably null is wrong, it values must be 0
								aeState.IntValue() == ((int)ControlAEState.Precapture) ||
								aeState.IntValue() == ((int)ControlAEState.FlashRequired))
						{
							owner.FsmState = CameraHandler.FsmStates.WAITING_NON_PRECAPTURE;
						}
						break;
					}
				case CameraHandler.FsmStates.WAITING_NON_PRECAPTURE:
					{
						// ControlAeState can be null on some devices
						Integer aeState = (Integer)result.Get(CaptureResult.ControlAeState);
						if (aeState == null || // TODO: Probably null is wrong, it values must be 0
							aeState.IntValue() != ((int)ControlAEState.Precapture))
						{
							owner.FsmState = CameraHandler.FsmStates.PICTURE_TAKEN;
							owner.CaptureStillPicture();
						}
						break;
					}
			}
		}
	}
}
