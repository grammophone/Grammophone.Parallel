using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grammophone.Parallel
{
	/// <summary>
	/// A subset of PLINQ-type enumerable for parallelizing iterations which take very long.
	/// </summary>
	/// <typeparam name="S">The type of the source sequence items.</typeparam>
	/// <typeparam name="T">The type of the target sequence items.</typeparam>
	public abstract class LongParallelQuery<S, T> : IEnumerable<T>
	{
		#region Private fields

		private IEnumerable<S> source;

		private LongParallelQuerySettings settings;

		#endregion

		#region Construction

		internal LongParallelQuery(IEnumerable<S> source)
		{
			if (source == null) throw new ArgumentNullException("source");

			this.source = source;
			this.settings = new LongParallelQuerySettings();
		}

		#endregion

		#region Internal properties

		internal IEnumerable<S> Source
		{
			get
			{
				return source;
			}
		}

		internal virtual Predicate<S> Predicate
		{
			get
			{
				return s => true;
			}
		}

		internal abstract Func<S, T> Selector
		{
			get;
		}

		internal LongParallelQuerySettings Settings
		{
			get
			{
				return settings;
			}
			set
			{
				if (value == null) throw new ArgumentNullException("value");
				settings = value;
			}
		}

		#endregion

		#region IEnumerable<T> Members

		/// <summary>
		/// Enumerate the results.
		/// </summary>
		public abstract IEnumerator<T> GetEnumerator();

		#endregion

		#region IEnumerable Members

		/// <summary>
		/// Calls <see cref="GetEnumerator"/>.
		/// </summary>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
