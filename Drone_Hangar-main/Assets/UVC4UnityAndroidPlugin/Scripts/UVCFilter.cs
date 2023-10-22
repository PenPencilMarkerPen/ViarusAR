using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Serenegiant.UVC
{

	[Serializable]
	public class UVCFilter
	{
		private const string TAG = "UVCFilter#";


		public string Description;

		public int Vid;

		public int Pid;

		public string DeviceName;

		public bool IsExclude;


		public bool Match(UVCDevice device)
		{
			bool result = device != null;

			if (result)
			{
				result &= ((Vid <= 0) || (Vid == device.vid))
					&& ((Pid <= 0) || (Pid == device.pid))
					&& (String.IsNullOrEmpty(DeviceName)
						|| DeviceName.Equals(device.name)
						|| DeviceName.Equals(device.name)
						|| (String.IsNullOrEmpty(device.name) || device.name.Contains(DeviceName))
						|| (String.IsNullOrEmpty(device.name) || device.name.Contains(DeviceName))
					);
			}

			return result;
		}

		//--------------------------------------------------------------------------------


		public static bool Match(UVCDevice device, List<UVCFilter> filters/*Nullable*/)
		{
			return Match(device, filters != null ? filters.ToArray() : (null as UVCFilter[]));
		}


		public static bool Match(UVCDevice device, UVCFilter[] filters/*Nullable*/)
		{
			var result = true;

			if ((filters != null) && (filters.Length > 0))
			{
				result = false;
				foreach (var filter in filters)
				{
					if (filter != null)
					{
						var b = filter.Match(device);
						if (b && filter.IsExclude)
						{   
							result = false;
							break;
						}
						else
						{   
							result |= b;
						}
					}
					else
					{
						
						result = true;
					}

				}
			}

#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}Match({device}):result={result}");
#endif
			return result;
		}

	}

}
