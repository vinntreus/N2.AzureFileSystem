using System.Configuration;

namespace N2.AzureFileSystem
{
    public static class ContentItemExtensions
    {
        private static readonly string HOST_NAME = ConfigurationManager.AppSettings[AppSettingsKeys.N2StorageUrl] ?? "";

        public static string GetFileUrl(this ContentItem item, string detailName)
        {
            var detail = item.GetDetail(detailName, "");

            if (string.IsNullOrEmpty(detail))
            {
                return detail;
            }

            return string.Format("{0}{1}", HOST_NAME, detail).ToLower();
        }
    }
}
