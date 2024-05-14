//***************************************************************************
// Copyright (c) Takahiro Fukushima All rights reserved.
// Licensed under the MIT license.
//***************************************************************************

namespace CFLog
{
#if DEBUG
	//=======================================================================
	/// <summary>
	/// テストでOSの時間設定変更せずに日時変更を伴う試験を行うためのクラス
	/// </summary>
	/// <remarks>DateTime.Now と DateTime.Today だけを置き換える</remarks>
	//=======================================================================
	internal static class DateTimeForTest
	{
		// システム日時と試験での設定日時との差分
		private static TimeSpan _offset = TimeSpan.Zero;

		//-------------------------------------------------------------------
		/// <summary>
		/// 試験用の仮想日時を設定 
		/// </summary>
		/// <param name="dateTime">仮想設定日時</param>
		/// <param name="timeZoneInfo">LoggerDefと同じタイムゾーンを設定のこと</param>
		//-------------------------------------------------------------------
		public static void SetVirtualDateTime(DateTime dateTime, TimeZoneInfo timeZoneInfo)
		{
			WorldDateTime.SetTimeZoneInfo(timeZoneInfo);
			_offset = dateTime - WorldDateTime.Now;
		}

		//-------------------------------------------------------------------
		/// <summary>
		/// 設定時間をリセットする
		/// </summary>
		//-------------------------------------------------------------------
		public static void ResetVirtualDateTime()
		{
			_offset = TimeSpan.Zero;
		}

		//-------------------------------------------------------------------
		/// <summary>
		/// テスト用に設定された時間を反映した日時を返す
		/// </summary>
		//-------------------------------------------------------------------
		public static DateTime Now { get { return WorldDateTime.Now + _offset; } }

		//-------------------------------------------------------------------
		/// <summary>
		/// テスト用に設定された時間を反映した日を返す
		/// </summary>
		//-------------------------------------------------------------------
		public static DateTime Today { get { return (WorldDateTime.Now + _offset).Date; } }

		//-------------------------------------------------------------------
		/// <summary>
		/// タイムゾーンの設定
		/// </summary>
		/// <param name="timeZoneInfo">タイムゾーン</param>
		//-------------------------------------------------------------------
		public static void SetTimeZoneInfo(TimeZoneInfo timeZoneInfo)
		{
			WorldDateTime.SetTimeZoneInfo(timeZoneInfo);
		}
	}
#endif
}
