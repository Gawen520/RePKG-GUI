using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using Newtonsoft.Json;


namespace RePKG_WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        static readonly string Temp_file = Environment.GetEnvironmentVariable("TMP");
        static readonly string Desktop_file = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        static readonly string OutputRootDir = "小红车壁纸解包目录";
        static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        static string OutputRootPath = LoadOutputPath();
        static readonly ArrayList Number_file = new ArrayList();
        static string RePKG_Directory = "";//RePKG.exe 所在目录
        static readonly List<string> UserCmdList = new List<string>();//用户选择的命令
        static readonly string UserCmd1 = " --osi -n";
        static readonly string UserCmd2 = " --no-tex-convert";

        /// <summary>
        /// 从配置文件加载输出路径
        /// </summary>
        private static string LoadOutputPath()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var configContent = File.ReadAllText(ConfigPath);
                    var config = JsonConvert.DeserializeObject<Dictionary<string, string>>(configContent);
                    if (config != null && config.ContainsKey("OutputPath"))
                    {
                        return config["OutputPath"];
                    }
                }
            }
            catch
            {
                // 如果读取失败，使用默认路径
            }

            // 默认输出到桌面
            return Path.Combine(Desktop_file, OutputRootDir);
        }

        /// <summary>
        /// 从配置文件加载复选框状态
        /// </summary>
        private void LoadCheckboxStates()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var configContent = File.ReadAllText(ConfigPath);
                    var config = JsonConvert.DeserializeObject<Dictionary<string, string>>(configContent);
                    if (config != null)
                    {
                        if (config.ContainsKey("NoTexConvert") && bool.TryParse(config["NoTexConvert"], out bool noTexConvert))
                        {
                            chkNoTexConvert.IsChecked = noTexConvert;
                        }
                        if (config.ContainsKey("OnlyImages") && bool.TryParse(config["OnlyImages"], out bool onlyImages))
                        {
                            chkOnlyImages.IsChecked = onlyImages;
                        }
                    }
                }
            }
            catch
            {
                // 如果读取失败，保持默认状态
            }
        }

        /// <summary>
        /// 保存输出路径到配置文件
        /// </summary>
        private static void SaveOutputPath(string path)
        {
            try
            {
                var config = new Dictionary<string, string>
                {
                    { "OutputPath", path }
                };
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
            }
            catch
            {
                // 如果保存失败，不做处理
            }
        }

        /// <summary>
        /// 保存复选框状态到配置文件
        /// </summary>
        private void SaveCheckboxStates()
        {
            try
            {
                var config = new Dictionary<string, string>
                {
                    { "OutputPath", OutputRootPath },
                    { "NoTexConvert", chkNoTexConvert.IsChecked.ToString() },
                    { "OnlyImages", chkOnlyImages.IsChecked.ToString() }
                };
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
            }
            catch
            {
                // 如果保存失败，不做处理
            }
        }

        /// <summary>
        /// 添加日志并自动滚动到底部
        /// </summary>
        private void AppendLog(string message)
        {
            tb_log.Text += message;
            scroll_log.ScrollToEnd();
        }

        public MainWindow()
        {
            InitializeComponent();
            RePKG_Directory = AppDomain.CurrentDomain.BaseDirectory;
            AppendLog($"\n[{DateTime.Now}]: RePKG 目录：{RePKG_Directory}");
            AppendLog($"\n[{DateTime.Now}]: 获取临时文件夹：{Temp_file}");
            AppendLog($"\n[{DateTime.Now}]: 获取桌面路径：{Desktop_file}");
            AppendLog($"\n[{DateTime.Now}]: 加载输出路径：{OutputRootPath}");

            // 加载复选框状态
            LoadCheckboxStates();
            AppendLog($"\n[{DateTime.Now}]: 加载复选框状态完成");

            if (!Related_functions.Release_file.CheckRePKG(RePKG_Directory))
            {
                AppendLog($"\n[{DateTime.Now}]: 警告：未找到 RePKG.exe，请确保该文件与程序在同一目录");
                System.Windows.MessageBox.Show("未找到 RePKG.exe！\n请确保 RePKG.exe 与程序在同一目录下。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            AppendLog($"\n[{DateTime.Now}]: 程序准备就绪...");
            tb_dir.Text = OutputRootPath;
        }
        //添加文件
        private void Select_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "PKG 文件 (*.PKG)|*.pkg",
                Multiselect = true
            };
            if (ofd.ShowDialog(this) == true)
            {
                foreach (string file in ofd.FileNames)
                {
                    if (!Number_file.Contains(file))//排除同类项
                    {
                        Number_file.Add(file);
                        lb_list.Items.Add(file);
                        AppendLog($"[{DateTime.Now}]: 已添加文件：{file}");
                    }
                    else
                    {
                        AppendLog($"[{DateTime.Now}]: 添加失败！原因：已存在此文件");
                    }
                }
            }
            expander_file.Header = $"已选择{Number_file.Count}个对象";
        }
        //清空文件列表
        private void ClearFile_Click(object sender, RoutedEventArgs e)
        {
            Number_file.Clear();
            expander_file.Header = "已选择 0 个对象";
            lb_list.Items.Clear();
            AppendLog($"\n[{DateTime.Now}]: 清除所选文件列表");
        }
        //清除日志文件
        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            tb_log.Text = null;
            tb_log.Text = $"[{DateTime.Now}]: 已清空日志...";
        }
        //选择输出目录
        private void Dir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new CommonOpenFileDialog
                {
                    IsFolderPicker = true
                };
                CommonFileDialogResult result = dialog.ShowDialog();
                if (dialog.FileName != "")
                {
                    OutputRootPath = Path.Combine(dialog.FileName, OutputRootDir);
                    tb_dir.Text = OutputRootPath;
                    AppendLog($"\n[{DateTime.Now}]: 重新定向输出文件夹：{OutputRootPath}");

                    // 保存新的输出路径到配置文件
                    SaveOutputPath(OutputRootPath);
                    AppendLog($"\n[{DateTime.Now}]: 已保存输出路径到配置文件");
                }
            }
            catch
            {
                // 用户取消选择文件夹，不做任何处理
            }
        }

        // 复选框状态改变时保存配置
        private void ChkNoTexConvert_Checked(object sender, RoutedEventArgs e)
        {
            SaveCheckboxStates();
            AppendLog($"\n[{DateTime.Now}]: 已保存复选框状态：不转换图片={(chkNoTexConvert.IsChecked ?? false)}");
        }

        private void ChkOnlyImages_Checked(object sender, RoutedEventArgs e)
        {
            SaveCheckboxStates();
            AppendLog($"\n[{DateTime.Now}]: 已保存复选框状态：只提取图片={(chkOnlyImages.IsChecked ?? false)}");
        }
        //开始输出
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (Number_file.Count == 0)
            {
                AppendLog($"\n[{DateTime.Now}]: 您似乎木有选择要解压的 PKG 文件 w(ﾟДﾟ) w");
            }
            else
            {
                if (!Related_functions.Release_file.CheckRePKG(RePKG_Directory))
                {
                    AppendLog($"\n[{DateTime.Now}]: 错误：找不到 RePKG.exe");
                    System.Windows.MessageBox.Show("找不到 RePKG.exe！\n无法继续执行解压操作。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // AppendLog($"\n[{DateTime.Now}]: 检查 RePKG.exe 文件...");

                // 确保输出目录存在
                if (!Directory.Exists(OutputRootPath))
                {
                    try
                    {
                        //tb_log.Text += $"\n[{DateTime.Now}]: 输出目录不存在，正在创建：{OutputRootPath}";
                        Directory.CreateDirectory(OutputRootPath);
                        //tb_log.Text += $"\n[{DateTime.Now}]: ✓ 输出目录创建成功";
                    }
                    catch (Exception ex)
                    {
                        AppendLog($"\n[{DateTime.Now}]: ✗ 错误！无法创建输出目录：{ex.Message}");
                        System.Windows.MessageBox.Show("无法创建输出目录！\n" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                UserCmdList.Remove(UserCmd1);
                if (chkOnlyImages.IsChecked == true)
                {
                    UserCmdList.Add(UserCmd1);
                }

                UserCmdList.Remove(UserCmd2);
                if (chkNoTexConvert.IsChecked == true)
                {
                    UserCmdList.Add(UserCmd2);
                }

                using (BackgroundWorker bw = new BackgroundWorker())
                {
                    bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Bw_RunWorkerCompleted);
                    bw.DoWork += new DoWorkEventHandler(Bw_DoWork2);//建立后台
                    bw.RunWorkerAsync();//开始执行
                }
            }
        }
        //后台执行解压
        void Bw_DoWork2(object sender, DoWorkEventArgs e)//后台
        {
            string repkgPath = Related_functions.Release_file.GetRePKGPath(RePKG_Directory);

            // 验证 RePKG.exe 是否存在
            if (!File.Exists(repkgPath))
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    AppendLog($"\n[{DateTime.Now}]: 错误！找不到 RePKG.exe: {repkgPath}");
                }));
                return;
            }

            for (int i = 0; i < Number_file.Count; i++)
            {
                string filePath = Number_file[i].ToString();
                // 使用 Path.Combine 确保路径格式正确
                string outputPath = Path.Combine(OutputRootPath, new FileInfo(filePath).Directory.Name, Path.GetFileNameWithoutExtension(filePath));
                if (UserCmdList.Contains(UserCmd1))
                {
                    outputPath = OutputRootPath;
                }
                // 确保输出目录存在
                try
                {
                    if (!Directory.Exists(outputPath))
                    {
                        Directory.CreateDirectory(outputPath);
                        //Dispatcher.Invoke(new Action(delegate
                        //{
                        //    AppendLog($"\n[{DateTime.Now}]: 创建文件输出目录：{outputPath}");
                        //}));
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        AppendLog($"\n[{DateTime.Now}]: 错误！无法创建输出目录：{ex.Message}");
                        AppendLog($"\n[{DateTime.Now}]: 完整错误：{ex}");
                    }));
                    return;
                }
                // 执行命令 - 使用 RunExe 直接调用 RePKG.exe
                string command = $"extract {string.Join("", UserCmdList)} \"{filePath}\" -o \"{outputPath}\"";
                Dispatcher.Invoke(new Action(delegate
                {
                    AppendLog("\n" + new string('-', 80));
                    AppendLog($"\n[{DateTime.Now}]: 文件({i + 1}/{Number_file.Count})：{filePath}");
                    AppendLog($"\n[{DateTime.Now}]: 文件({i + 1}/{Number_file.Count})：{filePath}");
                    AppendLog($"\n[{DateTime.Now}]: 输出路径：{outputPath}");
                    AppendLog($"\n[{DateTime.Now}]: 执行命令：\"{repkgPath}\" {command}");
                }));

                string result = Related_functions.CMD.RunExe(repkgPath, command);

                Dispatcher.Invoke(new Action(delegate
                {
                    AppendLog($"\n[{DateTime.Now}]: 命令执行完成");
                    if (!string.IsNullOrEmpty(result))
                    {
                        AppendLog("\n--- RePKG 输出开始 ---");
                        AppendLog(result);
                        AppendLog("\n--- RePKG 输出结束 ---");

                        // 检查是否真的解压成功
                        bool hasSuccessIndicator = result.Contains("Done") || result.Contains("extracting") || result.Contains("提取") || Directory.Exists(outputPath);
                        bool hasErrorIndicator = result.Contains("错误") || result.Contains("Error") || result.Contains("不正确") || result.Contains("失败") || result.Contains("[错误输出]");

                        if (hasSuccessIndicator && !hasErrorIndicator)
                        {
                            AppendLog($"\n[{DateTime.Now}]: ✓ 解压成功");
                        }
                        else if (hasErrorIndicator)
                        {
                            AppendLog($"\n[{DateTime.Now}]: ✗ 解压失败，请检查错误信息");
                        }
                        else if (!Directory.Exists(outputPath))
                        {
                            AppendLog($"\n[{DateTime.Now}]: ⚠ 警告：输出目录不存在，可能解压失败");
                        }
                        else
                        {
                            AppendLog($"\n[{DateTime.Now}]: ? 解压状态未知，请检查输出");
                        }
                    }
                    else
                    {
                        AppendLog($"\n[{DateTime.Now}]: 警告：RePKG 没有输出任何内容");
                    }

                    // 验证输出目录
                    if (Directory.Exists(outputPath))
                    {
                        var files = Directory.GetFiles(outputPath, "*.*", SearchOption.AllDirectories);
                        AppendLog($"\n[{DateTime.Now}]: 输出目录包含 {files.Length} 个文件");
                        if (files.Length == 0)
                        {
                            AppendLog($"\n[{DateTime.Now}]: ⚠ 警告：输出目录为空，解压可能未成功");
                        }
                    }
                    else
                    {
                        AppendLog($"\n[{DateTime.Now}]: ⚠ 警告：输出目录不存在，解压可能失败");
                    }
                }));
            }
        }
        //完成后返回
        void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            // 删除输出目录中的所有空文件夹
            try
            {
                DeleteEmptyDirectories(OutputRootPath);
            }
            catch (Exception ex)
            {
                tb_log.Text += $"\n[{DateTime.Now}]: ⚠ 警告：清理空文件夹失败 - {ex.Message}";
            }

            tb_log.Text += $"\n[{DateTime.Now}]: 全部解压完毕！";


            Number_file.Clear();
            lb_list.Items.Clear();
            expander_file.Header = $"已选择 0 个对象";
        }

        /// <summary>
        /// 递归删除目录中的所有空文件夹
        /// </summary>
        /// <param name="rootPath">根目录路径</param>
        /// <returns>删除的空文件夹数量</returns>
        private int DeleteEmptyDirectories(string rootPath)
        {
            if (!Directory.Exists(rootPath))
            {
                return 0;
            }

            int count = 0;
            var directories = Directory.GetDirectories(rootPath);

            // 先递归处理子目录
            foreach (var dir in directories)
            {
                count += DeleteEmptyDirectories(dir);
            }

            // 再检查并删除当前目录（如果是空的）
            try
            {
                var files = Directory.GetFiles(rootPath);
                var subDirs = Directory.GetDirectories(rootPath);

                // 如果没有文件且没有子目录，则为空目录
                if (files.Length == 0 && subDirs.Length == 0)
                {
                    Directory.Delete(rootPath);
                    count++;
                }
            }
            catch
            {
                // 忽略删除失败的目录
            }

            return count;
        }

        private void OpenDir_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(OutputRootPath))//判断输出目录是否存在
            {
                AppendLog($"\n[{DateTime.Now}]: 输出目录不存在，重新创建目录");
                Directory.CreateDirectory(OutputRootPath);//创建新路径
            }
            Related_functions.CMD.RunCmd($"explorer {OutputRootPath}");
        }

        private void Main_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in files)
            {
                if (file.ToLower().EndsWith(".pkg"))
                {
                    if (!Number_file.Contains(file))
                    {
                        Number_file.Add(file);
                        lb_list.Items.Add(file);
                        AppendLog($"\n[{DateTime.Now}]: 已添加文件：{file}");
                    }
                    else
                    {
                        AppendLog($"\n[{DateTime.Now}]: 重复文件：{file}");
                    }
                }
                else if (Directory.Exists(file))
                {
                    var pkgFiles = Directory.GetFiles(file, "*.pkg", SearchOption.AllDirectories);
                    if (pkgFiles.Length == 0)
                    {
                        AppendLog($"\n[{DateTime.Now}]: 目录中未找到 PKG 文件：{file}");
                    }
                    else
                    {
                        foreach (var pkgFile in pkgFiles)
                        {
                            if (!Number_file.Contains(pkgFile))
                            {
                                Number_file.Add(pkgFile);
                                lb_list.Items.Add(pkgFile);
                                AppendLog($"\n[{DateTime.Now}]: 已添加文件：{pkgFile}");
                            }
                            else
                            {
                                AppendLog($"\n[{DateTime.Now}]: 重复文件：{pkgFile}");
                            }
                        }
                    }
                }
                else
                {
                    AppendLog($"\n[{DateTime.Now}]: 无效文件：{file}");
                }
            }
            expander_file.Header = $"已选择{Number_file.Count}个对象";
        }

        private void Main_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
