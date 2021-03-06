﻿using ev3Receiver.enums;
using ev3Receiver.Model;
using ev3Receiver.SAL;
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

            var allEV3Commands = await ev3CommandsTable.IncludeTotalCount().ToListAsync();

            if (allEV3Commands.Count > 0)
            {
                commandsTextBlock.Text += "\nnext batch: \n";

                // queue up commands to send to the EV3 one at a time
                brickService.ExecuteCommands(allEV3Commands);

                // queue up commands to send to the EV3
                foreach (EV3Commands command in allEV3Commands)
                {
                    commandsTextBlock.Text += command.CMD + "\n";

                    // remove the command from the mobile service
                    await ev3CommandsTable.DeleteAsync(command);
                }
            }

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