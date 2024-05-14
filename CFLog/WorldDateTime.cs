//***************************************************************************
// Copyright (c) Takahiro Fukushima All rights reserved.
// Licensed under the MIT license.
//***************************************************************************

namespace CFLog
{
	//=======================================================================
	/// <summary>
	/// タイムゾーン設定に対応した時間クラス
	/// </summary>
	/// <remarks>DateTime.Now と DateTime.Today だけを置き換える</remarks>
	//=======================================================================
	internal class WorldDateTime
	{
		private static TimeZoneInfo _timeZoneInfo = TimeZoneInfo.Utc;

		//-------------------------------------------------------------------
		/// <summary>
		/// タイムゾーンの設定
		/// </summary>
		/// <param name="timeZoneInfo">タイムゾーン</param>
		//-------------------------------------------------------------------
		public static void SetTimeZoneInfo(TimeZoneInfo timeZoneInfo)
		{
			_timeZoneInfo = timeZoneInfo;
		}

		//-------------------------------------------------------------------
		/// <summary>
		/// 設定されたタイムゾーンの日時を返す
		/// </summary>
		//-------------------------------------------------------------------
		public static DateTime Now
		{
			get
			{
				return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,_timeZoneInfo);
			}
		}

		//-------------------------------------------------------------------
		/// <summary>
		/// 設定されたタイムゾーンの日を返す
		/// </summary>
		//-------------------------------------------------------------------
		public static DateTime Today
		{
			get
			{
				return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZoneInfo).Date;
			}
		}
	}
}
