using System;
using System.Configuration;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace N2.AzureFileSystem
{
    /// <summary> 
    /// Rules on Container names:
    /// http://msdn.microsoft.com/en-us/library/windowsazure/dd135715.aspx
    /// 
    //  1. Container names must start with a letter or number, and can contain only letters, numbers, and the dash (-) character.
    //  2. Every dash (-) character must be immediately preceded and followed by a letter or number; consecutive dashes are not permitted in container names.
    //  3. All letters in a container name must be lowercase.
    //  4. Container names must be from 3 through 63 characters long
    /// </summary>
    public static class ContainerNameParser
    {
        public static string Create()
        {
            var containerName = ConfigurationManager.AppSettings[AppSettingsKeys.N2StorageContainerName].Trim();

            //  1. Container names must start with a letter or number...
            var mustStartWithLetterOrNumber = new Regex(@"^(\d|[a-zA-Z])");
            if (!mustStartWithLetterOrNumber.IsMatch(containerName))
            {
                throw new ContainerNameException("Container names must start with a letter or number");
            }

            //  1. ...and can contain only letters, numbers, and the dash (-) character.
            var isNotLetterNumberOrDash = new Regex(@"[^\d|[a-zA-Z]|-]");
            if (isNotLetterNumberOrDash.IsMatch(containerName))
            {
                throw new ContainerNameException("Container names can contain only letters, numbers, and the dash (-) character");
            }

            // 3. All letters in a container name must be lowercase.
            var haveLetterThatIsNotLowercase = new Regex(@"[A-Z]");
            if (haveLetterThatIsNotLowercase.IsMatch(containerName))
            {
                throw new ContainerNameException("All letters in a container name must be lowercase");
            }

            //  4. Container names must be from 3 through 63 characters long
            if (containerName.Length < 3 || containerName.Length > 63)
            {
                throw new ContainerNameException("Container names must be from 3 through 63 characters long");
            }

            return containerName;
        }
    }

    [Serializable]
    public class ContainerNameException : Exception
    {
        public ContainerNameException(){}
        public ContainerNameException(string message) : base(message){}
        public ContainerNameException(string message, Exception inner) : base(message, inner){}
        protected ContainerNameException(SerializationInfo info,StreamingContext context) : base(info, context){}
    }
}