using System.IO;

namespace RePKG_WPF.Related_functions
{
    class Release_file
    {
        /// <summary>
        /// 检查 RePKG.exe 是否存在于指定目录
        /// </summary>
        /// <param name="directory">RePKG.exe 所在目录</param>
        /// <returns>文件是否存在</returns>
        public static bool CheckRePKG(string directory)
        {
            string repkgPath = Path.Combine(directory, "RePKG.exe");
            return File.Exists(repkgPath);
        }

        /// <summary>
        /// 获取 RePKG.exe 的完整路径
        /// </summary>
        /// <param name="directory">RePKG.exe 所在目录</param>
        /// <returns>RePKG.exe 的完整路径</returns>
        public static string GetRePKGPath(string directory)
        {
            return Path.Combine(directory, "RePKG.exe");
        }
    }
}
