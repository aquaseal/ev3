using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using HidLibrary;
using Lego.Ev3.Core;
using Microsoft.Win32.SafeHandles;

namespace Lego.Ev3.Desktop
{
	/// <summary>
	/// Communicate with EV3 brick over USB HID.
	/// </summary>
	public class UsbCommunication : ICommunication
	{
		// our brick VID and PID
		private const UInt16 VID = 0x0694;
		private const UInt16 PID = 0x0005;

		// device object
		private HidDevice brickDevice;

		// token to stoop reading loop
		private CancellationTokenSource stopSrc;

		// reading loop task
		private Task rxThread;

		/// <summary>
		/// Event fired when a complete report is received from the EV3 brick.
		/// </summary>
		public event EventHandler<ReportReceivedEventArgs> ReportReceived;

		/// <summary>
		/// Connect to the EV3 brick.
		/// </summary>
		/// <returns></returns>
		public Task ConnectAsync()
		{
			stopSrc = new CancellationTokenSource();

			return Task.Run(() =>
				{
					brickDevice = FindBrickDevice();
					brickDevice.OpenDevice(DeviceMode.Overlapped, DeviceMode.NonOverlapped, ShareMode.Exclusive);
					if (brickDevice.IsConnected && !brickDevice.IsOpen)
						throw new Exception("Device connection failed.");
					rxThread = RxLoop();
				});
		}

		/// <summary>
		/// Disconnect from the EV3 brick.
		/// </summary>
		public void Disconnect()
		{
			brickDevice.CloseDevice();

			stopSrc.Cancel();

			try
			{
				rxThread.Wait();
			}
			catch (AggregateException) { }
		}

		/// <summary>
		/// Write data to the EV3 brick.
		/// </summary>
		/// <param name="data">Byte array to send to the EV3 brick.</param>
		/// <returns></returns>
		public async Task WriteAsync(byte[] data)
		{
			HidReport report = brickDevice.CreateReport();
			report.Data = data;

            //TaskCompletionSource<bool> waitSrc = new TaskCompletionSource<bool>();
            //brickDevice.WriteReport(report, r => waitSrc.SetResult(r));
            //bool result = await waitSrc.Task;

			//bool result = brickDevice.WriteFeatureData(data);
            bool result = brickDevice.WriteReport(report);
            //bool result = await Task.Run<bool>(() => brickDevice.WriteReport(report));

			if (!result)
				throw new Exception("USB communication failed!");
		}

		private async Task RxLoop()
		{
			try
			{
				while (!stopSrc.IsCancellationRequested)
				{
					TaskCompletionSource<HidReport> waitReportSrc = new TaskCompletionSource<HidReport>();
					brickDevice.ReadReport(r => waitReportSrc.TrySetResult(r));
					stopSrc.Token.Register(() => waitReportSrc.TrySetCanceled());

					HidReport report = await waitReportSrc.Task;
					OnBrickReport(report);
				}
			}
			catch (TaskCanceledException)
			{
			}
		}

		private void OnBrickReport(HidReport hReport)
		{
			short size = (short)(hReport.Data[0] | hReport.Data[1] << 8);
			if (size == 0)
				return;

			byte[] report = new byte[size];
			Array.Copy(hReport.Data, 2, report, 0, size);

			if (ReportReceived != null)
				ReportReceived(this, new ReportReceivedEventArgs() { Report = report });
		}

		private static HidDevice FindBrickDevice()
		{
			foreach (HidDevice device in HidDevices.Enumerate())
			{
				if (device.Attributes.VendorId == VID && device.Attributes.ProductId == PID)
					return device;
			}

			throw new Exception("No LEGO EV3s found in HID device list.");
		}
	}
}
