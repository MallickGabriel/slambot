using System;
using System.Collections.Generic;

namespace Driver
{
	public class ScanEventArgs : EventArgs
	{
		public readonly List<double> Readings;

		#region Public Constructors

		public ScanEventArgs (List<double> readings)
		{
			this.Readings = readings;
		}

		#endregion

		#region Public Methods

		public override string ToString ()
		{
			return this.Readings.ToArray().ToString ();
		}

		#endregion

	}
}
