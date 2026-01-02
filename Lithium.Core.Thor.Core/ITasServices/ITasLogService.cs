using System;
using System.Collections.Generic;

namespace Lithium.Core.Thor.Core
{
    public interface ITasLogService : ITasService
    {
        string PathToLogFile { get; }
        
        bool OpenLogFile(string filePath);
        void CloseLogFile();
        
        void Log<T>(IEnumerable<T> t);
        void Log<T>(List<T> t);
        void Log<T>(T t) where T : IComparable;
        void Log(string str, params object[] args);
        void Log(string str, string str2);

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