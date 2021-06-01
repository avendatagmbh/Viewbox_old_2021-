using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using System.Text;
using DbAccess;
using Viewbox.Models;
using Viewbox.Properties;

namespace Viewbox
{
	public class MySqlAutoConfigModel : ViewboxModel
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

		private const long InitFourGigsOfRam = 4294967296L;

		private const long InitKeyBufferSize = 2147483648L;

		private const long InitReadBufferSize = 8388608L;

		private const long InitRndBufferSize = 8388608L;

		private const long InitMyIsamSortBufferSize = 4194304L;

		private const long InitMaxAllowedPacket = 1073741824L;

		private ulong _availableMemoryInBytes;

		private ulong _totalMemoryInBytes;

		private long _keyBufferSize;

		private long _readBufferSize;

		private long _readRndBufferSize;

		private long _myisamSortBufferSize;

		private long _maxAllowedPacket;

		private double calculatedKeyBufferSize = 0.0;

		private double calculatedReadBufferSize = 0.0;

		private double calculatedRndBufferSize = 0.0;

		private double calculatedMyIsamSortBufferSize = 0.0;

		public new DialogModel Dialog { get; set; }

		public override string LabelCaption
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public MySqlAutoConfigModel()
		{
			try
			{
				MEMORYSTATUSEX msex = new MEMORYSTATUSEX();
				GlobalMemoryStatusEx(msex);
				_availableMemoryInBytes = msex.ullAvailPhys;
				_totalMemoryInBytes = msex.ullTotalPhys;
				ReadSystemVariables();
			}
			catch (Exception)
			{
				_availableMemoryInBytes = 0uL;
				_totalMemoryInBytes = 0uL;
			}
		}

		private void ReadSystemVariables()
		{
			using DatabaseBase conn = ViewboxApplication.Database.ConnectionManager.GetConnection();
			if (!conn.IsOpen)
			{
				conn.Open();
			}
			string sqlQuery = $"SHOW VARIABLES WHERE variable_name IN ('key_buffer_size', 'read_buffer_size', 'read_rnd_buffer_size', 'myisam_sort_buffer_size', 'max_allowed_packet');";
			using (IDataReader reader = conn.ExecuteReader(sqlQuery))
			{
				while (reader.Read())
				{
					switch (reader.GetString(0))
					{
					case "key_buffer_size":
						_keyBufferSize = reader.GetInt64(1);
						break;
					case "read_buffer_size":
						_readBufferSize = reader.GetInt64(1);
						break;
					case "read_rnd_buffer_size":
						_readRndBufferSize = reader.GetInt64(1);
						break;
					case "myisam_sort_buffer_size":
						_myisamSortBufferSize = reader.GetInt64(1);
						break;
					case "max_allowed_packet":
						_maxAllowedPacket = reader.GetInt64(1);
						break;
					}
				}
			}
			calculatedKeyBufferSize = ((2147483648.0 * ((double)_totalMemoryInBytes / 4294967296.0) >= 4294967296.0) ? 4294967296.0 : (2147483648.0 * ((double)_totalMemoryInBytes / 4294967296.0)));
			calculatedReadBufferSize = ((8388608.0 * ((double)_totalMemoryInBytes / 4294967296.0) >= 8388608.0) ? 8388608.0 : (8388608.0 * ((double)_totalMemoryInBytes / 4294967296.0)));
			calculatedRndBufferSize = ((8388608.0 * ((double)_totalMemoryInBytes / 4294967296.0) >= 8388608.0) ? 8388608.0 : (8388608.0 * ((double)_totalMemoryInBytes / 4294967296.0)));
			calculatedMyIsamSortBufferSize = ((4194304.0 * ((double)_totalMemoryInBytes / 4294967296.0) >= 4194304.0) ? 4194304.0 : (4194304.0 * ((double)_totalMemoryInBytes / 4294967296.0)));
		}

		public void InitDialog()
		{
			int numberOfProcesses = NumberOfRunningProcesses();
			Dialog = new DialogModel
			{
				Title = Resources.mySQLConfQuestionTitle,
				Content = string.Format(Resources.mySQLConfQuestionText, numberOfProcesses),
				DialogType = DialogModel.Type.Warning,
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = "Yes"
					},
					new DialogModel.Button
					{
						Caption = "No"
					}
				}
			};
		}

		public bool IsMySqlConfigurationNeeded()
		{
			return calculatedKeyBufferSize > (double)_keyBufferSize * 1.05 || calculatedReadBufferSize > (double)_readBufferSize * 1.05 || calculatedRndBufferSize > (double)_readRndBufferSize * 1.05 || calculatedMyIsamSortBufferSize > (double)_myisamSortBufferSize * 1.05;
		}

		public void ConfigureMySqlServer()
		{
			using DatabaseBase conn = ViewboxApplication.Database.ConnectionManager.GetConnection();
			if (!conn.IsOpen)
			{
				conn.Open();
			}
			StringBuilder sqlQuery = new StringBuilder();
			if (1073741824 > _maxAllowedPacket)
			{
				sqlQuery.Append($"SET GLOBAL max_allowed_packet = {1073741824L};");
			}
			if (calculatedKeyBufferSize > (double)_keyBufferSize)
			{
				sqlQuery.Append($"SET GLOBAL key_buffer_size = {(long)calculatedKeyBufferSize};");
			}
			if (calculatedReadBufferSize > (double)_readBufferSize)
			{
				sqlQuery.Append($"SET GLOBAL read_buffer_size = {(long)calculatedReadBufferSize};");
			}
			if (calculatedRndBufferSize > (double)_readRndBufferSize)
			{
				sqlQuery.Append($"SET GLOBAL read_rnd_buffer_size = {(long)calculatedRndBufferSize};");
			}
			if (calculatedMyIsamSortBufferSize > (double)_myisamSortBufferSize)
			{
				sqlQuery.Append($"SET GLOBAL myisam_sort_buffer_size = {(long)calculatedMyIsamSortBufferSize};");
			}
			if (sqlQuery.Length > 0)
			{
				try
				{
					conn.ExecuteNonQuery(sqlQuery.ToString());
				}
				catch (Exception)
				{
				}
			}
		}

		private int NumberOfRunningProcesses()
		{
			using DatabaseBase conn = ViewboxApplication.Database.ConnectionManager.GetConnection();
			if (!conn.IsOpen)
			{
				conn.Open();
			}
			try
			{
				return int.Parse(conn.ExecuteScalar("SELECT COUNT(*)-1 FROM information_schema.processlist WHERE COMMAND = 'Query';").ToString());
			}
			catch (Exception)
			{
				return -1;
			}
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
