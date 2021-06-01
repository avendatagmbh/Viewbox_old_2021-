using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Viewbox.Models
{
	public class InformationModel : SettingsModel
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private class MEMORYSTATUSEX
		{
			public uint dwLength;

			public uint dwMemoryLoad;

			public ulong ullTotalPhys;

			public ulong ullAvailPhys;

			public ulong ullTotalPageFile;

			public ulong ullAvailPageFile;

			public ulong ullTotalVirtual;

			public ulong ullAvailVirtual;

			public ulong ullAvailExtendedVirtual;

			public MEMORYSTATUSEX()
			{
				dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
			}
		}

		public ControlCenterModel ControlModel { get; private set; }

		public override string Partial => "_InformationPartial";

		public InformationModel()
			: base(SettingsType.Information)
		{
			ControlModel = new ControlCenterModel();
			char[] buffer = new char[512];
			uint code = GetLogicalDriveStrings(512u, buffer);
			if (code == 0)
			{
				Console.WriteLine("Call failed");
				return;
			}
			List<string> list = new List<string>();
			int start = 0;
			for (int i = 0; i < code; i++)
			{
				if (buffer[i] == '\0')
				{
					string s = new string(buffer, start, i - start);
					list.Add(s);
					start = i + 1;
				}
			}
			int j = 0;
			foreach (string s2 in list)
			{
				GetDiskFreeSpaceEx(s2, out var freeBytesAvailable, out var totalBytes, out var _);
				ControlModel.FreeDiscSpace.Add(freeBytesAvailable);
				ControlModel.AvailableDiscSpace.Add(totalBytes);
				ControlModel.Discs.Info.Add(j, s2);
				j++;
			}
			MEMORYSTATUSEX msex = new MEMORYSTATUSEX();
			GlobalMemoryStatusEx(msex);
			ControlModel.FreeMem = msex.ullAvailPhys;
			ControlModel.AvailableMem = msex.ullTotalPhys;
			ControlModel.CpuLoad = ViewboxApplication.ProcessorTime.NextValue();
			ControlModel.DiscSpeed = ViewboxApplication.HardDiskTransferSpeed.NextValue();
			ControlModel.DiscSpeed /= 10f;
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		private static extern int GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);

		[DllImport("kernel32.dll")]
		private static extern uint GetLogicalDriveStrings(uint nBufferLength, [Out] char[] lpBuffer);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GlobalMemoryStatusEx([In][Out] MEMORYSTATUSEX lpBuffer);
	}
}
