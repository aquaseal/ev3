using ev3Receiver.enums;
using Lego.Ev3.Core;
#if NETFX_CORE
using Lego.Ev3.WinRT;
#elif WINDOWS_PHONE
using Lego.Ev3.Phone;
#endif
using System;
using System.Collections.Generic;
using ev3Receiver.Model;

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
#if !WINDOWS_PHONE
            brick = new Brick(new UsbCommunication());
#else
            brick = new Brick(new BluetoothCommunication());
#endif
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

        public async void ForwardCMD()
        {
            await brick.DirectCommand.StepMotorAtPowerAsync(OutputPort.A | OutputPort.B, 50, 0, 360, 0, false);
            await brick.DirectCommand.WaitOutputReadyAsync(OutputPort.A | OutputPort.B);
        }

        public async void BackwardCMD()
        {
            await brick.DirectCommand.StepMotorAtPowerAsync(OutputPort.A | OutputPort.B, -50, 0, 360, 0, false);
            await brick.DirectCommand.WaitOutputReadyAsync(OutputPort.A | OutputPort.B);
        }

        public async void ClockwiseCMD()
        {
            // assumes port A is connected to the left motor when the robot is facing forward
            await brick.DirectCommand.StepMotorAtPowerAsync(OutputPort.A, 50, 0, 360, 0, false);
            await brick.DirectCommand.WaitOutputReadyAsync(OutputPort.A);
        }

        public async void CounterClockwiseCMD()
        {
            // assumes port B is connected to the left motor when the robot is facing forward
            await brick.DirectCommand.StepMotorAtPowerAsync(OutputPort.B, 50, 0, 360, 0, false);
            await brick.DirectCommand.WaitOutputReadyAsync(OutputPort.B);
        }

        public async void ExecuteCommands(List<EV3Commands> commands)
        {
            foreach (EV3Commands command in commands)
            {

                if (command.CMD == Commands.forward.ToString())
                {
                    await brick.DirectCommand.StepMotorAtPowerAsync(OutputPort.A | OutputPort.B, 50, 0, 360, 0, false);
                    await brick.DirectCommand.WaitOutputReadyAsync(OutputPort.A | OutputPort.B);
                }
                else if (command.CMD == Commands.backward.ToString())
                {
                    await brick.DirectCommand.StepMotorAtPowerAsync(OutputPort.A | OutputPort.B, -50, 0, 360, 0, false);
                    await brick.DirectCommand.WaitOutputReadyAsync(OutputPort.A | OutputPort.B);
                }
                else if (command.CMD == Commands.clockwise.ToString())
                {
                    await brick.DirectCommand.StepMotorAtPowerAsync(OutputPort.A, 50, 0, 360, 0, false);
                    await brick.DirectCommand.WaitOutputReadyAsync(OutputPort.A);
                }
                else if (command.CMD == Commands.counterClockwise.ToString())
                {
                    await brick.DirectCommand.StepMotorAtPowerAsync(OutputPort.B, 50, 0, 360, 0, false);
                    await brick.DirectCommand.WaitOutputReadyAsync(OutputPort.B);
                }
            }
        }

        private Brick brick;
        private Status connectionStatus = Status.unknown;
        private bool tryToConnect = true;
    }
}