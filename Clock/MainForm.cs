using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Clock
{
    public partial class MainForm : Form
    {
        ColorDialog backgroundDialog;
        ColorDialog foregroundDialog;
        ChooseFont fontDialog;

                                                              // ======== БУИДИЛЬНИКИ ========
         List<Alarm> alarms = new List<Alarm>();
        string settingsFile = Path.Combine(Application.StartupPath, "alarms.json");
        System.Media.SoundPlayer soundPlayer = new System.Media.SoundPlayer();
        ToolStripMenuItem tsmiAlarms;
        HashSet<Alarm> _triggeredAlarms = new HashSet<Alarm>();

        public MainForm()
        {
            InitializeComponent();

            notifyIcon.Visible = true;
            notifyIcon.Text = "Clock SPU_411";

            SetVisibility(false);
            backgroundDialog = new ColorDialog();
            foregroundDialog = new ColorDialog();
            fontDialog = new ChooseFont();

            LoadAlarms();
            CreateAlarmsMenu();
            UpdateAlarmsMenu();

            this.Location = new Point(
                Screen.PrimaryScreen.Bounds.Width - this.labelTime.Width - 150,
                50
            );
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            var russianCulture = new CultureInfo("ru-RU");

            labelTime.Text = now.ToString("HH:mm:ss", russianCulture);

            if (checkBoxShowDate.Checked)
                labelTime.Text += $"\n{now:dd.MM.yyyy}";
            if (checkBoxShowWeekday.Checked)
                labelTime.Text += $"\n{now.ToString("dddd", russianCulture)}";

            CheckAlarms();
        }

        void SetVisibility(bool visible)
        {
            checkBoxShowDate.Visible = visible;
            checkBoxShowWeekday.Visible = visible;
            buttonHideControls.Visible = visible;
            this.FormBorderStyle = visible ? FormBorderStyle.FixedToolWindow : FormBorderStyle.None;
            this.TransparencyKey = visible ? Color.Empty : this.BackColor;
            this.ShowInTaskbar = visible;
        }

        private void buttonHideControls_Click(object sender, EventArgs e) => SetVisibility(false);
        private void labelTime_DoubleClick(object sender, EventArgs e) => SetVisibility(true);

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.TopMost = false;
        }

        private void tsmiQuit_Click(object sender, EventArgs e) => this.Close();
        private void tsmiTopmost_Click(object sender, EventArgs e) => this.TopMost = tsmiTopmost.Checked;
        private void tsmiShowDate_Click(object sender, EventArgs e) => checkBoxShowDate.Checked = tsmiShowDate.Checked;
        private void checkBoxShowDate_CheckedChanged(object sender, EventArgs e) => tsmiShowDate.Checked = checkBoxShowDate.Checked;
        private void tsmiShowWeekday_Click(object sender, EventArgs e) => checkBoxShowWeekday.Checked = tsmiShowWeekday.Checked;
        private void checkBoxShowWeekday_CheckedChanged(object sender, EventArgs e) => tsmiShowWeekday.Checked = checkBoxShowWeekday.Checked;
        private void tsmiShowControls_Click(object sender, EventArgs e) => SetVisibility(tsmiShowControls.Checked);

        private void tsmiBackgroudColor_Click(object sender, EventArgs e)
        {
            if (backgroundDialog.ShowDialog() == DialogResult.OK)
                labelTime.BackColor = backgroundDialog.Color;
        }

        private void tsmiForegroundColor_Click(object sender, EventArgs e)
        {
            if (foregroundDialog.ShowDialog() == DialogResult.OK)
                labelTime.ForeColor = foregroundDialog.Color;
        }

        private void tsmiChooseFont_Click(object sender, EventArgs e)
        {
            if (fontDialog.ShowDialog() == DialogResult.OK)
                labelTime.Font = fontDialog.Font;
        }

        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();
        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();

        private void tsmiShowConsole_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as ToolStripMenuItem).Checked) AllocConsole();
            else FreeConsole();
        }

                                                     // БУДИЛЬНИКИ 

        private void CreateAlarmsMenu()
        {
            tsmiAlarms = new ToolStripMenuItem("Будильники");

            var addItem = new ToolStripMenuItem("Добавить будильник");
            addItem.Click += (s, e) =>
            {
                using (var f = new AddAlarmForm())
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        alarms.Add(f.Alarm);
                        SaveAlarms();
                        UpdateAlarmsMenu();
                    }
            };

            var manageItem = new ToolStripMenuItem("Управление будильниками");
            manageItem.Click += (s, e) => ShowManageForm();

            tsmiAlarms.DropDownItems.Add(addItem);
            tsmiAlarms.DropDownItems.Add(manageItem);
            contextMenuStrip.Items.Insert(contextMenuStrip.Items.Count - 1, tsmiAlarms);
        }

        private void ShowManageForm()
        {
            if (alarms.Count == 0)
            {
                MessageBox.Show("Нет будильников", "Инфо", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var form = new Form { Text = "Управление будильниками", Size = new Size(500, 400), StartPosition = FormStartPosition.CenterParent };
            var listBox = new ListBox { Dock = DockStyle.Fill };
            var btnEdit = new Button { Text = "Редактировать", Dock = DockStyle.Bottom };
            var btnDelete = new Button { Text = "Удалить", Dock = DockStyle.Bottom };

            foreach (var a in alarms) listBox.Items.Add(a);

            btnEdit.Click += (s, e) =>
            {
                if (listBox.SelectedItem is Alarm alarm)
                {
                    using (var f = new AddAlarmForm(alarm))
                        if (f.ShowDialog() == DialogResult.OK)
                        {
                            SaveAlarms();
                            UpdateAlarmsMenu();
                            listBox.Items[listBox.SelectedIndex] = alarm;
                        }
                }
            };

            btnDelete.Click += (s, e) =>
            {
                if (listBox.SelectedItem is Alarm alarm && MessageBox.Show("Удалить будильник?", "Удаление", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    alarms.Remove(alarm);
                    listBox.Items.Remove(alarm);
                    SaveAlarms();
                    UpdateAlarmsMenu();
                }
            };

            form.Controls.Add(listBox);
            form.Controls.Add(btnEdit);
            form.Controls.Add(btnDelete);
            form.ShowDialog();
        }

        private void UpdateAlarmsMenu()
        {
            if (tsmiAlarms == null) return;

            var oldItems = tsmiAlarms.DropDownItems.OfType<ToolStripMenuItem>().Where(x => x.Tag is Alarm).ToList();
            foreach (var item in oldItems) tsmiAlarms.DropDownItems.Remove(item);

            foreach (var alarm in alarms)
            {
                var item = new ToolStripMenuItem(alarm.ToString())
                {
                    Tag = alarm,
                    CheckOnClick = true,
                    Checked = alarm.Enabled
                };
                item.Click += (s, e) => { alarm.Enabled = item.Checked; SaveAlarms(); };
                tsmiAlarms.DropDownItems.Add(item);
            }
        }

        private void CheckAlarms()
        {
            var now = DateTime.Now;
            foreach (var alarm in alarms.Where(a => a.Enabled))
            {  
                             // чтобы срабатывал 1 раз
                if (now.Hour == alarm.Time.Hours &&
                    now.Minute == alarm.Time.Minutes &&
                    now.Second == 0 &&
                    !_triggeredAlarms.Contains(alarm))
                {
                    _triggeredAlarms.Add(alarm);

                    Task.Run(() =>
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(alarm.SoundPath) && File.Exists(alarm.SoundPath))
                            {
                                if (alarm.SoundPath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                                {
                                    var player = new System.Media.SoundPlayer(alarm.SoundPath);
                                    player.PlaySync();
                                    player.Dispose();
                                }
                                else
                                {
                                    var psi = new ProcessStartInfo
                                    {
                                        FileName = alarm.SoundPath,
                                        UseShellExecute = true,
                                        CreateNoWindow = true,
                                        ErrorDialog = false
                                    };
                                    Process.Start(psi);
                                }
                            }
                        }
                        catch { }
                    });

                                                             //  Уведомление 
                    string timeText = alarm.Time.ToString(@"hh\:mm");

                    notifyIcon.ShowBalloonTip(10000, "Будильник!", $"{alarm.Name} — {timeText}", ToolTipIcon.Info);

                    if (this.WindowState == FormWindowState.Minimized)
                        this.WindowState = FormWindowState.Normal;

                    this.Show();
                    this.BringToFront();
                    this.Activate();

                    MessageBox.Show($"Будильник!\n{alarm.Name}\n{timeText}",
                        "Время пришло!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                                             // скидывает флажок после срабаттыванияч 
                if (now.Hour != alarm.Time.Hours || now.Minute != alarm.Time.Minutes)
                {
                    _triggeredAlarms.Remove(alarm);
                }
            }
        }

        private void LoadAlarms()
        {
            try
            {
                if (File.Exists(settingsFile))
                {
                    string json = File.ReadAllText(settingsFile);
                    alarms = JsonConvert.DeserializeObject<List<Alarm>>(json) ?? new List<Alarm>();
                }
            }
            catch { alarms = new List<Alarm>(); }
        }

        private void SaveAlarms()
        {
            try
            {
                string json = JsonConvert.SerializeObject(alarms, Formatting.Indented);
                File.WriteAllText(settingsFile, json);
            }
            catch { }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveAlarms();
            base.OnFormClosing(e);
        }
    }
}