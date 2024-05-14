//***************************************************************************
// Copyright (c) Takahiro Fukushima All rights reserved.
// Licensed under the MIT license.
//***************************************************************************

namespace CFLogSampleForm
{
	partial class CFLogSampleForm
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
			if(disposing && (components != null))
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CFLogSampleForm));
			Write1Button = new Button();
			ExtInfoButton = new Button();
			Line1TextBox = new TextBox();
			label1 = new Label();
			label2 = new Label();
			Line2TextBox = new TextBox();
			groupBox1 = new GroupBox();
			ByMTButton = new Button();
			ByMPButton = new Button();
			groupBox1.SuspendLayout();
			SuspendLayout();
			// 
			// Write1Button
			// 
			resources.ApplyResources(Write1Button, "Write1Button");
			Write1Button.Name = "Write1Button";
			Write1Button.UseVisualStyleBackColor = true;
			Write1Button.Click += TextWriteButton_Click;
			// 
			// ExtInfoButton
			// 
			resources.ApplyResources(ExtInfoButton, "ExtInfoButton");
			ExtInfoButton.Name = "ExtInfoButton";
			ExtInfoButton.UseVisualStyleBackColor = true;
			ExtInfoButton.Click += ExtInfoButton_Click;
			// 
			// Line1TextBox
			// 
			resources.ApplyResources(Line1TextBox, "Line1TextBox");
			Line1TextBox.Name = "Line1TextBox";
			// 
			// label1
			// 
			resources.ApplyResources(label1, "label1");
			label1.Name = "label1";
			// 
			// label2
			// 
			resources.ApplyResources(label2, "label2");
			label2.Name = "label2";
			// 
			// Line2TextBox
			// 
			resources.ApplyResources(Line2TextBox, "Line2TextBox");
			Line2TextBox.Name = "Line2TextBox";
			// 
			// groupBox1
			// 
			groupBox1.Controls.Add(Line1TextBox);
			groupBox1.Controls.Add(Line2TextBox);
			groupBox1.Controls.Add(Write1Button);
			groupBox1.Controls.Add(label1);
			groupBox1.Controls.Add(label2);
			resources.ApplyResources(groupBox1, "groupBox1");
			groupBox1.Name = "groupBox1";
			groupBox1.TabStop = false;
			// 
			// ByMTButton
			// 
			resources.ApplyResources(ByMTButton, "ByMTButton");
			ByMTButton.Name = "ByMTButton";
			ByMTButton.UseVisualStyleBackColor = true;
			ByMTButton.Click += ByMTButton_Click;
			// 
			// ByMPButton
			// 
			resources.ApplyResources(ByMPButton, "ByMPButton");
			ByMPButton.Name = "ByMPButton";
			ByMPButton.UseVisualStyleBackColor = true;
			ByMPButton.Click += ByMPButton_Click;
			// 
			// CFLogSampleForm
			// 
			resources.ApplyResources(this, "$this");
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(ByMPButton);
			Controls.Add(ByMTButton);
			Controls.Add(groupBox1);
			Controls.Add(ExtInfoButton);
			FormBorderStyle = FormBorderStyle.FixedSingle;
			MaximizeBox = false;
			Name = "CFLogSampleForm";
			groupBox1.ResumeLayout(false);
			groupBox1.PerformLayout();
			ResumeLayout(false);
		}

		#endregion
		private Button Write1Button;
		private Button ExtInfoButton;
		private TextBox Line1TextBox;
		private Label label1;
		private Label label2;
		private TextBox Line2TextBox;
		private GroupBox groupBox1;
		private Button ByMTButton;
		private Button ByMPButton;
	}
}
