#define ENABLE_LOG
using AOT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

#if UNITY_ANDROID && UNITY_2018_3_OR_NEWER
using UnityEngine.Android;
#endif

namespace Serenegiant.UVC
{
    [RequireComponent(typeof(AndroidUtils))]
	public class UVCManager : MonoBehaviour
	{
		private const string TAG = "UVCManager#";
		private const string FQCN_DETECTOR = "com.serenegiant.usb.DeviceDetectorFragment";
        private const int FRAME_TYPE_MJPEG = 0x000007;
        private const int FRAME_TYPE_H264 = 0x000014;
        private const int FRAME_TYPE_H264_FRAME = 0x030011;
	

		public Int32 DefaultWidth = 1280;

		public Int32 DefaultHeight = 720;

		public bool PreferH264 = false;

        public bool RenderBeforeSceneRendering = false;

		[SerializeField, ComponentRestriction(typeof(IUVCDrawer))]
		public Component[] UVCDrawers;

		public class CameraInfo
		{
			internal readonly UVCDevice device;
			internal Texture previewTexture;
            internal int frameType;
			internal Int32 activeId;
			private Int32 currentWidth;
			private Int32 currentHeight;
            private bool isRenderBeforeSceneRendering;
            private bool isRendering;


            internal CameraInfo(UVCDevice device)
			{
				this.device = device;
			}


			public Int32 Id{
				get { return device.id;  }
			}
	

			public string DeviceName
			{
				get { return device.name; }
			}


			public int Vid
			{
				get { return device.vid; }
			}


			public int Pid
			{
				get { return device.pid; }
			}


			public bool IsPreviewing
			{
				get { return (activeId != 0) && (previewTexture != null); }
			}


			public Int32 CurrentWidth
			{
				get { return currentWidth; }
			}


			public Int32 CurrentHeight
			{
				get { return currentHeight; }
			}


			internal void SetSize(Int32 width, Int32 height)
			{
				currentWidth = width;
				currentHeight = height;
			}

			public override string ToString()
			{
				return $"{base.ToString()}({currentWidth}x{currentHeight},id={Id},activeId={activeId},IsPreviewing={IsPreviewing})";
			}


            internal Coroutine StartRender(UVCManager manager, bool renderBeforeSceneRendering)
            {
                StopRender(manager);
                isRenderBeforeSceneRendering = renderBeforeSceneRendering;
                isRendering = true;
                if (renderBeforeSceneRendering)
                {
                    return manager.StartCoroutine(OnRenderBeforeSceneRendering());
                } else
                {
                    return manager.StartCoroutine(OnRender());
                }
            }


            internal void StopRender(UVCManager manager)
            {
                if (isRendering)
                {
                    isRendering = false;
                    if (isRenderBeforeSceneRendering)
                    {
                        manager.StopCoroutine(OnRenderBeforeSceneRendering());
                    }
                    else
                    {
                        manager.StopCoroutine(OnRender());
                    }
                }
            }


            private IEnumerator OnRenderBeforeSceneRendering()
			{
				var renderEventFunc = GetRenderEventFunc();
				for (; activeId != 0;)
				{
					yield return null;
					GL.IssuePluginEvent(renderEventFunc, activeId);
				}
				yield break;
			}


            private IEnumerator OnRender()
            {
                var renderEventFunc = GetRenderEventFunc();
                for (; activeId != 0;)
                {
                    yield return new WaitForEndOfFrame();
                    GL.IssuePluginEvent(renderEventFunc, activeId);
                }
                yield break;
            }

        }


        private SynchronizationContext mainContext;

		private OnDeviceChangedCallbackManager.OnDeviceChangedFunc callback;

		private List<UVCDevice> attachedDevices = new List<UVCDevice>();

		private Dictionary<Int32, CameraInfo> cameraInfos = new Dictionary<int, CameraInfo>();


        IEnumerator Start()
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}Start:");
#endif
			mainContext = SynchronizationContext.Current;
            callback = OnDeviceChangedCallbackManager.Add(this);
	
			yield return Initialize();
		}

#if (!NDEBUG && DEBUG && ENABLE_LOG)
		void OnApplicationFocus()
		{
			Console.WriteLine($"{TAG}OnApplicationFocus:");
		}
#endif

#if (!NDEBUG && DEBUG && ENABLE_LOG)
		void OnApplicationPause(bool pauseStatus)
		{
			Console.WriteLine($"{TAG}OnApplicationPause:{pauseStatus}");
		}
#endif

#if (!NDEBUG && DEBUG && ENABLE_LOG)
		void OnApplicationQuits()
		{
			Console.WriteLine($"{TAG}OnApplicationQuits:");
		}
#endif

		void OnDestroy()
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}OnDestroy:");
#endif
			StopAll();
            OnDeviceChangedCallbackManager.Remove(this);
		}

        public void OnDeviceChanged(IntPtr devicePtr, bool attached)
        {
            var id = UVCDevice.GetId(devicePtr);
#if (!NDEBUG && DEBUG && ENABLE_LOG)
            Console.WriteLine($"{TAG}OnDeviceChangedInternal:id={id},attached={attached}");
#endif
            if (attached)
            {
                UVCDevice device = new UVCDevice(devicePtr);
#if (!NDEBUG && DEBUG && ENABLE_LOG)
                Console.WriteLine($"{TAG}OnDeviceChangedInternal:device={device.ToString()}");
#endif
                if (HandleOnAttachEvent(device))
                {
                    attachedDevices.Add(device);
                    StartPreview(device);
                }
            }
            else
            {
                var found = attachedDevices.Find(item =>
                {
                    return item != null && item.id == id;
                });
                if (found != null)
                {
                    HandleOnDetachEvent(found);
                    StopPreview(found);
                    attachedDevices.Remove(found);
                }
            }
        }

        public List<CameraInfo> GetAttachedDevices()
		{
			var result = new List<CameraInfo>(cameraInfos.Count);

			foreach (var info in cameraInfos.Values)
			{
				result.Add(info);
			}

			return result;
		}



		private void StartPreview(UVCDevice device)
		{
			var info = CreateIfNotExist(device);
			if ((info != null) && !info.IsPreviewing) {

				int width = DefaultWidth;
				int height = DefaultHeight;


#if (!NDEBUG && DEBUG && ENABLE_LOG)
				Console.WriteLine($"{TAG}StartPreview:({width}x{height}),id={device.id}");
#endif
                int[] frameTypes = {
                    PreferH264 ? FRAME_TYPE_H264 : FRAME_TYPE_MJPEG,
                    PreferH264 ? FRAME_TYPE_MJPEG : FRAME_TYPE_H264,
                };
                foreach (var frameType in frameTypes)
                {
                    if (Resize(device.id, frameType, width, height) == 0)
                    {
                        info.frameType = frameType;
                        break;
                    }
                }
                    
				info.SetSize(width, height);
				info.activeId = device.id;
				mainContext.Post(__ =>
				{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
					Console.WriteLine($"{TAG}映像受け取り用テクスチャ生成:({width}x{height})");
#endif
					Texture2D tex = new Texture2D(
							width, height,
							TextureFormat.ARGB32,
							false, /* mipmap */
							true /* linear */);
					tex.filterMode = FilterMode.Point;
					tex.Apply();
					info.previewTexture = tex;
					var nativeTexPtr = info.previewTexture.GetNativeTexturePtr();
					Start(device.id, nativeTexPtr.ToInt32());
					HandleOnStartPreviewEvent(info);
					info.StartRender(this, RenderBeforeSceneRendering);
				}, null);
			}
		}

		private void StopPreview(UVCDevice device) {
			var info = Get(device);
			if ((info != null) && info.IsPreviewing)
			{
				mainContext.Post(__ =>
				{
					HandleOnStopPreviewEvent(info);
					Stop(device.id);
					info.StopRender(this);
					info.SetSize(0, 0);
					info.activeId = 0;
				}, null);
			}
		}


		private void StopAll() {
			List<CameraInfo> values = new List<CameraInfo>(cameraInfos.Values);
			foreach (var info in values)
			{
				StopPreview(info.device);
			}
		}


		private bool HandleOnAttachEvent(UVCDevice device/*NonNull*/)
		{
			if ((UVCDrawers == null) || (UVCDrawers.Length == 0))
			{   // IUVCDrawerが割り当てられていないときはtrue(接続されたUVC機器を使用する)を返す
				return true;
			}
			else
			{
				bool hasDrawer = false;
				foreach (var drawer in UVCDrawers)
				{
					if (drawer is IUVCDrawer)
					{
						hasDrawer = true;
						if ((drawer as IUVCDrawer).OnUVCAttachEvent(this, device))
						{   // どれか1つのIUVCDrawerがtrueを返せばtrue(接続されたUVC機器を使用する)を返す
							return true;
						}
					}
				}
				return !hasDrawer;
			}
		}


		private void HandleOnDetachEvent(UVCDevice device/*NonNull*/)
		{
			if ((UVCDrawers != null) && (UVCDrawers.Length > 0))
			{
				foreach (var drawer in UVCDrawers)
				{
					if (drawer is IUVCDrawer)
					{
						(drawer as IUVCDrawer).OnUVCDetachEvent(this, device);
					}
				}
			}
		}


		void HandleOnStartPreviewEvent(CameraInfo info)
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}HandleOnStartPreviewEvent:({info})");
#endif
			if ((info != null) && info.IsPreviewing && (UVCDrawers != null))
			{
				foreach (var drawer in UVCDrawers)
				{
					if ((drawer is IUVCDrawer) && (drawer as IUVCDrawer).CanDraw(this, info.device))
					{
						(drawer as IUVCDrawer).OnUVCStartEvent(this, info.device, info.previewTexture);
					}
				}
			} else {
#if (!NDEBUG && DEBUG && ENABLE_LOG)
				Console.WriteLine($"{TAG}HandleOnStartPreviewEvent:No UVCDrawers");
#endif
			}
		}


		void HandleOnStopPreviewEvent(CameraInfo info)
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}HandleOnStopPreviewEvent:({info})");
#endif
			if (UVCDrawers != null)
			{
				foreach (var drawer in UVCDrawers)
				{
					if ((drawer is IUVCDrawer) && (drawer as IUVCDrawer).CanDraw(this, info.device))
					{
						(drawer as IUVCDrawer).OnUVCStopEvent(this, info.device);
					}
				}
			}
		}


		private CameraInfo CreateIfNotExist(UVCDevice device)
		{
			if (!cameraInfos.ContainsKey(device.id))
			{
				cameraInfos[device.id] = new CameraInfo(device);
			}
			return cameraInfos[device.id];
		}


		private CameraInfo Get(UVCDevice device)
		{
			return cameraInfos.ContainsKey(device.id) ? cameraInfos[device.id] : null;
		}


		private IEnumerator Initialize()
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}Initialize:");
#endif
			if (AndroidUtils.CheckAndroidVersion(28))
			{
				yield return AndroidUtils.GrantCameraPermission((string permission, AndroidUtils.PermissionGrantResult result) =>
				{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
					Console.WriteLine($"{TAG}OnPermission:{permission}={result}");
#endif
					switch (result)
					{
						case AndroidUtils.PermissionGrantResult.PERMISSION_GRANT:
							InitPlugin();
							break;
						case AndroidUtils.PermissionGrantResult.PERMISSION_DENY:
							if (AndroidUtils.ShouldShowRequestPermissionRationale(AndroidUtils.PERMISSION_CAMERA))
							{

							}
							break;
						case AndroidUtils.PermissionGrantResult.PERMISSION_DENY_AND_NEVER_ASK_AGAIN:
							break;
					}
				});
			}
			else
			{
				InitPlugin();
			}

			yield break;
		}


		private void InitPlugin()
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}InitPlugin:");
#endif
			var hasDrawer = false;
			if ((UVCDrawers != null) && (UVCDrawers.Length > 0))
			{
				foreach (var drawer in UVCDrawers)
				{
					if (drawer is IUVCDrawer)
					{
						hasDrawer = true;
						break;
					}
				}
			}
			if (!hasDrawer)
			{   
#if (!NDEBUG && DEBUG && ENABLE_LOG)
				Console.WriteLine($"{TAG}InitPlugin:has no IUVCDrawer, try to get from gameObject");
#endif
				var drawers = GetComponents(typeof(IUVCDrawer));
				if ((drawers != null) && (drawers.Length > 0))
				{
					UVCDrawers = new Component[drawers.Length];
					int i = 0;
					foreach (var drawer in drawers)
					{
						UVCDrawers[i++] = drawer;
					}
				}
			}
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}InitPlugin:num drawers={UVCDrawers.Length}");
#endif
		
			using (AndroidJavaClass clazz = new AndroidJavaClass(FQCN_DETECTOR))
			{
				clazz.CallStatic("initUVCDeviceDetector",
					AndroidUtils.GetCurrentActivity());
			}
		}


		[DllImport("unityuvcplugin")]
		private static extern IntPtr GetRenderEventFunc();

        [DllImport("unityuvcplugin", EntryPoint = "Config")]
        private static extern Int32 Config(Int32 deviceId, Int32 enabled, Int32 useFirstConfig);

        [DllImport("unityuvcplugin", EntryPoint ="Start")]
		private static extern Int32 Start(Int32 deviceId, Int32 tex);

		[DllImport("unityuvcplugin", EntryPoint ="Stop")]
		private static extern Int32 Stop(Int32 deviceId);

		[DllImport("unityuvcplugin")]
		private static extern Int32 Resize(Int32 deviceId, Int32 frameType, Int32 width, Int32 height);
	}   


    public static class OnDeviceChangedCallbackManager
    {

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void OnDeviceChangedFunc(Int32 id, IntPtr devicePtr, bool attached);


        [DllImport("unityuvcplugin")]
        private static extern IntPtr Register(Int32 id, OnDeviceChangedFunc callback);

        [DllImport("unityuvcplugin")]
        private static extern IntPtr Unregister(Int32 id);

        private static Dictionary<Int32, UVCManager> sManagers = new Dictionary<Int32, UVCManager>();
  

        public static OnDeviceChangedFunc Add(UVCManager manager)
        {
            Int32 id = manager.GetHashCode();
            OnDeviceChangedFunc callback = new OnDeviceChangedFunc(OnDeviceChanged);
            sManagers.Add(id, manager);
            Register(id, callback);
            return callback;
        }

        public static void Remove(UVCManager manager)
        {
            Int32 id = manager.GetHashCode();
            Unregister(id);
            sManagers.Remove(id);
        }

        [MonoPInvokeCallback(typeof(OnDeviceChangedFunc))]
        public static void OnDeviceChanged(Int32 id, IntPtr devicePtr, bool attached)
        {
            var manager = sManagers.ContainsKey(id) ? sManagers[id] : null;
            if (manager != null)
            {
                manager.OnDeviceChanged(devicePtr, attached);
            }
        }
    } 


}
