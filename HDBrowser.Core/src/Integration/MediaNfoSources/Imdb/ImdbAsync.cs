// 
//    Copyright 2009 Janusz Skonieczny
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
// $Id: $
// 
using System;
using System.Threading;
using HDBrowser.Core.Integration.MediaNfoSources.Imdb;
using HDBrowser.Core.Model;
using log4net;

namespace HdBrowser.Integration {
    /// <summary>
    /// An asynchronous wrapper for <see cref="Imdb.Update"/>.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="ThreadPool.QueueUserWorkItem(System.Threading.WaitCallback,object)"/> to queue
    /// <see cref="Worker"/> methos calls that wraps <see cref="Imdb.Update"/> calls. 
    /// 
    /// A maximum number of concurrent workers is controlled by <see cref="MaxConcurrentWorkers"/>, 
    /// use this to tweak throughtput but take into account that high numer of concurrent web connections may
    /// trigger DOS protection, resulting in timeouts.
    /// 
    /// Exceptions from <see cref="Imdb.Update"/> are caught and added to the <see cref="UpdateCompleteEventArgs"/>.
    /// </remarks>
    public class ImdbAsync {
        private static readonly ILog log = LogManager.GetLogger(typeof(ImdbAsync));
        /// <summary>
        /// Controls how many <see cref="Imdb.Update"/> calls are workking simultanously.
        /// </summary>
        public static int MaxConcurrentWorkers = 2;
        private static int workerCount = 0;

        /// <summary>
        /// Queues an update on <see cref="ThreadPool"/>.
        /// </summary>
        /// <param name="titleInfo"></param>
        public static void Update(TitleInfo titleInfo) {
            ThreadPool.QueueUserWorkItem(Worker, titleInfo);
        }

        /// <summary>
        /// A worker method used to queue by <see cref="ThreadPool.QueueUserWorkItem(System.Threading.WaitCallback,object)"/>.
        /// </summary>
        /// <param name="title"></param>
        /// <remarks>
        /// Triggers an <see cref="UpdateComplete"/> event.
        /// </remarks>
        protected static void Worker(object title) {
            if (!(title is TitleInfo)) {
                throw new NotSupportedException("Type " + title.GetType() + " is not supported");
            }
            TitleInfo info = (TitleInfo) title;
            lock (log) {
                while (workerCount >= MaxConcurrentWorkers) {
                    log.DebugFormat("Worker: Waiting workerCount={0} {1}", workerCount, info.Name);
                    Monitor.Wait(log, 200);
                }
                log.DebugFormat("Worker: ++ workerCount={0} {1}", workerCount, info.Name);
                workerCount++;
            }
            Exception exception = null;
            try
            {
                Imdb.Update(info);
            }
            catch (Exception e)
            {
                exception = e;
                log.Error("Worker: failed", e);
            }
                lock (log) {
                    log.DebugFormat("Worker: -- workerCount={0} {1}", workerCount, info.Name);
                    workerCount--;
                    Monitor.Pulse(log);
            }
            OnUpdateComplete(info, exception);
        }

        /// <summary>
        /// Async workers count.
        /// </summary>
        public static int WorkerCount {
            get { return workerCount; }
        }

        /// <summary>
        /// Fired when an async update is complete regaldess of it's result.
        /// </summary>
        public static event UpdateCompleteHandler UpdateComplete;

        protected static void OnUpdateComplete(TitleInfo titleInfo, Exception ex) {
            if (UpdateComplete != null) {
                UpdateComplete(typeof(ImdbAsync), new UpdateCompleteEventArgs { TitleInfo = titleInfo, Error = ex});
            }
        }

        public delegate void UpdateCompleteHandler(object sender, UpdateCompleteEventArgs e);

        public class UpdateCompleteEventArgs : EventArgs {
            /// <summary>
            /// A <see cref="TitleInfo"/> which was updated.
            /// </summary>
            public TitleInfo TitleInfo { get; set;}
            /// <summary>
            /// An exception caught during update or null.
            /// </summary>
            public Exception Error { get; set;}
        }

    }
}