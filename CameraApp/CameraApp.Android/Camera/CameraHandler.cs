﻿using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Media;
using Android.OS;
using Android.Views;
using EE.Camera;
using Java.Lang;
using Java.Util.Concurrent;
using System;
using System.Collections.Generic;
using Size = Android.Util.Size;

namespace Camera2Xam
{
	public class CameraHandler
	{
		string TAG = typeof(CameraHandler).FullName;

		/// <summary>
		/// Enumeration of CameraDroid's FSM states.
		/// </summary>
		public enum FsmStates
		{
			/// <summary>
			/// Camera state: Showing camera preview.
			/// </summary>
			PREVIEW = 0,

			/// <summary>
			/// Camera state: Waiting for the focus to be locked.
			/// </summary>
			WAITING_LOCK,

			/// <summary>
			/// Camera state: Waiting for the exposure to be precapture state.
			/// </summary>
			WAITING_PRECAPTURE,

			/// <summary>
			///Camera state: Waiting for the exposure state to be something other than precapture.
			/// </summary>
			WAITING_NON_PRECAPTURE,

			/// <summary>
			/// Camera state: Picture was taken.
			/// </summary>
			PICTURE_TAKEN
		}

		#region Objects

		/// <summary>
		/// A {@link CameraCaptureSession } for camera preview.
		/// </summary>
		private CameraCaptureSession captureSession;

		/// <summary>
		/// CameraDevice.StateListener is called when a CameraDevice changes its state
		/// </summary>
		private readonly CameraStateListener stateCallback;

		/// <summary>
		/// An additional thread for running tasks that shouldn't block the UI.
		/// </summary>
		private HandlerThread backgroundThread;

		/// <summary>
		/// A {@link Handler} for running tasks in the background.
		/// </summary>
		private Handler backgroundHandler;

		/// <summary>
		/// An {@link ImageReader} that handles still image capture.
		/// </summary>
		private ImageReader imageReader;

		/// <summary>
		///{@link CaptureRequest.Builder} for the camera preview
		/// </summary>
		private CaptureRequest.Builder previewCaptureRequestBuilder;

		/// <summary>
		///{@link CaptureRequest.Builder} for the camera still picture
		/// </summary>
		private CaptureRequest.Builder stillCaptureRequestBuilder;

		/// <summary>
		/// {@link CaptureRequest} generated by {@link #previewCaptureRequestBuilder}
		/// </summary>
		private CaptureRequest previewRequest;

		/// <summary>
		/// A {@link CameraCaptureSession.CaptureCallback} that handles events related to JPEG capture.
		/// </summary>
		private readonly CameraCaptureCallback captureCallback;

		/// <summary>
		/// This is a system service that allows us to interact with CameraDevice objects.
		/// </summary>
		private CameraManager cameraManager;

		// TODO: comment
		private Surface previewSurface;

		#endregion

		#region Public Properties
		/// <summary>
		/// A reference to the opened CameraDevice
		/// </summary>
		public CameraDevice CameraDevice { get; set; }

		/// <summary>
		/// The current state of camera state for taking pictures.
		/// </summary>
		public FsmStates FsmState = FsmStates.PREVIEW;

		/// <summary>
		/// A {@link Semaphore} to prevent the app from exiting before closing the camera.
		/// </summary>
		/// </summary>
		public Semaphore OpenCloseSemaphore = new Semaphore(1);

		/// <summary>
		/// EventHandler for when the still picture is ready (is taken)
		/// </summary>
		public event EventHandler<byte[]> OnPhotoTakenEvent;

		public bool OpeningCamera { private get; set; } // TODO: Remove it, the semaphore is used instead

		/// <summary>
		/// A reference to the current CameraDevice's CameraCharacteristics Property 
		/// </summary>
		public CameraCharacteristics cameraCharacteristics;

		/// <summary>
		/// A reference to the current camera's options
		/// </summary>
		public Options CameraOptions { get; set; }
		#endregion

		// Constructor
		public CameraHandler(CameraManager cameraManager, Options options)
		{
			stateCallback = new CameraStateListener { Camera = this };

			captureCallback = new CameraCaptureCallback(this);
			this.cameraManager = cameraManager;

			CameraOptions = options;

			cameraCharacteristics = cameraManager.GetCameraCharacteristics(CameraOptions.Info.Id);
		}

		public void SetPreviewSurface(Surface previewSurface)
		{
			this.previewSurface = previewSurface;
		}

		// TODO: Comment
		private void SetUpCameraOutputs()
		{
			// This a callback object for the {@link ImageReader}.
			// "OnImageAvailable" will be called when a still image is ready to be saved.
			var onImageAvailableListener = new ImageAvailableCallback(); // TODO: Try replacing this stuff with Camear2Basic example
			onImageAvailableListener.OnPhotoTakenEvent += (sender, buffer) =>
			{
				OnPhotoTakenEvent?.Invoke(this, buffer);
			};

			// Set the resolution for the still picture
			imageReader = ImageReader.NewInstance(CameraOptions.CaptureSize.Width, CameraOptions.CaptureSize.Height, ImageFormatType.Jpeg, 1);
			imageReader.SetOnImageAvailableListener(onImageAvailableListener, backgroundHandler);
		}

		/// <summary>
		/// Opens the camera specified by {@link cameraId}.
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <exception cref="RuntimeException"></exception>
		public void OpenCamera()
		{
			if (OpeningCamera)
			{
				return;
			}

			OpeningCamera = true;

			SetUpCameraOutputs();

			try
			{
				if (!OpenCloseSemaphore.TryAcquire(2500, TimeUnit.Milliseconds))
				{
					throw new RuntimeException("Time out waiting to lock camera opening.");
				}
				cameraManager.OpenCamera(CameraOptions.Info.Id, stateCallback, backgroundHandler);
				/* NOTE: 
				 * The third argument deals with where this work will happen.
				 * Since we don’t want to occupy the main thread, it is better to 
				 * do this work in the background.
				 */
			}
			catch (CameraAccessException e)
			{
				e.PrintStackTrace();
			}
			catch (InterruptedException e)
			{
				throw new RuntimeException("Interrupted while trying to lock camera opening.", e);
			}
			// REZA: Replaced with above code
			//cameraManager.OpenCamera(cameraId, stateCallback, null); 
			/* NOTE: 
			 * The third argument deals with where this work will happen.
			 * Since we don’t want to occupy the main thread, it is better to do this work in the background.
			 * TODO: Try using another thread and pass its handler to the method instead of null)
			 */

		}

		/// <summary>
		/// Closes the current {@link CameraDevice}.
		/// </summary>
		/// <exception cref="RuntimeException"></exception>
		/// TODO: This method should be called when the view is paused
		private void CloseCamera()
		{
			try
			{
				OpenCloseSemaphore.Acquire();
				if (null != captureSession)
				{
					captureSession.Close();
					captureSession = null;
				}
				if (null != CameraDevice)
				{
					CameraDevice.Close();
					CameraDevice = null;
				}
				if (null != imageReader)
				{
					imageReader.Close();
					imageReader = null;
				}
			}
			catch (InterruptedException e)
			{
				throw new RuntimeException("Interrupted while trying to lock camera closing.", e);
			}
			finally
			{
				OpenCloseSemaphore.Release();
			}
		}

		/// <summary>
		/// Starts a background thread and its {@link Handler}.
		/// </summary>
		public void StartBackgroundThread()
		{
			backgroundThread = new HandlerThread($"Camera{CameraOptions.Info.Id}Background");
			backgroundThread.Start();
			backgroundHandler = new Handler(backgroundThread.Looper);
		}

		/// <summary>
		/// Stops the background thread and its {@link Handler}.
		/// </summary>
		public void StopBackgroundThread()
		{
			backgroundThread.QuitSafely();
			try
			{
				backgroundThread.Join();
				backgroundThread = null;
				backgroundHandler = null;
			}
			catch (InterruptedException e)
			{
				e.PrintStackTrace();
			}
		}

		/// <summary>
		/// Initiate a still image capture.
		/// </summary>
		public void TakePicture()
		{
			LockFocus();
		}

		/// <summary>
		/// Lock the focus as the first step for a still image capture.
		/// </summary>
		private void LockFocus()
		{
			try
			{
				// This is how to tell the camera to lock focus.
				previewCaptureRequestBuilder.Set(CaptureRequest.ControlAfTrigger, (int)ControlAFTrigger.Start);

				// Tell #captureCallback to wait for the lock.
				// In the 
				FsmState = FsmStates.WAITING_LOCK;
				captureSession.Capture(previewCaptureRequestBuilder.Build(), captureCallback, backgroundHandler);
			}
			catch (CameraAccessException e)
			{
				e.PrintStackTrace();
			}
		}

		/// <summary>
		/// Run the precapture sequence for capturing a still image. This method should be
		/// called when we get a response in {@link #captureCallback} from {@link #LockFocus()}.
		/// </summary>
		// 
		public void RunPrecaptureSequence()
		{
			try
			{
				// This is how to tell the camera to trigger.
				previewCaptureRequestBuilder.Set(CaptureRequest.ControlAePrecaptureTrigger, (int)ControlAEPrecaptureTrigger.Start);

				// Tell #captureCallback to wait for the precapture sequence to be set.
				FsmState = FsmStates.WAITING_PRECAPTURE;
				captureSession.Capture(previewCaptureRequestBuilder.Build(), captureCallback, backgroundHandler);
			}
			catch (CameraAccessException e)
			{
				e.PrintStackTrace();
			}
		}

		/// <summary>
		/// Capture a still picture. This method should be called when we get a response in
		/// {@link #captureCallback} from {@link #LockFocus()}.
		/// </summary>
		public void CaptureStillPicture()
		{
			if (CameraDevice == null) return;

			// This is the CaptureRequest.Builder that we use to take a picture.
			if (stillCaptureRequestBuilder == null)
				stillCaptureRequestBuilder = CameraDevice.CreateCaptureRequest(CameraTemplate.StillCapture);

			stillCaptureRequestBuilder.AddTarget(imageReader.Surface);

			// Use the same AE and AF modes as the preview.
			stillCaptureRequestBuilder.Set(CaptureRequest.ControlAfMode, (int)CameraOptions.AfMode);
			SetAutoFlash(stillCaptureRequestBuilder);

			// TODO: Orientation stuff again
			// int rotation = (int)activity.WindowManager.DefaultDisplay.Rotation;
			// stillCaptureBuilder.Set(CaptureRequest.JpegOrientation, GetOrientation(rotation));
			captureSession.StopRepeating();
			captureSession.Capture(stillCaptureRequestBuilder.Build(),
				new CameraCaptureStillPictureSessionCallback
				{
					OnCaptureCompletedAction = session =>
					{
						UnlockFocus();
					}
				}, null);
		}

		/// <summary>
		/// Unlock the focus. This method should be called when still image capture
		/// sequence is finished.
		/// </summary>
		public void UnlockFocus()
		{
			try
			{
				/// Reset the auto-focus trigger
				previewCaptureRequestBuilder.Set(CaptureRequest.ControlAfTrigger, (int)ControlAFTrigger.Cancel);
				SetAutoFlash(previewCaptureRequestBuilder);

				captureSession.Capture(previewCaptureRequestBuilder.Build(), captureCallback, backgroundHandler);
				/// After this, the camera will go back to the normal state of preview.
				FsmState = FsmStates.PREVIEW;
				captureSession.SetRepeatingRequest(previewRequest, captureCallback, backgroundHandler);
			}
			catch (CameraAccessException e)
			{
				e.PrintStackTrace();
			}
		}

		// TODO: Comment
		public void StartPreview()
		{
			if (CameraDevice == null) return;
			// throw new IllegalStateException("texture is null"); TODO: throw exception

			// We set up a CaptureRequest.Builder with the output Surface.
			previewCaptureRequestBuilder = CameraDevice.CreateCaptureRequest(CameraTemplate.Preview);
			previewCaptureRequestBuilder.AddTarget(previewSurface);

			// Here, we create a CameraCaptureSession for camera preview.
			// NOTE: Bellow code is deprecated, the commented code after this is more up to date
			List<Surface> surfaces = new List<Surface>
			{
				previewSurface,
				imageReader.Surface
			};

			CameraDevice.CreateCaptureSession(surfaces,
				new CameraCaptureSessionStateCallback
				{
					OnConfigureFailedAction = session =>
					{
						// TODO: Somehow we must notify the app/user that configuration failed
					},
					OnConfiguredAction = session =>
					{
						// When the session is ready, we start displaying the preview.
						captureSession = session;
						UpdatePreview();
					}
				},
				backgroundHandler);

			#region New Code to create Capture Session
			// ------------------------ Up to date code, NOTE: the new method don't accept backgroundHandler object and wants an Executor instead
			// ------------------------ The problem is other part's of the code needs backgroundHandler and I'm not sure if using two objects for running background tasks is
			// ------------------------ OK or not and also I need to be able to create, pause, resume, and destroy the Executor object the same way as backgroundHandler

			//List<OutputConfiguration> outputConfigs = new List<OutputConfiguration>()
			//{
			//	new OutputConfiguration(surface),
			//	new OutputConfiguration(imageReader.Surface)
			//};

			//CameraCaptureSessionStateCallback stateCallback = new CameraCaptureSessionStateCallback
			//{
			//	OnConfigureFailedAction = session =>
			//	{
			//		// TODO: Somehow must notify the app/user that configuration failed
			//	},
			//	OnConfiguredAction = session =>
			//	{
			//		// When the session is ready, we start displaying the preview.
			//		captureSession = session;
			//		UpdatePreview();
			//	}
			//};

			//var executor = Executors.NewSingleThreadExecutor();

			//SessionConfiguration sessionConfig = new SessionConfiguration((int)SessionType.Regular, outputConfigs, executor, stateCallback);
			//CameraDevice.CreateCaptureSession(sessionConfig);
			#endregion

		}

		// TODO: Comment
		private void UpdatePreview()
		{
			if (CameraDevice == null || captureSession == null) return; // The camera is already closed

			try
			{
				// Auto focus should be continuous for camera preview.
				previewCaptureRequestBuilder.Set(CaptureRequest.ControlAfMode, (int)ControlAFMode.ContinuousPicture);

				// Flash is automatically enabled when necessary.
				SetAutoFlash(previewCaptureRequestBuilder);

				// Finally, we start displaying the camera preview.
				previewRequest = previewCaptureRequestBuilder.Build();
				captureSession.SetRepeatingRequest(previewRequest, captureCallback, backgroundHandler);
			}
			catch (CameraAccessException e)
			{
				e.PrintStackTrace();
			}
		}

		// TODO: Comment
		public void SetAutoFlash(CaptureRequest.Builder requestBuilder)
		{
			if (CameraOptions.Info.HasFlash)
			{
				requestBuilder.Set(CaptureRequest.ControlAeMode, (int)CameraOptions.FlashMode);
			}
		}

	}
}