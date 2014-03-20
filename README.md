N2.AzureFileSystem
==================

Azure blob storage provider for N2CMS

Install by running:
```
Install-Package N2.AzureFileSystem
```

Configuration
```
<appSettings>
    <add key="N2StorageContainerName" value="mycontainer"/>
    <!-- N2StorageUrl is optional - used by N2.AzureFileSystem.ContentItemExtensions -->
    <add key="N2StorageUrl" value="//myaccount.blob.core.windows.net/mycontainer"/>
</appSettings>
<n2 xmlns="http://n2cms.com/schemas/configuration/v3">
    <!-- Adding the component is what actually replaces the provider -->
    <engine>
      <components>
        <add key="n2.fileSystem" service="N2.Edit.FileSystem.IFileSystem,N2" implementation="N2.AzureFileSystem.AzureFileSystemProvider, N2.AzureFileSystem" />
      </components>
    </engine>
    <!-- When urlPrefix is set that is actually saved in the database for uploaded files -->
    <!-- Which means if you want to be able to migrate your database by plain old copy = don't use below setting -->
    <edit>
      <uploadFolders>
        <clear />
        <add path="~/upload/" title="Azure Blob Storage" urlPrefix="//myAccount.blob.core.windows.net/mycontainer" />
      </uploadFolders>
    </edit>
</n2>
<connectionStrings>
    <add name="N2StorageConnectionString" connectionString="DefaultEndpointsProtocol=http;AccountName=myAccount;AccountKey=myKey;" />
</connectionStrings>
```

Usage in code when edit/uploadFolders is set
```
    public class BackgroundImageContentItem : ContentItem
    {
        // This will return e.g '//myAccount.blob.core.windows.net/mycontainer/upload/someimage.jpg'
        [EditableImageUpload(Title = "Bakgrundsbild")]
        public virtual string BackgroundImage { get; set; } 
    }
```

Usage in code without edit/uploadFolders
```
    using N2.AzureFileSystem;
    public class BackgroundImageContentItem : ContentItem
    {
        [EditableImageUpload(Title = "Bakgrundsbild")]
        public virtual string BackgroundImage { get; set; } // This will return e.g '/upload/someimage.jpg' as usual

        public string GetBackgroundImageUrl()
        {
            //this will return '//myAccount.blob.core.windows.net/mycontainer/upload/someimage.jpg' 
            //(using what's stored in the N2StorageUrl appSetting)
            //
            //if you in your development environment are using the regular file system provider
            //it will return '/upload/someimage.jpg'
            return this.GetFileUrl("BackgroundImage");
        }
    }
```

Example usage without edit/uploadFolders in an mvc html helper
```
using N2.AzureFileSystem;

public static class ImageHelper
{
    public static IHtmlString Image(this HtmlHelper helper, ContentItem page, string detailName = "Image", string cssClass = "")
    {
        var tag = new TagBuilder("img");
        tag.Attributes.Add("src", page.GetFileUrl(detailName));

        if (!string.IsNullOrEmpty(cssClass))
        {
            tag.Attributes.Add("class", cssClass);
        }

        return MvcHtmlString.Create(tag.ToString(TagRenderMode.SelfClosing));
    }
}

```