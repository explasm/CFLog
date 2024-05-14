//***************************************************************************
// Copyright (c) Takahiro Fukushima All rights reserved.
// Licensed under the MIT license.
//***************************************************************************

using CFLog.Tests.Support;
using System.Reflection;

namespace CFLog.Tests
{
	public class LogWriting
	{
#if DEBUG
		//-------------------------------------------------------------------
		/// ログ書出し・シングルスレッド・通常テキスト
		//-------------------------------------------------------------------
		[Theory]
		[Trait("FullAuto", "true")]
		/// １行書出し
		[InlineData("ID_020_010_010_010", "1行だけのログメッセージ", null, null)]
		/// ２行書出し（２行目のメッセージ開始位置がINDENT_SPACE.Lengthカラム目から開始になること）
		[InlineData("ID_020_010_010_020", "いちぎょうめ", "ここから２行目", null)]
		/// ３行書出し（２～３行目のメッセージ開始位置がINDENT_SPACE.Lengthカラム目から開始になること）
		[InlineData("ID_020_010_010_030", "一ぎょうめ", "ここから２行目", "３行目だよ")]
		public void ID_020_010_010_0XX(string _, string line1, string? line2, string? line3)
		{
			Setup.InitType1();

			var loggerDef = new LoggerDef()
			{
			};

			if(line2 != null && line3 != null)
			{
				// ３行目は２行目に統合
				line2 = line2 + Environment.NewLine + line3;
			}

			using(Logger.CreateLogger(loggerDef))
			{
				LOG.Write(I, line1, line2);
			}

			List<int> coulmns = new();
			Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), line1, coulmns) == 1);
			if(line3 != null)
			{
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), line3, coulmns) == 1);
				Assert.Equal<int>(loggerDef.INDENT_SPACE.Length, coulmns[1]);
			} else
			if(line2 != null)
			{
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), line2, coulmns) == 1);
				Assert.Equal<int>(loggerDef.INDENT_SPACE.Length, coulmns[1]);
			}

			// 残す必要がないので作成したログフォルダを削除
			Directory.Delete(loggerDef.LOG_DIR_PATH, true);
		}


		//-------------------------------------------------------------------
		/// ログ書出し・シングルスレッド・例外情報書出し
		//-------------------------------------------------------------------

		// 例外情報用カスタム例外クラス
		private class TestExp :Exception
		{
			public TestExp(in string mess) : base(mess) { }
			public TestExp(in string mess, Exception exp) : base(mess, exp) { }
		}

		[Theory]
		[Trait("FullAuto", "true")]
		/// 入れ子じゃない例外の書出し
		/// 1.例外メッセージの確認
		/// 2.例外その他の情報確認
		[InlineData("ID_020_010_020_010")]
		/// 入れ子になった例外情報の書出し（２重）
		/// 1.例外メッセージの確認
		/// 2.例外その他の情報確認
		[InlineData("ID_020_010_020_020")]
		/// 入れ子になった例外情報の書出し（３重）
		/// 1.例外メッセージの確認
		/// 2.例外その他の情報確認
		[InlineData("ID_020_010_020_030")]
		/// メッセージ以外を含まない例外情報の書出し
		/// 1.例外メッセージの確認
		/// 2.例外その他の情報確認
		[InlineData("ID_020_010_020_040")]
		public void ID_020_010_020_0XX(string ID)
		{
			Setup.InitType1();

			var loggerDef = new LoggerDef()
			{
				LOG_TYPE_FILTER = (lt) => lt <= LogType.E,
			};

			using(Logger.CreateLogger(loggerDef))
			{
				switch(ID)
				{
				case "ID_020_010_020_010":
					{
						try
						{
							MyThrow0();
						} catch(Exception ex)
						{
							LOG.Write(E, ID, ex);
						}
					}
					break;
				case "ID_020_010_020_020":
					{
						try
						{
							MyThrow1();
						} catch(Exception ex)
						{
							LOG.Write(E, ID, ex);
						}
					}
					break;
				case "ID_020_010_020_030":
					{
						try
						{
							MyThrow2();
						} catch(Exception ex)
						{
							LOG.Write(E, ID, ex);
						}
					}
					break;
				case "ID_020_010_020_040":
					{
						try
						{
							MyThrow1();
						} catch(Exception ex)
						{
							LOG.Write(E, ID, ex, false);
						}
					}
					break;
				}
			}

			const string info1 = "CFLog.Tests.LogWriting+TestExp:";
			const string info2 = "   at CFLog.Tests.LogWriting.";

			switch(ID)
			{
			case "ID_020_010_020_010":
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), "[MyThrow0]") == 1);
				// 以下Exception.ToString() に依存
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), info1) == 1);
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), info2) == 2);
				break;
			case "ID_020_010_020_020":
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), "[MyThrow1]<-[MyThrow0]") == 1);
				// 以下Exception.ToString() に依存
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), info1) == 2);
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), info2) == 4);
				break;
			case "ID_020_010_020_030":
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), "[MyThrow2]<-[MyThrow1]<-[MyThrow0]") == 1);
				// 以下Exception.ToString() に依存
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), info1) == 3);
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), info2) == 6);
				break;
			case "ID_020_010_020_040":
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), "[MyThrow1]<-[MyThrow0]") == 1);
				// 以下Exception.ToString() に依存
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), info1) == 0);
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), info2) == 0);
				break;
			}

			// 残す必要がないので作成したログフォルダを削除
			Directory.Delete(loggerDef.LOG_DIR_PATH, true);

			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			// 例外発生（ネストレベル0）
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			void MyThrow0()
			{
				throw new TestExp("MyThrow0");
			}

			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			// 例外発生（ネストレベル1）
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			void MyThrow1()
			{
				try
				{
					MyThrow0();
				} catch(Exception ex)
				{
					throw new TestExp("MyThrow1", ex);
				}
			}

			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			// 例外発生（ネストレベル2）
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			void MyThrow2()
			{
				try
				{
					MyThrow1();
				} catch(Exception ex)
				{
					throw new TestExp("MyThrow2", ex);
				}
			}
		}

		//-------------------------------------------------------------------
		/// ログ書出し・シングルスレッド・フィルタリング
		//-------------------------------------------------------------------
		[Theory]
		[Trait("FullAuto", "true")]
		/// LogType.I 以下とした場合
		[InlineData("ID_020_010_030_010")]
		/// LogType.W と LogType F だけとした場合
		[InlineData("ID_020_010_030_020")]
		public void ID_020_010_030_0XX(string ID)
		{
			Setup.InitType1();

			var loggerDef = new LoggerDef()
			{
				LOG_TYPE_FILTER = ID == "ID_020_010_030_010" ? (lt) => lt <= LogType.I : (lt) => ((LogType.W | LogType.F) & lt) != 0,
			};

			using(Logger.CreateLogger(loggerDef))
			{
				LOG.Write(T, "ロガータイプ T");
				LOG.Write(D, "ロガータイプ D");
				LOG.Write(I, "ロガータイプ I");
				LOG.Write(W, "ロガータイプ W");
				LOG.Write(E, "ロガータイプ E");
				LOG.Write(F, "ロガータイプ F");
			}

			if(ID == "ID_020_010_030_010")
			{
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), "ロガータイプ T") == 0);
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), "ロガータイプ D") == 0);
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), "ロガータイプ I") == 1);
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), "ロガータイプ W") == 1);
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), "ロガータイプ E") == 1);
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), "ロガータイプ F") == 1);
			} else
			{
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), "ロガータイプ T") == 0);
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), "ロガータイプ D") == 0);
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), "ロガータイプ I") == 0);
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), "ロガータイプ W") == 1);
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), "ロガータイプ E") == 0);
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef), "ロガータイプ F") == 1);
			}

			// 残す必要がないので作成したログフォルダを削除
			Directory.Delete(loggerDef.LOG_DIR_PATH, true);
		}

		//-------------------------------------------------------------------
		/// ログ書出し・シングルスレッド・ログファイル切替
		//-------------------------------------------------------------------
		[Theory]
		[Trait("FullAuto", "true")]
		/// 切替前のログファイルに切替メッセージが出力される
		[InlineData("ID_020_010_040_010")]
		/// 切替後のログファイルに切替メッセージが出力される切替メッセージの後にタイムゾーンIDが出力される
		[InlineData("ID_020_010_040_020")]
		/// Logger自身のメッセージをログに書き出さない設定にしたとき、切替[前]のログファイルに切替メッセージが出力されない
		[InlineData("ID_020_010_040_030")]
		/// Logger自身のメッセージをログに書き出さない設定にしたとき、切替[後]のログファイルに切替メッセージが出力されない
		[InlineData("ID_020_010_040_040")]
		public void ID_020_010_040_0XX(string ID)
		{
			Setup.InitType1();

			var loggerDef = new LoggerDef()
			{
				WRITE_START_AND_STOP_MESSAGE = ID switch
				{
					"ID_020_010_040_030" => false,
					"ID_020_010_040_040" => false,
					_ => true
				},
				TIME_ZONE_INFO = TimeZoneInfo.Utc,
			};

			// 日時設定
			DateTimeForTest.SetVirtualDateTime(new DateTime(2024, 4, 13, 23, 59, 59), loggerDef.TIME_ZONE_INFO);

			using(Logger.CreateLogger(loggerDef))
			{
				LOG.Write(I, "前日");

				// Logger内部の期限切れ削除処理が終了するまで十分の時間待つ
				// （終了するまでファイル切替処理が行われないため）
				Thread.Sleep(1000);

				// 日時変更
				DateTimeForTest.SetVirtualDateTime(new DateTime(2024, 4, 14, 0, 0, 0), loggerDef.TIME_ZONE_INFO);

				LOG.Write(I, "翌日");
			}

			switch(ID)
			{
			case "ID_020_010_040_010":
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef, today: new DateTime(2024, 4, 13)), MessageResource.SwitchStopLogging) == 1);
				break;
			case "ID_020_010_040_020":
				{
					List<int> columns = new List<int>();
					int messagePos = 0;
					int timeZoneIDPos = 0;
					Assert.True(Util.CountText(Util.MakeFilePath(loggerDef, today: new DateTime(2024, 4, 14)), MessageResource.SwitchStartLogging,columns) == 1);
					messagePos = columns[0];
					columns = new List<int>();
					Assert.True(Util.CountText(Util.MakeFilePath(loggerDef, today: new DateTime(2024, 4, 14)), "UTC", columns) == 1);
					timeZoneIDPos = columns[0];
					Assert.True(messagePos < timeZoneIDPos);
				}
				break;
			case "ID_020_010_040_030":
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef, today: new DateTime(2024, 4, 13)), MessageResource.SwitchStopLogging) == 0);
				break;
			case "ID_020_010_040_040":
				Assert.True(Util.CountText(Util.MakeFilePath(loggerDef, today: new DateTime(2024, 4, 14)), MessageResource.SwitchStartLogging) == 0);
				break;
			}

			// 残す必要がないので作成したログフォルダを削除
			Directory.Delete(loggerDef.LOG_DIR_PATH, true);

			// 設定した時間をリセットする
			DateTimeForTest.ResetVirtualDateTime();
		}

		//-------------------------------------------------------------------
		/// ログ書出し・シングルスレッド・ログファイル切替
		/// オープン直後にログ切替（ログファイル削除タスクが終了前に切替が発生する）しても問題なく動作する。日付変更後にまだ内部削除タスクが完了していなければ書出しは変更前に書き出され、内部削除処理タスクが完了後はファイル切替後のファイルに書き出される。
		//-------------------------------------------------------------------
		[Fact]
		[Trait("FullAuto", "true")]
		public void ID_020_010_040_050()
		{
			Setup.InitType1();

			var loggerDef = new LoggerDef()
			{
			};

			// 日時設定
			DateTimeForTest.SetVirtualDateTime(new DateTime(2024, 4, 13, 23, 59, 59), loggerDef.TIME_ZONE_INFO);

			using(Logger.CreateLogger(loggerDef))
			{
				LOG.Write(I, "前日");

				// 日時変更
				DateTimeForTest.SetVirtualDateTime(new DateTime(2024, 4, 14, 0, 0, 0), loggerDef.TIME_ZONE_INFO);

				// この内容は内部の期限切れ削除処理が完了していないため、実際には前日のファイルに記載されるはず
				// （書時間に依存しているため、削除処理が高速になった場合失敗する可能性がある）
				LOG.Write(I, "翌日1");

				// Logger内部の期限切れ削除処理が終了するまで十分の時間待つ
				// （終了するまでファイル切替処理が行われないため）
				Thread.Sleep(1000);

				LOG.Write(I, "翌日2");
			}

			// 前日のファイルを確認
			Assert.True(Util.CountText(Util.MakeFilePath(loggerDef, today: new DateTime(2024, 4, 13)), "翌日1") == 1);
			// 翌日のファイルを確認
			Assert.True(Util.CountText(Util.MakeFilePath(loggerDef, today: new DateTime(2024, 4, 14)), "翌日2") == 1);

			// 残す必要がないので作成したログフォルダを削除
			Directory.Delete(loggerDef.LOG_DIR_PATH, true);

			// 設定した時間をリセットする
			DateTimeForTest.ResetVirtualDateTime();
		}

		//-------------------------------------------------------------------
		/// ログ書出し・シングルスレッド・未初期化例外発生
		/// CreateLogger()未実施でログ書き出しを行う
		//-------------------------------------------------------------------
		[Fact]
		[Trait("FullAuto", "true")]
		public void ID_020_010_050_010()
		{
			// 初期化せずにログ出力
			var lex = Assert.Throws<Logger.LoggerInitException>(
				() => { LOG.Write(F,"書き出されないログ"); }
			);
			// 例外メッセージを確認
			Assert.Equal(MessageResource.UninitializedError, lex.Message);
		}

		//-------------------------------------------------------------------
		/// ログ書出し・シングルスレッド・書出し中の例外発生
		/// 削除処理終息後にログファイルストリームをクローズさせ、次のログ書き出しで例外を発生させる。
		//-------------------------------------------------------------------
		[Fact]
		[Trait("FullAuto", "true")]
		public void ID_020_010_050_020()
		{
			Setup.InitType1();

			var loggerDef = new LoggerDef()
			{
				WRITE_START_AND_STOP_MESSAGE = false,	// 終了メッセージを出そうとして例外が発生するので false に設定
			};

			using(var logger = Logger.CreateLogger(loggerDef))
			{

				LOG.Write(I, "TEST01");

				// 期限切れファイル削除処理が終息するのを待つ。
				Thread.Sleep(1000);

				// Write() で例外が発生するようにストリームをクローズする
				var _sw = typeof(Logger).GetField("_sw", BindingFlags.NonPublic | BindingFlags.Instance);
				StreamWriter? sw = (StreamWriter?)_sw?.GetValue(logger);
				sw?.Close();
				var _fs = typeof(Logger).GetField("_fs", BindingFlags.NonPublic | BindingFlags.Instance);
				FileStream? fs = (FileStream?)_fs?.GetValue(logger);
				fs?.Close();

				var lex = Assert.Throws<Logger.LoggerWriteException>(
					() => { LOG.Write(I, "例外が発生して書き出されないメッセージ"); }
				);

				// Logger.Dispose() で例外が発生しないようにしておく
				_sw?.SetValue(logger,null);
				_fs?.SetValue(logger,null);
			}

			// 残す必要がないので作成したログフォルダを削除
			Directory.Delete(loggerDef.LOG_DIR_PATH, true);
		}

		//-------------------------------------------------------------------
		/// ログ書出し・マルチルスレッド・多スレッドからの同時出力
		/// 16(多数)スレッドからログ書き出しを頻繁に行い、エラーにならないこと。
		//-------------------------------------------------------------------
		[Fact]
		[Trait("FullAuto", "true")]
		public void ID_020_020_010_010()
		{
			/* xUnitは例外を感知するとエラー扱いになるため、
			** あえてここでは Assert を使用していない
			*/


			Setup.InitType1();

			var loggerDef = new LoggerDef()
			{
				LOG_TYPE_FILTER = (lt) => lt <= LogType.I,
			};

			using var startEvent = new ManualResetEventSlim();

			using(Logger.CreateLogger(loggerDef))
			{
				Task[] tasks = new Task[16];

				for(int i = 0; i < tasks.Length; i++)
				{
					string name = $"Thread[{i+1}]";
					tasks[i] = Task.Run(() => { logWriteLoop(name); });
				}

				Thread.Sleep(100);

				// 一斉に処理を開始させる
				startEvent.Set();

				// タスクの終了を待つ
				Task.WaitAll(tasks);
			}

			// 残す必要がないので作成したログフォルダを削除
			Directory.Delete(loggerDef.LOG_DIR_PATH, true);

			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			// スレッド処理
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			void logWriteLoop(string name)
			{
				startEvent.Wait();
				for(int i = 0 ; i < 100 ; i++)
				{
					LOG.Write(I, $"{name} > TEST({i})");
				}
			}
		}

		//-------------------------------------------------------------------
		/// ログ書出し・マルチルスレッド・ログファイル切替
		/// 16(多数)のスレッドからログ書き出しが連続している最中に日付が変わり、ログファイルが切り替わってもエラーにならない
		//-------------------------------------------------------------------
		[Fact]
		[Trait("FullAuto", "true")]
		public void ID_020_020_010_020()
		{
			/* xUnitは例外を感知するとエラー扱いになるため、
			** あえてここでは Assert を使用していない
			*/

			Setup.InitType1();

			var loggerDef = new LoggerDef()
			{
			};

			// 日時設定
			DateTimeForTest.SetVirtualDateTime(new DateTime(2024, 4, 13, 23, 59, 50), loggerDef.TIME_ZONE_INFO);

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

			// 残す必要がないので作成したログフォルダを削除
			Directory.Delete(loggerDef.LOG_DIR_PATH, true);

			// 設定した時間をリセットする
			DateTimeForTest.ResetVirtualDateTime();

			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			// スレッド処理
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			void logWriteLoop(string name)
			{
				if(name == "Thread[4]")
				{	// このスレッドに日付変更を行わせる
					startEvent.Wait();
					for(int i = 0 ; i < 20 ; i++)
					{
						LOG.Write(I, $"{name} > TEST({i})");
					}

					DateTimeForTest.SetVirtualDateTime(new DateTime(2024, 4, 14, 0, 0, 0), loggerDef.TIME_ZONE_INFO);

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
#endif
	}
}
