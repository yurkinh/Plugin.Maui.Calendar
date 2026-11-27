namespace SampleApp.Benchmarks;

/// <summary>Completes at the start of the next display frame (Android Choreographer, iOS/Mac CADisplayLink).</summary>
static class DisplayFrames
{
#if ANDROID
	public static Task NextAsync()
	{
		var frame = new TaskCompletionSource();
		Android.Views.Choreographer.Instance!.PostFrameCallback(new FrameCallback(frame));
		return frame.Task;
	}

	sealed class FrameCallback(TaskCompletionSource frame) : Java.Lang.Object, Android.Views.Choreographer.IFrameCallback
	{
		public void DoFrame(long frameTimeNanos) => frame.TrySetResult();
	}
#elif IOS || MACCATALYST
	public static Task NextAsync()
	{
		var frame = new TaskCompletionSource();
		CoreAnimation.CADisplayLink link = null;
		link = CoreAnimation.CADisplayLink.Create(() =>
		{
			link!.Invalidate();
			frame.TrySetResult();
		});
		link.AddToRunLoop(Foundation.NSRunLoop.Main, Foundation.NSRunLoopMode.Common);
		return frame.Task;
	}
#else
	// Windows: about one frame at 60 Hz
	public static Task NextAsync() => Task.Delay(16);
#endif
}
