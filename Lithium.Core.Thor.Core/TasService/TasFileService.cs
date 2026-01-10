using System;
using System.Collections.Generic;
using System.IO;
using Thor.Core;
using UnityEngine;

namespace Lithium.Core.Thor.Core
{
    public class TasFileService : TasService, ITasFileService
    {
        public string Name => "FileService";
        public float LoadProgress => 1f;
        private readonly Dictionary<string, FileStream> m_fileStreams = new Dictionary<string, FileStream>();
        private readonly Dictionary<string, StreamWriter> m_writers = new Dictionary<string, StreamWriter>();
        private readonly Dictionary<string, StreamReader> m_readers = new Dictionary<string, StreamReader>();

        public string PathToTasDir { get; private set; }
        public string PathToSavesDir { get; private set; }

        public bool OpenFile(string filePath)
        {
            if (m_fileStreams.ContainsKey(filePath))
                return true;
            
            if (string.IsNullOrEmpty(filePath))
                return false;

            if (!ExistsDirectory(Path.GetDirectoryName(filePath)))
                CreateDirectory(Path.GetDirectoryName(filePath));
            
            try
            {
                var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                m_fileStreams[filePath] = fs;
                m_writers[filePath] = new StreamWriter(fs);
                m_readers[filePath] = new StreamReader(fs);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void Update() { }

        public bool Initialize()
        {
            PathToTasDir = Path.Combine(Services.File.RootPath, "Tas");
            PathToSavesDir = Path.Combine(Services.File.RootPath, "Saves");
            CreateDirectory(PathToTasDir);
            CreateDirectory(PathToSavesDir);
            
            return true;
        }

        public bool WriteToFile(string filePath, string content, bool append = true)
        {
            if (!m_writers.ContainsKey(filePath) && !OpenFile(filePath))
                return false;

            var writer = m_writers[filePath];
            if (!append)
                writer.BaseStream.SetLength(0);
            
            writer.Write(content);
            writer.Flush();

            return true;
        }

        public bool ReadFromFile(string filePath, out string content)
        {
            content = null;
            if (!m_readers.ContainsKey(filePath) && !OpenFile(filePath))
                return false;
            
            var fs = m_fileStreams[filePath];
            fs.Seek(0, SeekOrigin.Begin);
            content = m_readers[filePath].ReadToEnd();
            return true;
        }

        public bool CopyFile(string sourceFilePath, string destFilePath, bool overwrite)
        {
            try
            {
                if (!File.Exists(sourceFilePath))
                    return false;
                
                if (!ExistsDirectory(Path.GetDirectoryName(destFilePath)))
                        CreateDirectory(Path.GetDirectoryName(destFilePath));
                
                File.Copy(sourceFilePath, destFilePath, overwrite);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public void CloseFile(string filePath)
        {
            if (m_writers.TryGetValue(filePath, out var writer))
            {
                writer.Flush();
                writer.Dispose();
                m_writers.Remove(filePath);
            }
            if (m_readers.TryGetValue(filePath, out var reader))
            {
                reader.Dispose();
                m_readers.Remove(filePath);
            }
            if (m_fileStreams.TryGetValue(filePath, out var fs))
            {
                fs.Dispose();
                m_fileStreams.Remove(filePath);
            }
        }

        public string ReadFromFile(string filePath)
        {
            if (!m_readers.ContainsKey(filePath) && !OpenFile(filePath))
                return null;
            
            var fs = m_fileStreams[filePath];
            fs.Seek(0, SeekOrigin.Begin);
            return m_readers[filePath].ReadToEnd();
        }

        public bool CreateDirectory(string dirPath)
        {
            try
            {
                Directory.CreateDirectory(dirPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool GetDirectoryName(string filePath, out string dirName)
        {
            dirName = Path.GetDirectoryName(filePath);
            return !string.IsNullOrEmpty(dirName);
        }

        public bool ExistsDirectory(string dirPath) => Directory.Exists(dirPath);

        public bool ExistsFile(string filePath) => File.Exists(filePath);

        public bool DeleteFile(string filePath)
        {
            try
            {
                File.Delete(filePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteDirectory(string dirPath)
        {
            try
            {
                Directory.Delete(dirPath, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string[] GetFilesInDirectory(string dirPath, string searchPattern, bool searchSubDirs)
        {
            var option = searchSubDirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            return Directory.GetFiles(dirPath, searchPattern, option);
        }

        // IService
        public System.Collections.IEnumerator InitializeAsync() { yield return null; }
        public void CollectDebugState(System.Collections.Generic.Dictionary<string, object> debugStateProperties) { }
        public void Shutdown() { }
    }
}