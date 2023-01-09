using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Camera2Xam;
using CameraApp.Droid;
using System.Collections.Generic;
using Xamarin.Forms;
using Math = Java.Lang.Math;
using Size = Android.Util.Size;
namespace EE.Camera
{
	public class CameraViewBackend : FrameLayout, TextureView.ISurfaceTextureListener
	{
		private string TAG = typeof(CameraViewBackend).FullName;//"EE.Camera.CameraViewBackend";

		/// <summary>
		/// TextureView for camera preview
		/// </summary>
		private readonly AutoFitTextureView textureView;

		public CameraHandler cameraHandler;

		private SurfaceTexture texture;

		private Size previewSize;

		private Context context;
		// Constructor
		public CameraViewBackend(Context context, CameraHandler cameraHandler) : base(context)
		{
			this.context = context;

			var inflater = LayoutInflater.FromContext(context);

			if (inflater == null) return;
			var view = inflater.Inflate(Resource.Layout.CameraLayout, this);

			textureView = view.FindViewById<AutoFitTextureView>(Resource.Id.cameraTexture);

			textureView.SurfaceTextureListener = this;

			this.cameraHandler = cameraHandler;
		}

		public Size GetPreviewSize(int width, int height)
		{
			var streamConfigurationMap = (StreamConfigurationMap)cameraHandler.cameraCharacteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap);

			// TODO: the Camera2Basic example do some stuff related to the orientation and stuff
			// Danger, W.R.! Attempting to use too large a preview size could  exceed the camera
			// bus' bandwidth limitation, resulting in gorgeous previews but the storage of
			// garbage capture data.
			return ChooseOptimalSizeForPreview(streamConfigurationMap.GetOutputSizes(Java.Lang.Class.FromType(typeof(SurfaceTexture))), width, height);
		}

		Size ChooseOptimalSizeForPreview(IList<Size> sizes, int h, int w)
		{
			double AspectTolerance = 0.1;
			double targetRatio = (double)w / h;

			if (sizes == null)
			{
				return null;
			}

			Size optimalSize = null;
			double minDiff = double.MaxValue;
			int targetHeight = h;

			while (optimalSize == null)
			{
				foreach (Size size in sizes)
				{
					double ratio = (double)size.Width / size.Height;

					if (System.Math.Abs(ratio - targetRatio) > AspectTolerance)
						continue;
					if (System.Math.Abs(size.Height - targetHeight) < minDiff)
					{
						optimalSize = size;
						minDiff = System.Math.Abs(size.Height - targetHeight);
					}
				}

				if (optimalSize == null)
					AspectTolerance += 0.1f;
			}

			return optimalSize;
		}

		// Configures the necessary {@link android.graphics.Matrix}
		// transformation to `mTextureView`.
		// This method should be called after the camera preview size is determined in
		// setUpCameraOutputs and also the size of `mTextureView` is fixed.
		public void ConfigureTransform(int viewWidth, int viewHeight)
		{
			var rotation = (int)((Activity)context).WindowManager.DefaultDisplay.Rotation;
			Matrix matrix = new Matrix();
			RectF viewRect = new RectF(0, 0, viewWidth, viewHeight);
			RectF bufferRect = new RectF(0, 0, previewSize.Height, previewSize.Width);
			float centerX = viewRect.CenterX();
			float centerY = viewRect.CenterY();
			if ((int)SurfaceOrientation.Rotation90 == rotation || (int)SurfaceOrientation.Rotation270 == rotation)
			{
				bufferRect.Offset(centerX - bufferRect.CenterX(), centerY - bufferRect.CenterY());
				matrix.SetRectToRect(viewRect, bufferRect, Matrix.ScaleToFit.Fill);
				float scale = Math.Max((float)viewHeight / previewSize.Height, (float)viewWidth / previewSize.Width);
				matrix.PostScale(scale, scale, centerX, centerY);
				matrix.PostRotate(90 * (rotation - 2), centerX, centerY);
			}
			else if ((int)SurfaceOrientation.Rotation180 == rotation)
			{
				matrix.PostRotate(180, centerX, centerY);
			}
			textureView.SetTransform(matrix);
		}

		#region SurfaceTextureListener callbacks
		/// TODO: the code in these callbacks are different from Camera2Basic example (file: Camera2BasicSurfaceTextureListener.cs), some of them must be in page opened/closed/resumed 
		/*
		 * This callback is crucial when using the camera.
		 * This is because we want to be notified when the SurfaceTexture 
		 * is available so we can start displaying the feed on it.
		 * Be aware that only once the TextureView is attached to a window does it become available.
		*/
		public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
		{
			Log.Info(TAG, $"OnSurfaceTextureAvailable");

			cameraHandler.StartBackgroundThread();

			// Danger, W.R.! Attempting to use too large a preview size could  exceed the camera
			// bus' bandwidth limitation, resulting in gorgeous previews but the storage of
			// garbage capture data.
			previewSize = new Size(cameraHandler.CameraOptions.PreviewSize.Width, cameraHandler.CameraOptions.PreviewSize.Height);

			// We fit the aspect ratio of TextureView to the size of preview we picked.
			var orientation = Resources.Configuration.Orientation;
			if (orientation == Android.Content.Res.Orientation.Landscape)
			{
				textureView.SetAspectRatio(previewSize.Width, previewSize.Height);
			}
			else
			{
				textureView.SetAspectRatio(previewSize.Height, previewSize.Width);
			}

			texture = textureView.SurfaceTexture;
			// We configure the size of default buffer to be the size of camera preview we want.
			texture.SetDefaultBufferSize(previewSize.Width, previewSize.Height);

			cameraHandler.SetPreviewSurface(new Surface(texture));

			ConfigureTransform(width, height);
			cameraHandler.OpenCamera();

			/// NOTE:
			/// When the screen is turned off and turned back on, the SurfaceTexture is already
			/// available, and "onSurfaceTextureAvailable" will not be called. In that case, we can open
			/// a camera and start preview from here (otherwise, we wait until the surface is ready in
			/// the SurfaceTextureListener).
			/// TODO: bellow code must be in OnResueme of window (when screen turn on again)
			///if (textureView.IsAvailable)
			///{
			///	OpenCamera(width, height);
			///}
			///else
			///{
			///	textureView.SurfaceTextureListener = this;
			///}
		}

		public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
		{
			Log.Info(TAG, $"OnSurfaceTextureDestroyed");

			cameraHandler.StopBackgroundThread();

			return true;
		}

		public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
		{
			Log.Info(TAG, $"OnSurfaceTextureSizeChanged");

			ConfigureTransform(width, height);
		}

		public void OnSurfaceTextureUpdated(SurfaceTexture surface)
		{
			/// NOTE: this callback will be constantly called
			///Log.Info(TAG, $"OnSurfaceTextureUpdated callback");
		}
		#endregion

		#region FrameLayout callbacks
		protected override void OnVisibilityChanged(Android.Views.View changedView, [GeneratedEnum] ViewStates visibility)
		{
			base.OnVisibilityChanged(changedView, visibility);
			Log.Debug(TAG, $"OnVisibilityChanged, Visibility = {visibility}");
			if (visibility == ViewStates.Visible)
			{
				// OnResume() 
			}
			else
			{
				// OnPause()
			}
		}

		protected override void OnFocusChanged(bool gainFocus, [GeneratedEnum] FocusSearchDirection direction, Android.Graphics.Rect previouslyFocusedRect)
		{
			base.OnFocusChanged(gainFocus, direction, previouslyFocusedRect);
			Log.Debug(TAG, $"OnFocusChanged, Focus = {gainFocus}");
			if (gainFocus)
			{
				// OnResume() 
			}
			else
			{
				// OnPause()
			}
		}

		protected override void OnDetachedFromWindow()
		{
			base.OnDetachedFromWindow();
			Log.Debug(TAG, $"OnDetachedFromWindow");
			// OnDestroid()
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();
			Log.Debug(TAG, $"OnAttachedToWindow");
			// OnCreate()
		}

		#endregion

	}
}