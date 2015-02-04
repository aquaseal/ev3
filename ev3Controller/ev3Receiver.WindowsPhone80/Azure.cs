using Microsoft.WindowsAzure.MobileServices;

namespace ev3Receiver.WindowsPhone80
{
    public static class Azure
    {
        public static MobileServiceClient MobileService = new MobileServiceClient(
            "https://ev3controller.azure-mobile.net/", "icyMyejcoKJpeywMFMqElDfqJCFGTs49");
    }
}