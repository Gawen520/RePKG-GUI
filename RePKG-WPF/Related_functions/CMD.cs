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
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";         //确定程序名
            p.StartInfo.Arguments = "/c " + command;   //确定程式命令行
            p.StartInfo.UseShellExecute = false;      //Shell 的使用
            p.StartInfo.RedirectStandardInput = false;  //不重定向输入
            p.StartInfo.RedirectStandardOutput = true; //重定向输出
            p.StartInfo.RedirectStandardError = true;  //重定向输出错误
            p.StartInfo.CreateNoWindow = true;        //设置不显示窗口
            p.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8; //设置 UTF-8 编码
            p.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
            p.Start();

            // 先读取标准输出
            string output = p.StandardOutput.ReadToEnd();
            // 再读取标准错误
            string error = p.StandardError.ReadToEnd();

            // 等待进程完全退出
            p.WaitForExit();

            int exitCode = p.ExitCode; // 获取退出码

            // 关闭进程
            p.Close();
            p.Dispose();

            // 合并标准输出和标准错误输出
            if (exitCode != 0 && !string.IsNullOrEmpty(error))
            {
                return output + "\n[错误输出]:\n" + error + "\n[退出码]: " + exitCode;
            }
            else if (!string.IsNullOrEmpty(error))
            {
                return output + "\n[错误输出]:\n" + error;
            }
            return output;
        }

        /// <summary>
        /// 直接执行可执行文件（不通过 cmd.exe）
        /// </summary>
        /// <param name="fileName">可执行文件路径</param>
        /// <param name="arguments">参数</param>
        /// <returns>执行结果</returns>
        public static string RunExe(string fileName, string arguments)
        {
            Process p = new Process();
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
            int exitCode = p.ExitCode;
            p.Close();
            p.Dispose();

            if (exitCode != 0 && !string.IsNullOrEmpty(error))
            {
                return output + "\n[错误输出]:\n" + error + "\n[退出码]: " + exitCode;
            }
            else if (!string.IsNullOrEmpty(error))
            {
                return output + "\n[错误输出]:\n" + error;
            }
            return output;
        }
    }
}
