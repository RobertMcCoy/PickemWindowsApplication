namespace PickemTest
{
    partial class Settings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.steamCommunityIdBox = new System.Windows.Forms.TextBox();
            this.authCodeBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.selectedEventLbl = new System.Windows.Forms.Label();
            this.saveBtn = new System.Windows.Forms.Button();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.steamIdBox = new System.Windows.Forms.TextBox();
            this.validateBtn = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.steamName = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // steamCommunityIdBox
            // 
            this.steamCommunityIdBox.Location = new System.Drawing.Point(145, 24);
            this.steamCommunityIdBox.Name = "steamCommunityIdBox";
            this.steamCommunityIdBox.Size = new System.Drawing.Size(221, 20);
            this.steamCommunityIdBox.TabIndex = 0;
            // 
            // authCodeBox
            // 
            this.authCodeBox.Location = new System.Drawing.Point(145, 102);
            this.authCodeBox.Name = "authCodeBox";
            this.authCodeBox.Size = new System.Drawing.Size(297, 20);
            this.authCodeBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(105, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Steam Community ID";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 105);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(134, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Game Authentication Code";
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "9",
            "10"});
            this.comboBox1.Location = new System.Drawing.Point(145, 128);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(81, 21);
            this.comboBox1.TabIndex = 5;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 131);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Select Event ID";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(5, 157);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(115, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Currently Select Event:";
            // 
            // selectedEventLbl
            // 
            this.selectedEventLbl.Location = new System.Drawing.Point(126, 150);
            this.selectedEventLbl.Name = "selectedEventLbl";
            this.selectedEventLbl.Size = new System.Drawing.Size(315, 31);
            this.selectedEventLbl.TabIndex = 8;
            this.selectedEventLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // saveBtn
            // 
            this.saveBtn.Location = new System.Drawing.Point(180, 260);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(75, 23);
            this.saveBtn.TabIndex = 9;
            this.saveBtn.Text = "Save";
            this.saveBtn.UseVisualStyleBackColor = true;
            this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoEllipsis = true;
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(5, 233);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(429, 13);
            this.linkLabel1.TabIndex = 10;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "https://help.steampowered.com/en/wizard/HelpWithGameIssue/?appid=730&issueid=128";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(5, 207);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(385, 26);
            this.label5.TabIndex = 11;
            this.label5.Text = "The Game Authentication Code must be generated and is unique to your profile. \r\nG" +
    "enerate it at this link:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(5, 182);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(420, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Please enter either your Steam ID or your Community ID in either box and press va" +
    "lidate.";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 53);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(51, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "Steam ID";
            // 
            // steamIdBox
            // 
            this.steamIdBox.Location = new System.Drawing.Point(145, 50);
            this.steamIdBox.Name = "steamIdBox";
            this.steamIdBox.Size = new System.Drawing.Size(221, 20);
            this.steamIdBox.TabIndex = 13;
            // 
            // validateBtn
            // 
            this.validateBtn.Location = new System.Drawing.Point(371, 24);
            this.validateBtn.Name = "validateBtn";
            this.validateBtn.Size = new System.Drawing.Size(70, 46);
            this.validateBtn.TabIndex = 15;
            this.validateBtn.Text = "VALIDATE";
            this.validateBtn.UseVisualStyleBackColor = true;
            this.validateBtn.Click += new System.EventHandler(this.validateBtn_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 78);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(68, 13);
            this.label8.TabIndex = 17;
            this.label8.Text = "Steam Name";
            // 
            // steamName
            // 
            this.steamName.Enabled = false;
            this.steamName.Location = new System.Drawing.Point(145, 76);
            this.steamName.Name = "steamName";
            this.steamName.Size = new System.Drawing.Size(296, 20);
            this.steamName.TabIndex = 16;
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 295);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.steamName);
            this.Controls.Add(this.validateBtn);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.steamIdBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.saveBtn);
            this.Controls.Add(this.selectedEventLbl);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.authCodeBox);
            this.Controls.Add(this.steamCommunityIdBox);
            this.Name = "Settings";
            this.Text = "Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox steamCommunityIdBox;
        private System.Windows.Forms.TextBox authCodeBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label selectedEventLbl;
        private System.Windows.Forms.Button saveBtn;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox steamIdBox;
        private System.Windows.Forms.Button validateBtn;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox steamName;
    }
}