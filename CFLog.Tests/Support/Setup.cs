//***************************************************************************
// Copyright (c) Takahiro Fukushima All rights reserved.
// Licensed under the MIT license.
//***************************************************************************

using System.Diagnostics;
using static CFLog.Logger;

namespace CFLog.Tests.Support
{
	internal class Setup
	{

		/// <summary>
		/// 初期化タイプ１
		/// </summary>
		/// <param name="logDir">ログ出力フォルダパス</param>
		/// <remarks>ログ出力先フォルダ削除</remarks>
		public static void InitType1(string? chgLogDir = null)
		{
			var defaultLoggerDef = new LoggerDef();

			string logDir = chgLogDir ?? defaultLoggerDef.LOG_DIR_PATH;

			// ログ出力フォルダを削除
			if(Directory.Exists(logDir))
			{
				try
				{
					Directory.Delete(logDir, true);
				} catch(IOException ioEx)
				{
					Debug.WriteLine(ioEx);
				}
			}

			MessageResource.Culture = null;
		}

		/// <summary>
		/// 初期化タイプ2
		/// </summary>
		public static void InitType2()
		{
			MessageResource.Culture = null;
		}
	}
}
