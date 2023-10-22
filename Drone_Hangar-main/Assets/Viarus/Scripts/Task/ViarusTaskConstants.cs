namespace ViarusTask
{
    public enum TASK_ACTION
    {
        VIDEO_PLAY = 101,
        OPEN_FILE,
        SHOW_IMAGE,
        SETTINGS,
        EXPLORER,
        DEVICE_DRIVER
    }

    public enum SELECTION_TASK_ACTION
    {
        FILE = 1001
    }


    public class Setting
    {
        
        public const string SETTINGS_KEY_TYPE = "settings_type";
        
        public const string SETTINGS_TYPE_MAIN = "android.settings.LANGUAGE_SETTINGS";
        public const string SETTINGS_TYPE_WIFI = "android.nibiru.settings.WIFI_SETTINGS";
        public const string SETTINGS_TYPE_BLUETOOTH = "android.nibiru.settings.BLUE_SETTINGS";
        public const string SETTINGS_TYPE_SYSTEM = "android.nibiru.settings.SYSTEM_SETTINGS";
        public const string SETTINGS_TYPE_GENERAL = "android.nibiru.settings.NORMAL_SETTINGS";
    }

    public class Brower
    {
        
        public const string EXPLORER_KEY_URL = "url";
        
        public const string EXPLORER_KEY_ACTIONBAR = "hideActionBar";
        
        public const string EXPLORER_KEY_ACTIONBAR_HIDE = "true";
        public const string EXPLORER_KEY_ACTIONBAR_SHOW = "false";
    }

}
