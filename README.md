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

