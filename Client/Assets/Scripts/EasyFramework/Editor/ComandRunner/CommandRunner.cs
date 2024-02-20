namespace Easy
{

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using UnityEngine;

    public class CommandRunner
    {
        public string ExecutablePath { get; }
        public string WorkingDirectory { get; }

        public bool CreateNoWindow { get; }
        public bool RedirectStandardOutput { get;} 
        public bool UseShellExecute {get; }

        private Process process;

        public CommandRunner(string executablePath, string workingDirectory,bool createNoWindow,bool redirectStandardOutput,bool useShellExecute)
        {
            ExecutablePath = executablePath ?? throw new ArgumentNullException(nameof(executablePath));
            WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(executablePath);
            CreateNoWindow = createNoWindow;
            RedirectStandardOutput = redirectStandardOutput;
            UseShellExecute = useShellExecute;
        }

        public string Run(string arguments)
        {
            var info = new ProcessStartInfo(ExecutablePath, arguments)
            {
                CreateNoWindow = CreateNoWindow,
                RedirectStandardOutput = RedirectStandardOutput,
                UseShellExecute = UseShellExecute,
                WorkingDirectory = WorkingDirectory,
            };
            process = new Process
            {
                StartInfo = info,
            };
            process.Start();
            return process.StandardOutput.ReadToEnd();
        }

        public void Run(string arguments, DataReceivedEventHandler handler)
        {
            var info = new ProcessStartInfo(ExecutablePath, arguments)
            {
                CreateNoWindow = CreateNoWindow,
                RedirectStandardOutput = RedirectStandardOutput,
                UseShellExecute = UseShellExecute,
                WorkingDirectory = WorkingDirectory,
            };
            var process = new Process
            {
                StartInfo = info,
            };
            process.Start();
            process.BeginOutputReadLine();
            process.OutputDataReceived += handler;
        }

        public void Close()
        {
            process?.Close();
        }
    }

}