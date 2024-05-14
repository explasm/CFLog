//***************************************************************************
// Copyright (c) Takahiro Fukushima All rights reserved.
// Licensed under the MIT license.
//***************************************************************************

using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;

namespace CFLog.Tests.Support
{
	internal class Util
	{
		//-------------------------------------------------------------------------
		/// <summary>
		/// ファイルフルパス名の作成
		/// </summary>
		/// <param name="loggerDef">Logger用パラメータ</param>
		/// <returns>作成したファイルフルパス名</returns>
		//-------------------------------------------------------------------------
		public static string MakeFilePath(
			in LoggerDef loggerDef,
			int processNumber = 1,
			DateTime? today = null,
			bool isOnlyFilename = false)
		{
			// ファイル名を再現
			var dateTime = today ?? TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, loggerDef.TIME_ZONE_INFO).Date;
			var dateStr = dateTime.ToString("yyyyMMdd");
			string fileName = loggerDef.FILE_PREFIX + dateStr + loggerDef.FILE_SUFFIX;
			string dirPath = Path.GetFullPath(loggerDef.LOG_DIR_PATH);
			if(loggerDef.ALLOW_MULTIPLE_PROCESSES)
				dirPath += $"\\{processNumber}";
			string logFilePath = Path.Combine(dirPath, fileName);

			if(isOnlyFilename)
				return fileName;
			else
				return logFilePath;
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// ログフォルダの権限をチェックする
		/// </summary>
		/// <param name="loggerDef">権限内容とログフォルダ情報</param>
		/// <returns>正しければtrue</returns>
		//-------------------------------------------------------------------------
		public static bool ChkDirAccessRule(in LoggerDef loggerDef)
		{
			bool result = false;

			if(loggerDef.DIR_RIGHTS_TARGET != null)
			{
				var dirPath = Path.GetFullPath(loggerDef.LOG_DIR_PATH);
				result = ChkDirAccessRule(dirPath, loggerDef.DIR_RIGHTS_TARGET, loggerDef.DIR_RIGHTS);

				// マルチプロセス許可？
				if(result && loggerDef.ALLOW_MULTIPLE_PROCESSES)
				{
					// プロセス番号のサブフォルダも確認する
					string dirSubPath = Path.Combine(dirPath, "1");
					result = ChkDirAccessRule(dirSubPath, loggerDef.DIR_RIGHTS_TARGET, loggerDef.DIR_RIGHTS);
				}
			}

			return result;
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// ログフォルダの権限をチェックする
		/// </summary>
		/// <param name="dirPath"></param>
		/// <param name="dirRightsTarget"></param>
		/// <param name="dirRights"></param>
		/// <returns></returns>
		//-------------------------------------------------------------------------
		private static bool ChkDirAccessRule(
			string dirPath,
			NTAccount dirRightsTarget,
			FileSystemRights dirRights)
		{
			bool result = false;

			DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
			DirectorySecurity dirSecurity = dirInfo.GetAccessControl();
			foreach(FileSystemAccessRule r in dirSecurity.GetAccessRules(true, true, typeof(NTAccount)))
			{
				if(r.IdentityReference != null)
				{
					if(((NTAccount)r.IdentityReference).Value == dirRightsTarget.Value)
					{
						Debug.Write((r.IdentityReference as NTAccount)?.Value ?? "(null)");
						Debug.WriteLine($": {Convert.ToString((uint)r.FileSystemRights, 2)} & {Convert.ToString((uint)dirRights, 2)}");

						if(
							((uint)r.FileSystemRights & (uint)dirRights) == (uint)dirRights)
						{
							result = true;
						}
					}
				}
			}

			return result;
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// ログフォルダの権限を設定する
		/// </summary>
		//-------------------------------------------------------------------------
		public static void SetDirAccessRuleToNTUsers(
			string dirPath,
			FileSystemRights dirRights,
			AccessControlType act,
			bool isAdd = true,
			bool inherit = true)
		{
			FileSystemAccessRule rule = new(
				new NTAccount(@"BUILTIN\Users"),
				dirRights,
				inherit ? InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit : InheritanceFlags.None,
				PropagationFlags.None,
				act);

			DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
			DirectorySecurity dirSecurity = dirInfo.GetAccessControl();
			if(isAdd)
				dirSecurity.AddAccessRule(rule);
			else
				dirSecurity.RemoveAccessRule(rule);

			FileSystemAclExtensions.SetAccessControl(dirInfo, dirSecurity);
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// ファイルの権限を設定する
		/// </summary>
		//-------------------------------------------------------------------------
		public static void SetFileAccessRuleToNTUsers(
			string filePath,
			FileSystemRights fileRights,
			AccessControlType act,
			bool isAdd = true)
		{
			FileSystemAccessRule rule = new(
				new NTAccount(@"BUILTIN\Users"),
				fileRights,
				InheritanceFlags.None,
				PropagationFlags.None,
				act);

			FileInfo fileInfo = new (filePath);
			FileSecurity fileSecurity = fileInfo.GetAccessControl();
			if(isAdd)
				fileSecurity.AddAccessRule(rule);
			else
				fileSecurity.RemoveAccessRule(rule);

			FileSystemAclExtensions.SetAccessControl(fileInfo, fileSecurity);
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// ファイルサイズの取得
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		//-------------------------------------------------------------------------
		public static long GetFileSize(string filePath)
		{
			var fi = new FileInfo(filePath);
			return fi?.Length ?? 0;
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// テキストファイル中に指定された文字列がいくつ含まれるかをカウントする
		/// </summary>
		/// <param name="filePath">検索対象ファイルパス</param>
		/// <param name="text">検索テキスト</param>
		/// <param name="coulmns">開始位置リスト</param>
		/// <returns>カウント数</returns>
		//-------------------------------------------------------------------------
		public static int CountText(string filePath,string text,List<int>? coulmns = null)
		{
			int result = 0;
			List<string?> textLines = new ();

			try
			{
				string allText = string.Empty;
				using(var sr = new StreamReader(filePath))
				{
					string? textLine;
					while((textLine = sr.ReadLine()) != null)
					{
						var pos = textLine.IndexOf(text);
						if(pos >= 0)
						{
							result++;
							coulmns?.Add(pos);
						}
					}
				}

			} catch
			{
				result = -1;
			}

			return result;
		}
	}
}
