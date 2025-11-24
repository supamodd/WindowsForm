using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Clock
{
    public partial class AddAlarmForm : Form
    {
        // Это объект-данные будильника, который мы будем возвращать
        public Alarm Alarm { get; private set; }

        public AddAlarmForm(Alarm existing = null)
        {
            InitializeComponent();

            // Если редактируем существующий будильник — копируем данные
            Alarm = existing ?? new Alarm();

            timePicker.Value = DateTime.Today.Add(Alarm.Time);
            txtName.Text = Alarm.Name;
            UpdateSoundLabel();
        }

        private void UpdateSoundLabel()
        {
            lblSound.Text = string.IsNullOrEmpty(Alarm.SoundPath)
                ? "Не выбрано"
                : Path.GetFileName(Alarm.SoundPath);
        }

        // ←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←
        // ЭТОТ МЕТОД БЫЛ НУЖЕН ДИЗАЙНЕРУ!
        private void btnChooseSound_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Звуковые файлы (*.wav;*.mp3)|*.wav;*.mp3|Все файлы (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    Alarm.SoundPath = ofd.FileName;
                    UpdateSoundLabel();
                }
            }
        }

        // ←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←
        // И ЭТОТ ТОЖЕ!
        private void btnOK_Click(object sender, EventArgs e)
        {
            Alarm.Time = timePicker.Value.TimeOfDay;
            Alarm.Name = string.IsNullOrWhiteSpace(txtName.Text) ? "Будильник" : txtName.Text.Trim();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}