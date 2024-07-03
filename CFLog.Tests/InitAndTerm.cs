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
		/// 初期化と終了・インスタンス生成・言語設定
		/// 言語設定なしで、OSの実行環境がリソースに反映されていること。
		//-------------------------------------------------------------------
		[Fact]
		[Trait("FullAuto", "true")]
		public void ID_010_010_010_010()
		{
			Setup.InitType1();

			// 言語設定変更なし
			var loggerDef = new LoggerDef()
			{
				CULTURE_INFO = null,
			};

			using(Logger.CreateLogger(loggerDef))
			{
				Assert.Null(MessageResource.Culture);
			}

			// 残す必要がないので作成したログフォルダを削除
			Directory.Delete(loggerDef.LOG_DIR_PATH, true);
		}

		//-------------------------------------------------------------------
		/// 初期化と終了・インスタンス生成・言語設定
		/// 設定したOSの環境とは異なるカルチャーがリソースに反映されていること。
		//-------------------------------------------------------------------
		[Fact]
		[Trait("FullAuto", "true")]
		public void ID_010_010_020_010()
		{
			Setup.InitType1();

			// 言語設定変更
			var loggerDef = new LoggerDef()
			{
				CULTURE_INFO = new("ms-LA"),
			};

			using(Logger.CreateLogger(loggerDef))
			{
				Assert.Equal(loggerDef.CULTURE_INFO, MessageResource.Culture);
			}

			// 残す必要がないので作成したログフォルダを削除
			Directory.Delete(loggerDef.LOG_DIR_PATH, true);
		}

		//-------------------------------------------------------------------
		/// 初期化と終了・インスタンス生成・二重初期化エラー
		/// LoggerInitException例外が発生し、メッセージが MessageResource.AlreadyInitializedError の内容であること。
		//-------------------------------------------------------------------
		[Theory]
		[Trait("FullAuto", "true")]
		[InlineData("ID_010_010_030_010")]
		[InlineData("ID_010_010_030_011")]
		public void ID_010_010_030_01X(string ID)
		{
			Setup.InitType1();

			// 1度目の初期化
			using(Logger.CreateLogger())
			{
				switch(ID)
				{
				case "ID_010_010_030_010":
					{
						// ２度目の初期化で例外発生を確認
						var lex = Assert.Throws<Logger.LoggerInitException>(
							() => { using var logger = Logger.CreateLogger(); }
						);
						// 例外メッセージを確認
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
						// 例外メッセージを確認
						Assert.Equal(MessageResource.AlreadyInitializedError, message);
					}
					break;
				}
			}

			// 残す必要がないので作成したログフォルダを削除
			Setup.InitType1();
		}

		//-------------------------------------------------------------------
		/// 初期化と終了・インスタンス生成・再初期化
		/// Dispose()呼び出し後の初期化が成功し、２度目の初期化後もログが正常に書き出される
		//-------------------------------------------------------------------
		[Fact]
		[Trait("FullAuto", "true")]
		public void ID_010_010_040_010()
		{
			Setup.InitType1();

			var loggerDef = new LoggerDef()
			{
			};

			// 1度目
			using(Logger.CreateLogger(loggerDef))
			{
				LOG.Write(I, "１度目");
			}

			// ２度目
			using(Logger.CreateLogger(loggerDef))
			{
				LOG.Write(I, "２度目");
			}

			Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), "２度目") == 1);

			// 残す必要がないので作成したログフォルダを削除
			Setup.InitType1();
		}

		//-------------------------------------------------------------------
		/// 初期化と終了・コンストラクタ・プロセス間排他制御
		//-------------------------------------------------------------------
		[Theory]
		/// マルチプロセスを許可する、同一ログフォルダ＆ファイル名を使用するプロセスが排他部分の処理を終了するまで、テスト側がその終了を待って実行すること
		[InlineData("ID_010_020_010_010", @".\Log", "CFLogTests")]      // Support.exe と同じ
		/// 排他制御あり、ファイル名が異なる場合に排他処理されない
		[InlineData("ID_010_020_010_020", @".\Log", "CFLogTests2")]     // Support.exe とファイル名が異なる
		/// 排他制御あり、ログ出力フォルダが異なる場合に排他処理されない
		[InlineData("ID_010_020_010_030", @".\Log2", "CFLogTests")]     // Support.exe とフォルダが異なる
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

			// Loggerを使用するテスト用プロセスの起動
			var supportProcess = Process.Start("CFLog.Test.Support.exe", $"-test01 {WAIT_TIME}");
			// ウインドウを持たないプロセスなので確実に起動完了を待つ方法がないためSleep()で待つ
			Thread.Sleep(500);


			// タイマースタート
			var sw = new Stopwatch();
			sw.Start();

			using(Logger.CreateLogger(logDef))
			{
				LOG.Write(I, ID);
			}

			// 誤差分追加
			Thread.Sleep(500);

			sw.Stop();

			if(ID == "ID_010_020_010_010")
				Assert.True(sw.ElapsedMilliseconds > WAIT_TIME);
			else
				Assert.False(sw.ElapsedMilliseconds > WAIT_TIME);

			// サポートプロセスがログファイルをオープンしていて、ログ出力フォルダを削除できないので
			// 終了を待つ（そもそもテストの並列実行はOFFの前提とする）
			supportProcess?.WaitForExit();

			// 残す必要がないので作成したログフォルダを削除
			Directory.Delete(logDir, true);
		}

		//-------------------------------------------------------------------
		/// 初期化と終了・コンストラクタ・ログフォルダの指定
		//-------------------------------------------------------------------
		[Theory]
		[Trait("FullAuto", "true")]
		/// 相対パスで、最後にセパレータ文字無しでも意図したフォルダにログファイルが作られること。
		[InlineData("ID_010_020_020_010", @".\CFLogtest2024\L010", @".\CFLogTest2024", false)]
		/// 相対パスで、最後にセパレータ文字無しでも意図したフォルダにログファイルが作られること。
		[InlineData("ID_010_020_020_011", @".\CFLogtest2024\L010", @".\CFLogTest2024", true)]
		/// 相対パスで、最後にセパレータ文字有りでも意図したフォルダにログファイルが作られること。
		[InlineData("ID_010_020_020_020", @".\CFLogtest2024\L020\", @".\CFLogTest2024", false)]
		/// 相対パスで、最後にセパレータ文字有りでも意図したフォルダにログファイルが作られること。
		[InlineData("ID_010_020_020_021", @".\CFLogtest2024\L020\", @".\CFLogTest2024", true)]
		/// 絶対パスで、最後にセパレータ文字無しでも意図したフォルダにログファイルが作られること。
		[InlineData("ID_010_020_020_030", @"C:\CFLogtest2024\L030", @"C:\CFLogTest2024", false)]
		/// 絶対パスで、最後にセパレータ文字無しでも意図したフォルダにログファイルが作られること。
		[InlineData("ID_010_020_020_031", @"C:\CFLogtest2024\L030", @"C:\CFLogTest2024", true)]
		/// 絶対パスで、最後にセパレータ文字有りでも意図したフォルダにログファイルが作られること。
		[InlineData("ID_010_020_020_040", @"C:\CFLogtest2024\L040\", @"C:\CFLogTest2024", false)]
		/// 絶対パスで、最後にセパレータ文字有りでも意図したフォルダにログファイルが作られること。
		[InlineData("ID_010_020_020_041", @"C:\CFLogtest2024\L040\", @"C:\CFLogTest2024", true)]
		/// ネットワークフォルダ指定でも正常にログファイルが作成されること
		[InlineData("ID_010_020_020_045", @"\\localhost\CFLogTestLog\Log", @"\\localhost\CFLogTestLog\Log", false)]
		/// ネットワークフォルダ指定でも正常にログファイルが作成されること
		[InlineData("ID_010_020_020_046", @"\\localhost\CFLogTestLog\Log", @"\\localhost\CFLogTestLog\Log", true)]
		/// ログ出力フォルダにルートフォルダ指定（ドライブ無し）
		[InlineData("ID_010_020_020_050", @"\", "", false)]
		/// ログ出力フォルダにルートフォルダ指定（ドライブ指定あり）
		[InlineData("ID_010_020_020_060", @"C:\", "", false)]
		/// ログフォルダに存在しないドライブを指定
		[InlineData("ID_010_020_020_070", @"A:\LogtestError", "", false)]  // テスト環境に存在しないドライブ
		/// ログフォルダに存在しないネットワークサーバを指定
		[InlineData("ID_010_020_020_080", @"\\ErrorServer\LogtestError", "", false)]   // テスト環境に存在しないサーバ
		public void ID_010_020_020_0XX(string ID, string logDir, string delDir, bool allowMultipleProcesses)
		{
			// 言語設定変更なし
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

					// 例外メッセージを確認
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

					// 例外メッセージを確認
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
				// 残す必要がないので作成したログフォルダを削除
				Directory.Delete(delDir, true);
				break;
			default:
				Setup.InitType1(delDir);
				using(Logger.CreateLogger(loggerDef))
				{
					Assert.True(File.Exists(Util.MakeFilePath(loggerDef)));
				}

				// 残す必要がないので作成したログフォルダを削除
				Directory.Delete(delDir, true);
				break;
			}
		}

		//-------------------------------------------------------------------
		/// 初期化と終了・コンストラクタ・ログフォルダの権限設定
		//-------------------------------------------------------------------
		[Theory]
		[Trait("FullAuto", "true")]
		/// マルチプロセスを許可しない場合のログフォルダ
		[InlineData("ID_010_020_030_010", false)]
		/// マルチプロセスを許可する場合のログフォルダ
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


			// 残す必要がないので作成したログフォルダを削除
			Directory.Delete(loggerDef.LOG_DIR_PATH, true);
		}

		//-------------------------------------------------------------------
		/// 初期化と終了・コンストラクタ・マルチプロセス許可で、最大数までのログオープン
		//-------------------------------------------------------------------
		[Theory]
		[Trait("FullAuto", "true")]
		/// 最大数を4とし、4番目のプロセスが正常にオープンできること
		[InlineData("ID_010_020_040_010", 4)]
		/// 最大数を4とし、5番目のプロセスでオープンが失敗すること
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
			// Loggerを使用するテスト用プロセスの起動
			for(int i = 0 ; i < processCount - 1 ; i++)
			{
				supportProcesses.Add(Process.Start("CFLog.Test.Support.exe", $"-test02 {WAIT_TIME}"));
			}
			// ウインドウを持たないプロセスなので確実に起動完了を待つ方法がないためSleep()で待つ
			Thread.Sleep(500);

			switch(ID)
			{
			case "ID_010_020_040_010":  // 正常系
										// 本テストプログラムがパラメータの processCount 番目になる
				using(Logger.CreateLogger(loggerDef))
				{
				}
				waitAllProcesses(supportProcesses);

				for(int i = 1 ; i <= processCount ; i++)
				{
					Assert.True(File.Exists(Util.MakeFilePath(loggerDef, i)));
				}
				break;
			case "ID_010_020_040_020":  // 準異常系
				var lex = Assert.Throws<Logger.LoggerInitException>(
					() => { using var logger = Logger.CreateLogger(loggerDef); }
				);
				// 例外メッセージを確認
				Assert.Equal(
					string.Format(MessageResource.MaxProcess, loggerDef.MAX_PROCESS_COUNT),
					lex.Message);

				waitAllProcesses(supportProcesses);
				break;
			}

			// 残す必要がないので作成したログフォルダを削除
			Directory.Delete(loggerDef.LOG_DIR_PATH, true);

			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			// 起動したプロセスすべての終了を待つ
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
		/// 初期化と終了・コンストラクタ・既存ログファイルのオープン
		//-------------------------------------------------------------------
		[Theory]
		[Trait("FullAuto", "true")]
		/// "マルチプロセス不許可で、Loggerのオープンクローズの１度目と２度目でログファイルのサイズが増えること"
		[InlineData("ID_010_020_050_010", false)]
		/// マルチプロセス許可で、Loggerのオープンクローズの１度目と２度目でログファイルのサイズが増えること
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
				LOG.Write(I, ID, "１度目");
			}

			// １度目後のファイルサイズ
			long size1st = Util.GetFileSize(Util.MakeFilePath(loggerDef));

			using(Logger.CreateLogger(loggerDef))
			{
				LOG.Write(I, ID, "２度目");
			}

			// ２度目後のファイルサイズ
			long size2nd = Util.GetFileSize(Util.MakeFilePath(loggerDef));

			Assert.True((0 < size1st) && (size1st < size2nd));

			Debug.WriteLine($"★ size1st = {size1st} < size2nd = {size2nd}");


			// 残す必要がないので作成したログフォルダを削除
			Directory.Delete(loggerDef.LOG_DIR_PATH, true);
		}

		//-------------------------------------------------------------------
		/// 初期化と終了・コンストラクタ・ディレクトリ作成失敗
		/// ログフォルダの上の階層に読み取り専用属性がついていてフォルダ作成に失敗し、例外が発生する
		//-------------------------------------------------------------------
		[Fact]
		[Trait("FullAuto", "true")]
		public void ID_010_020_060_010()
		{
			Setup.InitType2();

			var loggerDef = new LoggerDef()
			{
				LOG_DIR_PATH = @".\ROnly\Log",  // Log部分の作成時にエラーを発生させる
			};
			const string parentLogDir = @".\ROnly";

			// ログフォルダを作り、新規フォルダ作成拒否の設定をする
			Directory.CreateDirectory(parentLogDir);
			Util.SetDirAccessRuleToNTUsers(parentLogDir, FileSystemRights.CreateDirectories, AccessControlType.Deny, true);

			var lex = Assert.Throws<Logger.LoggerInitException>(
				() => { using var logger = Logger.CreateLogger(loggerDef); }
			);
			// 例外メッセージを確認
			Assert.Equal(string.Format(MessageResource.CannotOpenLogfileError, Util.MakeFilePath(loggerDef)), lex.Message);

			// 残す必要がないので作成したログフォルダを削除
			Directory.Delete(parentLogDir, true);
		}

		//-------------------------------------------------------------------
		/// 初期化と終了・コンストラクタ・ファイルストリームオープン失敗
		/// ログフォルダがない状態でファイルストリームオープンが失敗。ログフォルダの権限設定に制限されたものにする。
		//-------------------------------------------------------------------
		[Fact]
		[Trait("FullAuto", "true")]
		public void ID_010_020_070_010()
		{
			Setup.InitType2();

			var loggerDef = new LoggerDef()
			{
				LOG_DIR_PATH = @".\ROnly\Log",  // Log部分の作成時にエラーを発生させる
				DIR_RIGHTS_TARGET = null,       // Loggerで権限設定させない
			};
			const string parentLogDir = @".\ROnly";

			// ログフォルダを作り、新規ファイル作成拒否の設定をする
			Directory.CreateDirectory(parentLogDir);
			Util.SetDirAccessRuleToNTUsers(parentLogDir, FileSystemRights.CreateFiles, AccessControlType.Deny, true);

			var lex = Assert.Throws<Logger.LoggerInitException>(
				() => { using var logger = Logger.CreateLogger(loggerDef); }
			);
			// 例外メッセージを確認
			Assert.Equal(string.Format(MessageResource.CannotOpenLogfileError, Util.MakeFilePath(loggerDef)), lex.Message);

			// 残す必要がないので作成したログフォルダを削除
			Directory.Delete(parentLogDir, true);
		}

		//-------------------------------------------------------------------
		/// 初期化と終了・コンストラクタ・ファイルストリームオープン失敗
		/// 既にログフォルダ＆ファイルが存在し、ファイルに読み取り専用属性が付いていてファイルストリームオープンが失敗
		//-------------------------------------------------------------------
		[Fact]
		[Trait("FullAuto", "true")]
		public void ID_010_020_070_020()
		{
			Setup.InitType1();

			var loggerDef = new LoggerDef()
			{
			};

			// 一度正常動作させてログファイルを作成する
			using(Logger.CreateLogger(loggerDef))
			{
			}

			// 作られたログファイルに書き込み禁止を追加する
			Util.SetFileAccessRuleToNTUsers(Util.MakeFilePath(loggerDef), FileSystemRights.WriteData, AccessControlType.Deny, true);

			var lex = Assert.Throws<Logger.LoggerInitException>(
				() => { using var logger = Logger.CreateLogger(loggerDef); }
			);
			// 例外メッセージを確認
			Assert.Equal(string.Format(MessageResource.CannotOpenLogfileError, Util.MakeFilePath(loggerDef)), lex.Message);

			// 残す必要がないので作成したログフォルダを削除
			Directory.Delete(loggerDef.log_dir_full_path, true);
		}

		//-------------------------------------------------------------------
		/// 初期化と終了・コンストラクタ・保管期限切れファイル削除
		//-------------------------------------------------------------------
		[Theory]
		[Trait("FullAuto", "true")]
		/// 保存日数の設定が3で、4日以上古いファイルが削除され、それより新しいファイルは削除されない。また、他のプレフィックスのファイルは削除されない。
		[InlineData("ID_010_020_080_010", 3)]
		/// 保存日数の設定が1で、2日以上古いファイルが削除され、それより新しいファイルは削除されない。また、他のプレフィックスのファイルは削除されない。"
		[InlineData("ID_010_020_080_020", 1)]
		/// 保存日数0で、1年前のファイルも昨日のファイルも削除されないまた、削除処理されない旨ログファイルに書き出される
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

			// 他のログファイル（一か月前）
			DateTimeForTest.SetVirtualDateTime(new(2023, 12, 2, 12, 0, 0),loggerDef.TIME_ZONE_INFO);
			// ログファイルを作成する
			using(Logger.CreateLogger(loggerDefOther))
			{
			}
			Assert.True(File.Exists(Util.MakeFilePath(loggerDefOther, today: new DateTime(2023, 12, 2, 12, 0, 0))));


			// 現在時刻を設定（1年前）
			DateTimeForTest.SetVirtualDateTime(new(2023, 1, 2, 12, 0, 0), loggerDef.TIME_ZONE_INFO);
			// ログファイルを作成する
			using(Logger.CreateLogger(loggerDef))
			{
			}
			Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 1, 2, 12, 0, 0))));

			// 現在時刻を設定（5日前）
			DateTimeForTest.SetVirtualDateTime(new(2023, 12, 28, 12, 0, 0), loggerDef.TIME_ZONE_INFO);
			// ログファイルを作成する
			using(Logger.CreateLogger(loggerDef))
			{
			}
			Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 12, 28, 12, 0, 0))));

			// 現在時刻を設定（4日前）
			DateTimeForTest.SetVirtualDateTime(new(2023, 12, 29, 12, 0, 0), loggerDef.TIME_ZONE_INFO);
			// ログファイルを作成する
			using(Logger.CreateLogger(loggerDef))
			{
			}
			Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 12, 29, 12, 0, 0))));

			// 現在時刻を設定（3日前）
			DateTimeForTest.SetVirtualDateTime(new(2023, 12, 30, 12, 0, 0), loggerDef.TIME_ZONE_INFO);
			// ログファイルを作成する
			using(Logger.CreateLogger(loggerDef))
			{
			}
			Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 12, 30, 12, 0, 0))));

			// 現在時刻を設定（2日前）
			DateTimeForTest.SetVirtualDateTime(new(2023, 12, 31, 12, 0, 0), loggerDef.TIME_ZONE_INFO);
			// ログファイルを作成する
			using(Logger.CreateLogger(loggerDef))
			{
			}
			Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 12, 31, 12, 0, 0))));

			// 現在時刻を設定（1日前）
			DateTimeForTest.SetVirtualDateTime(new(2024, 1, 1, 12, 0, 0), loggerDef.TIME_ZONE_INFO);
			// ログファイルを作成する
			using(Logger.CreateLogger(loggerDef))
			{
			}
			Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2024, 1, 1, 12, 0, 0))));

			// 現在時刻を設定（今日）
			DateTimeForTest.SetVirtualDateTime(new(2024, 1, 2, 12, 0, 0), loggerDef.TIME_ZONE_INFO);
			// ログファイルを作成する
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

				// ログファイル内容のチェック
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

			// 残す必要がないので作成したログフォルダを削除
			Directory.Delete(loggerDef.log_dir_full_path, true);

			// 時刻設定を元に戻す
			DateTimeForTest.ResetVirtualDateTime();
		}

		//-------------------------------------------------------------------
		/// 初期化と終了・コンストラクタ・保管期限切れファイル削除処理時、ファイル一覧取得失敗
		/// フォルダの一覧取得権限が拒否設定になっていてファイル一覧取得ができない際、処理は続行し、ログにその旨書き出される
		//-------------------------------------------------------------------
		[Fact]
		[Trait("FullAuto", "true")]
		public void ID_010_020_090_010()
		{
			Setup.InitType1();

			var loggerDef = new LoggerDef()
			{
			};

			// ログフォルダを作り、フォルダ一覧取得権限を拒否設定する
			Directory.CreateDirectory(loggerDef.LOG_DIR_PATH);
			Util.SetDirAccessRuleToNTUsers(loggerDef.LOG_DIR_PATH, (FileSystemRights.CreateFiles | FileSystemRights.Write | FileSystemRights.Modify) & ~FileSystemRights.ListDirectory, AccessControlType.Allow, true, false);
			Util.SetDirAccessRuleToNTUsers(loggerDef.LOG_DIR_PATH, FileSystemRights.ListDirectory, AccessControlType.Deny, true, true);

			// ログファイルを作成する
			using(Logger.CreateLogger(loggerDef))
			{
			}
			// フォルダ内一覧取得拒否を削除しないとファイルのリードオープンに失敗するため削除
			Util.SetDirAccessRuleToNTUsers(loggerDef.LOG_DIR_PATH, FileSystemRights.ListDirectory, AccessControlType.Deny, false, true);

			// ログファイル内容のチェック
			Assert.True(
				Util.CountText(
					Util.MakeFilePath(loggerDef),
					MessageResource.EnumrateFilesError
				) == 1);

			// 残す必要がないので作成したログフォルダを削除
			Directory.Delete(loggerDef.LOG_DIR_PATH, true);
		}

		//-------------------------------------------------------------------
		/// 初期化と終了・コンストラクタ・保管期限切れファイル削除処理時、削除対象ファイルが削除できない
		/// 削除対象ファイルがオープンされていて削除できないとき、処理は続行し、ログにその旨書き出される
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

			// 現在時刻を設定（1年前）
			DateTimeForTest.SetVirtualDateTime(new(2023, 1, 2, 12, 0, 0), loggerDef.TIME_ZONE_INFO);
			// ログファイルを作成する
			using(Logger.CreateLogger(loggerDef))
			{
			}

			// 削除対象ファイルをオープンして削除できないようにする
			using(new StreamReader(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 1, 2, 12, 0, 0))))
			{
				// 現在時刻を設定（今日）
				DateTimeForTest.SetVirtualDateTime(new(2024, 1, 2, 12, 0, 0), loggerDef.TIME_ZONE_INFO);
				// ログファイルを作成する
				using(Logger.CreateLogger(loggerDef))
				{
				}
			}

			Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2024, 1, 2, 12, 0, 0))));
			Assert.True(File.Exists(Util.MakeFilePath(loggerDef, today: new DateTime(2023, 1, 2, 12, 0, 0))));

			// ログファイル内容のチェック
			Assert.True(
					Util.CountText(
						Util.MakeFilePath(
							loggerDef,
							today: new DateTime(2024, 1, 2, 12, 0, 0)
						),
						string.Format(MessageResource.DeleteFailedError, Util.MakeFilePath(loggerDef, today: new DateTime(2023, 1, 2, 12, 0, 0), isOnlyFilename: true))
					) == 1);


			// 残す必要がないので作成したログフォルダを削除
			Directory.Delete(loggerDef.log_dir_full_path, true);

			// 時刻設定を元に戻す
			DateTimeForTest.ResetVirtualDateTime();
		}

		//-------------------------------------------------------------------
		/// 初期化と終了・クローズ処理
		//-------------------------------------------------------------------
		[Theory]
		[Trait("FullAuto", "true")]
		/// 開始終了メッセージのログ書き出し・オープンメッセージがログファイルに記録されること、オープンメッセージに自国のタイムゾーンIDが出力されていること
		[InlineData("ID_010_030_010_010")]
		/// 開始終了メッセージのログ書き出し・クローズメッセージがログファイルに記録されること
		[InlineData("ID_010_030_010_020")]
		/// 開始終了メッセージ無しで、ユーザログのみを書き出す・Logger自身のメッセージをログに書き出さない設定にしたとき、オープンメッセージもクローズメッセージも記録されず、通常のログのみ記録されること
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

			// ログファイル内容のチェック
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

					// ログファイル内容のチェック（開始メッセージ）
					Assert.True(
							Util.CountText(
								Util.MakeFilePath(loggerDef),
								MessageResource.StartLogging,
								columns
							) == 1);
					messagePos = columns[0];
					// 開始メッセージの右に来るタイムゾーンID
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
				// ログファイル内容のチェック（終了メッセージ）
				Assert.True(
						Util.CountText(
							Util.MakeFilePath(loggerDef),
							MessageResource.StopLogging
						) == 1);
				Debug.WriteLine("010_020");
				break;
			case "ID_010_030_020_010":
				// ログファイル内容のチェック（開始メッセージ）
				Assert.True(
						Util.CountText(
							Util.MakeFilePath(loggerDef),
							MessageResource.StartLogging
						) == 0);
				// ログファイル内容のチェック（終了メッセージ）
				Assert.True(
						Util.CountText(
							Util.MakeFilePath(loggerDef),
							MessageResource.StopLogging
						) == 0);
				Debug.WriteLine("020_010");
				break;
			}

			// 残す必要がないので作成したログフォルダを削除
			Directory.Delete(loggerDef.log_dir_full_path, true);
		}
#endif
	}
}

