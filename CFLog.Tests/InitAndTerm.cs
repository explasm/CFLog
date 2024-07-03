//***************************************************************************
// Copyright (c) Takahiro Fukushima All rights reserved.
// Licensed under the MIT license.
//***************************************************************************

using CFLog.Tests.Support;
using System.Diagnostics;
using System.Security.AccessControl;

namespace CFLog.Tests
{
    public class InitAndTerm
	{
#if DEBUG
		//-------------------------------------------------------------------
		/// �������ƏI���E�C���X�^���X�����E����ݒ�
		/// ����ݒ�Ȃ��ŁAOS�̎��s�������\�[�X�ɔ��f����Ă��邱�ƁB
		//-------------------------------------------------------------------
		[Fact]
		[Trait("FullAuto", "true")]
		public void ID_010_010_010_010()
		{
			Setup.InitType1();

			// ����ݒ�ύX�Ȃ�
			var loggerDef = new LoggerDef()
			{
				CULTURE_INFO = null,
			};

			using(Logger.CreateLogger(loggerDef))
			{
				Assert.Null(MessageResource.Culture);
			}

			// �c���K�v���Ȃ��̂ō쐬�������O�t�H���_���폜
			Directory.Delete(loggerDef.LOG_DIR_PATH, true);
		}

		//-------------------------------------------------------------------
		/// �������ƏI���E�C���X�^���X�����E����ݒ�
		/// �ݒ肵��OS�̊��Ƃ͈قȂ�J���`���[�����\�[�X�ɔ��f����Ă��邱�ƁB
		//-------------------------------------------------------------------
		[Fact]
		[Trait("FullAuto", "true")]
		public void ID_010_010_020_010()
		{
			Setup.InitType1();

			// ����ݒ�ύX
			var loggerDef = new LoggerDef()
			{
				CULTURE_INFO = new("ms-LA"),
			};

			using(Logger.CreateLogger(loggerDef))
			{
				Assert.Equal(loggerDef.CULTURE_INFO, MessageResource.Culture);
			}

			// �c���K�v���Ȃ��̂ō쐬�������O�t�H���_���폜
			Directory.Delete(loggerDef.LOG_DIR_PATH, true);
		}

		//-------------------------------------------------------------------
		/// �������ƏI���E�C���X�^���X�����E��d�������G���[
		/// LoggerInitException��O���������A���b�Z�[�W�� MessageResource.AlreadyInitializedError �̓��e�ł��邱�ƁB
		//-------------------------------------------------------------------
		[Theory]
		[Trait("FullAuto", "true")]
		[InlineData("ID_010_010_030_010")]
		[InlineData("ID_010_010_030_011")]
		public void ID_010_010_030_01X(string ID)
		{
			Setup.InitType1();

			// 1�x�ڂ̏�����
			using(Logger.CreateLogger())
			{
				switch(ID)
				{
				case "ID_010_010_030_010":
					{
						// �Q�x�ڂ̏������ŗ�O�������m�F
						var lex = Assert.Throws<Logger.LoggerInitException>(
							() => { using var logger = Logger.CreateLogger(); }
						);
						// ��O���b�Z�[�W���m�F
						Assert.Equal(MessageResource.AlreadyInitializedError, lex.Message);
					}
					break;
				case "ID_010_010_030_011":
					{
						string message = string.Empty;
						try
						{
							using var logger = Logger.CreateLogger();
						} catch(Logger.LoggerException ex)
						{
							message = ex.Message;
						}
						// ��O���b�Z�[�W���m�F
						Assert.Equal(MessageResource.AlreadyInitializedError, message);
					}
					break;
				}
			}

			// �c���K�v���Ȃ��̂ō쐬�������O�t�H���_���폜
			Setup.InitType1();
		}

		//-------------------------------------------------------------------
		/// �������ƏI���E�C���X�^���X�����E�ď�����
		/// Dispose()�Ăяo����̏��������������A�Q�x�ڂ̏�����������O������ɏ����o�����
		//-------------------------------------------------------------------
		[Fact]
		[Trait("FullAuto", "true")]
		public void ID_010_010_040_010()
		{
			Setup.InitType1();

			var loggerDef = new LoggerDef()
			{
			};

			// 1�x��
			using(Logger.CreateLogger(loggerDef))
			{
				LOG.Write(I, "�P�x��");
			}

			// �Q�x��
			using(Logger.CreateLogger(loggerDef))
			{
				LOG.Write(I, "�Q�x��");
			}

			Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), "�Q�x��") == 1);

			// �c���K�v���Ȃ��̂ō쐬�������O�t�H���_���폜
			Setup.InitType1();
		}

		//-------------------------------------------------------------------
		/// �������ƏI���E�R���X�g���N�^�E�v���Z�X�Ԕr������
		//-------------------------------------------------------------------
		[Theory]
		/// �}���`�v���Z�X��������A���ꃍ�O�t�H���_���t�@�C�������g�p����v���Z�X���r�������̏������I������܂ŁA�e�X�g�������̏I����҂��Ď��s���邱��
		[InlineData("ID_010_020_010_010", @".\Log", "CFLogTests")]      // Support.exe �Ɠ���
		/// �r�����䂠��A�t�@�C�������قȂ�ꍇ�ɔr����������Ȃ�
		[InlineData("ID_010_020_010_020", @".\Log", "CFLogTests2")]     // Support.exe �ƃt�@�C�������قȂ�
		/// �r�����䂠��A���O�o�̓t�H���_���قȂ�ꍇ�ɔr����������Ȃ�
		[InlineData("ID_010_020_010_030", @".\Log2", "CFLogTests")]     // Support.exe �ƃt�H���_���قȂ�
		[Trait("FullAuto", "true")]
		public void ID_010_020_010_0XX(string ID, string logDir, string file_prefix)
		{
			const int WAIT_TIME = 1500;
			Setup.InitType1(logDir);

			LoggerDef logDef = new()
			{
				LOG_TYPE_FILTER = (lt) => lt <= LogType.I,
				LOG_DIR_PATH = logDir,
				FILE_PREFIX = file_prefix,
				ALLOW_MULTIPLE_PROCESSES = true,
			};

			// Logger���g�p����e�X�g�p�v���Z�X�̋N��
			var supportProcess = Process.Start("CFLog.Test.Support.exe", $"-test01 {WAIT_TIME}");
			// �E�C���h�E�������Ȃ��v���Z�X�Ȃ̂Ŋm���ɋN��������҂��@���Ȃ�����Sleep()�ő҂�
			Thread.Sleep(500);


			// �^�C�}�[�X�^�[�g
			var sw = new Stopwatch();
			sw.Start();

			using(Logger.CreateLogger(logDef))
			{
				LOG.Write(I, ID);
			}

			// �덷���ǉ�
			Thread.Sleep(500);

			sw.Stop();

			if(ID == "ID_010_020_010_010")
				Assert.True(sw.ElapsedMilliseconds > WAIT_TIME);
			else
				Assert.False(sw.ElapsedMilliseconds > WAIT_TIME);

			// �T�|�[�g�v���Z�X�����O�t�@�C�����I�[�v�����Ă��āA���O�o�̓t�H���_���폜�ł��Ȃ��̂�
			// �I����҂i���������e�X�g�̕�����s��OFF�̑O��Ƃ���j
			supportProcess?.WaitForExit();

			// �c���K�v���Ȃ��̂ō쐬�������O�t�H���_���폜
			Directory.Delete(logDir, true);
		}

		//-------------------------------------------------------------------
		/// �������ƏI���E�R���X�g���N�^�E���O�t�H���_�̎w��
		//-------------------------------------------------------------------
		[Theory]
		[Trait("FullAuto", "true")]
		/// ���΃p�X�ŁA�Ō�ɃZ�p���[�^���������ł��Ӑ}�����t�H���_�Ƀ��O�t�@�C��������邱�ƁB
		[InlineData("ID_010_020_020_010", @".\CFLogtest2024\L010", @".\CFLogTest2024", false)]
		/// ���΃p�X�ŁA�Ō�ɃZ�p���[�^���������ł��Ӑ}�����t�H���_�Ƀ��O�t�@�C��������邱�ƁB
		[InlineData("ID_010_020_020_011", @".\CFLogtest2024\L010", @".\CFLogTest2024", true)]
		/// ���΃p�X�ŁA�Ō�ɃZ�p���[�^�����L��ł��Ӑ}�����t�H���_�Ƀ��O�t�@�C��������邱�ƁB
		[InlineData("ID_010_020_020_020", @".\CFLogtest2024\L020\", @".\CFLogTest2024", false)]
		/// ���΃p�X�ŁA�Ō�ɃZ�p���[�^�����L��ł��Ӑ}�����t�H���_�Ƀ��O�t�@�C��������邱�ƁB
		[InlineData("ID_010_020_020_021", @".\CFLogtest2024\L020\", @".\CFLogTest2024", true)]
		/// ��΃p�X�ŁA�Ō�ɃZ�p���[�^���������ł��Ӑ}�����t�H���_�Ƀ��O�t�@�C��������邱�ƁB
		[InlineData("ID_010_020_020_030", @"C:\CFLogtest2024\L030", @"C:\CFLogTest2024", false)]
		/// ��΃p�X�ŁA�Ō�ɃZ�p���[�^���������ł��Ӑ}�����t�H���_�Ƀ��O�t�@�C��������邱�ƁB
		[InlineData("ID_010_020_020_031", @"C:\CFLogtest2024\L030", @"C:\CFLogTest2024", true)]
		/// ��΃p�X�ŁA�Ō�ɃZ�p���[�^�����L��ł��Ӑ}�����t�H���_�Ƀ��O�t�@�C��������邱�ƁB
		[InlineData("ID_010_020_020_040", @"C:\CFLogtest2024\L040\", @"C:\CFLogTest2024", false)]
		/// ��΃p�X�ŁA�Ō�ɃZ�p���[�^�����L��ł��Ӑ}�����t�H���_�Ƀ��O�t�@�C��������邱�ƁB
		[InlineData("ID_010_020_020_041", @"C:\CFLogtest2024\L040\", @"C:\CFLogTest2024", true)]
		/// �l�b�g���[�N�t�H���_�w��ł�����Ƀ��O�t�@�C�����쐬����邱��
		[InlineData("ID_010_020_020_045", @"\\localhost\CFLogTestLog\Log", @"\\localhost\CFLogTestLog\Log", false)]
		/// �l�b�g���[�N�t�H���_�w��ł�����Ƀ��O�t�@�C�����쐬����邱��
		[InlineData("ID_010_020_020_046", @"\\localhost\CFLogTestLog\Log", @"\\localhost\CFLogTestLog\Log", true)]
		/// ���O�o�̓t�H���_�Ƀ��[�g�t�H���_�w��i�h���C�u�����j
		[InlineData("ID_010_020_020_050", @"\", "", false)]
		/// ���O�o�̓t�H���_�Ƀ��[�g�t�H���_�w��i�h���C�u�w�肠��j
		[InlineData("ID_010_020_020_060", @"C:\", "", false)]
		/// ���O�t�H���_�ɑ��݂��Ȃ��h���C�u���w��
		[InlineData("ID_010_020_020_070", @"A:\LogtestError", "", false)]  // �e�X�g���ɑ��݂��Ȃ��h���C�u
		/// ���O�t�H���_�ɑ��݂��Ȃ��l�b�g���[�N�T�[�o���w��
		[InlineData("ID_010_020_020_080", @"\\ErrorServer\LogtestError", "", false)]   // �e�X�g���ɑ��݂��Ȃ��T�[�o
		public void ID_010_020_020_0XX(string ID, string logDir, string delDir, bool allowMultipleProcesses)
		{
			// ����ݒ�ύX�Ȃ�
			var loggerDef = new LoggerDef()
			{
				ALLOW_MULTIPLE_PROCESSES = allowMultipleProcesses,
				LOG_DIR_PATH = logDir,
			};

			switch(ID)
			{
			case "ID_010_020_020_050":
			case "ID_010_020_020_060":
				{
					Setup.InitType2();

					var lex = Assert.Throws<Logger.LoggerInitException>(
						() => { using var logger = Logger.CreateLogger(loggerDef); }
					);

					// ��O���b�Z�[�W���m�F
					Assert.Equal(MessageResource.NoDirectoryError, lex.Message);
				}
				break;
			case "ID_010_020_020_070":
			case "ID_010_020_020_080":
				{
					Setup.InitType2();

					var lex = Assert.Throws<Logger.LoggerInitException>(
						() => { using var logger = Logger.CreateLogger(loggerDef); }
					);

					// ��O���b�Z�[�W���m�F
					Assert.Equal(
						string.Format(MessageResource.CannotOpenLogfileError, Util.MakeFilePath(loggerDef)),
						lex.Message);
				}
				break;
			case "ID_010_020_020_045":
			case "ID_010_020_020_046":
				Setup.InitType1(delDir);
				using(Logger.CreateLogger(loggerDef))
				{
					Assert.True(File.Exists(Util.MakeFilePath(loggerDef)));
				}
				// �c���K�v���Ȃ��̂ō쐬�������O�t�H���_���폜
				Directory.Delete(delDir, true);
				break;
			default:
				Setup.InitType1(delDir);
				using(Logger.CreateLogger(loggerDef))
				{
					Assert.True(File.Exists(Util.MakeFilePath(loggerDef)));
				}

				// �c���K�v���Ȃ��̂ō쐬�������O�t�H���_���폜
				Directory.Delete(delDir, true);
				break;
			}
		}

		//-------------------------------------------------------------------
		/// �������ƏI���E�R���X�g���N�^�E���O�t�H���_�̌����ݒ�
		//-------------------------------------------------------------------
		[Theory]
		[Trait("FullAuto", "true")]
		/// �}���`�v���Z�X�������Ȃ��ꍇ�̃��O�t�H���_
		[InlineData("ID_010_020_030_010", false)]
		/// �}���`�v���Z�X��������ꍇ�̃��O�t�H���_
		[InlineData("ID_010_020_030_020", true)]
		public void ID_010_020_030_0XX(string _, bool allowMultiProcess)
		{
			Setup.InitType1();

			var loggerDef = new LoggerDef()
			{
				ALLOW_MULTIPLE_PROCESSES = allowMultiProcess,
			};

			using(Logger.CreateLogger(loggerDef))
			{
			}

			Assert.True(Util.ChkDirAccessRule(loggerDef));


			// �c���K�v���Ȃ��̂ō쐬�������O�t�H���_���폜
			Directory.Delete(loggerDef.LOG_DIR_PATH, true);
		}

		//-------------------------------------------------------------------
		/// �������ƏI���E�R���X�g���N�^�E�}���`�v���Z�X���ŁA�ő吔�܂ł̃��O�I�[�v��
		//-------------------------------------------------------------------
		[Theory]
		[Trait("FullAuto", "true")]
		/// �ő吔��4�Ƃ��A4�Ԗڂ̃v���Z�X������ɃI�[�v���ł��邱��
		[InlineData("ID_010_020_040_010", 4)]
		/// �ő吔��4�Ƃ��A5�Ԗڂ̃v���Z�X�ŃI�[�v�������s���邱��
		[InlineData("ID_010_020_040_020", 5)]
		public void ID_010_020_040_0XX(string ID, int processCount)
		{
			const int WAIT_TIME = 3000;
			Setup.InitType1();

			LoggerDef loggerDef = new()
			{
				ALLOW_MULTIPLE_PROCESSES = true,
				MAX_PROCESS_COUNT = 4,
				FILE_PREFIX = "CFLogTests",
			};

			List<Process?> supportProcesses = new();
			// Logger���g�p����e�X�g�p�v���Z�X�̋N��
			for(int i = 0 ; i < processCount - 1 ; i++)
			{
				supportProcesses.Add(Process.Start("CFLog.Test.Support.exe", $"-test02 {WAIT_TIME}"));
			}
			// �E�C���h�E�������Ȃ��v���Z�X�Ȃ̂Ŋm���ɋN��������҂��@���Ȃ�����Sleep()�ő҂�
			Thread.Sleep(500);

			switch(ID)
			{
			case "ID_010_020_040_010":  // ����n
										// �{�e�X�g�v���O�������p�����[�^�� processCount �ԖڂɂȂ�
				using(Logger.CreateLogger(loggerDef))
				{
				}
				waitAllProcesses(supportProcesses);

				for(int i = 1 ; i <= processCount ; i++)
				{
					Assert.True(File.Exists(Util.MakeFilePath(loggerDef, i)));
				}
				break;
			case "ID_010_020_040_020":  // ���ُ�n
				var lex = Assert.Throws<Logger.LoggerInitException>(
					() => { using var logger = Logger.CreateLogger(loggerDef); }
				);
				// ��O���b�Z�[�W���m�F
				Assert.Equal(
					string.Format(MessageResource.MaxProcess, loggerDef.MAX_PROCESS_COUNT),
					lex.Message);

				waitAllProcesses(supportProcesses);
				break;
			}

			// �c���K�v���Ȃ��̂ō쐬�������O�t�H���_���폜
			Directory.Delete(loggerDef.LOG_DIR_PATH, true);

			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			// �N�������v���Z�X���ׂĂ̏I����҂�
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			void waitAllProcesses(in List<Process?> allProcesses)
			{
				foreach(var process in allProcesses)
				{
					process?.WaitForExit();
				}
			}
		}

		//-------------------------------------------------------------------
		/// �������ƏI���E�R���X�g���N�^�E�������O�t�@�C���̃I�[�v��
		//-------------------------------------------------------------------
		[Theory]
		[Trait("FullAuto", "true")]
		/// "�}���`�v���Z�X�s���ŁALogger�̃I�[�v���N���[�Y�̂P�x�ڂƂQ�x�ڂŃ��O�t�@�C���̃T�C�Y�������邱��"
		[InlineData("ID_010_020_050_010", false)]
		/// �}���`�v���Z�X���ŁALogger�̃I�[�v���N���[�Y�̂P�x�ڂƂQ�x�ڂŃ��O�t�@�C���̃T�C�Y�������邱��
		[InlineData("ID_010_020_050_020", true)]
		public void ID_010_020_050_0XX(string ID, bool allowMultiProcess)
		{
			Setup.InitType1();

			var loggerDef = new LoggerDef()
			{
				ALLOW_MULTIPLE_PROCESSES = allowMultiProcess,
			};

			using(Logger.CreateLogger(loggerDef))
			{
				LOG.Write(I, ID, "�P�x��");
			}

			// �P�x�ڌ�̃t�@�C���T�C�Y
			long size1st = Util.GetFileSize(Util.MakeFilePath(loggerDef));

			using(Logger.CreateLogger(loggerDef))
			{
				LOG.Write(I, ID, "�Q�x��");
			}

			// �Q�x�ڌ�̃t�@�C���T�C�Y
			long size2nd = Util.GetFileSize(Util.MakeFilePath(loggerDef));

			Assert.True((0 < size1st) && (size1st < size2nd));

			Debug.WriteLine($"�� size1st = {size1st} < size2nd = {size2nd}");


			// �c���K�v���Ȃ��̂ō쐬�������O�t�H���_���폜
			Directory.Delete(loggerDef.LOG_DIR_PATH, true);
		}

		//-------------------------------------------------------------------
		/// �������ƏI���E�R���X�g���N�^�E�f�B���N�g���쐬���s
		/// ���O�t�H���_�̏�̊K�w�ɓǂݎ���p���������Ă��ăt�H���_�쐬�Ɏ��s���A��O����������
		//-------------------------------------------------------------------
		[Fact]
		[Trait("FullAuto", "true")]
		public void ID_010_020_060_010()
		{
			Setup.InitType2();

			var loggerDef = new LoggerDef()
			{
				LOG_DIR_PATH = @".\ROnly\Log",  // Log�����̍쐬���ɃG���[�𔭐�������
			};
			const string parentLogDir = @".\ROnly";

			// ���O�t�H���_�����A�V�K�t�H���_�쐬���ۂ̐ݒ������
			Directory.CreateDirectory(parentLogDir);
			Util.SetDirAccessRuleToNTUsers(parentLogDir, FileSystemRights.CreateDirectories, AccessControlType.Deny, true);

			var lex = Assert.Throws<Logger.LoggerInitException>(
				() => { using var logger = Logger.CreateLogger(loggerDef); }
			);
			// ��O���b�Z�[�W���m�F
			Assert.Equal(string.Format(MessageResource.CannotOpenLogfileError, Util.MakeFilePath(loggerDef)), lex.Message);

			// �c���K�v���Ȃ��̂ō쐬�������O�t�H���_���폜
			Directory.Delete(parentLogDir, true);
		}

		//-------------------------------------------------------------------
		/// �������ƏI���E�R���X�g���N�^�E�t�@�C���X�g���[���I�[�v�����s
		/// ���O�t�H���_���Ȃ���ԂŃt�@�C���X�g���[���I�[�v�������s�B���O�t�H���_�̌����ݒ�ɐ������ꂽ���̂ɂ���B
		//-------------------------------------------------------------------
		[Fact]
		[Trait("FullAuto", "true")]
		public void ID_010_020_070_010()
		{
			Setup.InitType2();

			var loggerDef = new LoggerDef()
			{
				LOG_DIR_PATH = @".\ROnly\Log",  // Log�����̍쐬���ɃG���[�𔭐�������
				DIR_RIGHTS_TARGET = null,       // Logger�Ō����ݒ肳���Ȃ�
			};
			const string parentLogDir = @".\ROnly";

			// ���O�t�H���_�����A�V�K�t�@�C���쐬���ۂ̐ݒ������
			Directory.CreateDirectory(parentLogDir);
			Util.SetDirAccessRuleToNTUsers(parentLogDir, FileSystemRights.CreateFiles, AccessControlType.Deny, true);

			var lex = Assert.Throws<Logger.LoggerInitException>(
				() => { using var logger = Logger.CreateLogger(loggerDef); }
			);
			// ��O���b�Z�[�W���m�F
			Assert.Equal(string.Format(MessageResource.CannotOpenLogfileError, Util.MakeFilePath(loggerDef)), lex.Message);

			// �c���K�v���Ȃ��̂ō쐬�������O�t�H���_���폜
			Directory.Delete(parentLogDir, true);
		}

		//-------------------------------------------------------------------
		/// �������ƏI���E�R���X�g���N�^�E�t�@�C���X�g���[���I�[�v�����s
		/// ���Ƀ��O�t�H���_���t�@�C�������݂��A�t�@�C���ɓǂݎ���p�������t���Ă��ăt�@�C���X�g���[���I�[�v�������s
		//-------------------------------------------------------------------
		[Fact]
		[Trait("FullAuto", "true")]
		public void ID_010_020_070_020()
		{
			Setup.InitType1();

			var loggerDef = new LoggerDef()
			{
			};

			// ��x���퓮�삳���ă��O�t�@�C�����쐬����
			using(Logger.CreateLogger(loggerDef))
			{
			}

			// ���ꂽ���O�t�@�C���ɏ������݋֎~��ǉ�����
			Util.SetFileAccessRuleToNTUsers(Util.MakeFilePath(loggerDef), FileSystemRights.WriteData, AccessControlType.Deny, true);

			var lex = Assert.Throws<Logger.LoggerInitException>(
				() => { using var logger = Logger.CreateLogger(loggerDef); }
			);
			// ��O���b�Z�[�W���m�F
			Assert.Equal(string.Format(MessageResource.CannotOpenLogfileError, Util.MakeFilePath(loggerDef)), lex.Message);

			// �c���K�v���Ȃ��̂ō쐬�������O�t�H���_���폜
			Directory.Delete(loggerDef.log_dir_full_path, true);
		}

		//-------------------------------------------------------------------
		/// �������ƏI���E�R���X�g���N�^�E�ۊǊ����؂�t�@�C���폜
		//-------------------------------------------------------------------
		[Theory]
		[Trait("FullAuto", "true")]
		/// �ۑ������̐ݒ肪3�ŁA4���ȏ�Â��t�@�C�����폜����A������V�����t�@�C���͍폜����Ȃ��B�܂��A���̃v���t�B�b�N�X�̃t�@�C���͍폜����Ȃ��B
		[InlineData("ID_010_020_080_010", 3)]
		/// �ۑ������̐ݒ肪1�ŁA2���ȏ�Â��t�@�C�����폜����A������V�����t�@�C���͍폜����Ȃ��B�܂��A���̃v���t�B�b�N�X�̃t�@�C���͍폜����Ȃ��B"
		[InlineData("ID_010_020_080_020", 1)]
		/// �ۑ�����0�ŁA1�N�O�̃t�@�C��������̃t�@�C�����폜����Ȃ��܂��A�폜��������Ȃ��|���O�t�@�C���ɏ����o�����
		[InlineData("ID_010_020_080_030", 0)]
		public void ID_010_020_080_0XX(string ID, int storageDays)
		{
			Setup.InitType1();

			var loggerDef = new LoggerDef()
			{
				STORAGE_DAYS = storageDays,
				LOG_TYPE_FILTER = (lt) => lt <= LogType.I,
			};
			var loggerDefOther = new LoggerDef()
			{
				FILE_PREFIX = "Other",
				STORAGE_DAYS = storageDays,
				LOG_TYPE_FILTER = (lt) => lt <= LogType.I,
			};

			// ���̃��O�t�@�C���i�ꂩ���O�j
			DateTimeForTest.SetVirtualDateTime(new(2023, 12, 2, 12, 0, 0),loggerDef.TIME_ZONE_INFO);
			// ���O�t�@�C�����쐬����
			using(Logger.CreateLogger(loggerDefOther))
			{
			}
			Assert.True(File.Exists(Util.MakeFilePath(loggerDefOther, today: new DateTime(2023, 12, 2, 12, 0, 0))));


			// ���ݎ�����ݒ�i1�N�O�j
			DateTimeForTest.SetVirtualDateTime(new(2023, 1, 2, 12, 0, 0), loggerDef.TIME_ZONE_INFO);
			// ���O�t�@�C�����쐬����
			using(Logger.CreateLogger(loggerDef))
			{
			}
			Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 1, 2, 12, 0, 0))));

			// ���ݎ�����ݒ�i5���O�j
			DateTimeForTest.SetVirtualDateTime(new(2023, 12, 28, 12, 0, 0), loggerDef.TIME_ZONE_INFO);
			// ���O�t�@�C�����쐬����
			using(Logger.CreateLogger(loggerDef))
			{
			}
			Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 12, 28, 12, 0, 0))));

			// ���ݎ�����ݒ�i4���O�j
			DateTimeForTest.SetVirtualDateTime(new(2023, 12, 29, 12, 0, 0), loggerDef.TIME_ZONE_INFO);
			// ���O�t�@�C�����쐬����
			using(Logger.CreateLogger(loggerDef))
			{
			}
			Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 12, 29, 12, 0, 0))));

			// ���ݎ�����ݒ�i3���O�j
			DateTimeForTest.SetVirtualDateTime(new(2023, 12, 30, 12, 0, 0), loggerDef.TIME_ZONE_INFO);
			// ���O�t�@�C�����쐬����
			using(Logger.CreateLogger(loggerDef))
			{
			}
			Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 12, 30, 12, 0, 0))));

			// ���ݎ�����ݒ�i2���O�j
			DateTimeForTest.SetVirtualDateTime(new(2023, 12, 31, 12, 0, 0), loggerDef.TIME_ZONE_INFO);
			// ���O�t�@�C�����쐬����
			using(Logger.CreateLogger(loggerDef))
			{
			}
			Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 12, 31, 12, 0, 0))));

			// ���ݎ�����ݒ�i1���O�j
			DateTimeForTest.SetVirtualDateTime(new(2024, 1, 1, 12, 0, 0), loggerDef.TIME_ZONE_INFO);
			// ���O�t�@�C�����쐬����
			using(Logger.CreateLogger(loggerDef))
			{
			}
			Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2024, 1, 1, 12, 0, 0))));

			// ���ݎ�����ݒ�i�����j
			DateTimeForTest.SetVirtualDateTime(new(2024, 1, 2, 12, 0, 0), loggerDef.TIME_ZONE_INFO);
			// ���O�t�@�C�����쐬����
			using(Logger.CreateLogger(loggerDef))
			{
			}
			Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2024, 1, 2, 12, 0, 0))));

			switch(ID)
			{
			case "ID_010_020_080_010":
				Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2024, 1, 1, 12, 0, 0))));
				Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 12, 31, 12, 0, 0))));
				Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 12, 30, 12, 0, 0))));
				Assert.False(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 12, 29, 12, 0, 0))));
				Assert.False(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 12, 28, 12, 0, 0))));
				Assert.False(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 1, 2, 12, 0, 0))));
				break;
			case "ID_010_020_080_020":
				Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2024, 1, 1, 12, 0, 0))));
				Assert.False(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 12, 31, 12, 0, 0))));
				Assert.False(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 12, 30, 12, 0, 0))));
				Assert.False(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 12, 29, 12, 0, 0))));
				Assert.False(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 12, 28, 12, 0, 0))));
				Assert.False(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 1, 2, 12, 0, 0))));
				break;
			case "ID_010_020_080_030":
				Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2024, 1, 1, 12, 0, 0))));
				Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 12, 31, 12, 0, 0))));
				Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 12, 30, 12, 0, 0))));
				Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 12, 29, 12, 0, 0))));
				Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 12, 28, 12, 0, 0))));
				Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 1, 2, 12, 0, 0))));

				// ���O�t�@�C�����e�̃`�F�b�N
				Assert.True(
					Util.CountText(
						Util.MakeFilePath(
							loggerDef,
							today: new DateTime(2023, 1, 2, 12, 0, 0)
						),
						MessageResource.DoNotDeleteInfo
					) == 1);
				break;
			}
			Assert.True(File.Exists(Util.MakeFilePath(loggerDefOther, today: new DateTime(2023, 12, 2, 12, 0, 0))));

			// �c���K�v���Ȃ��̂ō쐬�������O�t�H���_���폜
			Directory.Delete(loggerDef.log_dir_full_path, true);

			// �����ݒ�����ɖ߂�
			DateTimeForTest.ResetVirtualDateTime();
		}

		//-------------------------------------------------------------------
		/// �������ƏI���E�R���X�g���N�^�E�ۊǊ����؂�t�@�C���폜�������A�t�@�C���ꗗ�擾���s
		/// �t�H���_�̈ꗗ�擾���������ېݒ�ɂȂ��Ă��ăt�@�C���ꗗ�擾���ł��Ȃ��ہA�����͑��s���A���O�ɂ��̎|�����o�����
		//-------------------------------------------------------------------
		[Fact]
		[Trait("FullAuto", "true")]
		public void ID_010_020_090_010()
		{
			Setup.InitType1();

			var loggerDef = new LoggerDef()
			{
			};

			// ���O�t�H���_�����A�t�H���_�ꗗ�擾���������ېݒ肷��
			Directory.CreateDirectory(loggerDef.LOG_DIR_PATH);
			Util.SetDirAccessRuleToNTUsers(loggerDef.LOG_DIR_PATH, (FileSystemRights.CreateFiles | FileSystemRights.Write | FileSystemRights.Modify) & ~FileSystemRights.ListDirectory, AccessControlType.Allow, true, false);
			Util.SetDirAccessRuleToNTUsers(loggerDef.LOG_DIR_PATH, FileSystemRights.ListDirectory, AccessControlType.Deny, true, true);

			// ���O�t�@�C�����쐬����
			using(Logger.CreateLogger(loggerDef))
			{
			}
			// �t�H���_���ꗗ�擾���ۂ��폜���Ȃ��ƃt�@�C���̃��[�h�I�[�v���Ɏ��s���邽�ߍ폜
			Util.SetDirAccessRuleToNTUsers(loggerDef.LOG_DIR_PATH, FileSystemRights.ListDirectory, AccessControlType.Deny, false, true);

			// ���O�t�@�C�����e�̃`�F�b�N
			Assert.True(
				Util.CountText(
					Util.MakeFilePath(loggerDef),
					MessageResource.EnumrateFilesError
				) == 1);

			// �c���K�v���Ȃ��̂ō쐬�������O�t�H���_���폜
			Directory.Delete(loggerDef.LOG_DIR_PATH, true);
		}

		//-------------------------------------------------------------------
		/// �������ƏI���E�R���X�g���N�^�E�ۊǊ����؂�t�@�C���폜�������A�폜�Ώۃt�@�C�����폜�ł��Ȃ�
		/// �폜�Ώۃt�@�C�����I�[�v������Ă��č폜�ł��Ȃ��Ƃ��A�����͑��s���A���O�ɂ��̎|�����o�����
		//-------------------------------------------------------------------
		[Fact]
		[Trait("FullAuto", "true")]
		public void ID_010_020_100_010()
		{
			Setup.InitType1();

			var loggerDef = new LoggerDef()
			{
				STORAGE_DAYS = 2,
				LOG_TYPE_FILTER = (lt) => lt <= LogType.I,
			};

			// ���ݎ�����ݒ�i1�N�O�j
			DateTimeForTest.SetVirtualDateTime(new(2023, 1, 2, 12, 0, 0), loggerDef.TIME_ZONE_INFO);
			// ���O�t�@�C�����쐬����
			using(Logger.CreateLogger(loggerDef))
			{
			}

			// �폜�Ώۃt�@�C�����I�[�v�����č폜�ł��Ȃ��悤�ɂ���
			using(new StreamReader(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 1, 2, 12, 0, 0))))
			{
				// ���ݎ�����ݒ�i�����j
				DateTimeForTest.SetVirtualDateTime(new(2024, 1, 2, 12, 0, 0), loggerDef.TIME_ZONE_INFO);
				// ���O�t�@�C�����쐬����
				using(Logger.CreateLogger(loggerDef))
				{
				}
			}

			Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2024, 1, 2, 12, 0, 0))));
			Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 1, 2, 12, 0, 0))));

			// ���O�t�@�C�����e�̃`�F�b�N
			Assert.True(
					Util.CountText(
						Util.MakeFilePath(
							loggerDef,
							today: new DateTime(2024, 1, 2, 12, 0, 0)
						),
						string.Format(MessageResource.DeleteFailedError, Util.MakeFilePath(loggerDef, today: new DateTime(2023, 1, 2, 12, 0, 0), isOnlyFilename: true))
					) == 1);


			// �c���K�v���Ȃ��̂ō쐬�������O�t�H���_���폜
			Directory.Delete(loggerDef.log_dir_full_path, true);

			// �����ݒ�����ɖ߂�
			DateTimeForTest.ResetVirtualDateTime();
		}

		//-------------------------------------------------------------------
		/// �������ƏI���E�N���[�Y����
		//-------------------------------------------------------------------
		[Theory]
		[Trait("FullAuto", "true")]
		/// �J�n�I�����b�Z�[�W�̃��O�����o���E�I�[�v�����b�Z�[�W�����O�t�@�C���ɋL�^����邱�ƁA�I�[�v�����b�Z�[�W�Ɏ����̃^�C���]�[��ID���o�͂���Ă��邱��
		[InlineData("ID_010_030_010_010")]
		/// �J�n�I�����b�Z�[�W�̃��O�����o���E�N���[�Y���b�Z�[�W�����O�t�@�C���ɋL�^����邱��
		[InlineData("ID_010_030_010_020")]
		/// �J�n�I�����b�Z�[�W�����ŁA���[�U���O�݂̂������o���ELogger���g�̃��b�Z�[�W�����O�ɏ����o���Ȃ��ݒ�ɂ����Ƃ��A�I�[�v�����b�Z�[�W���N���[�Y���b�Z�[�W���L�^���ꂸ�A�ʏ�̃��O�̂݋L�^����邱��
		[InlineData("ID_010_030_020_010")]
		public void ID_010_030_0XX_0XX(string ID)
		{
			string timeZoneID = "Easter Island Standard Time";

			Setup.InitType1();

			var loggerDef = new LoggerDef()
			{
				LOG_TYPE_FILTER = (lt) => lt <= LogType.I,
				WRITE_START_AND_STOP_MESSAGE = ID != "ID_010_030_020_010",
				TIME_ZONE_INFO = TimeZoneInfo.FindSystemTimeZoneById(timeZoneID),
			};

			using(Logger.CreateLogger(loggerDef))
			{
				LOG.Write(I, ID);
			}

			// ���O�t�@�C�����e�̃`�F�b�N
			Assert.True(
					Util.CountText(
						Util.MakeFilePath(loggerDef),
						ID
					) == 1);

			switch(ID)
			{
			case "ID_010_030_010_010":
				{
					List<int> columns = new List<int>();
					int messagePos = 0;
					int timeZoneIDPos = 0;

					// ���O�t�@�C�����e�̃`�F�b�N�i�J�n���b�Z�[�W�j
					Assert.True(
							Util.CountText(
								Util.MakeFilePath(loggerDef),
								MessageResource.StartLogging,
								columns
							) == 1);
					messagePos = columns[0];
					// �J�n���b�Z�[�W�̉E�ɗ���^�C���]�[��ID
					columns = new List<int>();
					Assert.True(
							Util.CountText(
								Util.MakeFilePath(loggerDef),
								timeZoneID,
								columns
							) == 1);
					Debug.WriteLine("010_010");
					timeZoneIDPos = columns[0];
					Assert.True(messagePos < timeZoneIDPos);
				}
				break;
			case "ID_010_030_010_020":
				// ���O�t�@�C�����e�̃`�F�b�N�i�I�����b�Z�[�W�j
				Assert.True(
						Util.CountText(
							Util.MakeFilePath(loggerDef),
							MessageResource.StopLogging
						) == 1);
				Debug.WriteLine("010_020");
				break;
			case "ID_010_030_020_010":
				// ���O�t�@�C�����e�̃`�F�b�N�i�J�n���b�Z�[�W�j
				Assert.True(
						Util.CountText(
							Util.MakeFilePath(loggerDef),
							MessageResource.StartLogging
						) == 0);
				// ���O�t�@�C�����e�̃`�F�b�N�i�I�����b�Z�[�W�j
				Assert.True(
						Util.CountText(
							Util.MakeFilePath(loggerDef),
							MessageResource.StopLogging
						) == 0);
				Debug.WriteLine("020_010");
				break;
			}

			// �c���K�v���Ȃ��̂ō쐬�������O�t�H���_���폜
			Directory.Delete(loggerDef.log_dir_full_path, true);
		}
#endif
	}
}

