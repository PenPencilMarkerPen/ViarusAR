
using System;
using System.Runtime.InteropServices;



namespace Serenegiant.UVC
{

	[Serializable]
	public class UVCDevice
	{
		public readonly Int32 id;
		public readonly int vid;
		public readonly int pid;
		public readonly int deviceClass;
		public readonly int deviceSubClass;
		public readonly int deviceProtocol;

		public readonly string name;

		public UVCDevice(IntPtr devicePtr) {
			id = GetId(devicePtr);
			vid = GetVendorId(devicePtr);
			pid = GetProductId(devicePtr);
			name = GetName(devicePtr);
			deviceClass = GetDeviceClass(devicePtr);
			deviceSubClass = GetDeviceSubClass(devicePtr);
			deviceProtocol = GetDeviceProtocol(devicePtr);
		}

		public override string ToString()
		{
			return $"{base.ToString()}(id={id},vid={vid},pid={pid},name={name},deviceClass={deviceClass},deviceSubClass={deviceSubClass},deviceProtocol={deviceProtocol})";
		}



		public bool IsRicoh
		{
			get { return (vid == 1482); }
		}


        public bool IsTHETA
        {
            get { return IsTHETA_S || IsTHETA_V || IsTHETA_Z1; }
        }
   
		public bool IsTHETA_S
		{
			get { return (vid == 1482) && (pid == 10001); }
		}

		public bool IsTHETA_V
		{
			get { return (vid == 1482) && (pid == 10002); }
		}
        public bool IsTHETA_Z1
        {
            get { return (vid == 1482) && (pid == 10005); }
        }

        [DllImport("unityuvcplugin", EntryPoint = "DeviceInfo_get_id")]
		public static extern Int32 GetId(IntPtr devicePtr);

		[DllImport("unityuvcplugin", EntryPoint = "DeviceInfo_get_device_class")]
		private static extern Byte GetDeviceClass(IntPtr devicePtr);

		[DllImport("unityuvcplugin", EntryPoint = "DeviceInfo_get_device_sub_class")]
		private static extern Byte GetDeviceSubClass(IntPtr devicePtr);
		[DllImport("unityuvcplugin", EntryPoint = "DeviceInfo_get_device_protocol")]
		private static extern Byte GetDeviceProtocol(IntPtr devicePtr);

		
		[DllImport("unityuvcplugin", EntryPoint = "DeviceInfo_get_vendor_id")]
		private static extern UInt16 GetVendorId(IntPtr devicePtr);

		[DllImport("unityuvcplugin", EntryPoint = "DeviceInfo_get_product_id")]
		private static extern UInt16 GetProductId(IntPtr devicePtr);

		[DllImport("unityuvcplugin", EntryPoint = "DeviceInfo_get_name")]
		[return: MarshalAs(UnmanagedType.LPStr)]
		private static extern string GetName(IntPtr devicePtr);

	} 

}

