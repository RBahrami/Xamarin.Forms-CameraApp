using Camera2XamF;
using EE.Camera;
using EE.Interfaces;
using PermissionsHelper;
using SkiaSharp;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using static Xamarin.Essentials.Permissions;

namespace CameraApp
{
	public partial class MainPage : ContentPage
	{
		private CameraView cameraView;
		public MainPage()
		{
			DependencyService.Get<IOrientationService>().Landscape();
			NavigationPage.SetHasNavigationBar(this, false); // https://stackoverflow.com/questions/61313976/how-to-make-a-xamarin-app-fullscreen-all-screen
			
			InitializeComponent();

			Task<bool> hasCameraPermission = GetPermissions.Camera();
			hasCameraPermission.Wait();
			if (!hasCameraPermission.Result)
			{
				DisplayAlert("Camera Permission Error", $"The App doesn't have access to the camera", "", "Close");
				// TODO: Close App
				return;
			}


			ICameraService cameraService = DependencyService.Get<ICameraService>();

			var cameras = cameraService.GetCameras();
			var camera = cameras[1];
			// Danger, W.R.! Attempting to use too large a preview size could  exceed the camera
			// bus' bandwidth limitation, resulting in gorgeous previews but the storage of
			// garbage capture data.
			var previewSize = ImageSize.GetMaximumSize(camera.SupportedJpegSizes, "16:9");
			var captureSize = ImageSize.GetMaximumSize(camera.SupportedJpegSizes, "16:9");

			// NOTE: There is a problem with Flash of camera, to turn it off, pass 1, for auto-flash pass 4, overall the flash is buggy
			var options = new Options { Info = camera, AfMode = AfMode.ContinuousPicture, PreviewSize = previewSize, CaptureSize = captureSize, FlashMode = (FlashMode)1  };
			cameraService.SelectCamera(options);
			

			cameraView = new CameraView
			{
				CameraService = cameraService,
				AutoInitCamera = false,
				Margin = new Thickness(0, 0, 0, 0),
			};

			mainGrid.Children.Add(cameraView, 0, 0);

			// Timer is to make sure that cameraView is created (OnElementChanged() is called), then executing the below code
			Device.StartTimer(TimeSpan.FromSeconds(1), () =>
			{
				// Timer is to make sure that cameraView is created (i.e. OnElementChanged() is called), then executing the below code
				
				cameraView.InitCamera?.Execute(null);
				cameraView.PictureFinished += OnPictureFinished;
				Device.BeginInvokeOnMainThread(() =>
				{
					cameraView.HorizontalOptions = LayoutOptions.Center;
					cameraView.VerticalOptions = LayoutOptions.Center;

				});
				return false; // True = Repeat again, False = Stop the timer
			});
		}

		protected override void OnAppearing() // can be async
		{
			base.OnAppearing();
		}

		private void OnPictureFinished(byte[] imgSource)
		{
			//var i = SKBitmap.Decode(imgSource);
			var i = SKImage.FromEncodedData(imgSource);
			DisplayAlert("Confirm", $"Picture Taken {i.Height}, {i.Width}", "", "OK");
			Navigation.PushAsync(new ViewPage(i));
		}

		private void CaptureBtn_Clicked(object sender, EventArgs e)
		{
			cameraView.TakePicture?.Execute(null);
		}
	}
}
