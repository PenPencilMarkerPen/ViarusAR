using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class ViarusPlatFormCheck
{
	static ViarusPlatFormCheck()
	{
		if (SystemInfo.operatingSystem.StartsWith("Windows"))
		{
			Debug.Log("На Windows");
			
#if UNITY_ANDROID
			Debug.Log("На Android");
#else
            Debug.Log("Other Platform");
            if (EditorUtility.DisplayDialog("Переключить платформу", "Выбранная платформа не Android\nПереключить платформу?", "Да", "Нет"))
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android,BuildTarget.Android);
                Debug.Log("Переключено на Android.");
            }
#endif
		}
	}
}