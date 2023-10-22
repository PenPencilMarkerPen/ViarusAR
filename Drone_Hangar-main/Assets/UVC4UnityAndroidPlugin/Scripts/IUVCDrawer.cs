using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Serenegiant.UVC
{


	public interface IUVCDrawer
	{

		bool OnUVCAttachEvent(UVCManager manager, UVCDevice device);

		void OnUVCDetachEvent(UVCManager manager, UVCDevice device);

		bool CanDraw(UVCManager manager, UVCDevice device);

		void OnUVCStartEvent(UVCManager manager, UVCDevice device, Texture tex);

		void OnUVCStopEvent(UVCManager manager, UVCDevice device);

	}  

}
