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
			// コマンドラインパラメータ解析
			int subProcessNumber = parseCommandLine();

			try
			{
				if(subProcessNumber == 0)
				{   /////////////////////////////////////////////////////////
					// 通常起動された場合
					/////////////////////////////////////////////////////////

					ApplicationConfiguration.Initialize();

					// ログ機能設定
					var logDef = new LoggerDef()
					{
						// ログファイル名プレフィックスの設定
						FILE_PREFIX = "CFLogSampleForm",
#if DEBUG
						// ログタイプデバッグ時用のフィルタ設定
						LOG_TYPE_FILTER = (lt) => (lt & LogType.FILTER_DEBUG) != 0,
#else
						// ログタイプリリース時用のフィルタ設定
						LOG_TYPE_FILTER = (lt) => (lt & LogType.FILTER_RELEASE) != 0,
#endif
						// ログ保管日数を3日に設定
						STORAGE_DAYS = 3,
					};

					// ログ開始
					using(CreateLogger(logDef))
					{
						// アプリケーション開始
						Application.Run(new CFLogSampleForm());
					}
				} else
				{   /////////////////////////////////////////////////////////
					// サブプロセスとして起動された場合
					// フォーム画面の[マルチプロセス出力]ボタンが押下された際に
					// 起動された場合の胥吏
					/////////////////////////////////////////////////////////

					// ログ機能設定
					var logDef = new LoggerDef()
					{
						// ログファイル名プレフィックスの設定
						FILE_PREFIX = "SubProcess",

						// 「情報」以下を出力
						LOG_TYPE_FILTER = (lt) => lt <= LogType.I,

						// 同一実行モジュールの複数プロセスを許可
						ALLOW_MULTIPLE_PROCESSES = true,

						// ログ開始／終了メッセージをログに書き出さない
						WRITE_START_AND_STOP_MESSAGE = false,
					};

					// ログ開始
					using(CreateLogger(logDef))
					{
						// サブプロセスとして起動された際の処理

						for(int i = 0 ; i < 10 ; i++)
						{
							LOG.Write(I, $"サブプロセス(Sub process) <{subProcessNumber}> [{i}]");
							Thread.Sleep(200);
						}
					}

					// ★約２秒でプロセスを終了する
				}
#if true	// Loggerの例外クラスにルートクラスを追加したことで1つの例外クラスでcatchできるようにした（2024.7.3）
			} catch(LoggerException ex)
			{   // 当然ながら、Loggerが出す例外はログに書き出せない
				System.Diagnostics.Debug.WriteLine(ex.Message);
				MessageBox.Show(ex.Message);
			}
#else
			} catch(LoggerInitException ex)
			{	// 当然ながら、Loggerが出す例外はログに書き出せない
				System.Diagnostics.Debug.WriteLine(ex.Message);
				MessageBox.Show(ex.Message);
			} catch(LoggerWriteException ex)
			{	// ログ書出しの際に例外が発生した際にも処理を中断する場合の例
				System.Diagnostics.Debug.WriteLine(ex.Message);
				MessageBox.Show(ex.Message);
			}
#endif

			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			// コマンドライン引数で、サブプロセス番号を取得する
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			int parseCommandLine()
			{
				/*
				 * この処理は CFLogSampleForm クラスの ByMPButton_Click() から
				 * 起動される際に指定されるコマンドラインパラメータを解析するためだけの
				 * ものです。
				 * 同一のログ出力を行うプロセスを複数起動する際のサンプルとしてこの機能を
				 * 実装しています。
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
