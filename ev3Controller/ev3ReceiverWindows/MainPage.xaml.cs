using ev3Receiver.enums;
using ev3Receiver.Model;
using ev3Receiver.SAL;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ev3ReceiverWindows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }


        private async void BrickService_ConnectionStatusChanged(object sender, Status brickStatus)
        {
            this.brickStatus = brickStatus;
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
                    queryForCommands();
            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        /// query the azure mobile service for any EV3 commands that may be in the table
        /// </summary>
        private async void queryForCommands()
        {
            if (brickStatus != Status.connected)
                return;

            commandsTextBlock.Text = string.Empty;
            var allEV3Commands = await ev3CommandsTable.IncludeTotalCount().ToListAsync();

            // queue up commands to send to the EV3
            foreach (EV3Commands command in allEV3Commands)
            {
                commandsTextBlock.Text += command.CMD + "\n";

                if (command.CMD == Commands.forward.ToString())
                    brickService.ForwardCMD();
                else if (command.CMD == Commands.backward.ToString())
                    brickService.BackwardCMD();
                else if (command.CMD == Commands.clockwise.ToString())
                    brickService.ClockwiseCMD();
                else if (command.CMD == Commands.counterClockwise.ToString())
                    brickService.CounterClockwiseCMD();

                // remove the command from the mobile service
                await ev3CommandsTable.DeleteAsync(command);
            }

            if (allEV3Commands.Count > 0)
                brickService.ExecuteCommands();

            //todo do we need to break out of this loop
            queryForCommands();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            brickService = new BrickService();
            brickService.ConnectionStatusChanged += BrickService_ConnectionStatusChanged;
            brickService.Connect();

            ev3StatusTable = AzureService.MobileService.GetTable<EV3Status>();
            ev3CommandsTable = AzureService.MobileService.GetTable<EV3Commands>();
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
        private IMobileServiceTable<EV3Commands> ev3CommandsTable;
        private Status brickStatus;
    }
}
