using MonitorSlim.Monitors;

namespace MonitorSlim.Tests
{
    public class ShortMonitorsShould
    {
        [Theory(DisplayName = "Protect critical sections using ShortMonitor")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public async void ProtectCriticalSectionsWithShortMonitor(int nThreads)
        {
            var monitor = new ShortMonitor();
            var count = 0;
#if RELEASE
            var loops = 3_000_000;
#else
            var loops = 100_000;
#endif
            var action = () =>
            {
                for (int i = 0; i < loops; i++)
                {
                    monitor.Enter();
                    try
                    {
                        var r = Interlocked.Increment(ref count);
                        Assert.True(r < 2);
                        Interlocked.Decrement(ref count);
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        monitor.Exit();
                    }
                }
            };

            var actions = new List<Action>();
            for (int i = 0; i < nThreads; i++)
                actions.Add(action);

            await Task.WhenAll(actions.Select(a => Task.Run(a)).ToArray());
        }

        [Theory(DisplayName = "Protect critical sections using ShortNonSpinnedMonitor")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public async void ProtectCriticalSectionsWithShortNonSpinnedMonitor(int nThreads)
        {
            var monitor = new ShortNonSpinnedMonitor();
            var count = 0;
#if RELEASE
            var loops = 3_000_000;
#else
            var loops = 100_000;
#endif

            var action = () =>
            {
                for (int i = 0; i < loops; i++)
                {
                    monitor.Enter();
                    try
                    {
                        var r = Interlocked.Increment(ref count);
                        Assert.True(r < 2);
                        Interlocked.Decrement(ref count);
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        monitor.Exit();
                    }
                }
            };

            var actions = new List<Action>();
            for (int i = 0; i < nThreads; i++)
                actions.Add(action);

            await Task.WhenAll(actions.Select(a => Task.Run(a)).ToArray());
        }

        [Theory(DisplayName = "Allow exact thread count to enter using ShortCountMonitor")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public async void AllowExactThreadCountToEnterUsingShortCountMonitor(int nThreads)
        {
            var monitor = new ShortCountMonitor(2);
            var count = 0;
#if RELEASE
            var loops = 3_000_000;
#else
            var loops = 100_000;
#endif

            var action = () =>
            {
                for (int i = 0; i < loops; i++)
                {
                    monitor.Enter();
                    try
                    {
                        var r = Interlocked.Increment(ref count);
                        Assert.True(r < 3);
                        Interlocked.Decrement(ref count);
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        monitor.Exit();
                    }
                }
            };

            var actions = new List<Action>();
            for (int i = 0; i < nThreads; i++)
                actions.Add(action);

            await Task.WhenAll(actions.Select(a => Task.Run(a)).ToArray());
        }

        [Theory(DisplayName = "Allow thread recursive enter using ShortRecursiveMonitor")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public async void AllowThreadRecursiveEnterUsingShortRecursiveMonitor(int nThreads)
        {
            var monitor = new ShortRecursiveMonitor();
            var count = 0;
#if RELEASE
            var loops = 3_000_000;
#else
            var loops = 100_000;
#endif

            var action = () =>
            {
                for (int i = 0; i < loops; i++)
                {
                    monitor.Enter();
                    try
                    {
                        var r = Interlocked.Increment(ref count);
                        Assert.True(r < 2);
                        monitor.Enter();
                        try
                        {
                            Assert.True(count < 2);
                        }
                        catch
                        {
                            throw;
                        }
                        finally
                        {
                            monitor.Exit();
                            Interlocked.Decrement(ref count);
                        }
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        monitor.Exit();
                    }
                }
            };

            var actions = new List<Action>();
            for (int i = 0; i < nThreads; i++)
                actions.Add(action);

            await Task.WhenAll(actions.Select(a => Task.Run(a)).ToArray());
        }

        [Theory(DisplayName = "Allow one writer multiple reader using ShortReadWriteMonitor")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public async void AllowOneWriterMultipleReaderUsingShortReadWriteMonitor(int nThreads)
        {
            var monitor = new ShortReadWriteMonitor();
            var countWrite = 0;
#if RELEASE
            var loops = 3_000_000;
#else
            var loops = 100_000;
#endif
            var action = () =>
            {
                for (int i = 0; i < loops; i++)
                {
                    if (i % 15 == 0)
                    {
                        monitor.EnterWrite();
                        try
                        {
                            var r = Interlocked.Increment(ref countWrite);
                            Assert.True(r < 2);
                            Interlocked.Decrement(ref countWrite);
                        }
                        catch
                        {
                            throw;
                        }
                        finally
                        {
                            monitor.ExitWrite();
                        }
                    }
                    else
                    {
                        monitor.EnterRead();
                        try
                        {
                            Assert.Equal(0, Volatile.Read(ref countWrite));
                        }
                        catch
                        {
                            throw;
                        }
                        finally
                        {
                            monitor.ExitRead();
                        }
                    }
                }
            };

            var actions = new List<Action>();
            for (int i = 0; i < nThreads; i++)
                actions.Add(action);

            await Task.WhenAll(actions.Select(a => Task.Run(a)).ToArray());
        }
    }
}