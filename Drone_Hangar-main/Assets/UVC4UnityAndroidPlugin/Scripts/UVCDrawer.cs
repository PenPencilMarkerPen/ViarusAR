using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Serenegiant.UVC
{

	public class UVCDrawer : MonoBehaviour, IUVCDrawer
	{

		public int DefaultWidth = 1280;

		public int DefaultHeight = 720;


		public UVCFilter[] UVCFilters;


		public List<GameObject> RenderTargets;

		//--------------------------------------------------------------------------------
		private const string TAG = "UVCDrawer#";


		private UnityEngine.Object[] TargetMaterials;

		private Texture[] SavedTextures;

		private Quaternion[] quaternions;


		void Start()
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}Start:");
#endif
			UpdateTarget();

		}


		public bool OnUVCAttachEvent(UVCManager manager, UVCDevice device)
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}OnUVCAttachEvent:{device}");
#endif

			var result = !device.IsRicoh || device.IsTHETA;

			result &= UVCFilter.Match(device, UVCFilters);

			return result;
		}


		public void OnUVCDetachEvent(UVCManager manager, UVCDevice device)
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}OnUVCDetachEvent:{device}");
#endif
		}

//		/**
//		 * 解像度選択
//		 * IOnUVCSelectSizeHandlerの実装
//		 * @param manager 呼び出し元のUVCManager
//		 * @param device 対象となるUVC機器の情報
//		 * @param formats 対応している解像度についての情報
//		 */
//		public SupportedFormats.Size OnUVCSelectSize(UVCManager manager, UVCDevice device, SupportedFormats formats)
//		{
//#if (!NDEBUG && DEBUG && ENABLE_LOG)
//			Console.WriteLine($"{TAG}OnUVCSelectSize:{device}");
//#endif
//			if (device.IsTHETA_V || device.IsTHETA_Z1)
//			{
//#if (!NDEBUG && DEBUG && ENABLE_LOG)
//				Console.WriteLine($"{TAG}OnUVCSelectSize:THETA V/Z1");
//#endif
//				return FindSize(formats, 3840, 1920);
//			}
//			else if (device.IsTHETA_S)
//			{
//#if (!NDEBUG && DEBUG && ENABLE_LOG)
//				Console.WriteLine($"{TAG}OnUVCSelectSize:THETA S");
//#endif
//				return FindSize(formats, 1920, 1080);
//			}
//			else
//			{
//#if (!NDEBUG && DEBUG && ENABLE_LOG)
//				Console.WriteLine($"{TAG}OnUVCSelectSize:other UVC device,{device}");
//#endif
//				return formats.Find(DefaultWidth, DefaultHeight);
//			}
//		}


		public bool CanDraw(UVCManager manager, UVCDevice device)
		{
			return UVCFilter.Match(device, UVCFilters);
		}


		public void OnUVCStartEvent(UVCManager manager, UVCDevice device, Texture tex)
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}OnUVCStartEvent:{device}");
#endif
			HandleOnStartPreview(tex);
		}


		public void OnUVCStopEvent(UVCManager manager, UVCDevice device)
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}OnUVCStopEvent:{device}");
#endif
			HandleOnStopPreview();
		}


		private void UpdateTarget()
		{
			bool found = false;
			if ((RenderTargets != null) && (RenderTargets.Count > 0))
			{
				TargetMaterials = new UnityEngine.Object[RenderTargets.Count];
				SavedTextures = new Texture[RenderTargets.Count];
				quaternions = new Quaternion[RenderTargets.Count];
				int i = 0;
				foreach (var target in RenderTargets)
				{
					if (target != null)
					{
						var material = TargetMaterials[i] = GetTargetMaterial(target);
						if (material != null)
						{
							found = true;
						}
#if (!NDEBUG && DEBUG && ENABLE_LOG)
						Console.WriteLine($"{TAG}UpdateTarget:material={material}");
#endif
					}
					i++;
				}
			}
			if (!found)
			{   
				TargetMaterials = new UnityEngine.Object[1];
				SavedTextures = new Texture[1];
				quaternions = new Quaternion[1];
				TargetMaterials[0] = GetTargetMaterial(gameObject);
				found = TargetMaterials[0] != null;
			}

			if (!found)
			{
				throw new UnityException("no target material found.");
			}
		}


		UnityEngine.Object GetTargetMaterial(GameObject target/*NonNull*/)
		{
			var skyboxs = target.GetComponents<Skybox>();
			if (skyboxs != null)
			{
				foreach (var skybox in skyboxs)
				{
					if (skybox.isActiveAndEnabled && (skybox.material != null))
					{
						RenderSettings.skybox = skybox.material;
						return skybox.material;
					}
				}
			}
			var renderers = target.GetComponents<Renderer>();
			if (renderers != null)
			{
				foreach (var renderer in renderers)
				{
					if (renderer.enabled && (renderer.material != null))
					{
						return renderer.material;
					}

				}
			}
			var rawImages = target.GetComponents<RawImage>();
			if (rawImages != null)
			{
				foreach (var rawImage in rawImages)
				{
					if (rawImage.enabled && (rawImage.material != null))
					{
						return rawImage;
					}

				}
			}
			var material = target.GetComponent<Material>();
			if (material != null)
			{
				return material;
			}
			return null;
		}

		private void RestoreTexture()
		{
			for (int i = 0; i < TargetMaterials.Length; i++)
			{
				var target = TargetMaterials[i];
				try
				{
					if (target is Material)
					{
						(target as Material).mainTexture = SavedTextures[i];
					}
					else if (target is RawImage)
					{
						(target as RawImage).texture = SavedTextures[i];
					}
				}
				catch
				{
					Console.WriteLine($"{TAG}RestoreTexture:Exception cought");
				}
				SavedTextures[i] = null;
				quaternions[i] = Quaternion.identity;
			}
		}

		private void ClearTextures()
		{
			for (int i = 0; i < SavedTextures.Length; i++)
			{
				SavedTextures[i] = null;
			}
		}

		private void HandleOnStartPreview(Texture tex)
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}HandleOnStartPreview:({tex})");
#endif
			int i = 0;
			foreach (var target in TargetMaterials)
			{
				if (target is Material)
				{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
					Console.WriteLine($"{TAG}HandleOnStartPreview:assign Texture to Material({target})");
#endif
					SavedTextures[i++] = (target as Material).mainTexture;
					(target as Material).mainTexture = tex;
				}
				else if (target is RawImage)
				{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
					Console.WriteLine($"{TAG}HandleOnStartPreview:assign Texture to RawImage({target})");
#endif
					SavedTextures[i++] = (target as RawImage).texture;
					(target as RawImage).texture = tex;
				}
			}
		}

		private void HandleOnStopPreview()
		{
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}HandleOnStopPreview:");
#endif
			RestoreTexture();
#if (!NDEBUG && DEBUG && ENABLE_LOG)
			Console.WriteLine($"{TAG}HandleOnStopPreview:finished");
#endif
		}

	} 

} 
