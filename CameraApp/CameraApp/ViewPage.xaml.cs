using SkiaSharp.Views.Forms;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using EE.Interfaces;

namespace CameraApp
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ViewPage : ContentPage
	{
		SKImage image;
		public ViewPage(SKImage image)
		{
			DependencyService.Get<IOrientationService>().Landscape();
			NavigationPage.SetHasNavigationBar(this, false); // https://stackoverflow.com/questions/61313976/how-to-make-a-xamarin-app-fullscreen-all-screen
			InitializeComponent();
			this.image = image;
		}

		private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			SKImageInfo info = e.Info;
			SKSurface surface = e.Surface;
			SKCanvas canvas = surface.Canvas;

			
			canvas.Clear();


			//canvas.RotateDegrees(90, info.Width / 2, info.Height / 2);

			int imgW = image.Width;
			int imgH = image.Height;

			// Stretching while preserving the aspect ratio
			// https://learn.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/bitmaps/displaying#stretching-while-preserving-the-aspect-ratio
			float scale = Math.Min((float)info.Width / imgW, (float)info.Height / imgH);
			float x1 = (info.Width - scale * imgW) / 2;
			float x2 = x1 + scale * imgW;
			float y1 = (info.Height - scale * imgH) / 2;
			float y2 = y1 + scale * imgH;

			SKRect destRect = new SKRect(x1, y1, x2, y2);
			
			canvas.DrawImage(image, destRect);
		}
	}
}