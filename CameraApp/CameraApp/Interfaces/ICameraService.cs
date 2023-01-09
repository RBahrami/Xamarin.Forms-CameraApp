using System;
using System.Collections.Generic;
using System.Text;

namespace EE.Camera
{
	public interface ICameraService
	{
		List<Info> GetCameras();
		void SelectCamera(Options options);
	}

}
