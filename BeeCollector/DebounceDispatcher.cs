using System;
using System.Threading.Tasks;

public class DebounceDispatcher : DebounceDispatcher<bool>
{
	public DebounceDispatcher(int interval) : base(interval)
	{
	}

	public Task DebounceAsync(Func<Task> action)
	{
		return base.DebounceAsync(async () =>
		{
			await action.Invoke();
			return true;
		});
	}

	public async void Debounce(Action action)
	{
		Func<Task<bool>> actionAsync = () => Task.Run(() =>
		{
			action.Invoke();
			return true;
		});

		await DebounceAsync(actionAsync);
	}
}

public class DebounceDispatcher<T>
{
	private DateTime lastInvokeTime;
	private readonly int interval;
	private Func<Task<T>>? functToInvoke;
	private object locker = new object();
	private Task<T>? waitingTask;

	public DebounceDispatcher(int interval) => this.interval = interval;

	public Task<T> DebounceAsync(Func<Task<T>> functToInvoke)
	{
		lock (locker)
		{
			this.functToInvoke = functToInvoke;
			this.lastInvokeTime = DateTime.Now;
			if (waitingTask?.IsCompleted ?? true)
				waitingTask = Task.Run(() =>
				{
					do
					{
						int delay = (int)(interval - (DateTime.Now - lastInvokeTime).TotalMilliseconds);
						Task.Delay(delay).Wait();
					} while ((DateTime.Now - lastInvokeTime).TotalMilliseconds < interval);

					T res;
					try
					{
						res = this.functToInvoke.Invoke().Result;
					}
					catch (Exception)
					{
						throw;
					}

					return res;
				});
			return waitingTask;
		}
	}
}