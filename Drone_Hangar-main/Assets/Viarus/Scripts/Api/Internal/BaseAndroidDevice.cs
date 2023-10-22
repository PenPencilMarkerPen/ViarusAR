













using UnityEngine;


namespace Vrs.Internal {
  public abstract class BaseAndroidDevice : BaseARDevice {
#if UNITY_ANDROID

    protected AndroidJavaObject androidActivity;

    public override void Destroy() {
      if (androidActivity != null) {
        androidActivity.Dispose();
        androidActivity = null;
      }
      base.Destroy();
    }

    protected virtual void ConnectToActivity() {
      try {
        using (AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
          androidActivity = player.GetStatic<AndroidJavaObject>("currentActivity");
        }
      } catch (AndroidJavaException e) {
        androidActivity = null;
        Debug.LogError("Exception while connecting to the Activity: " + e);
      }
    }

    public static AndroidJavaClass GetClass(string className) {
      try {
        return new AndroidJavaClass(className);
      } catch (AndroidJavaException e) {
        Debug.LogError("Exception getting class " + className + ": " + e);
        return null;
      }
    }

    public static AndroidJavaObject Create(string className, params object[] args) {
      try {
        return new AndroidJavaObject(className, args);
      } catch (AndroidJavaException e) {
        Debug.LogError("Exception creating object " + className + ": " + e);
        return null;
      }
    }

    public static bool CallStaticMethod(AndroidJavaObject jo, string name, params object[] args) {
      if (jo == null) {
        Debug.LogError("Object is null when calling static method " + name);
        return false;
      }
      try {
        jo.CallStatic(name, args);
        return true;
      } catch (AndroidJavaException e) {
        Debug.LogError("Exception calling static method " + name + ": " + e);
        return false;
      }
    }

    public static bool CallObjectMethod(AndroidJavaObject jo, string name, params object[] args) {
      if (jo == null) {
        Debug.LogError("Object is null when calling method " + name);
        return false;
      }
      try {
        jo.Call(name, args);
        return true;
      } catch (AndroidJavaException e) {
        Debug.LogError("Exception calling method " + name + ": " + e);
        return false;
      }
    }

    public static bool CallStaticMethod<T>(ref T result, AndroidJavaObject jo, string name,
                                              params object[] args) {
      if (jo == null) {
        Debug.LogError("Object is null when calling static method " + name);
        return false;
      }
      try {
        result = jo.CallStatic<T>(name, args);
        return true;
      } catch (AndroidJavaException e) {
        Debug.LogError("Exception calling static method " + name + ": " + e);
        return false;
      }
    }

    
    public static void RunOnUIThread(AndroidJavaObject activityObj , AndroidJavaRunnable r)
    {
         activityObj.Call("runOnUiThread", r);
    }

     static AndroidJavaClass EnvironmentCls;
    
    public static string GetAndroidStoragePath()
    {
        if (Application.platform == RuntimePlatform.Android)
            {
                if(EnvironmentCls == null) EnvironmentCls = new AndroidJavaClass("android.os.Environment");
                return EnvironmentCls.CallStatic<AndroidJavaObject>("getExternalStorageDirectory").Call<string>("getPath");
            }
        else
            return null;
    }

    public static bool CallObjectMethod<T>(ref T result, AndroidJavaObject jo, string name,
                                              params object[] args) {
      if (jo == null) {
        Debug.LogError("Object is null when calling method " + name);
        return false;
      }
      try {
        result = jo.Call<T>(name, args);
        return true;
      } catch (AndroidJavaException e) {
        Debug.LogError("Exception calling method " + name + ": " + e);
        return false;
      }
    }
#elif UNITY_STANDALONE_WIN || ANDROID_REMOTE_NRR
        public static AndroidJavaClass GetClass(string className)
        { return null; }
#endif

    }

}


