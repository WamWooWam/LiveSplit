namespace LiveSplit.View
{
    partial class TimerForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TimerForm));
            RightClickMenu = new System.Windows.Forms.ContextMenuStrip(components);
            editSplitsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openSplitsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveSplitsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveSplitsAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            closeSplitsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            controlMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            comparisonMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            shareMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            endRaceSection = new System.Windows.Forms.ToolStripSeparator();
            editLayoutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openLayoutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveLayoutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveLayoutAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            settingsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            aboutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            splitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            resetMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            undoSplitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            skipSplitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            pauseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            undoPausesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            hotkeysMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            RightClickMenu.SuspendLayout();
            SuspendLayout();
            // 
            // RightClickMenu
            // 
            RightClickMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { editSplitsMenuItem, openSplitsMenuItem, saveSplitsMenuItem, saveSplitsAsMenuItem, closeSplitsMenuItem, toolStripSeparator5, controlMenuItem, comparisonMenuItem, toolStripSeparator1, shareMenuItem, toolStripSeparator3, toolStripMenuItem1, endRaceSection, editLayoutMenuItem, openLayoutMenuItem, saveLayoutMenuItem, saveLayoutAsMenuItem, toolStripSeparator2, settingsMenuItem, toolStripSeparator4, aboutMenuItem, exitMenuItem });
            RightClickMenu.Name = "RightClickMenu";
            RightClickMenu.Size = new System.Drawing.Size(181, 414);
            RightClickMenu.Opening += RightClickMenu_Opening;
            // 
            // editSplitsMenuItem
            // 
            editSplitsMenuItem.Name = "editSplitsMenuItem";
            editSplitsMenuItem.Size = new System.Drawing.Size(180, 22);
            editSplitsMenuItem.Text = "Edit Splits...";
            editSplitsMenuItem.Click += editSplitsMenuItem_Click;
            // 
            // openSplitsMenuItem
            // 
            openSplitsMenuItem.Name = "openSplitsMenuItem";
            openSplitsMenuItem.Size = new System.Drawing.Size(180, 22);
            openSplitsMenuItem.Text = "Open Splits";
            // 
            // saveSplitsMenuItem
            // 
            saveSplitsMenuItem.Name = "saveSplitsMenuItem";
            saveSplitsMenuItem.Size = new System.Drawing.Size(180, 22);
            saveSplitsMenuItem.Text = "Save Splits";
            saveSplitsMenuItem.Click += saveSplitsMenuItem_Click;
            // 
            // saveSplitsAsMenuItem
            // 
            saveSplitsAsMenuItem.Name = "saveSplitsAsMenuItem";
            saveSplitsAsMenuItem.Size = new System.Drawing.Size(180, 22);
            saveSplitsAsMenuItem.Text = "Save Splits As...";
            saveSplitsAsMenuItem.Click += saveAsMenuItem_Click;
            // 
            // closeSplitsMenuItem
            // 
            closeSplitsMenuItem.Name = "closeSplitsMenuItem";
            closeSplitsMenuItem.Size = new System.Drawing.Size(180, 22);
            closeSplitsMenuItem.Text = "Close Splits";
            closeSplitsMenuItem.Click += closeSplitsMenuItem_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(177, 6);
            // 
            // controlMenuItem
            // 
            controlMenuItem.Name = "controlMenuItem";
            controlMenuItem.Size = new System.Drawing.Size(180, 22);
            controlMenuItem.Text = "Control";
            // 
            // comparisonMenuItem
            // 
            comparisonMenuItem.Name = "comparisonMenuItem";
            comparisonMenuItem.Size = new System.Drawing.Size(180, 22);
            comparisonMenuItem.Text = "Compare Against";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // shareMenuItem
            // 
            shareMenuItem.Name = "shareMenuItem";
            shareMenuItem.Size = new System.Drawing.Size(180, 22);
            shareMenuItem.Text = "Share...";
            shareMenuItem.Click += shareMenuItem_Click;
            // 
            // endRaceSection
            // 
            endRaceSection.Name = "endRaceSection";
            endRaceSection.Size = new System.Drawing.Size(177, 6);
            // 
            // editLayoutMenuItem
            // 
            editLayoutMenuItem.Name = "editLayoutMenuItem";
            editLayoutMenuItem.Size = new System.Drawing.Size(180, 22);
            editLayoutMenuItem.Text = "Edit Layout...";
            editLayoutMenuItem.Click += editLayoutMenuItem_Click;
            // 
            // openLayoutMenuItem
            // 
            openLayoutMenuItem.Name = "openLayoutMenuItem";
            openLayoutMenuItem.Size = new System.Drawing.Size(180, 22);
            openLayoutMenuItem.Text = "Open Layout";
            // 
            // saveLayoutMenuItem
            // 
            saveLayoutMenuItem.Name = "saveLayoutMenuItem";
            saveLayoutMenuItem.Size = new System.Drawing.Size(180, 22);
            saveLayoutMenuItem.Text = "Save Layout";
            saveLayoutMenuItem.Click += saveLayoutMenuItem_Click;
            // 
            // saveLayoutAsMenuItem
            // 
            saveLayoutAsMenuItem.Name = "saveLayoutAsMenuItem";
            saveLayoutAsMenuItem.Size = new System.Drawing.Size(180, 22);
            saveLayoutAsMenuItem.Text = "Save Layout As...";
            saveLayoutAsMenuItem.Click += saveLayoutAsMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(177, 6);
            // 
            // settingsMenuItem
            // 
            settingsMenuItem.Name = "settingsMenuItem";
            settingsMenuItem.Size = new System.Drawing.Size(180, 22);
            settingsMenuItem.Text = "Settings";
            settingsMenuItem.Click += settingsMenuItem_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(177, 6);
            // 
            // aboutMenuItem
            // 
            aboutMenuItem.Name = "aboutMenuItem";
            aboutMenuItem.Size = new System.Drawing.Size(180, 22);
            aboutMenuItem.Text = "About";
            aboutMenuItem.Click += aboutMenuItem_Click;
            // 
            // exitMenuItem
            // 
            exitMenuItem.Name = "exitMenuItem";
            exitMenuItem.Size = new System.Drawing.Size(180, 22);
            exitMenuItem.Text = "Exit";
            exitMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // splitMenuItem
            // 
            splitMenuItem.Name = "splitMenuItem";
            splitMenuItem.Size = new System.Drawing.Size(152, 22);
            splitMenuItem.Text = "Start";
            splitMenuItem.Click += splitMenuItem_Click;
            // 
            // resetMenuItem
            // 
            resetMenuItem.Enabled = false;
            resetMenuItem.Name = "resetMenuItem";
            resetMenuItem.Size = new System.Drawing.Size(152, 22);
            resetMenuItem.Text = "Reset";
            resetMenuItem.Click += resetMenuItem_Click;
            // 
            // undoSplitMenuItem
            // 
            undoSplitMenuItem.Enabled = false;
            undoSplitMenuItem.Name = "undoSplitMenuItem";
            undoSplitMenuItem.Size = new System.Drawing.Size(152, 22);
            undoSplitMenuItem.Text = "Undo Split";
            undoSplitMenuItem.Click += undoSplitMenuItem_Click;
            // 
            // skipSplitMenuItem
            // 
            skipSplitMenuItem.Enabled = false;
            skipSplitMenuItem.Name = "skipSplitMenuItem";
            skipSplitMenuItem.Size = new System.Drawing.Size(152, 22);
            skipSplitMenuItem.Text = "Skip Split";
            skipSplitMenuItem.Click += skipSplitMenuItem_Click;
            // 
            // pauseMenuItem
            // 
            pauseMenuItem.Enabled = false;
            pauseMenuItem.Name = "pauseMenuItem";
            pauseMenuItem.Size = new System.Drawing.Size(152, 22);
            pauseMenuItem.Text = "Pause";
            pauseMenuItem.Click += pauseMenuItem_Click;
            // 
            // undoPausesMenuItem
            // 
            undoPausesMenuItem.Enabled = false;
            undoPausesMenuItem.Name = "undoPausesMenuItem";
            undoPausesMenuItem.Size = new System.Drawing.Size(152, 22);
            undoPausesMenuItem.Text = "Undo All Pauses";
            undoPausesMenuItem.Click += undoPausesMenuItem_Click;
            // 
            // hotkeysMenuItem
            // 
            hotkeysMenuItem.Name = "hotkeysMenuItem";
            hotkeysMenuItem.Size = new System.Drawing.Size(152, 22);
            hotkeysMenuItem.Text = "Global Hotkeys";
            hotkeysMenuItem.Click += hotkeysMenuItem_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(177, 6);
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            toolStripMenuItem1.Text = "Races...";
            toolStripMenuItem1.Click += Races_Click;
            // 
            // TimerForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Black;
            ClientSize = new System.Drawing.Size(175, 173);
            ContextMenuStrip = RightClickMenu;
            DoubleBuffered = true;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "TimerForm";
            Text = "LiveSplit";
            TopMost = true;
            FormClosing += TimerForm_FormClosing;
            Shown += TimerForm_Shown;
            ResizeBegin += TimerForm_ResizeBegin;
            ResizeEnd += TimerForm_ResizeEnd;
            Paint += TimerForm_Paint;
            MouseDown += TimerForm_MouseDown;
            MouseMove += TimerForm_MouseMove;
            MouseUp += TimerForm_MouseUp;
            MouseWheel += TimerForm_MouseWheel;
            RightClickMenu.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip RightClickMenu;
        private System.Windows.Forms.ToolStripMenuItem openSplitsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editSplitsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveSplitsMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem openLayoutMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveLayoutAsMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem settingsMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem aboutMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeSplitsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveSplitsAsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editLayoutMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveLayoutMenuItem;
        private System.Windows.Forms.ToolStripMenuItem shareMenuItem;
        private System.Windows.Forms.ToolStripSeparator endRaceSection;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem controlMenuItem;
        private System.Windows.Forms.ToolStripMenuItem splitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoSplitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem skipSplitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pauseMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoPausesMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hotkeysMenuItem;
        private System.Windows.Forms.ToolStripMenuItem comparisonMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
    }
}

