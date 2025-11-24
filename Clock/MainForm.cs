using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;                    
using Newtonsoft.Json;

namespace Clock
{
    public partial class MainForm : Form
    {
        ColorDialog backgroundDialog;
        ColorDialog foregroundDialog;
        ChooseFont fontDialog;

        // ======== БУДИЛЬНИКИ ========
        private List<Alarm> alarms = new List<Alarm>();
        private readonly string settingsFile = Path.Combine(Application.StartupPath, "alarms.json");
        private System.Media.SoundPlayer soundPlayer = new System.Media.SoundPlayer();
        private ToolStripMenuItem tsmiAlarms; // ссылка на пункт "Будильники"

        public MainForm()
        {
            InitializeComponent();
            SetVisibility(false);
            backgroundDialog = new ColorDialog();
            foregroundDialog = new ColorDialog();
            fontDialog = new ChooseFont();

            // Загружаем будильники и создаём меню
            LoadAlarms();
            CreateAlarmsMenu();
            UpdateAlarmsMenu();

            this.Location = new Point(
                Screen.PrimaryScreen.Bounds.Width - this.labelTime.Width - 150,
                50
            );
        }

                //============= ТАЙМЕР ======================
        private void timer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;

            labelTime.Text = now.ToString("hh:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture);

            if (checkBoxShowDate.Checked)
                labelTime.Text += $"\n{now:yyyy.MM.dd}";
            if (checkBoxShowWeekday.Checked)
                labelTime.Text += $"\n{now.DayOfWeek}";

            CheckAlarms(); // Проверяем будильники каждую секунду
        }

        // ====================== ВИДИМОСТЬ КОНТРОЛОВ ======================
        void SetVisibility(bool visible)
        {
            checkBoxShowDate.Visible = visible;
            checkBoxShowWeekday.Visible = visible;
            buttonHideControls.Visible = visible;
            this.FormBorderStyle = visible ? FormBorderStyle.FixedToolWindow : FormBorderStyle.None;
            this.TransparencyKey = visible ? Color.Empty : this.BackColor;
            this.ShowInTaskbar = visible;
        }

        private void buttonHideControls_Click(object sender, EventArgs e) =>
            SetVisibility(tsmiShowControls.Checked = false);

        private void labelTime_DoubleClick(object sender, EventArgs e) =>
            SetVisibility(tsmiShowControls.Checked = true);

        // ====================== МЕНЮ ======================
        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.TopMost = false;
        }

        private void tsmiQuit_Click(object sender, EventArgs e) => this.Close();

        private void tsmiTopmost_Click(object sender, EventArgs e) =>
            this.TopMost = tsmiTopmost.Checked;

        private void tsmiShowDate_Click(object sender, EventArgs e) =>
            checkBoxShowDate.Checked = tsmiShowDate.Checked;

        private void checkBoxShowDate_CheckedChanged(object sender, EventArgs e) =>
            tsmiShowDate.Checked = checkBoxShowDate.Checked;

        private void tsmiShowWeekday_Click(object sender, EventArgs e) =>
            checkBoxShowWeekday.Checked = tsmiShowWeekday.Checked;

        private void checkBoxShowWeekday_CheckedChanged(object sender, EventArgs e) =>
            tsmiShowWeekday.Checked = (sender as CheckBox).Checked;

        private void tsmiShowControls_Click(object sender, EventArgs e) =>
            SetVisibility(tsmiShowControls.Checked);

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
            bool console = (sender as ToolStripMenuItem).Checked ? AllocConsole() : FreeConsole();
        }

        // ====================== БУДИЛЬНИКИ ======================

        private void CreateAlarmsMenu()
        {
            tsmiAlarms = new ToolStripMenuItem("Будильники");

            var tsmiAdd = new ToolStripMenuItem("Добавить будильник");
            tsmiAdd.Click += (s, e) =>
            {
                using (var form = new AddAlarmForm())
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        alarms.Add(form.Alarm);
                        SaveAlarms();
                        UpdateAlarmsMenu();
                    }
                }
            };

            var tsmiManage = new ToolStripMenuItem("Управление будильниками");
            tsmiManage.Click += (s, e) =>
            {
                if (alarms.Count == 0)
                {
                    MessageBox.Show("Нет будильников для управления.", "Будильники", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var manageForm = new Form
                {
                    Text = "Управление будильниками",
                    Size = new Size(500, 400),
                    StartPosition = FormStartPosition.CenterParent
                };
                var listBox = new ListBox { Dock = DockStyle.Fill };
                var btnEdit = new Button { Text = "Редактировать", Dock = DockStyle.Bottom };
                var btnDelete = new Button { Text = "Удалить", Dock = DockStyle.Bottom };

                foreach (var a in alarms) listBox.Items.Add(a);

                btnEdit.Click += (s2, e2) =>
                {
                    if (listBox.SelectedItem is Alarm alarm)
                    {
                        using (var form = new AddAlarmForm(alarm))
                        {
                            if (form.ShowDialog() == DialogResult.OK)
                            {
                                SaveAlarms();
                                UpdateAlarmsMenu();
                                listBox.Items[listBox.SelectedIndex] = alarm;
                            }
                        }
                    }
                };

                btnDelete.Click += (s2, e2) =>
                {
                    if (listBox.SelectedItem is Alarm alarm)
                    {
                        if (MessageBox.Show($"Удалить будильник «{alarm.Name}»?", "Удаление", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            alarms.Remove(alarm);
                            listBox.Items.Remove(alarm);
                            SaveAlarms();
                            UpdateAlarmsMenu();
                        }
                    }
                };

                manageForm.Controls.Add(listBox);
                manageForm.Controls.Add(btnEdit);
                manageForm.Controls.Add(btnDelete);
                manageForm.ShowDialog();
            };

            tsmiAlarms.DropDownItems.Add(tsmiAdd);
            tsmiAlarms.DropDownItems.Add(tsmiManage);

            // Вставляем перед "Quit"
            contextMenuStrip.Items.Insert(contextMenuStrip.Items.Count - 1, tsmiAlarms);
        }

        private void UpdateAlarmsMenu()
        {
            if (tsmiAlarms == null) return;

            // Удаляем старые динамические пункты
            var toRemove = tsmiAlarms.DropDownItems.OfType<ToolStripMenuItem>()
                .Where(x => x.Tag is Alarm).ToList();
            foreach (var item in toRemove)
                tsmiAlarms.DropDownItems.Remove(item);

            // Добавляем текущие будильники
            foreach (var alarm in alarms)
            {
                var item = new ToolStripMenuItem(alarm.ToString())
                {
                    Tag = alarm,
                    CheckOnClick = true,
                    Checked = alarm.Enabled
                };
                item.Click += (s, e) =>
                {
                    alarm.Enabled = item.Checked;
                    SaveAlarms();
                };
                tsmiAlarms.DropDownItems.Add(item);
            }
        }

        private void CheckAlarms()
        {
            var now = DateTime.Now;
            foreach (var alarm in alarms.Where(a => a.Enabled))
            {
                if (now.Hour == alarm.Time.Hours && now.Minute == alarm.Time.Minutes && now.Second < 3)
                {
                    // Звук
                    if (!string.IsNullOrEmpty(alarm.SoundPath) && File.Exists(alarm.SoundPath))
                    {
                        try
                        {
                            soundPlayer.SoundLocation = alarm.SoundPath;
                            soundPlayer.Play();
                        }
                        catch { }
                    }

                    // Оповещение
                    MessageBox.Show($"Будильник!\n{alarm.Name}\n{alarm.Time:h\\:mm}",
                        "Время пришло!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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

        private void MainForm_Load(object sender, EventArgs e) { }
    }
}