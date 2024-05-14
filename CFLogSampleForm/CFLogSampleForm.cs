//***************************************************************************
// Copyright (c) Takahiro Fukushima All rights reserved.
// Licensed under the MIT license.
//***************************************************************************

namespace CFLogSampleForm
{
	using System.Diagnostics;
	
	public partial class CFLogSampleForm :Form
	{
		public CFLogSampleForm()
		{
			InitializeComponent();
		}

		//-------------------------------------------------------------------
		// �e�L�X�g�o�̓{�^��
		//-------------------------------------------------------------------
		private void TextWriteButton_Click(object sender, EventArgs e)
		{
			LOG.Write(I, Line1TextBox.Text, Line2TextBox.Text);
		}

		//-------------------------------------------------------------------
		// ��O���o�̓{�^��
		//-------------------------------------------------------------------
		private void ExtInfoButton_Click(object sender, EventArgs e)
		{
			try
			{
				try
				{
					int x = 10;
					int y = 0;
					int a = x / y;
				} catch(Exception ex)
				{
					throw new Exception("a = x / y", ex);
				}
			} catch(Exception ex)
			{
				LOG.Write(F, "Nest Exception", ex);
			}
		}

		//-------------------------------------------------------------------
		// �}���`�X���b�h�ŏo�̓{�^��
		//-------------------------------------------------------------------
		private void ByMTButton_Click(object sender, EventArgs e)
		{
			LOG.Write(I, "�X���b�h�J�n(Start thread)");

			Thread t1 = new Thread(new ParameterizedThreadStart(otherThread));
			t1.IsBackground = true;
			t1.Start(1);

			Thread t2 = new Thread(new ParameterizedThreadStart(otherThread));
			t2.IsBackground = true;
			t2.Start(2);

			Thread t3 = new Thread(new ParameterizedThreadStart(otherThread));
			t3.IsBackground = true;
			t3.Start(3);

			t1.Join();
			t2.Join();
			t3.Join();

			LOG.Write(I, "�X���b�h�I��(Stop thread)");

			//~~~~~~~~~~~~~~~~~~~~~~~~~
			// �X���b�h����
			//~~~~~~~~~~~~~~~~~~~~~~~~~
			void otherThread(object? n)
			{
				for(int i = 0 ; i < 5 ; i++)
				{
					LOG.Write(I, $"�ʃX���b�h(Other thread) {n}");
					Thread.Sleep(300);
				}
			}
		}

		//-------------------------------------------------------------------
		// �}���`�v���Z�X�ŏo�̓{�^��
		//-------------------------------------------------------------------
		private void ByMPButton_Click(object sender, EventArgs e)
		{
			LOG.Write(I, "�T�u�v���Z�X�J�n(Start sub process)");

			var p1 = Process.Start("CFLogSampleForm.exe", "-sub 1");
			var p2 = Process.Start("CFLogSampleForm.exe", "-sub 2");
			var p3 = Process.Start("CFLogSampleForm.exe", "-sub 3");

			p1?.WaitForExit();
			p2?.WaitForExit();
			p3?.WaitForExit();

			// �T�u�t�H���_"1"�֏o�͂���邱�Ƃɒ���
			var p4 = Process.Start("CFLogSampleForm.exe", "-sub 4");
			p4?.WaitForExit();

			LOG.Write(I, "�T�u�v���Z�X�I��(Stop sub process)");
		}
	}
}
