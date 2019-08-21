namespace json_merge
{
    partial class MergeVisualForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MergeVisualForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.aTextBox = new System.Windows.Forms.RichTextBox();
            this.bTextBox = new System.Windows.Forms.RichTextBox();
            this.cTextBox = new System.Windows.Forms.RichTextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.navigateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nextDifferenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.previousDifferenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.cTextBox);
            this.splitContainer1.Size = new System.Drawing.Size(840, 698);
            this.splitContainer1.SplitterDistance = 383;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.aTextBox);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.bTextBox);
            this.splitContainer2.Size = new System.Drawing.Size(840, 383);
            this.splitContainer2.SplitterDistance = 415;
            this.splitContainer2.TabIndex = 0;
            // 
            // aTextBox
            // 
            this.aTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.aTextBox.Font = new System.Drawing.Font("Consolas", 9F);
            this.aTextBox.Location = new System.Drawing.Point(0, 0);
            this.aTextBox.Name = "aTextBox";
            this.aTextBox.Size = new System.Drawing.Size(415, 383);
            this.aTextBox.TabIndex = 0;
            this.aTextBox.Text = "";
            this.aTextBox.WordWrap = false;
            this.aTextBox.VScroll += new System.EventHandler(this.aTextBox_Scroll);
            this.aTextBox.HScroll += new System.EventHandler(this.aTextBox_Scroll);
            // 
            // bTextBox
            // 
            this.bTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bTextBox.Font = new System.Drawing.Font("Consolas", 9F);
            this.bTextBox.Location = new System.Drawing.Point(0, 0);
            this.bTextBox.Name = "bTextBox";
            this.bTextBox.Size = new System.Drawing.Size(421, 383);
            this.bTextBox.TabIndex = 1;
            this.bTextBox.Text = "";
            this.bTextBox.WordWrap = false;
            this.bTextBox.VScroll += new System.EventHandler(this.bTextBox_Scroll);
            this.bTextBox.HScroll += new System.EventHandler(this.bTextBox_Scroll);
            // 
            // cTextBox
            // 
            this.cTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cTextBox.Font = new System.Drawing.Font("Consolas", 9F);
            this.cTextBox.Location = new System.Drawing.Point(0, 0);
            this.cTextBox.Name = "cTextBox";
            this.cTextBox.Size = new System.Drawing.Size(840, 311);
            this.cTextBox.TabIndex = 2;
            this.cTextBox.Text = "";
            this.cTextBox.WordWrap = false;
            this.cTextBox.VScroll += new System.EventHandler(this.cTextBox_Scroll);
            this.cTextBox.HScroll += new System.EventHandler(this.cTextBox_Scroll);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.navigateToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(840, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // navigateToolStripMenuItem
            // 
            this.navigateToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nextDifferenceToolStripMenuItem,
            this.previousDifferenceToolStripMenuItem});
            this.navigateToolStripMenuItem.Name = "navigateToolStripMenuItem";
            this.navigateToolStripMenuItem.Size = new System.Drawing.Size(66, 20);
            this.navigateToolStripMenuItem.Text = "Navigate";
            // 
            // nextDifferenceToolStripMenuItem
            // 
            this.nextDifferenceToolStripMenuItem.Name = "nextDifferenceToolStripMenuItem";
            this.nextDifferenceToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Down)));
            this.nextDifferenceToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.nextDifferenceToolStripMenuItem.Text = "Next Difference";
            this.nextDifferenceToolStripMenuItem.Click += new System.EventHandler(this.nextDifferenceToolStripMenuItem_Click);
            // 
            // previousDifferenceToolStripMenuItem
            // 
            this.previousDifferenceToolStripMenuItem.Name = "previousDifferenceToolStripMenuItem";
            this.previousDifferenceToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Up)));
            this.previousDifferenceToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.previousDifferenceToolStripMenuItem.Text = "Previous Difference";
            this.previousDifferenceToolStripMenuItem.Click += new System.EventHandler(this.previousDifferenceToolStripMenuItem_Click);
            // 
            // MergeVisualForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(840, 722);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MergeVisualForm";
            this.Text = "Json Merge";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.RichTextBox aTextBox;
        private System.Windows.Forms.RichTextBox bTextBox;
        private System.Windows.Forms.RichTextBox cTextBox;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem navigateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nextDifferenceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem previousDifferenceToolStripMenuItem;
    }
}