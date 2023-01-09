using Android.Content;
using Camera2Xam;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Camera2XamF;
using Android.Util;
using EE.Camera;
using Android.Hardware.Camera2;

[assembly: ExportRenderer(typeof(CameraView), typeof(CameraViewServiceRenderer))]
namespace Camera2Xam
{
	public class CameraViewServiceRenderer : ViewRenderer<CameraView, CameraViewBackend>
	{
		private CameraViewBackend cameraViewBackend;
		private readonly Context context;
		private CameraView cameraView;
		
		public CameraViewServiceRenderer(Context context) : base(context)
		{
			this.context = context;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<CameraView> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				// Unsubscribe from event handlers and cleanup any resources
			}

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					// Instantiate the native control and assign it to the Control property with
					// the SetNativeControl method

					cameraView = e.NewElement;

					CameraHandler cameraHandler = ((CameraService)cameraView.CameraService).GetCameraHandler();
					
					cameraView.InitCamera = new Command(() => { InitCamera(cameraHandler); });

					if (cameraView.AutoInitCamera == true)
					{
						InitCamera(cameraHandler);
					}

				}

				// Configure the control and subscribe to event handlers
			}
		}

		protected override void Dispose(bool disposing)
		{
			if(cameraViewBackend != null)
				cameraViewBackend.cameraHandler.OnPhotoTakenEvent -= OnPhotoTaken;

			base.Dispose(disposing);
		}

		private void InitCamera(CameraHandler cameraHandler)
		{
			// Create new CameraDroid object to interact with Android's CameraDevice 
			cameraViewBackend = new CameraViewBackend(this.context, cameraHandler);

			SetNativeControl(cameraViewBackend);

			if (cameraView != null && cameraViewBackend != null)
			{
				cameraView.TakePicture = new Command(() => { cameraViewBackend.cameraHandler.TakePicture(); });	
				cameraViewBackend.cameraHandler.OnPhotoTakenEvent += OnPhotoTaken;
			}
		}

		private void OnPhotoTaken(object sender, byte[] imgSource)
		{
			//Here you have the image byte data to do whatever you want 
			Device.BeginInvokeOnMainThread(() =>
			{
				cameraView?.PictureTaken(imgSource);
			});
		}
	}
}
