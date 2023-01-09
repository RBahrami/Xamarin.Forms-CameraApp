using System.Threading.Tasks;
using Xamarin.Essentials;

using CameraApp;

namespace PermissionsHelper
{
	public class GetPermissions
	{
        public static async Task<bool> Camera()
        {
            var status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                await App.Current.MainPage.DisplayAlert("Camera", "I Need Permission for Access to Camera!", "OK");
                return false;
            }
            return true;
        }

        public static async Task<bool> Microphone()
		{
			var status = await Permissions.RequestAsync<Permissions.Microphone>();
			if (status != PermissionStatus.Granted)
			{
				await App.Current.MainPage.DisplayAlert("Microphone", "I Need Permission for Access to Microphone!", "OK");
				return false;
			}
			return true;
		}

		public static async Task<bool> StorageWrite()
		{
			var status = await Permissions.RequestAsync<Permissions.StorageWrite>();
			if (status != PermissionStatus.Granted)
			{
				await App.Current.MainPage.DisplayAlert("Storage", "I Need Permission for Write in Storage!", "OK");
				return false;
			}
			return true;
		}

		public static async Task<bool> StorageRead()
		{
			var status = await Permissions.RequestAsync<Permissions.StorageRead>();
			if (status != PermissionStatus.Granted)
			{
				await App.Current.MainPage.DisplayAlert("Storage", "I Need Permission for Read from Storage!", "OK");
				return false;
			}
			return true;
		}
	}
}
