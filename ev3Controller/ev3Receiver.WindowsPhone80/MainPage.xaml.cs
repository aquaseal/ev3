using ev3Receiver.enums;
using ev3Receiver.SAL;
using ev3Receiver.WindowsPhone80.Model;
using ev3Receiver.WindowsPhone80.SAL;
using Microsoft.Phone.Controls;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Windows.Navigation;

namespace ev3Receiver.WindowsPhone80
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private async void BrickService_ConnectionStatusChanged(object sender, Status brickStatus)
        {
            statusTextBlock.Text = brickStatus.ToString();

            try
            {
                // update the ev3 status
                var ev3StatusValues = await ev3StatusTable.Take(1).ToListAsync();
                if (ev3StatusValues.Count < 1)
                {
                    EV3Status ev3Status = new EV3Status { status = brickStatus.ToString() };
                    await ev3StatusTable.InsertAsync(ev3Status);
                }
                else
                {
                    EV3Status ev3Status = ev3StatusValues[0];
                    ev3Status.status = brickStatus.ToString();
                    await ev3StatusTable.UpdateAsync(ev3Status);
                }

                // if we are connected to the EV3, start querying the mobile service for commands
                if (brickStatus == Status.connected)
                {
                    // todo ... when i execute a command to the robot and it works, pull it off the queue and get the next
                }
            }
            catch (Exception e)
            {
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            brickService = new BrickService();
            brickService.ConnectionStatusChanged += BrickService_ConnectionStatusChanged;
            brickService.Connect();

            ev3StatusTable = AzureService.MobileService.GetTable<EV3Status>();

            base.OnNavigatedTo(e);
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            brickService.ConnectionStatusChanged -= BrickService_ConnectionStatusChanged;
            brickService.Disconnect();
            base.OnNavigatingFrom(e);
        }

        private BrickService brickService;
        private IMobileServiceTable<EV3Status> ev3StatusTable;

    }
}