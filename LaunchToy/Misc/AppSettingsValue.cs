using System.Configuration;

namespace LaunchToy
{
    public class AppSettingsValueInt
    {
        private string key;
        private int defaultValue;

        public static implicit operator int(AppSettingsValueInt d) =>
            int.TryParse(ConfigurationManager.AppSettings[d.key], out var intValue) ? intValue : d.defaultValue;

        public void Set(int value)
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            if (settings[this.key] == null)
            {
                settings.Add(this.key, value.ToString());
            }
            else
            {
                settings[this.key].Value = value.ToString();
            }
            configFile.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
        }

        public AppSettingsValueInt(string key, int defaultValue = 0)
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }
    }

    public class AppSettingsValueString
    {
        private string key;
        private string defaultValue;

        public static implicit operator string(AppSettingsValueString d) =>
            ConfigurationManager.AppSettings[d.key] ?? d.defaultValue;

        public void Set(string value)
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            if (settings[this.key] == null)
            {
                settings.Add(this.key, value);
            }
            else
            {
                settings[this.key].Value = value;
            }
            configFile.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
        }

        public AppSettingsValueString(string key, string defaultValue = "")
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }
    }
}
