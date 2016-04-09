using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grammophone.Parallel
{
	/// <summary>
	/// The root query created when <see cref="LongParallelLinq.AsLongParallel"/> is invoked.
	/// </summary>
	/// <typeparam name="S">The type of the source sequence items.</typeparam>
	internal class RootLongParallelQuery<S> : LongParallelQuery<S, S>
	{
		public RootLongParallelQuery(IEnumerable<S> source)
			: base(source)
		{

		}

		internal override Func<S, S> Selector
		{
			get { return s => s; }
		}

		public override IEnumerator<S> GetEnumerator()
		{
			return this.Source.GetEnumerator();
		}
	}
}
