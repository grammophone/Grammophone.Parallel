using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Gramma.Parallel
{
	/// <summary>
	/// A subset of PLINQ implemetation for parallelizing iterations which take very long.
	/// </summary>
	public static class LongParallelLinq
	{
		/// <summary>
		/// Parallelize a sequence whose processing of each item is expected take long time.
		/// </summary>
		/// <typeparam name="S">The type of items in the source sequence.</typeparam>
		/// <param name="source">The source sequence.</param>
		/// <returns>Returns a parallel query base.</returns>
		public static LongParallelQuery<S, S> AsLongParallel<S>(this IEnumerable<S> source)
		{
			return new RootLongParallelQuery<S>(source);
		}

		/// <summary>
		/// Filter a sequence in parallel whose processing of each item is expected take long time.
		/// </summary>
		/// <typeparam name="S">The type of the source sequence items.</typeparam>
		/// <typeparam name="T">The type of the output sequence items.</typeparam>
		/// <param name="baseQuery">The query to extend.</param>
		/// <param name="predicate">The filter predicate.</param>
		/// <returns>Returns the filtered query.</returns>
		public static LongParallelQuery<S, T> Where<S, T>(this LongParallelQuery<S, T> baseQuery, Predicate<S> predicate)
		{
			return SelectLongParallelQuery<S, T>.ExtendQuery(baseQuery, predicate);
		}

		/// <summary>
		/// Map a sequence to another sequence whose processing of each item is expected take long time.
		/// </summary>
		/// <typeparam name="S">The type of the source sequence items.</typeparam>
		/// <typeparam name="Q">The type of the output sequence items of the original query.</typeparam>
		/// <typeparam name="T">The type of the output sequence items.</typeparam>
		/// <param name="baseQuery">The original query.</param>
		/// <param name="selector">The function that maps elements of type <typeparamref name="Q"/> to type <typeparamref name="T"/>.</param>
		/// <returns>Returns the mapped query.</returns>
		public static LongParallelQuery<S, T> Select<S, Q, T>(this LongParallelQuery<S, Q> baseQuery, Func<Q, T> selector)
		{
			return SelectLongParallelQuery<S, T>.ExtendQuery(baseQuery, selector);
		}

		/// <summary>
		/// Associate a cancellation token with the query in order to be able to abort it.
		/// </summary>
		/// <typeparam name="S">The type of the source sequence items.</typeparam>
		/// <typeparam name="T">The type of the output sequence items.</typeparam>
		/// <param name="query">The query to associate with the cancellation token.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>Returns the same query after having set the cancellation token.</returns>
		public static LongParallelQuery<S, T> WithCancellation<S, T>(this LongParallelQuery<S, T> query, CancellationToken cancellationToken)
		{
			if (query == null) throw new ArgumentNullException("query");

			query.Settings.CancellationToken = cancellationToken;

			return query;
		}

		/// <summary>
		/// Set the number of concurrently executing tasks that will be used to process the query.
		/// </summary>
		/// <typeparam name="S">The type of the source sequence items.</typeparam>
		/// <typeparam name="T">The type of the output sequence items.</typeparam>
		/// <param name="query">The query to set the degree of parallelism.</param>
		/// <param name="degreeOfparallelism">The number of concurrently executing tasks that will be used to process the query.</param>
		/// <returns>Returns the same query after having set the degree of parallelism.</returns>
		public static LongParallelQuery<S, T> WithDegreeOfParallelism<S, T>(this LongParallelQuery<S, T> query, int degreeOfparallelism)
		{
			if (query == null) throw new ArgumentNullException("query");

			query.Settings.DegreeOfParallelism = degreeOfparallelism;

			return query;
		}
	}
}
