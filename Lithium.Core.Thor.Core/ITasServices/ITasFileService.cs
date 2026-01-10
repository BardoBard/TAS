using System;

namespace Lithium.Core.Thor.Core
{
    public struct FileOptions
    {
    }
    
    public interface ITasFileService : ITasService
    {
        string PathToTasDir { get; }
        string PathToSavesDir { get; }
        
        bool OpenFile(string filePath);
        void CloseFile(string filePath);
        bool WriteToFile(string filePath, string content, bool append = true);
        bool ReadFromFile(string filePath, out string content);
        bool CopyFile(string sourceFilePath, string destFilePath, bool overwrite);
        
        bool CreateDirectory(string dirPath);
        bool GetDirectoryName(string filePath, out string dirName);
        bool ExistsDirectory(string dirPath);
        bool ExistsFile(string filePath);
        bool DeleteFile(string filePath);
        bool DeleteDirectory(string dirPath);
        
        string[] GetFilesInDirectory(string dirPath, string searchPattern, bool searchSubDirs);
    }
}