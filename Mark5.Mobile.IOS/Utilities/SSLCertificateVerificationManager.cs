using System.Net;
using System.Net.Security;
using Mark5.Mobile.Common;
using ModernHttpClient;

namespace Mark5.Mobile.IOS.Utilities
{
    public class SSLCertificateVerificationManager
    {
        readonly RemoteCertificateValidationCallback remoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => { return true; };

        public void EnableSelfSignedCertificates()
        {
            CommonConfig.Logger.Warning("**** ENABLING CUSTOM SSL VALIDATION CALLBACK ****");

            ServicePointManager.ServerCertificateValidationCallback = remoteCertificateValidationCallback;
            CommonConfig.HttpClientHandler = () => new InsecureNativeMessageHandler { AutomaticDecompression = Config.AcceptedResponseCompression };
        }

        public void DisableSelfSignedCertificates()
        {
            CommonConfig.Logger.Info("Using standard SSL validation callback");

            ServicePointManager.ServerCertificateValidationCallback = null;
            CommonConfig.HttpClientHandler = () => new NativeMessageHandler { AutomaticDecompression = Config.AcceptedResponseCompression };
        }
    }
}