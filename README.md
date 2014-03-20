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
    <add key="N2StorageUrl" value="http://myaccount.blob.core.windows.net/mycontainer"/> <!-- This is used by N2.AzureFileSystem.ContentItemExtensions -->
</appSettings>
<n2 xmlns="http://n2cms.com/schemas/configuration/v3">
    <engine>
      <components>
        <add key="n2.fileSystem" service="N2.Edit.FileSystem.IFileSystem,N2" implementation="N2.AzureFileSystem.AzureFileSystemProvider, N2.AzureFileSystem" />
      </components>
    </engine>
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
````

