﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AngelSix.SolidDna
{

    /// <summary>
    /// Adds the ability to safely await on tasks to be complete that need limited access
    /// For example, only allowing one task to access some data at a time, like the old 
    /// async locks
    /// 
    /// This awaiter uses the safer semaphore to prevent any chance of a deadlock
    /// </summary>
    public static class AsyncAwaitor
    {
        #region Private Members

        /// <summary>
        /// A semaphore to lock the semaphore list
        /// </summary>
        private static SemaphoreSlim mSelfLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// A list of all semaphore locks (one per key)
        /// </summary>
        private static Dictionary<string, SemaphoreDetails> mSemaphores = new Dictionary<string, SemaphoreDetails>();

        #endregion

        /// <summary>
        /// Awaits for any outstanding tasks to complete that are accessing the same key
        /// </summary>
        /// <param name="key">The key to await</param>
        /// <param name="task">The task to perform inside of the semaphore lock</param>
        /// <param name="maxAccessCount">If this is the first call, sets the maximum number of tasks that can access this task before it waiting</param>
        /// <returns></returns>
        public static async Task Await(string key, Func<Task> task, int maxAccessCount = 1)
        {
            #region Create Semaphore

            //
            // Asynchronously wait to enter the Semaphore
            //
            //      If no-one has been granted access to the Semaphore
            //      code execution will proceed
            //      Otherwise this thread waits here until the semaphore is released 
            //
            await mSelfLock.WaitAsync();

            try
            {
                // Create semaphore if it doesn't already exist
                if (!mSemaphores.ContainsKey(key))
                    mSemaphores.Add(key, new SemaphoreDetails(key, maxAccessCount));
            }
            finally
            {
                //
                // When the task is ready, release the semaphore
                //
                //      It is vital to ALWAYS release the semaphore when we are ready
                //      or else we will end up with a Semaphore that is forever locked
                //      This is why it is important to do the Release within a try...finally clause
                //      Program execution may crash or take a different path, this way you are guaranteed execution
                //
                mSelfLock.Release();
            }

            #endregion

            // Now use this semaphore and perform the desired task inside its lock
            // NOTE: We never remove semaphores after creating them, so this will never be null
            var semaphore = mSemaphores[key];

            // Await this semaphore
            await semaphore.Semaphore.WaitAsync();

            try
            {
                // Perform the job
                await task();
            }
            finally
            {
                // Release the semaphore
                semaphore.Semaphore.Release();
            }
        }
    }
}