using ev3Receiver.enums;
using Lego.Ev3.Core;
using Lego.Ev3.Phone;
using System;

namespace ev3Receiver.WindowsPhone80.SAL
{
    public class BrickService
    {
        public Status ConnectionStatus
        {
            get { return connectionStatus; }
            private set
            {
                if (connectionStatus == value)
                    return;

                connectionStatus = value;
                if (ConnectionStatusChanged != null)
                    ConnectionStatusChanged(this, ConnectionStatus);
            }
        }

        public event EventHandler<Status> ConnectionStatusChanged;

        public BrickService()
        {
            brick = new Brick(new BluetoothCommunication());

            if (ConnectionStatusChanged != null)
                ConnectionStatusChanged(this, ConnectionStatus);

            tryToConnect = true;
        }

        public async void Connect()
        {
            if (!tryToConnect)
                return;

            try
            {
                await brick.ConnectAsync();
                ConnectionStatus = Status.connected;
                tryToConnect = false;
            }
            catch (Exception e)
            {
                ConnectionStatus = Status.disconnected;
            }

            Connect();
        }

        public void Disconnect()
        {
            brick.Disconnect();
            tryToConnect = false;
            brick = null;
            ConnectionStatus = Status.disconnected;
        }

        // todo have the commandQueue here and send them via a BatchCommand with SendCommandAsync
        
        private Brick brick;
        private Status connectionStatus;
        private bool tryToConnect = true;
    }
}