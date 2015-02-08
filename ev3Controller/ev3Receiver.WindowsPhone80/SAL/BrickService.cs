using ev3Receiver.enums;
using Lego.Ev3.Core;
#if NETFX_CORE
using Lego.Ev3.WinRT;
#elif WINDOWS_PHONE
using Lego.Ev3.Phone;
#endif
using System;

namespace ev3Receiver.SAL
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
            brick = new Brick(new UsbCommunication());

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

        public void ForwardCMD()
        {
            //brick.BatchCommand.StepMotorAtPower(OutputPort.A | OutputPort.B, 50, 30, false);
            brick.BatchCommand.TurnMotorAtPower(OutputPort.A | OutputPort.B, 50);
        }

        public void BackwardCMD()
        {
            //brick.BatchCommand.StepMotorAtPower(OutputPort.A | OutputPort.B, -50, 30, false);
            brick.BatchCommand.TurnMotorAtPower(OutputPort.A | OutputPort.B, -50);
        }

        public void ClockwiseCMD()
        {
            // assumes port A is connected to the left motor when the robot is facing forward
            //brick.BatchCommand.StepMotorAtPower(OutputPort.A, -50, 30, false);
            brick.BatchCommand.TurnMotorAtPower(OutputPort.A, 30);
        }

        public void CounterClockwiseCMD()
        {
            // assumes port B is connected to the left motor when the robot is facing forward
            //brick.BatchCommand.StepMotorAtPower(OutputPort.B, -50, 30, false);
            brick.BatchCommand.TurnMotorAtPower(OutputPort.B, -30);
        }

        public async void ExecuteCommands()
        {
            await brick.BatchCommand.SendCommandAsync();
        }

        // todo have the commandQueue here and send them via a BatchCommand with SendCommandAsync

        private Brick brick;
        private Status connectionStatus = Status.unknown;
        private bool tryToConnect = true;
    }
}