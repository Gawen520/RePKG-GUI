using System.Diagnostics;

namespace RePKG_WPF.Related_functions
{
    class CMD
    {
        /// <summary>
        /// 执行命令行
        /// </summary>
        /// <param name="command">命令字符串</param>
        /// <returns>返回执行结果（包含标准输出和标准错误）</returns>
        public static string RunCmd(string command)
        {
            using (Process p = new Process())
            {
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = "/c " + command;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                p.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
                p.Start();

                string output = p.StandardOutput.ReadToEnd();
                string error = p.StandardError.ReadToEnd();
                p.WaitForExit();

                return FormatResult(output, error, p.ExitCode);
            }
        }

        /// <summary>
        /// 直接执行可执行文件（不通过 cmd.exe）
        /// </summary>
        /// <param name="fileName">可执行文件路径</param>
        /// <param name="arguments">参数</param>
        /// <returns>执行结果</returns>
        public static string RunExe(string fileName, string arguments)
        {
            using (Process p = new Process())
            {
                p.StartInfo.FileName = fileName;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                p.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
                p.Start();

                string output = p.StandardOutput.ReadToEnd();
                string error = p.StandardError.ReadToEnd();
                p.WaitForExit();

                return FormatResult(output, error, p.ExitCode);
            }
        }

        /// <summary>
        /// 格式化进程执行结果
        /// </summary>
        private static string FormatResult(string output, string error, int exitCode)
        {
            if (exitCode != 0 && !string.IsNullOrEmpty(error))
            {
                return $"{output}\n[错误输出]:\n{error}\n[退出码]: {exitCode}";
            }
            else if (!string.IsNullOrEmpty(error))
            {
                return $"{output}\n[错误输出]:\n{error}";
            }
            return output;
        }
    }
}
