using System.Windows.Forms;

namespace WinFormsApp1321
{
    public partial class Form1 : Form
    {
        private bool isOn = false; // 按钮状态
        private int currentCycle = 0; // 当前循环次数
        private int totalCycles = 0; // 总循环次数
        private CancellationTokenSource cancellationTokenSource; // 控制循环停止
        private bool isCalibrationMode = false;

        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (isOn)  // 当前是开启状态，点击后要关闭
            {
                StopCalibration();  // 关闭自校准模式并取消任务
            }
            else
            {
                // 选择文件窗口
                SelectionForm selectionForm = new SelectionForm();
                selectionForm.ShowDialog();

                if (selectionForm.DialogResult == DialogResult.OK)
                {
                    // 放入样棒框
                    DialogResult result = MessageBox.Show(
                        $"系统文件：C:\\system\\system.ini\n" +
                        $"标样文件：{selectionForm.StandardFilePath}\n" +
                        $"标定循环次数：{selectionForm.CalibrationCount}\n" +
                        $"时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n" +
                        "放入样棒后点击确认？",
                        "放入样棒",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question
                    );

                    // “取消”，就直接返回
                    if (result == DialogResult.Cancel)
                    {
                        MessageBox.Show("操作已取消，自校准模式未开启。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }


                    string selectedStandardFile = selectionForm.StandardFilePath;
                    totalCycles = selectionForm.CalibrationCount;
                    currentCycle = 0;

                    // 确认后，创建 循环任务 并启动循环
                    cancellationTokenSource = new CancellationTokenSource();
                    CancellationToken token = cancellationTokenSource.Token;
                    /*  var token = cancellationTokenSource.Token;*/
                    Task.Run(() => RunCalibrationLoop(token));

                    //状态
                    isOn = true;
                    button1.Text = "自校准模式已开启";
                    label1.Text = "当前状态：自校准模式";
                    button2.Enabled = false;
                }
            }
        }

       
        private void button2_Click(object sender, EventArgs e)
        {



            label1.Text = "当前状态：检测模式";
            isOn = !isOn; // 切换状态
            button2.Text = isOn ? "检测模式已开启" : "检测模式关闭";
            if (isOn == false)
            {
                label1.Text = "当前状态：待机状态";
            }
            //MessageBox.Show("检测模式已关闭！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            button1.Enabled = !isOn;

        }
        private void button3_Click(object sender, EventArgs e)
        {
            // 复位状态
            //isCalibrationMode = false;
            isOn = false;


            label1.Text = "当前状态：待机状态";


            button1.Enabled = true;
            button2.Enabled = true;
            button1.Text = "自校准模式关闭";
            button2.Text = "检测模式关闭";

            MessageBox.Show("系统已恢复为待机状态！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            StopCalibration();
        }

       
        private byte[]? lastReceivedA = null; // 存储上次收到的 A
        private byte[]? lastReceivedB = null; // 存储上次收到的 B

        private async Task RunCalibrationLoop(CancellationToken token)
        {
            TimeSpan dataTimeout = TimeSpan.FromSeconds(20);
            // currentCycle = 0;
            DateTime lastCycleEndTime = DateTime.Now;
            string iniPath = "C:\\system\\system.ini";

            while (currentCycle <= totalCycles)
            {
                if (token.IsCancellationRequested)
                {
                    MessageBox.Show("自校准任务已停止！", "停止", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    StopCalibration();
                    return;
                }

                currentCycle++;
                UpdateCycleLabel();
                DateTime dataStartTime = DateTime.Now;
                // 等待 A 和 B 都收到数据
                while (lastReceivedA == null || lastReceivedB == null)
                {
                    if (DateTime.Now - dataStartTime > dataTimeout)
                    {
                        MessageBox.Show("超时未接收到数据，任务已停止！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        StopCalibration();
                        return;
                    }
                    await Task.Delay(100); 
                }

                // 合并数据
                bool isMatched = TryMergeAndCheckDefects(lastReceivedA, lastReceivedB);

                // 清空数据
                lastReceivedA = null;
                lastReceivedB = null;

                if (!isMatched)
                {
                    MessageBox.Show("检测不合格，出现误检或缺陷未检出！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    StopCalibration();
                    return;
                }

                if (currentCycle > totalCycles)
                {
                    MessageBox.Show("检测完成！所有循环已执行。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    DateTime validUntil = lastCycleEndTime.AddHours(2); // 计算有效期限
                    WriteDeadlineToIni(iniPath, validUntil); // 写入 system.ini
                    UpdateValidUntilLabel(validUntil); // 更新 UI

                    StopCalibration();
                }

                await Task.Delay(10000, token); // 等待 10 秒
            }
        }

        private bool TryMergeAndCheckDefects(byte[]? dataA, byte[]? dataB)
        {
            if (dataA == null || dataB == null || dataA.Length < 6 || dataB.Length < 6)
            {
                MessageBox.Show("接收到的数据格式错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // 误检检查
            if (dataA[0] == 0xA1 || dataB[0] == 0xA1)
            {
                MessageBox.Show("误检发生，检测不合格！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false; // A 或 B 一个误检就立即停止
            }

            int defectCountA = BitConverter.ToInt32(dataA, 2);  // 缺陷数量
            int defectCountB = BitConverter.ToInt32(dataB, 2);  // 缺陷数量

            if (defectCountA != defectCountB)
            {
                MessageBox.Show("A 和 B 缺陷数量不一致，检测不合格！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // 合并 A 和 B 的缺陷位置
            bool[] defectPositions = new bool[defectCountA];  // 假设 A 和 B 的缺陷数量相同

            for (int i = 6; i < dataA.Length; i++)  // 从第7个字节开始是缺陷检测结果
            {
                // 如果 A 或 B 其中一个有缺陷就认为该位置已经检测出来
                defectPositions[i - 6] = (dataA[i] == 0xA0 || dataB[i] == 0xA0);
            }

            // 检查是否所有缺陷都被检测出
            foreach (bool detected in defectPositions)
            {
                if (!detected)
                {
                    MessageBox.Show("缺陷位置未检出，检测不合格！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false; // 如果有位置未检出，则检测不合格
                }
            }

            return true; // 通过检测
        }
        // 处理接收数据
        private void ProcessReceivedData(byte[] receivedData)
        {
            if (receivedData.Length < 2) return;

            if (receivedData[2] == 0xAA) // 涡流 A
            {
                lastReceivedA = receivedData;
            }
            else if (receivedData[2] == 0xBB) // 涡流 B
            {
                lastReceivedB = receivedData;
            }
        }



        private void WriteDeadlineToIni(string iniPath, DateTime deadline)
        {
            try
            {
                List<string> lines = new List<string>();

                if (File.Exists(iniPath))
                {
                    lines = File.ReadAllLines(iniPath).ToList();
                }

                bool found = false;
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].StartsWith("Deadline="))
                    {
                        lines[i] = $"Deadline={deadline:yyyy-MM-dd HH:mm:ss}"; // 直接更新
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    lines.Add($"Deadline={deadline:yyyy-MM-dd HH:mm:ss}"); // 确保一行
                }

                File.WriteAllLines(iniPath, lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"写入系统文件失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void UpdateValidUntilLabel(DateTime validUntil)
        {
            if (label3.InvokeRequired)
            {
                label3.Invoke(new Action(() => UpdateValidUntilLabel(validUntil)));
            }
            else
            {
                label3.Text = $"检测有效期限：{validUntil:yyyy-MM-dd HH:mm:ss}";
            }
        }



        private DateTime ReadDeadlineFromIni(string iniPath)
        {
            try
            {
                if (!File.Exists(iniPath))
                    return DateTime.MinValue;

                string[] lines = File.ReadAllLines(iniPath);
                foreach (string line in lines)
                {
                    if (line.StartsWith("Deadline="))
                    {
                        string deadlineStr = line.Split('=')[1].Trim();
                        if (DateTime.TryParse(deadlineStr, out DateTime deadline))
                        {
                            return deadline;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取系统文件失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return DateTime.MinValue;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            string iniPath = "C:\\system\\system.ini";
            DateTime deadline = ReadDeadlineFromIni(iniPath);
            if (deadline != DateTime.MinValue)
            {
                UpdateValidUntilLabel(deadline);
            }

           Task.Run(() => CheckDeadline()); // 启动检查任务
        }


        private async void CheckDeadline()
        {
            while (true)
            {
                string iniPath = "C:\\system\\system.ini";
                DateTime deadline = ReadDeadlineFromIni(iniPath);
                DateTime now = DateTime.Now;

                if (deadline != DateTime.MinValue)
                {
                    TimeSpan remaining = deadline - now;

                    if (remaining.TotalMinutes <= 60 && remaining.TotalMinutes > 59)
                    {
                        MessageBox.Show("检测有效期即将到期！剩余不到 1 小时。", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else if (remaining.TotalSeconds <= 0)
                    {
                        MessageBox.Show("检测有效期已过期！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        // 使用 Invoke 确保 UI 线程操作
                        if (button2.InvokeRequired)
                        {
                            button2.Invoke(new Action(() => button2.Enabled = false));
                        }
                        else
                        {
                            button2.Enabled = false;
                        }
                    }
                   /* else
                    {
                        // 使用 Invoke 确保 UI 线程操作
                        if (button2.InvokeRequired)
                        {
                            button2.Invoke(new Action(() => button2.Enabled = true));
                        }
                        else
                        {
                           button2.Enabled = true;
                        }
                    }*/
                }

                await Task.Delay(1800000); // 每 30fz检查一次
            }
        }

    



    private void UpdateCycleLabel()
        {
            if (label2.InvokeRequired)
            {
                // 如果在非UI线程，使用Invoke来回到UI线程更新
                label2.Invoke(new Action(UpdateCycleLabel));
            }
            else
            {
                label2.Text = $"当前循环次数：{currentCycle} / {totalCycles}";
            }
        }


        private void StopCalibration()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();  // 取消任务
                cancellationTokenSource.Dispose(); // 释放资源
                cancellationTokenSource = null;
            }

            currentCycle = 0;
            totalCycles = 0;
            isOn = false;

            // 在UI线程上更新
            this.Invoke(new Action(() =>
            {
                button1.Text = "自校准模式关闭";
                label1.Text = "当前状态：待机状态";
                label2.Text = "当前循环次数：0";
                button2.Enabled = true;  // 启用按钮2
            }));
        }


       

        /* private Dictionary<string, int> ReadIniValues(string iniPath, string section)
         {
             Dictionary<string, int> values = new Dictionary<string, int>();

             string[] lines = File.ReadAllLines(iniPath);
             bool inSection = false;

             foreach (string line in lines)
             {
                 string trimmedLine = line.Trim();

                 if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                 {
                     inSection = trimmedLine.Equals($"[{section}]", StringComparison.OrdinalIgnoreCase);
                     continue;
                 }

                 if (inSection && trimmedLine.Contains("="))
                 {
                     string[] parts = trimmedLine.Split('=');
                     if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out int value))
                     {
                         values[parts[0].Trim()] = value;
                     }
                 }
             }

             return values;
         }*/


      /*  private int ReadIniTolerance(string iniPath)
        {
            string[] lines = File.ReadAllLines(iniPath);
            foreach (string line in lines)
            {
                if (line.StartsWith("Value=") && int.TryParse(line.Split('=')[1].Trim(), out int tolerance))
                {
                    return tolerance;
                }
            }
            return 10; // 默认误差±10
        }
*/
        private void toolStripComboBox1_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }



       /* private void Form1_Load(object sender, EventArgs e)
        {

        }*/


        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
