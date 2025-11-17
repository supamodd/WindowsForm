using Clock.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Clock
{
    public partial class MainForm : Form
    {
        // === Константы для автозапуска ===
        private const string StartupRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "DigitalClock";

        public MainForm()
        {
            InitializeComponent();

            // === Привязка контекстного меню ===
            labelTime.ContextMenuStrip = contextMenuStrip;
            notifyIcon.ContextMenuStrip = contextMenuStrip;

            // === Дополнительно: принудительное открытие по ПКМ ===
            labelTime.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                    contextMenuStrip.Show(labelTime, e.Location);
            };

            // === Подписки на новые пункты меню ===
            tsmiForegroundColor.Click += tsmiForegroundColor_Click;
            tsmiBackgroundColor.Click += tsmiBackgroundColor_Click;
            tsmiChooseFont.Click += tsmiChooseFont_Click;
            tsmiRunOnStartup.Click += tsmiRunOnStartup_Click;

            // === Загружаем настройки и позиционируем ===
            LoadSettings();
            StartPosition = FormStartPosition.Manual;
            PositionInTopRight();
            timer.Start();
        }

        // === Обновление времени ===
        private void timer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            labelTime.Text = now.ToString("hh:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture);

            if (checkBoxShowDate.Checked)
                labelTime.Text += $"\n{now:yyyy.MM.dd}";
            if (checkBoxShowWeekday.Checked)
                labelTime.Text += $"\n{now:dddd}"; // Monday, Tuesday...

            // Применяем только шрифт и цвет текста
            labelTime.Font = Settings.Default.CustomFont;
            labelTime.ForeColor = Settings.Default.ForeColor;
        }

        // === Позиция в правом верхнем углу ===
        private void PositionInTopRight()
        {
            var area = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(area.Right - this.Width - 10, area.Top + 10);
        }

        // === Управление видимостью элементов ===
        private void SetVisibility(bool visible)
        {
            checkBoxShowDate.Visible = visible;
            checkBoxShowWeekday.Visible = visible;
            buttonHideControls.Visible = visible;
            this.FormBorderStyle = visible ? FormBorderStyle.FixedToolWindow : FormBorderStyle.None;

            // TransparencyKey использует уже загруженный BackColor
            this.TransparencyKey = visible ? Color.Empty : this.BackColor;

            ShowInTaskbar = visible;
            Settings.Default.ShowControls = visible;
        }

        // === Обработчики кнопок и чекбоксов ===
        private void buttonHideControls_Click(object sender, EventArgs e) =>
            SetVisibility(tsmiShowControls.Checked = false);

        private void labelTime_DoubleClick(object sender, EventArgs e) =>
            SetVisibility(tsmiShowControls.Checked = true);

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.TopMost = false;
        }

        private void tsmiQuit_Click(object sender, EventArgs e)
        {
            SaveSettings();
            Application.Exit();
        }

        private void tsmiTopmost_Click(object sender, EventArgs e) =>
            this.TopMost = tsmiTopmost.Checked;

        private void tsmiShowDate_Click(object sender, EventArgs e) =>
            checkBoxShowDate.Checked = tsmiShowDate.Checked;

        private void checkBoxShowDate_CheckedChanged(object sender, EventArgs e) =>
            tsmiShowDate.Checked = checkBoxShowDate.Checked;

        private void tsmiShowWeekday_Click(object sender, EventArgs e) =>
            checkBoxShowWeekday.Checked = tsmiShowWeekday.Checked;

        private void checkBoxShowWeekday_CheckedChanged(object sender, EventArgs e) =>
            tsmiShowWeekday.Checked = checkBoxShowWeekday.Checked;

        private void tsmiShowControls_Click(object sender, EventArgs e) =>
            SetVisibility(tsmiShowControls.Checked);

        // === 1. Цвет текста ===
        private void tsmiForegroundColor_Click(object sender, EventArgs e)
        {
            using (var dlg = new ColorDialog { Color = Settings.Default.ForeColor })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Settings.Default.ForeColor = dlg.Color;
                    labelTime.ForeColor = dlg.Color;
                }
            }
        }

        // === 1. Цвет фона ===
        private void tsmiBackgroundColor_Click(object sender, EventArgs e)
        {
            using (var dlg = new ColorDialog { Color = Settings.Default.BackColor })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Settings.Default.BackColor = dlg.Color;
                    this.BackColor = dlg.Color;
                    this.TransparencyKey = tsmiShowControls.Checked ? Color.Empty : dlg.Color;
                }
            }
        }

        // === 3. Выбор шрифта ===
        private void tsmiChooseFont_Click(object sender, EventArgs e)
        {
            using (var dlg = new FontDialog { Font = Settings.Default.CustomFont })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Settings.Default.CustomFont = dlg.Font;
                    labelTime.Font = dlg.Font;
                }
            }
        }

        // === 4. Автозапуск при старте Windows ===
        private void tsmiRunOnStartup_Click(object sender, EventArgs e)
        {
            bool enable = tsmiRunOnStartup.Checked;
            using (var key = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, true))
            {
                if (enable)
                    key?.SetValue(AppName, $"\"{Application.ExecutablePath}\"");
                else
                    key?.DeleteValue(AppName, false);
            }
            Settings.Default.RunOnStartup = enable;
        }

        // === 5. Сохранение настроек ===
        private void SaveSettings()
        {
            var s = Settings.Default;
            s.Topmost = this.TopMost;
            s.ShowDate = checkBoxShowDate.Checked;
            s.ShowWeekday = checkBoxShowWeekday.Checked;
            s.ShowControls = tsmiShowControls.Checked;
            s.WindowLocation = this.Location;
            s.RunOnStartup = tsmiRunOnStartup.Checked;
            s.Save();
        }

        // === 5. Загрузка настроек (ИСПРАВЛЕН ПОРЯДОК!) ===
        private void LoadSettings()
        {
            var s = Settings.Default;

            // 1. Сначала цвета и шрифт (чтобы TransparencyKey был правильным)
            labelTime.ForeColor = s.ForeColor;
            this.BackColor = s.BackColor;
            labelTime.Font = s.CustomFont;

            // 2. Позиция
            if (s.WindowLocation != Point.Empty)
                this.Location = s.WindowLocation;
            else
                PositionInTopRight();

            // 3. Видимость (уже с правильным BackColor!)
            tsmiShowControls.Checked = s.ShowControls;
            SetVisibility(s.ShowControls);

            // 4. Остальные настройки
            this.TopMost = s.Topmost; tsmiTopmost.Checked = s.Topmost;
            checkBoxShowDate.Checked = s.ShowDate; tsmiShowDate.Checked = s.ShowDate;
            checkBoxShowWeekday.Checked = s.ShowWeekday; tsmiShowWeekday.Checked = s.ShowWeekday;
            tsmiRunOnStartup.Checked = s.RunOnStartup; UpdateStartupRegistry();
        }

        // === Синхронизация автозапуска с реестром ===
        private void UpdateStartupRegistry()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, true))
            {
                bool exists = key?.GetValue(AppName) != null;
                if (tsmiRunOnStartup.Checked != exists)
                {
                    if (tsmiRunOnStartup.Checked)
                        key?.SetValue(AppName, $"\"{Application.ExecutablePath}\"");
                    else
                        key?.DeleteValue(AppName, false);
                }
            }
        }

        // === Сохранение при закрытии ===
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveSettings();
            base.OnFormClosing(e);
        }
    }
}