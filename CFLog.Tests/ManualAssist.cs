//***************************************************************************
// Copyright (c) Takahiro Fukushima All rights reserved.
// Licensed under the MIT license.
//***************************************************************************

using CFLog.Tests.Support;
using System.Diagnostics;
using System.Text;

namespace CFLog.Tests
{
	public class ManualAssist
	{
#if DEBUG
		//-------------------------------------------------------------------
		/// ログ内容・フォーマット
		/// （別提供の試験項目ドキュメント参照）
		//-------------------------------------------------------------------
		[Fact]
		[Trait("FullAuto", "false")]
		public void ID_110_010_010_0XX()
		{
			var loggerDef = new LoggerDef()
			{
				LOG_TYPE_FILTER = (lt) => true,	// 全タイプ対象
				LOG_DIR_PATH = "LOG_110_010_010",
			};
			Setup.InitType1(loggerDef.LOG_DIR_PATH);

			using(Logger.CreateLogger(loggerDef))
			{
				LOG.Write(T, "トレース1行目", "トレース２行目\r\n３行目\r\n４行\r\n");
				LOG.Write(D, "デバッグ1行目", "デバッグ２行目");
				LOG.Write(I, "情報１行目のみ");
				LOG.Write(W, "警告");
				LOG.Write(E, "エラー1行目", "2行目\r\n３行目\r\n④行");
				LOG.Write(F, "致命的エラー", "２行目\n３行目\n４行\n５行目少し長め");
			}

			// メモ帳起動
			var notepad = Process.Start(new ProcessStartInfo
			{
				FileName = "notepad.exe",
				Arguments = $"\"{Util.MakeFilePath(loggerDef)}\"",
				UseShellExecute = false,
			});

			// Notpadを開いている間本メソッドは終了しない。
			// その間に、システムでプロセスIDを確認すること。
			notepad?.WaitForExit();

			Directory.Delete(loggerDef.LOG_DIR_PATH, true);
		}

		//-------------------------------------------------------------------
		/// ログ内容・フォーマット
		/// （別提供の試験項目ドキュメント参照）
		//-------------------------------------------------------------------
		[Theory]
		[Trait("FullAuto", "false")]
		[InlineData("ID_110_010_010_021")]
		[InlineData("ID_110_010_010_022")]
		public void ID_110_010_010_02X(string ID)
		{
			var loggerDef = new LoggerDef()
			{
				LOG_TYPE_FILTER = (lt) => lt <= LogType.I,
				LOG_DIR_PATH = "LOG_110_010_020_02X",
				TIME_ZONE_INFO = ID switch
				{
					"ID_110_010_010_021" => TimeZoneInfo.Utc,
					"ID_110_010_010_022" => TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time"),
					_ => TimeZoneInfo.Local
				}
			};
			Setup.InitType1(loggerDef.LOG_DIR_PATH);

			using(Logger.CreateLogger(loggerDef))
			{
				LOG.Write(I, $"TimeZone {loggerDef.TIME_ZONE_INFO.DisplayName}");
			}

			// メモ帳起動
			var notepad = Process.Start(new ProcessStartInfo
			{
				FileName = "notepad.exe",
				Arguments = $"\"{Util.MakeFilePath(loggerDef)}\"",
				UseShellExecute = false,
			});

			// Notpadを開いている間本メソッドは終了しない。
			// その間に、システムでプロセスIDを確認すること。
			notepad?.WaitForExit();

			Directory.Delete(loggerDef.LOG_DIR_PATH, true);
		}


		//-------------------------------------------------------------------
		/// ログ内容・スレッド分散
		/// （別提供の試験項目ドキュメント参照）
		//-------------------------------------------------------------------
		[Fact]
		[Trait("FullAuto","false")]
		public void ID_110_010_020_010()
		{
			Setup.InitType1();

			var loggerDef = new LoggerDef()
			{
				LOG_TYPE_FILTER = (lt) => lt <= LogType.I,
				LOG_DIR_PATH = "LOG_110_010_020_010",
			};

			using var startEvent = new ManualResetEventSlim();

			using(Logger.CreateLogger(loggerDef))
			{
				Task[] tasks = new Task[8];

				for(int i = 0 ; i < tasks.Length ; i++)
				{
					string name = $"Thread[{i + 1}]";
					tasks[i] = Task.Run(() => { logWriteLoop(name); });
				}

				Thread.Sleep(100);

				// 一斉に処理を開始させる
				startEvent.Set();

				// タスクの終了を待つ
				Task.WaitAll(tasks);
			}

			// メモ帳起動
			var notepad = Process.Start(new ProcessStartInfo
			{
				FileName = "notepad.exe",
				Arguments = $"\"{Util.MakeFilePath(loggerDef)}\"",
				UseShellExecute = false,
			});
			notepad?.WaitForExit();

			// 残す必要がないので作成したログフォルダを削除
			Directory.Delete(loggerDef.LOG_DIR_PATH, true);

			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			// スレッド処理
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			void logWriteLoop(string name)
			{
				startEvent.Wait();
				for(int i = 0 ; i < 50 ; i++)
				{
					LOG.Write(I, $"{name} > TEST({i})");
				}
			}
		}

		//-------------------------------------------------------------------
		/// ログ内容・スレッド分散
		/// （別提供の試験項目ドキュメント参照）
		//-------------------------------------------------------------------
		[Fact]
		[Trait("FullAuto", "false")]
		public void ID_110_010_020_XXX()
		{
			var yesterday = new DateTime(2024, 4, 13, 23, 59, 50);
			var today = new DateTime(2024, 4, 14, 0, 0, 0);

			Setup.InitType1();

			var loggerDef = new LoggerDef()
			{
				LOG_TYPE_FILTER = (lt) => lt <= LogType.I,
				LOG_DIR_PATH = "LOG_110_010_020",
			};

			// 日時設定
			DateTimeForTest.SetVirtualDateTime(yesterday, loggerDef.TIME_ZONE_INFO);

			using var startEvent = new ManualResetEventSlim();

			using(Logger.CreateLogger(loggerDef))
			{
				Task[] tasks = new Task[16];

				for(int i = 0 ; i < tasks.Length ; i++)
				{
					string name = $"Thread[{i + 1}]";
					tasks[i] = Task.Run(() => { logWriteLoop(name); });
				}

				Thread.Sleep(300);

				// 一斉に処理を開始させる
				startEvent.Set();

				// タスクの終了を待つ
				Task.WaitAll(tasks);
			}

			// 設定した時間をリセットする
			DateTimeForTest.ResetVirtualDateTime();

			// メモ帳起動（２つのファイルをパラメータで渡せないらしい）
			var notepad = Process.Start(new ProcessStartInfo
			{
				FileName = "notepad.exe",
				Arguments = $"\"{Util.MakeFilePath(loggerDef, today: yesterday)}\"",
				UseShellExecute = false,
			});
			var notepad2 = Process.Start(new ProcessStartInfo
			{
				FileName = "notepad.exe",
				Arguments = $"\"{Util.MakeFilePath(loggerDef, today: today)}\"",
				UseShellExecute = false,
			});
			notepad?.WaitForExit();
			notepad2?.WaitForExit();


			// 残す必要がないので作成したログフォルダを削除
			Directory.Delete(loggerDef.LOG_DIR_PATH, true);


			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			// スレッド処理
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			void logWriteLoop(string name)
			{
				if(name == "Thread[4]")
				{   // このスレッドに日付変更を行わせる
					startEvent.Wait();
					for(int i = 0 ; i < 20 ; i++)
					{
						LOG.Write(I, $"{name} > TEST({i})");
					}

					DateTimeForTest.SetVirtualDateTime(today, loggerDef.TIME_ZONE_INFO);

					for(int i = 20 ; i < 100 ; i++)
					{
						LOG.Write(I, $"{name} > TEST({i})");
					}
				} else
				{
					startEvent.Wait();
					for(int i = 0 ; i < 100 ; i++)
					{
						LOG.Write(I, $"{name} > TEST({i})");
					}
				}
			}
		}

		//-------------------------------------------------------------------
		/// ログ内容・文字コード
		/// （別提供の試験項目ドキュメント参照）
		//-------------------------------------------------------------------
		[Fact]
		[Trait("FullAuto", "false")]
		public void ID_110_010_030_XXX()
		{
			var loggerDef1 = new LoggerDef()
			{
				LOG_TYPE_FILTER = (lt) => lt <= LogType.I,
				LOG_DIR_PATH = "LOG_110_010_030_010",
				LOG_TEXT_ENCODING = Encoding.UTF8,
			};
			var loggerDef2 = new LoggerDef()
			{
				LOG_TYPE_FILTER = (lt) => lt <= LogType.I,
				LOG_DIR_PATH = "LOG_110_010_030_020",
				LOG_TEXT_ENCODING = Encoding.Unicode,
			};
			var loggerDef3 = new LoggerDef()
			{
				LOG_TYPE_FILTER = (lt) => lt <= LogType.I,
				LOG_DIR_PATH = "LOG_110_010_030_030",
				// 環境によってはShift-JISが使えない可能性がある
				LOG_TEXT_ENCODING = CodePagesEncodingProvider.Instance.GetEncoding(932) ?? Encoding.Default,
			};
			Setup.InitType1(loggerDef1.LOG_DIR_PATH);
			using(Logger.CreateLogger(loggerDef1))
			{
				LOG.Write(I, "UTF-8");
			}
			Setup.InitType1(loggerDef2.LOG_DIR_PATH);
			using(Logger.CreateLogger(loggerDef2))
			{
				LOG.Write(I, "UTF-16");
			}
			Setup.InitType1(loggerDef3.LOG_DIR_PATH);
			using(Logger.CreateLogger(loggerDef3))
			{
				LOG.Write(I, "Shift-JIS");
			}

			// メモ帳起動
			var notepad1= Process.Start(new ProcessStartInfo
			{
				FileName = "notepad.exe",
				Arguments = $"\"{Util.MakeFilePath(loggerDef1)}\"",
				UseShellExecute = false,
			});
			var notepad2 = Process.Start(new ProcessStartInfo
			{
				FileName = "notepad.exe",
				Arguments = $"\"{Util.MakeFilePath(loggerDef2)}\"",
				UseShellExecute = false,
			});
			var notepad3 = Process.Start(new ProcessStartInfo
			{
				FileName = "notepad.exe",
				Arguments = $"\"{Util.MakeFilePath(loggerDef3)}\"",
				UseShellExecute = false,
			});

			notepad1?.WaitForExit();
			notepad2?.WaitForExit();
			notepad3?.WaitForExit();

			Directory.Delete(loggerDef1.LOG_DIR_PATH, true);
			Directory.Delete(loggerDef2.LOG_DIR_PATH, true);
			Directory.Delete(loggerDef3.LOG_DIR_PATH, true);
		}

		//-------------------------------------------------------------------
		/// ログ内容・デバッグ出力
		/// （別提供の試験項目ドキュメント参照）
		//-------------------------------------------------------------------
		[Fact]
		[Trait("FullAuto", "false")]
		public void ID_110_010_030_040()
		{
			var loggerDef = new LoggerDef()
			{
				LOG_TYPE_FILTER = (lt) => (FILTER_RELEASE & lt) != 0,
				LOG_DIR_PATH = "LOG_110_010_040",
				DEUBG_WRITE = true,
			};
			Setup.InitType1(loggerDef.LOG_DIR_PATH);

			using(Logger.CreateLogger(loggerDef))
			{
				LOG.Write(E, "エラー1行目", "2行目\n３行目\n④行");
			}

			// メモ帳起動
			var notepad = Process.Start(new ProcessStartInfo
			{
				FileName = "notepad.exe",
				Arguments = $"\"{Util.MakeFilePath(loggerDef)}\"",
				UseShellExecute = false,
			});

			// Notpadを開いている間本メソッドは終了しない。
			// その間に、システムでプロセスIDを確認すること。
			notepad?.WaitForExit();

			Directory.Delete(loggerDef.LOG_DIR_PATH, true);
		}

		//-------------------------------------------------------------------
		/// ログ内容・デバッグ出力
		/// （別提供の試験項目ドキュメント参照）
		//-------------------------------------------------------------------
		[Fact]
		[Trait("FullAuto", "false")]
		public void ID_110_010_030_050()
		{
			var loggerDef = new LoggerDef()
			{
				LOG_TYPE_FILTER = (lt) => (FILTER_RELEASE & lt) != 0,
				LOG_DIR_PATH = "LOG_110_010_050",
				DEUBG_WRITE = false,
			};
			Setup.InitType1(loggerDef.LOG_DIR_PATH);

			using(Logger.CreateLogger(loggerDef))
			{
				LOG.Write(E, "エラー1行目", "2行目\n３行目\n④行");
			}

			// メモ帳起動
			var notepad = Process.Start(new ProcessStartInfo
			{
				FileName = "notepad.exe",
				Arguments = $"\"{Util.MakeFilePath(loggerDef)}\"",
				UseShellExecute = false,
			});

			// Notpadを開いている間本メソッドは終了しない。
			// その間に、システムでプロセスIDを確認すること。
			notepad?.WaitForExit();

			Directory.Delete(loggerDef.LOG_DIR_PATH, true);
		}

#endif
	}
}
