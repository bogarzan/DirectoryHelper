using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DirectoryHelperLibrary
{
    public class DirectoryProcessor
    {
        public string ReportFile{ get; set; }

        private string _rootDirectory = null;
        private IFileProcessor _fileProcessor;

        public DirectoryProcessor(IFileProcessor fileProcessor)
        {
            _fileProcessor = fileProcessor;
        }
        public async Task ProcessDirectoryAsync(string dir, CancellationToken ct = default(CancellationToken))
        {
            await Task.Run(() =>
            {
                ProcessDirectory(dir, _fileProcessor, ct);
            }, ct);
        }
        private void ProcessDirectory(string dir, IFileProcessor fleProcessor, CancellationToken ct)
        {
            _rootDirectory = dir;
            if (!string.IsNullOrEmpty(ReportFile))
            {
                File.Delete(ReportFile);
                SearchDirectoryInner(dir, fleProcessor, ct);
            }
        }

        private void SearchDirectoryInner(string dir , IFileProcessor fleProcessor, CancellationToken ct)
        {
            try
            {
                foreach (string file in Directory.GetFiles(dir))
                {
                    //When "Just My Code" is enabled, Visual Studio in some cases will break on the line that throws the exception 
                    //and display an error message that says "exception not handled by user code." This error is benign.
                    //You can press F5 to continue from it.To prevent Visual Studio from breaking on the first error, just uncheck 
                    //the "Just My Code" checkbox under Tools, Options, Debugging, General.
                    ct.ThrowIfCancellationRequested();
                    SaveReport(fleProcessor.Process(file.Substring(_rootDirectory.Length).TrimStart(Path.DirectorySeparatorChar)));
                }
                foreach (string subDir in Directory.GetDirectories(dir))
                {
                    SearchDirectoryInner(subDir, fleProcessor, ct);
                }
            }
            catch (System.UnauthorizedAccessException excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        private void SaveReport(string report)
        {
            if (!string.IsNullOrEmpty(report))
            {
                File.AppendAllText(ReportFile, report + Environment.NewLine);
                Console.WriteLine(report);
            }
        }
    }

    public class SimpleFileProcessor : IFileProcessor
    {
        public string Process(string file)
        {
            return file;
        }
    }

    public class CsFileProcessor : IFileProcessor
    {
        public string Process(string file)
        {
            if(string.Compare(Path.GetExtension(file), ".cs", true) == 0)
                return file + "/";//add string "/"
            else
                return null;
        }
    }

    public class ReverseFileProcessor1 : IFileProcessor
    {
        public string Process(string file)
        {
            return string.Join(Path.DirectorySeparatorChar.ToString(), file.Split(Path.DirectorySeparatorChar).Reverse());
        }
    }

    public class ReverseFileProcessor2 : IFileProcessor
    {
        public string Process(string file)
        {
            char[] arr = file.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }
    }
}
