using System;
using System.Collections.Generic;

namespace Lithium.Core.Thor.Core
{
    public interface ITasLogService : ITasService
    {
        /// <summary>
        /// Gets the path to the TAS/log file in use.
        /// </summary>
        string PathToLogFile { get; }

        /// <summary>
        /// Whenever a log writes occurs, the provided action will be called with the log message.
        /// </summary>
        /// <param name="onLogEvent">The action to call on log writes.</param>
        /// <returns>True if registration was successful, false otherwise.</returns>
        void RegisterOnLogEvent(Action<string> onLogEvent);
        
        void Log<T>(IEnumerable<T> t);
        void Log<T>(List<T> t);
        void Log<T>(T t) where T : IComparable;
        void Log(string str, params object[] args);
        void Log(string str, string str2);

        /// <summary>
        /// Logs stacktrace information of the current thread.
        /// </summary>
        void LogStackTrace();
        void StopLogging();
        void ContinueLogging();
        bool IsLogging();

        void ResetCount();
        void IncrementCount();
        void DecrementCount();
        void SetCount(int count);
        int GetCount();
        
        void ClearLog();
    }
}