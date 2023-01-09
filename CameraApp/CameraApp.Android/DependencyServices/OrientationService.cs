using Android.App;
using Android.Content.PM;
using CameraApp.Droid;
using EE.Droid.DependencyServices;
using EE.Interfaces;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(OrientationService))]
namespace EE.Droid.DependencyServices
{
	// https://stackoverflow.com/questions/42290561/how-to-set-contentpage-orientation-or-screen-orientation-on-particular-page-in-x
	public class OrientationService : IOrientationService
	{
		public void Landscape()
		{
			// NOTE: Forms.Context is Obsolete, https://stackoverflow.com/questions/51258783/forms-context-is-obsolete-so-how-should-i-get-activity-of-my-single-activity-app/59284116#59284116
			((Activity)Forms.Context).RequestedOrientation = ScreenOrientation.Landscape;
			
		}

		public void Portrait()
		{
			((Activity)Forms.Context).RequestedOrientation = ScreenOrientation.Portrait;
		}
	}
}