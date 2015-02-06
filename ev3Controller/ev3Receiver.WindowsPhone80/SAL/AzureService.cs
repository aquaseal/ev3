using Microsoft.WindowsAzure.MobileServices;

namespace ev3Receiver.SAL
{
    public static class AzureService
    {
        public static MobileServiceClient MobileService = new MobileServiceClient(
            "https://ev3controller.azure-mobile.net/", "icyMyejcoKJpeywMFMqElDfqJCFGTs49");
    }
}