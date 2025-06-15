
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ADOAnalyser.Extensions
{
    public static class Extensions
    {
        /// <summary>
        /// Concurrently executes asynchronous actions for each item in the enumerable.
        /// </summary>
        /// <typeparam name="T">The type of the items in the enumerable.</typeparam>
        /// <param name="enumerable">The enumerable to iterate over.</param>
        /// <param name="action">An asynchronous action to execute for each item.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <param name="maxDegreeOfParallelism">Optional. Maximum number of concurrent tasks. If null or less than 1, full concurrency is used.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task ForEachAsyncConcurrent<T>(
 this IEnumerable<T> enumerable,
 Func<T, Task> action,
 CancellationToken cancellationToken,
 int? maxDegreeOfParallelism = null)
        {
            if (maxDegreeOfParallelism.HasValue && maxDegreeOfParallelism.Value > 0)
            {
                await Task.WhenAll(
                Partitioner.Create(enumerable).GetPartitions(maxDegreeOfParallelism.Value)
                .Select(partition => Task.Run(async () =>
                {
                    using (partition)
                    {
                        while (partition.MoveNext())
                        {
                            try
                            {
                                await action(partition.Current);
                            }
                            catch (Exception ex)
                            {
                                // Log or handle exception as needed
                                Console.Error.WriteLine($"Error processing item: {ex.Message}");
                            }
                        }
                    }
                }, cancellationToken)));
            }
            else
            {
                var tasks = enumerable.Select(async item =>
                {
                    try
                    {
                        await action(item);
                    }
                    catch (Exception ex)
                    {
                        // Log or handle exception as needed
                        Console.Error.WriteLine($"Error processing item: {ex.Message}");
                    }
                });

                await Task.WhenAll(tasks);
            }
        }
    }
}
