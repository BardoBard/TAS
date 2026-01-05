using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Thor.Core;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Lithium.Core.Thor.Core
{
    /// <summary>
    /// For now this is only used for log but can be extended for other purposes later by adding dynamic Open/Close methods.
    /// </summary>
    public class TasLogService : TasService, ITasLogService
    {
        private bool m_isLogging = true;
        private int m_logCount = 0;
        private StreamWriter m_logFileWriter = null;
        private Action<string> m_outputMethod = null;

        public string PathToLogFile { get; private set; } = string.Empty;

        public string Name => "LogService";
        public static string TasDirectory => "Tas";
        public float LoadProgress => 1f;

        public bool Initialize()
        {
            OpenLogFile(Path.Combine(Path.Combine(Application.persistentDataPath, Services.Platform.LoggedInUserID), Path.Combine(TasDirectory, "TasLog.txt")));
            ClearLog();
            Log("--- Tas Log Service Initialized ---");
            return true;
        }

        public void Update()
        {
        }

        public void Shutdown()
        {
            m_logFileWriter?.Close();
        }
        
        private bool OpenLogFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            
            // Open the log file for writing
            m_logFileWriter?.Close();
            m_logFileWriter = new StreamWriter(filePath, append: true);
            PathToLogFile = filePath;
            m_outputMethod = message =>
            {
                m_logFileWriter.WriteLine(message);
                m_logFileWriter.Flush(); // Ensure the message is written immediately
            };
            return true;
        }

        private void CloseLogFile()
        {
            m_logFileWriter?.Close();
            m_logFileWriter = null;
            m_outputMethod = null;
        }

        public void Log<T>(IEnumerable<T> t)
        {
            foreach (var num in t)
                Log(num.ToString(), m_outputMethod);
        }

        public void Log<T>(List<T> t)
        {
            if (t == null || t.Count == 0)
            {
                Log("List is empty or null", m_outputMethod);
                return;
            }
            foreach (var num in t)
                Log(num.ToString(), m_outputMethod);
        }

        public void Log<T>(T t) where T : IComparable
        {
            Log(t.ToString(), m_outputMethod);
        }

        public void Log(string str, params object[] args)
        {
            if (m_outputMethod == null)
                return;
            string message = string.Format(str ?? string.Empty, args);
            m_outputMethod.Invoke(message);
        }

        public void Log(string str, string str2)
        {
            if (m_outputMethod == null)
                return;
            string message = str + "\n\t" + str2;
            m_outputMethod.Invoke(message);
        }

        public void LogStackTrace()
        {
            var frames = new StackTrace().GetFrames();
            for (var index = 1; index < frames.Length; index++)
            {
                var frame = frames[index];
                var method = frame.GetMethod();

                //full path
                var fullPath = method?.DeclaringType != null
                    ? $"{method.DeclaringType.FullName}.{method.Name}"
                    : method?.Name;

                Log(fullPath);
            }

            Log("");
        }

        public void StopLogging() => m_isLogging = false;

        public void ContinueLogging() => m_isLogging = true;

        public bool IsLogging() => m_isLogging;

        public void ResetCount() => m_logCount = 0;

        public void IncrementCount() => m_logCount++;

        public void DecrementCount() => m_logCount--;

        public void SetCount(int count) => m_logCount = count;

        public int GetCount() => m_logCount;

        public void ClearLog()
        {
            if (m_logFileWriter == null) return;
            
            m_logFileWriter.Close();
            File.WriteAllText(PathToLogFile, string.Empty);
            m_logFileWriter = new StreamWriter(PathToLogFile, append: true);

        }
        
        // IService
        public IEnumerator InitializeAsync()
        {
            yield return null;
        }
        public void CollectDebugState(Dictionary<string, object> debugStateProperties)
        {
        }
    }
}