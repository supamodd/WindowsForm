namespace Clock
{
    partial class AddAlarmForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.timePicker = new System.Windows.Forms.DateTimePicker();
            this.txtName = new System.Windows.Forms.TextBox();
            this.btnChooseSound = new System.Windows.Forms.Button();
            this.lblSound = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // timePicker
            // 
            this.timePicker.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.timePicker.ShowUpDown = true;
            this.timePicker.Location = new System.Drawing.Point(12, 40);
            this.timePicker.Size = new System.Drawing.Size(200, 23);
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(12, 80);
            this.txtName.Size = new System.Drawing.Size(220, 23);
            this.txtName.Text = "Будильник";
            // 
            // btnChooseSound
            // 
            this.btnChooseSound.Location = new System.Drawing.Point(12, 120);
            this.btnChooseSound.Size = new System.Drawing.Size(220, 35);
            this.btnChooseSound.Text = "Выбрать мелодию";
            this.btnChooseSound.UseVisualStyleBackColor = true;
            this.btnChooseSound.Click += new System.EventHandler(this.btnChooseSound_Click);
            // 
            // lblSound
            // 
            this.lblSound.AutoSize = true;
            this.lblSound.Location = new System.Drawing.Point(12, 170);
            this.lblSound.Size = new System.Drawing.Size(62, 15);
            this.lblSound.Text = "Не выбрано";
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(12, 220);
            this.btnOK.Size = new System.Drawing.Size(100, 40);
            this.btnOK.Text = "ОК";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(132, 220);
            this.btnCancel.Size = new System.Drawing.Size(100, 40);
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // Alarm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 281);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblSound);
            this.Controls.Add(this.btnChooseSound);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.timePicker);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Alarm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Добавить будильник";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.DateTimePicker timePicker;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Button btnChooseSound;
        private System.Windows.Forms.Label lblSound;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}