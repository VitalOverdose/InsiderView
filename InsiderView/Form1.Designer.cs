namespace InsiderView
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            lvProjecsFound = new ListView();
            BtnSelectFolder = new Button();
            btnStartIndex = new Button();
            tbxTargetLocation = new TextBox();
            tbxExtension = new TextBox();
            btnOpenLocation = new Button();
            btnCopySlected = new Button();
            btnClose = new Button();
            btnRunWithDefaultApp = new Button();
            lvInside = new ListView();
            tbxSearchText = new TextBox();
            chkBxRecurseiveSearch = new CheckBox();
            splitContainer1 = new SplitContainer();
            lblStatus = new Label();
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            button4 = new Button();
            button5 = new Button();
            button6 = new Button();
            button7 = new Button();
            button8 = new Button();
            button9 = new Button();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semibold", 14.25F, FontStyle.Bold | FontStyle.Underline, GraphicsUnit.Point, 0);
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(116, 25);
            label1.TabIndex = 0;
            label1.Text = "Insider View";
            // 
            // lvProjecsFound
            // 
            lvProjecsFound.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lvProjecsFound.Location = new Point(0, 36);
            lvProjecsFound.Name = "lvProjecsFound";
            lvProjecsFound.Size = new Size(504, 423);
            lvProjecsFound.TabIndex = 1;
            lvProjecsFound.UseCompatibleStateImageBehavior = false;
            // 
            // BtnSelectFolder
            // 
            BtnSelectFolder.AutoSize = true;
            BtnSelectFolder.Font = new Font("Segoe UI", 11.25F);
            BtnSelectFolder.Location = new Point(168, 12);
            BtnSelectFolder.Name = "BtnSelectFolder";
            BtnSelectFolder.Size = new Size(160, 30);
            BtnSelectFolder.TabIndex = 2;
            BtnSelectFolder.Text = "Select Folder";
            BtnSelectFolder.UseVisualStyleBackColor = true;
            // 
            // btnStartIndex
            // 
            btnStartIndex.AutoSize = true;
            btnStartIndex.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnStartIndex.Location = new Point(171, 45);
            btnStartIndex.Name = "btnStartIndex";
            btnStartIndex.Size = new Size(177, 31);
            btnStartIndex.TabIndex = 3;
            btnStartIndex.Text = "Start Index";
            btnStartIndex.UseVisualStyleBackColor = true;
            // 
            // tbxTargetLocation
            // 
            tbxTargetLocation.Font = new Font("Segoe UI", 11.25F);
            tbxTargetLocation.Location = new Point(334, 12);
            tbxTargetLocation.Name = "tbxTargetLocation";
            tbxTargetLocation.PlaceholderText = "Search folder";
            tbxTargetLocation.Size = new Size(253, 27);
            tbxTargetLocation.TabIndex = 4;
            // 
            // tbxExtension
            // 
            tbxExtension.Font = new Font("Segoe UI", 11.25F);
            tbxExtension.Location = new Point(593, 12);
            tbxExtension.Name = "tbxExtension";
            tbxExtension.PlaceholderText = ".exe";
            tbxExtension.Size = new Size(86, 27);
            tbxExtension.TabIndex = 5;
            // 
            // btnOpenLocation
            // 
            btnOpenLocation.AutoSize = true;
            btnOpenLocation.Font = new Font("Segoe UI", 11.25F);
            btnOpenLocation.Location = new Point(12, 336);
            btnOpenLocation.Name = "btnOpenLocation";
            btnOpenLocation.Size = new Size(145, 30);
            btnOpenLocation.TabIndex = 6;
            btnOpenLocation.Text = "Open In Explorer";
            btnOpenLocation.UseVisualStyleBackColor = true;
            // 
            // btnCopySlected
            // 
            btnCopySlected.AutoSize = true;
            btnCopySlected.Font = new Font("Segoe UI", 11.25F);
            btnCopySlected.Location = new Point(12, 372);
            btnCopySlected.Name = "btnCopySlected";
            btnCopySlected.Size = new Size(145, 30);
            btnCopySlected.TabIndex = 7;
            btnCopySlected.Text = "Copy location";
            btnCopySlected.UseVisualStyleBackColor = true;
            // 
            // btnClose
            // 
            btnClose.AutoSize = true;
            btnClose.Font = new Font("Segoe UI", 11.25F);
            btnClose.Location = new Point(12, 516);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(145, 30);
            btnClose.TabIndex = 8;
            btnClose.Text = "Exit";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // btnRunWithDefaultApp
            // 
            btnRunWithDefaultApp.AutoSize = true;
            btnRunWithDefaultApp.Font = new Font("Segoe UI", 11.25F);
            btnRunWithDefaultApp.Location = new Point(12, 480);
            btnRunWithDefaultApp.Name = "btnRunWithDefaultApp";
            btnRunWithDefaultApp.Size = new Size(145, 30);
            btnRunWithDefaultApp.TabIndex = 9;
            btnRunWithDefaultApp.Text = "Run with default";
            btnRunWithDefaultApp.UseVisualStyleBackColor = true;
            // 
            // lvInside
            // 
            lvInside.Dock = DockStyle.Fill;
            lvInside.Location = new Point(0, 0);
            lvInside.Name = "lvInside";
            lvInside.Size = new Size(427, 462);
            lvInside.TabIndex = 10;
            lvInside.UseCompatibleStateImageBehavior = false;
            // 
            // tbxSearchText
            // 
            tbxSearchText.Font = new Font("Segoe UI", 11.25F);
            tbxSearchText.Location = new Point(3, 5);
            tbxSearchText.Name = "tbxSearchText";
            tbxSearchText.PlaceholderText = "Search folder";
            tbxSearchText.Size = new Size(499, 27);
            tbxSearchText.TabIndex = 11;
            // 
            // chkBxRecurseiveSearch
            // 
            chkBxRecurseiveSearch.AutoSize = true;
            chkBxRecurseiveSearch.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            chkBxRecurseiveSearch.Location = new Point(354, 50);
            chkBxRecurseiveSearch.Name = "chkBxRecurseiveSearch";
            chkBxRecurseiveSearch.Size = new Size(136, 24);
            chkBxRecurseiveSearch.TabIndex = 12;
            chkBxRecurseiveSearch.Text = "Recursive search";
            chkBxRecurseiveSearch.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer1.Location = new Point(168, 84);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(lvProjecsFound);
            splitContainer1.Panel1.Controls.Add(tbxSearchText);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(lvInside);
            splitContainer1.Size = new Size(935, 462);
            splitContainer1.SplitterDistance = 504;
            splitContainer1.TabIndex = 13;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 11.25F, FontStyle.Underline, GraphicsUnit.Point, 0);
            lblStatus.Location = new Point(171, 561);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(52, 20);
            lblStatus.TabIndex = 14;
            lblStatus.Text = "Status:";
            // 
            // button1
            // 
            button1.AutoSize = true;
            button1.Font = new Font("Segoe UI", 11.25F);
            button1.Location = new Point(12, 84);
            button1.Name = "button1";
            button1.Size = new Size(145, 30);
            button1.TabIndex = 14;
            button1.Text = "Show hits";
            button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.AutoSize = true;
            button2.Font = new Font("Segoe UI", 11.25F);
            button2.Location = new Point(12, 120);
            button2.Name = "button2";
            button2.Size = new Size(145, 30);
            button2.TabIndex = 15;
            button2.Text = "Show all";
            button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.AutoSize = true;
            button3.Font = new Font("Segoe UI", 11.25F);
            button3.Location = new Point(12, 156);
            button3.Name = "button3";
            button3.Size = new Size(145, 30);
            button3.TabIndex = 16;
            button3.Text = "A - Z";
            button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            button4.AutoSize = true;
            button4.Font = new Font("Segoe UI", 11.25F);
            button4.Location = new Point(12, 192);
            button4.Name = "button4";
            button4.Size = new Size(145, 30);
            button4.TabIndex = 17;
            button4.Text = "Newest first";
            button4.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            button5.AutoSize = true;
            button5.Font = new Font("Segoe UI", 11.25F);
            button5.Location = new Point(12, 228);
            button5.Name = "button5";
            button5.Size = new Size(145, 30);
            button5.TabIndex = 18;
            button5.Text = "Oldest First";
            button5.UseVisualStyleBackColor = true;
            // 
            // button6
            // 
            button6.AutoSize = true;
            button6.Font = new Font("Segoe UI", 11.25F);
            button6.Location = new Point(12, 264);
            button6.Name = "button6";
            button6.Size = new Size(145, 30);
            button6.TabIndex = 19;
            button6.Text = "Largest First";
            button6.UseVisualStyleBackColor = true;
            // 
            // button7
            // 
            button7.AutoSize = true;
            button7.Font = new Font("Segoe UI", 11.25F);
            button7.Location = new Point(12, 300);
            button7.Name = "button7";
            button7.Size = new Size(145, 30);
            button7.TabIndex = 20;
            button7.Text = "Smallest First";
            button7.UseVisualStyleBackColor = true;
            // 
            // button8
            // 
            button8.AutoSize = true;
            button8.Font = new Font("Segoe UI", 11.25F);
            button8.Location = new Point(12, 408);
            button8.Name = "button8";
            button8.Size = new Size(145, 30);
            button8.TabIndex = 21;
            button8.Text = "Copy to...";
            button8.UseVisualStyleBackColor = true;
            // 
            // button9
            // 
            button9.AutoSize = true;
            button9.Font = new Font("Segoe UI", 11.25F);
            button9.Location = new Point(12, 444);
            button9.Name = "button9";
            button9.Size = new Size(145, 30);
            button9.TabIndex = 22;
            button9.Text = "Move to...";
            button9.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1116, 588);
            Controls.Add(lblStatus);
            Controls.Add(button9);
            Controls.Add(button8);
            Controls.Add(button7);
            Controls.Add(button6);
            Controls.Add(button5);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(btnStartIndex);
            Controls.Add(chkBxRecurseiveSearch);
            Controls.Add(BtnSelectFolder);
            Controls.Add(tbxExtension);
            Controls.Add(splitContainer1);
            Controls.Add(tbxTargetLocation);
            Controls.Add(btnRunWithDefaultApp);
            Controls.Add(btnClose);
            Controls.Add(btnCopySlected);
            Controls.Add(btnOpenLocation);
            Controls.Add(label1);
            Name = "Form1";
            Text = "Form1";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private ListView lvProjecsFound;
        private Button BtnSelectFolder;
        private Button btnStartIndex;
        private TextBox tbxTargetLocation;
        private TextBox tbxExtension;
        private Button btnOpenLocation;
        private Button btnCopySlected;
        private Button btnClose;
        private Button btnRunWithDefaultApp;
        private ListView lvInside;
        private TextBox tbxSearchText;
        private CheckBox chkBxRecurseiveSearch;
        private SplitContainer splitContainer1;
        private Label lblStatus;
        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private Button button5;
        private Button button6;
        private Button button7;
        private Button button8;
        private Button button9;
    }
}
