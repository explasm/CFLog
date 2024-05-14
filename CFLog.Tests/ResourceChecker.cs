//***************************************************************************
// Copyright (c) Takahiro Fukushima All rights reserved.
// Licensed under the MIT license.
//***************************************************************************

using System.Globalization;

namespace CFLog.Tests
{
	public class ResourceChecker
	{
		//-------------------------------------------------------------------
		/// リソース・テキストリソース・定義内容
		/// 日本語リソース
		//-------------------------------------------------------------------
		[Theory]
		[Trait("FullAuto", "true")]
		[MemberData(nameof(GetTextResourceData))]
		public void ID_030_010_010_010(string key, string _, string ja )
		{
			CultureInfo culture = new("ja-JP");	// 日本語
			string? text = MessageResource.ResourceManager.GetString(key,culture);
			Assert.Equal(ja, text);
		}

		//-------------------------------------------------------------------
		/// リソース・テキストリソース・定義内容
		/// 英語リソース
		//-------------------------------------------------------------------
		[Theory]
		[Trait("FullAuto", "true")]
		[MemberData(nameof(GetTextResourceData))]
		public void ID_030_010_010_020(string key, string en, string _)
		{
			CultureInfo culture = new("en");	// 日本語以外
			string? text = MessageResource.ResourceManager.GetString(key, culture);
			Assert.Equal(en, text);
		}

		// テキストリソースの内容を定義
		public static IEnumerable<object[]> GetTextResourceData()
		{
			return new List<object[]>
			{
				// [0]キーワード、[1]英語テキスト、[2]日本語テキスト
				new object[] { "AlreadyInitializedError",
					"CreateLogger() has already been executed.",
					"既にCreateLogger()は実施されています。" },
				new object[] { "CannotOpenLogfileError",
					"Could not open log file \"{0}\".",
					"ログファイル {0} はオープンできませんでした。" },
				new object[] { "CompareFilesDebug",
					"Compare file [{0}] in folder with longest stored file name [{1}].",
					"フォルダ中のファイル[{0}]と保管最長ファイル名[{1}]を比較します。" },
				new object[] { "DeletedFileInfo",
					"Deleted log file [{0}].",
					"ログファイル[{0}]を削除しました。" },
				new object[] { "DeleteFailedError",
					"Failed to delete log file [{0}].",
					"ログファイル[{0}]の削除に失敗しました。" },
				new object[] { "DeleteTargetDebug",
					"File [{0}] in the folder is subject to deletion.",
					"フォルダ中のファイル[{0}]は削除対象です。" },
				new object[] { "DoNotDeleteInfo",
					"The deletion process was not performed because the specified number of log retention days was less than 0.",
					"指定されたログ保管日数が0以下のため、削除処理を行いませんでした。" },
				new object[] { "EnumrateFilesError",
					"Mainly, it failed to get the file list.",
					"主な原因はファイルリスト取得の失敗です。" },
				new object[] { "MaxProcess",
					"The number of processes has reached the maximum number ({0}).",
					"プロセス数が最大数({0})に達しました。" },
				new object[] { "NoDirectoryError",
					"Root directory specified or no directory specified.",
					"ルートディレクトリが指定されているか、ディレクトリの指定がありません。" },
				new object[] { "StartLogging",
					"=== Start logging ===",
					"=== ログを開始します ===" },
				new object[] { "StopLogging",
					"=== Stop logging ===",
					"=== ログを終了します ===" },
				new object[] { "SwitchStartLogging",
					"--- Continued logging from previous file ---",
					"--- 前日より引き継ぎました ---" },
				new object[] { "SwitchStopLogging",
					"--- Continued logging to next file ---",
					"--- 翌日へ引き継ぎます ---" },
				new object[] { "ToBeSaved",
					"The file [{0}] in the folder is to be saved.",
					"フォルダ中のファイル[{0}]は保管対象です。" },
				new object[] { "UninitializedError",
					"CreateLogger() has not been executed.",
					"CreateLogger()が実施されていません。" },
			};
		}
	}
}
