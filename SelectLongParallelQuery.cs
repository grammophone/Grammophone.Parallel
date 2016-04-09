using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grammophone.Parallel
{
	/// <summary>
	/// Query for mapping a source sequence to a target sequence in parallel, optionally filtering items.
	/// </summary>
	/// <typeparam name="S">The type of the source sequence items.</typeparam>
	/// <typeparam name="T">The type of the output sequence items.</typeparam>
	internal class SelectLongParallelQuery<S, T> : LongParallelQuery<S, T>
	{
		#region Private fields

		private Func<S, T> selector;

		private Predicate<S> predicate;

		#endregion

		#region Auxilliary types

		internal class SelectEnumerator : IEnumerator<T>
		{
			#region Private fields

			private object sourceLock;

			private SelectLongParallelQuery<S, T> query;

			private IEnumerator<S> sourceEnumerator;

			private BlockingCollection<T> queryResults;

			private T current;

			private AggregateException exception;

			private bool earlyExit;

			#endregion

			#region Construction

			public SelectEnumerator(SelectLongParallelQuery<S, T> query)
			{
				if (query == null) throw new ArgumentNullException("query");

				this.query = query;

				this.sourceEnumerator = query.Source.GetEnumerator();

				sourceLock = new object();

				queryResults = new BlockingCollection<T>();

				earlyExit = false;

				var pumpTask = Task.Factory.StartNew(StartPumping, TaskCreationOptions.LongRunning);

				var exceptionHandlingTask = pumpTask.ContinueWith(HandleAggregateException, TaskContinuationOptions.NotOnRanToCompletion);

				exceptionHandlingTask.ContinueWith(FinishResults);

				pumpTask.ContinueWith(FinishResults, TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			#endregion

			#region IEnumerator<T> Members

			public T Current
			{
				get { return current; }
			}

			#endregion

			#region IDisposable Members

			public void Dispose()
			{

			}

			#endregion

			#region IEnumerator Members

			object System.Collections.IEnumerator.Current
			{
				get
				{
					if (exception != null) throw exception;

					return this.Current;
				}
			}

			public bool MoveNext()
			{
				bool takeSuccess = queryResults.TryTake(out this.current, System.Threading.Timeout.Infinite, query.Settings.CancellationToken);

				if (exception != null) throw exception;

				return takeSuccess;
			}

			public void Reset()
			{
				throw new NotSupportedException();
			}

			#endregion

			#region Private methods

			private bool TryTakeFromSource(out S sourceItem)
			{
				lock (sourceLock)
				{
					if (sourceEnumerator.MoveNext())
					{
						sourceItem = sourceEnumerator.Current;
						return true;
					}
					else
					{
						sourceItem = default(S);
						return false;
					}
				}
			}

			private void StartPumping()
			{
				var tasks = new Task[query.Settings.DegreeOfParallelism];

				for (int i = 0; i < query.Settings.DegreeOfParallelism; i++)
				{
					var task = Task.Factory.StartNew(Work, TaskCreationOptions.LongRunning | TaskCreationOptions.AttachedToParent);

					task.ContinueWith(HandleWorkerException, TaskContinuationOptions.OnlyOnFaulted);

					tasks[i] = task;
				}
			}

			private void LogException(Exception exception)
			{
				if (exception == null) return;

				var aggregateException = exception as AggregateException;

				if (aggregateException != null)
				{
					System.Diagnostics.Trace.WriteLine("AggregateException:");

					foreach (var innerException in aggregateException.InnerExceptions)
					{
						LogException(innerException);
					}

					return;
				}

				string message;

				if (!String.IsNullOrEmpty(exception.Source))
				{
					message =
						String.Format(
						"Exception type '{0}', message: '{1}', source: '{2}'.",
						exception.GetType().FullName,
						exception.Message,
						exception.Source);
				}
				else
				{
					message =
						String.Format(
						"Exception type '{0}', message: '{1}'.",
						exception.GetType().FullName,
						exception.Message);
				}

				System.Diagnostics.Trace.WriteLine(message);

				if (exception.InnerException != null)
				{
					System.Diagnostics.Trace.WriteLine("Inner exception:");

					LogException(exception.InnerException);
				}
			}

			private void Work()
			{
				S sourceItem;

				var settings = query.Settings;

				while (TryTakeFromSource(out sourceItem))
				{
					if (earlyExit) break;

					if (!query.Predicate(sourceItem)) continue;

					settings.CancellationToken.ThrowIfCancellationRequested();

					queryResults.Add(query.Selector(sourceItem));
				}
			}

			private void FinishResults(Task tasks)
			{
				queryResults.CompleteAdding();
			}

			private void HandleAggregateException(Task task)
			{
				if (task == null) throw new ArgumentNullException("task");

				exception = task.Exception;
			}

			private void HandleWorkerException(Task task)
			{
				if (task == null) throw new ArgumentNullException("task");

				earlyExit = true;
			}

			#endregion
		}

		#endregion

		#region Consrtuction

		public SelectLongParallelQuery(IEnumerable<S> source, Func<S, T> selector, Predicate<S> predicate, LongParallelQuerySettings settings)
			: base(source)
		{
			if (selector == null) throw new ArgumentNullException("selector");
			if (predicate == null) throw new ArgumentNullException("predicate");
			if (settings == null) throw new ArgumentNullException("settings");

			this.selector = selector;
			this.predicate = predicate;

			this.Settings = settings;
		}

		#endregion

		#region Internal properties

		internal override Func<S, T> Selector
		{
			get { return selector; }
		}

		internal override Predicate<S> Predicate
		{
			get { return predicate; }
		}

		#endregion

		#region Public methods

		public override IEnumerator<T> GetEnumerator()
		{
			return new SelectEnumerator(this);
		}

		#endregion

		#region Internal methods

		internal static SelectLongParallelQuery<S, T> ExtendQuery<Q>(LongParallelQuery<S, Q> baseQuery, Func<Q, T> selector, Predicate<S> predicate = null)
		{
			if (baseQuery == null) throw new ArgumentNullException("baseQuery");
			if (selector == null) throw new ArgumentNullException("selector");

			Func<S, T> compoundSelector = s => selector(baseQuery.Selector(s));

			Predicate<S> compoundPredicate;

			if (predicate != null)
			{
				compoundPredicate = s => predicate(s) && baseQuery.Predicate(s);
			}
			else
			{
				compoundPredicate = baseQuery.Predicate;
			}

			return new SelectLongParallelQuery<S, T>(baseQuery.Source, compoundSelector, compoundPredicate, baseQuery.Settings);
		}

		internal static SelectLongParallelQuery<S, T> ExtendQuery(LongParallelQuery<S, T> baseQuery, Predicate<S> predicate)
		{
			if (baseQuery == null) throw new ArgumentNullException("baseQuery");
			if (predicate == null) throw new ArgumentNullException("predicate");

			Predicate<S> compoundPredicate = s => baseQuery.Predicate(s) && predicate(s);

			return new SelectLongParallelQuery<S, T>(baseQuery.Source, baseQuery.Selector, compoundPredicate, baseQuery.Settings);
		}

		#endregion

	}
}
