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