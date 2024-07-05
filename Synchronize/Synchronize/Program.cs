using System;
using System.Diagnostics;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 4) 
        {
            Console.WriteLine("Arguments are: sourceFolderPath(string) destinationFolderPath(string)" +
                " logFolderPath(string) syncInterval(uint-milliseconds)");
            Console.ReadKey();
        }                            

        string sourcePath = args[0];
        string destinationPath = args[1];
        string logPath = args[2];
        uint syncInterval = uint.Parse(args[3]);

        while (true)
        {
            Loop(sourcePath, destinationPath, logPath);
            Thread.Sleep((int)syncInterval);
        }        
    }       

    static void PrintToLogAndConsole(StreamWriter logFile, string FileName, string Action)
    {
        string Time = DateTime.Now.ToString("h:mm:ss tt");
        string Output = "[" + Time + "]: " + Action + " " + FileName + ".";

        if (logFile != null)
        {
            logFile.WriteLine(Output);
        }
        
        Console.WriteLine(Output);
    }

    static void Loop(string sourcePath, string destinationPath, string logPath)
    {
        string logFullPath = Path.Combine(logPath, "log.txt");

        DirectoryInfo dirLog = new DirectoryInfo(logPath);
        if (dirLog.GetFiles().Length != 0)
        {
            File.Delete(logFullPath);
        }
        StreamWriter logFile = File.CreateText(logFullPath);

        Console.WriteLine("Running...");

        DirectoryInfo dirSource = new DirectoryInfo(sourcePath);
        DirectoryInfo dirDestination = new DirectoryInfo(destinationPath);

        IEnumerable<FileSystemInfo> listOfFilesSource = dirSource.GetFileSystemInfos("*.*",
        SearchOption.AllDirectories);

        IEnumerable<FileSystemInfo> listOfFilesDestination = dirDestination.GetFileSystemInfos("*.*",
        SearchOption.AllDirectories);

        if (dirSource.GetFiles().Length == 0)
        {
            foreach (FileSystemInfo destinationFile in listOfFilesDestination)
            {
                File.Delete(Path.Combine(destinationPath, destinationFile.Name));
                PrintToLogAndConsole(logFile, destinationFile.Name, "REMOVE");
            }
        }

        foreach (FileSystemInfo sourceFile in listOfFilesSource)
        {
            foreach (FileSystemInfo destinationFile in listOfFilesDestination)
            {
                bool bExistInBoth = sourceFile.Name == destinationFile.Name;
                bool bDestinationFileIsInSource = File.Exists(Path.Combine(sourcePath, destinationFile.Name));
                // copy
                if (bExistInBoth)
                {
                    File.Copy(sourceFile.FullName,
                        Path.Combine(destinationPath, sourceFile.Name), true);
                    PrintToLogAndConsole(logFile, destinationFile.Name, "COPY");
                }
                else if (bDestinationFileIsInSource == false) // remove
                {
                    File.Delete(Path.Combine(destinationPath, destinationFile.Name));
                    PrintToLogAndConsole(logFile, destinationFile.Name, "REMOVE");
                }
            }

            bool bSourceFileIsInDestination = File.Exists(Path.Combine(destinationPath, sourceFile.Name));
            // add
            if (bSourceFileIsInDestination == false)
            {
                File.Copy(sourceFile.FullName,
                        Path.Combine(destinationPath, sourceFile.Name), false);
                PrintToLogAndConsole(logFile, sourceFile.Name, "ADD");
            }
        }

        if (logFile != null)
        {
            logFile.Close();
        }

        Console.WriteLine("Done.");        
    }
}