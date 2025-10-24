using Microsoft.Extensions.Configuration;

namespace FGMS.Utils
{
    public class ConfigHelper
    {
        private static IConfigurationRoot? configurationRoot;

        public ConfigHelper()
        {
            configurationRoot = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory()))
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
        }

        /// <summary>
        /// Get ConnectionStrings
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string? GetConnectionString(string key, string? defaultValue = null)
        {
            string result = configurationRoot.GetConnectionString(key);
            if (result == null)
                return defaultValue;
            return result;
        }

        /// <summary>
        /// Get appsettings section
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetAppSettings<T>(string key)
        {
            IConfigurationSection configSection = configurationRoot!.GetSection(key);
            if (configSection.Value != null)
            {
                return (T)Convert.ChangeType(configSection.Value, typeof(T));
            }
            return default!;
        }
    }
}
