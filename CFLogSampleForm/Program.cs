//***************************************************************************
// Copyright (c) Takahiro Fukushima All rights reserved.
// Licensed under the MIT license.
//***************************************************************************

namespace CFLogSampleForm
{
	internal static class Program
	{
		[STAThread]
		static void Main()
		{
			// �R�}���h���C���p�����[�^���
			int subProcessNumber = parseCommandLine();

			try
			{
				if(subProcessNumber == 0)
				{   /////////////////////////////////////////////////////////
					// �ʏ�N�����ꂽ�ꍇ
					/////////////////////////////////////////////////////////

					ApplicationConfiguration.Initialize();

					// ���O�@�\�ݒ�
					var logDef = new LoggerDef()
					{
						// ���O�t�@�C�����v���t�B�b�N�X�̐ݒ�
						FILE_PREFIX = "CFLogSampleForm",
#if DEBUG
						// ���O�^�C�v�f�o�b�O���p�̃t�B���^�ݒ�
						LOG_TYPE_FILTER = (lt) => (lt & LogType.FILTER_DEBUG) != 0,
#else
						// ���O�^�C�v�����[�X���p�̃t�B���^�ݒ�
						LOG_TYPE_FILTER = (lt) => (lt & LogType.FILTER_RELEASE) != 0,
#endif
						// ���O�ۊǓ�����3���ɐݒ�
						STORAGE_DAYS = 3,
					};

					// ���O�J�n
					using(CreateLogger(logDef))
					{
						// �A�v���P�[�V�����J�n
						Application.Run(new CFLogSampleForm());
					}
				} else
				{   /////////////////////////////////////////////////////////
					// �T�u�v���Z�X�Ƃ��ċN�����ꂽ�ꍇ
					// �t�H�[����ʂ�[�}���`�v���Z�X�o��]�{�^�����������ꂽ�ۂ�
					// �N�����ꂽ�ꍇ���
					/////////////////////////////////////////////////////////

					// ���O�@�\�ݒ�
					var logDef = new LoggerDef()
					{
						// ���O�t�@�C�����v���t�B�b�N�X�̐ݒ�
						FILE_PREFIX = "SubProcess",

						// �u���v�ȉ����o��
						LOG_TYPE_FILTER = (lt) => lt <= LogType.I,

						// ������s���W���[���̕����v���Z�X������
						ALLOW_MULTIPLE_PROCESSES = true,

						// ���O�J�n�^�I�����b�Z�[�W�����O�ɏ����o���Ȃ�
						WRITE_START_AND_STOP_MESSAGE = false,
					};

					// ���O�J�n
					using(CreateLogger(logDef))
					{
						// �T�u�v���Z�X�Ƃ��ċN�����ꂽ�ۂ̏���

						for(int i = 0 ; i < 10 ; i++)
						{
							LOG.Write(I, $"�T�u�v���Z�X(Sub process) <{subProcessNumber}> [{i}]");
							Thread.Sleep(200);
						}
					}

					// ����Q�b�Ńv���Z�X���I������
				}
#if true	// Logger�̗�O�N���X�Ƀ��[�g�N���X��ǉ��������Ƃ�1�̗�O�N���X��catch�ł���悤�ɂ����i2024.7.3�j
			} catch(LoggerException ex)
			{   // ���R�Ȃ���ALogger���o����O�̓��O�ɏ����o���Ȃ�
				System.Diagnostics.Debug.WriteLine(ex.Message);
				MessageBox.Show(ex.Message);
			}
#else
			} catch(LoggerInitException ex)
			{	// ���R�Ȃ���ALogger���o����O�̓��O�ɏ����o���Ȃ�
				System.Diagnostics.Debug.WriteLine(ex.Message);
				MessageBox.Show(ex.Message);
			} catch(LoggerWriteException ex)
			{	// ���O���o���̍ۂɗ�O�����������ۂɂ������𒆒f����ꍇ�̗�
				System.Diagnostics.Debug.WriteLine(ex.Message);
				MessageBox.Show(ex.Message);
			}
#endif

			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			// �R�}���h���C�������ŁA�T�u�v���Z�X�ԍ����擾����
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			int parseCommandLine()
			{
				/*
				 * ���̏����� CFLogSampleForm �N���X�� ByMPButton_Click() ����
				 * �N�������ۂɎw�肳���R�}���h���C���p�����[�^����͂��邽�߂�����
				 * ���̂ł��B
				 * ����̃��O�o�͂��s���v���Z�X�𕡐��N������ۂ̃T���v���Ƃ��Ă��̋@�\��
				 * �������Ă��܂��B
				 * 
				 */

				int result = 0;

				var args = Environment.GetCommandLineArgs();
				for(int i = 1 ; i < args.Length ; ++i)
				{
					if((args[i] == "-sub") && ((i + 1) < args.Length))
					{
						result = int.Parse(args[i + 1]);
						break;
					}
				}

				return result;
			}
		}
	}
}
