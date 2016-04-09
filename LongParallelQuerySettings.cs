using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Grammophone.Parallel
{
	/// <summary>
	/// The settings for executing a <see cref="LongParallelQuery{S, T}"/>.
	/// </summary>
	internal class LongParallelQuerySettings
	{
		private int degreeOfParallelism;

		private CancellationToken cancellationToken;

		public LongParallelQuerySettings()
		{
			degreeOfParallelism = Environment.ProcessorCount;
			cancellationToken = new CancellationToken();
		}

		public int DegreeOfParallelism
		{
			get
			{
				return degreeOfParallelism;
			}
			set
			{
				if (value < 0) throw new ArgumentException("Value must be positive.");
				
				degreeOfParallelism = value;
			}
		}

		public CancellationToken CancellationToken
		{
			get
			{
				return cancellationToken;
			}
			set
			{
				cancellationToken = value;
			}
		}
	}
}
