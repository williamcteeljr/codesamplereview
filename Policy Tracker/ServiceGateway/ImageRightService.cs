using PolicyTracker.Platform.Logging;
using ServiceGateway.ImageRightServiceReference;
using System;
using System.Linq;
using System.Security.Principal;

namespace ServiceGateway.ImageRight
{
    public static class ImageRightServiceGateway
    {
        public static readonly string _UserName = "ImageRight";
        public static readonly string _Password = "Im@g^r!ght23";

        public static RWebService40SoapClient CreateClient()
        {
            var proxy = new RWebService40SoapClient();
            // Tells the server that the client allows the server to impersonate the user

            proxy.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;

            /* Note: if you ultimately run your web server on a machine from a remote server to the application server
             * then ensure that you set this to TokenImpersonationLevel.Delegation instead. You will also 
             * need to set up your environment for delegation so that the web server has the privilege to
             * forward a user's credentials on their behalf to the application server service
             */

            return proxy;
        }

        public static Drawer[] GetDrawers()
        {
            var secToken = String.Empty;
            Drawer[] drawer = null;

            using (RWebService40SoapClient proxy = CreateClient())
            {
                var availableConnList = proxy.AvailableConnections();

                if (availableConnList.Count > 0)
                {
                    try
                    {
                        secToken = proxy.UserLogin(_UserName, _Password, availableConnList[0]);
                        drawer = proxy.GetDrawers(ref secToken, false);
                    }
                    catch (Exception ex)
                    {
                        LogManager.Log(LogLevel.INFO, "Error processing ImageRight web service request.");
                    }
                    finally
                    {
                        if (!String.IsNullOrEmpty(secToken))
                            proxy.UserLogoff(secToken);
                    }
                }
            }

            return drawer;
        }

        public static File GetFile(Drawer drawer, string fileNumber, string fileType)
        {
            var secToken = String.Empty;
            File file = null;

            using (RWebService40SoapClient proxy = CreateClient())
            {
                var availableConnList = proxy.AvailableConnections();

                if (availableConnList.Count > 0)
                {
                    try
                    {
                        secToken = proxy.UserLogin(_UserName, _Password, availableConnList[0]);
                        file = proxy.GetFile(ref secToken, drawer.Name, fileNumber, "", "", fileType, true, false);
                    }
                    catch (Exception ex)
                    {
                        LogManager.Log(LogLevel.INFO, "Error processing ImageRight web service request.");
                    }
                    finally
                    {
                        if (!String.IsNullOrEmpty(secToken))
                            proxy.UserLogoff(secToken);
                    }
                }
            }

            return file;
        }

        public static FileRef CreateFile(Drawer drawer)
        {
            var secToken = String.Empty;
            FileRef createdFile = null;

            using (RWebService40SoapClient proxy = CreateClient())
            {
                var availableConnList = proxy.AvailableConnections();

                if (availableConnList.Count > 0)
                {
                    try
                    {
                        secToken = proxy.UserLogin(_UserName, _Password, availableConnList[0]);
                        var fileType = drawer.FileTypes.Where(x => x.Name == "Atlanta Agriculture").FirstOrDefault();

                        CreateFileData fileData = new CreateFileData()
                        {
                            Name = "fileName",
                            Description = "description",
                            ObjTypeId = fileType.Id.RefId
                        };

                        createdFile = proxy.CreateFile(ref secToken, drawer.Id.RefId, fileData, new AttributeData[0]);
                    }
                    catch (Exception ex)
                    {
                        LogManager.Log(LogLevel.INFO, "Error processing ImageRight web service request.");
                    }
                    finally
                    {
                        if (!String.IsNullOrEmpty(secToken))
                            proxy.UserLogoff(secToken);
                    }
                }
            }

            return createdFile;
        }
    }
}
