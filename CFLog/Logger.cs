//***************************************************************************
// Copyright (c) Takahiro Fukushima All rights reserved.
// Licensed under the MIT license.
//***************************************************************************
//===========================================================================
// "CFLog" ログ出力
//---------------------------------------------------------------------------
// 例外発生： LoggerInitException, LoggerWriteException
//---------------------------------------------------------------------------
// ソースコードファイル名や行番号まで含めたログをファイルに出力する。
// ファイル名には日付が含まれていて、指定された保存期間（日数）を過ぎたものは使用開始時に
// 削除する。（設定日数が0以下の場合は削除処理を行わない）
// スレッドフリー、また、複数インスタンスを許容する。複数インスタンス（プロセス）の場合、
// プロセス起動順に数値（1～）のサブフォルダにログを出力する。
//===========================================================================
using System.Text;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Globalization;
using System.Runtime.Serialization;

#if DEBUG
using DT = CFLog.DateTimeForTest;
#else
using DT = CFLog.WorldDateTime;
#endif

namespace CFLog
{
	//=======================================================================
	/// <summary>
	/// ログ出力クラス（シングルトン）
	/// </summary>
	//=======================================================================
	public sealed class Logger :IDisposable
	{
		//===================================================================
		/// <summary>
		/// ログ種別定義
		/// </summary>
		/// <remarks>ビットマスク、または、レベル判定によるフィルタリングに対応</remarks>
		//===================================================================
#pragma warning disable CA1707 // 識別子はアンダースコアを含むことはできません
		public enum LogType : uint
		{
			/// <summary>トレース用（実装時用）</summary>
			T = 0b00100000,
			/// <summary>デバッグ用（試験時用）</summary>
			D = 0b00010000,
			/// <summary>情報</summary>
			I = 0b00001000,
			/// <summary>警告</summary>
			W = 0b00000100,
			/// <summary>復帰可能エラー</summary>
			E = 0b00000010,
			/// <summary>致命的エラー</summary>
			F = 0b00000001,

			/// <summary>リリースモードフィルタ</summary>
			FILTER_RELEASE = (F | E | W | I),

			/// <summary>デバッグモードフィルタ</summary>
			FILTER_DEBUG = (F | E | W | I | D),
		}
#pragma warning restore CA1707 // 識別子はアンダースコアを含むことはできません

		//===================================================================
		/// <summary>
		/// ログ種別による出力判定デリゲート定義
		/// </summary>
		/// <param name="logType">Logger.Write()で指定されるログ種別</param>
		/// <returns>trueで出力</returns>
		/// <remarks>LoggerDef.LOG_FILTERに設定することでカスタマイズされたログ種別の選別を行える</remarks>
		//===================================================================
		public delegate bool LogTypeFilter(LogType logType);

		//===================================================================
		/// <summary>
		/// CFLog の設定をまとめたクラス
		/// </summary>
		/// <remarks>
		/// 直接ここを編集するか、LoggerDef のコンストラクタで設定して
		/// Logger.CreateLogger() にパラメータで渡してください。
		/// </remarks>
		//===================================================================
#pragma warning disable CA1707 // 識別子はアンダースコアを含むことはできません
#pragma warning disable CA1805 // 不必要に初期化しません
		public class LoggerDef
		{
			//---------------------------------------------------------------
			/// <summary>ログ書き出しフォルダ</summary>
			/// <remarks>
			/// プロセスの実行時フォルダからの相対パス、または絶対パスを指定。
			/// ドライブのルートフォルダの指定はできません。
			/// </remarks>
			//---------------------------------------------------------------
			public string LOG_DIR_PATH { get; init; } = @".\Log";

			//---------------------------------------------------------------
			/// <summary>
			/// LOG_DIR_PATH をフルパスに変換しただけのもの
			/// </summary>
			/// <remarks>
			/// Loggerのコンストラクタで設定される。ユーザは直接設定できません。
			/// 本クラスコンストラクタで行わないのは、Loggerコンストラクタまでのタイミングに
			/// カレントディレクトリが変更される可能性への配慮。
			/// </remarks>
			//---------------------------------------------------------------
			internal string log_dir_full_path { get; set; } = string.Empty;

			//---------------------------------------------------------------
			/// <summary>ログファイル名プレフィクス</summary>
			/// <remarks>通常 CFLog を使用するアプリケーション名を設定してください</remarks>
			//---------------------------------------------------------------
			public string FILE_PREFIX { get; init; } = "CFLog";

			//---------------------------------------------------------------
			/// <summary>ログファイル名サフィックス</summary>
			//---------------------------------------------------------------
			public string FILE_SUFFIX { get; init; } = "Log.txt";

			//---------------------------------------------------------------
			/// <summary>ログ保存日数</summary>
			/// <remarks>0で無期限（削除しない）</remarks>
			//---------------------------------------------------------------
			public int STORAGE_DAYS { get; init; } = 7;

			//---------------------------------------------------------------
			/// <summary>ログ出力先フォルダの権限追加設定対象ユーザまたはグループ</summary>
			/// <remarks>
			/// nullを設定すると権限設定処理自体を行いません。
			/// IIS上で動作するものなどサービス型のアプリの場合はここでは設定せず、
			/// アプリインストール時にログ出力フォルダをあらかじめ作成して権限設定まで
			/// 行っておくことをお勧めします。
			/// </remarks>
			//---------------------------------------------------------------
			public NTAccount? DIR_RIGHTS_TARGET { get; init; } = new(@"BUILTIN\Users");     // 各ログインユーザが同一のログフォルダを使用するクライアントアプリの場合推奨
			//public NTAccount? DIR_RIGHTS_TARGET { get; init; } = null;								// 作成したフォルダの権限設定処理をしない

			//---------------------------------------------------------------
			/// <summary>ログ出力フォルダ作成時に設定する権限</summary>
			/// <remarks>
			/// 他のユーザが同一フォルダにログを出力する可能性がある場合は FullControl等 の指定が便利です。
			/// 厳密に必要最小限の設定をするのであれば、
			/// ・ディレクトリの読み取り権限
			/// ・ファイルの作成権限
			/// ・ファイルの書込権限
			/// ・ファイルの追記権限
			/// ・ファイルの削除権限
			/// ・マルチプロセスを許可する場合にはフォルダの作成権限
			/// 等が必要になることに注意してください。
			/// CFLogではフォルダ作成時にこの権限を設定しますので、
			/// 既にフォルダが存在する場合は権限の設定自体が行われないことにも注意してください。
			/// </remarks>
			//---------------------------------------------------------------
			public FileSystemRights DIR_RIGHTS { get; init; } = FileSystemRights.FullControl & ~FileSystemRights.ExecuteFile;
			//public FileSystemRights DIR_RIGHTS { get; init; } = FileSystemRights.FullControl;
			//public FileSystemRights DIR_RIGHTS { get; init; } = FileSystemRights.Modify;

			//---------------------------------------------------------------
			/// <summary>同一アプリケーションの複数起動許可</summary>
			/// <remarks>
			/// ★この場合の同一アプリとは同じログ出力フォルダ、かつ同じファイル名を使用するプロセスのことです。
			/// 複数起動を許可した場合、ログ出力フォルダにプロセスごとのサブフォルダが追加され、
			/// プロセスごとに振り分けられます。同じファイルを複数のプロセスが使用するわけではありません。
			/// これを許可せずに複数起動した場合、２番目以降のプロセスでログファイルのオープンが競合して
			/// エラーとなり、初期化時に例外が発生します。
			/// </remarks>
			//---------------------------------------------------------------
			public bool ALLOW_MULTIPLE_PROCESSES { get; init; } = false;

			//---------------------------------------------------------------
			/// <summary>複数起動の最大許可数</summary>
			/// <remarks>
			/// ALLOW_MULTIPLE_PROCESSES=true 時のオープンできるファイルを探す際の最大プロセス番号です。
			/// 同一ログファイル名を使用するプロセスが多数になるようなアプリケーションでは、
			/// FILE_PREFIX に個別の名前を設定するようにし、
			/// ALLOW_MULTIPLE_PROCESSES は false にしてください。
			/// Loggerクラスは ALLOW_MULTIPLE_PROCESSES=true の場合、プロセス番号1から
			/// 順にログファイルのオープンを試み、失敗するとプロセス番号（内部カウンタ）を
			/// インクリメントして再度オープンを試みるというループ処理を行います。この処理中は
			/// Mutexによる排他制御を行うので、同時に多くのプロセスを起動する場合、パフォーマンス
			/// のボトルネックになる可能性があります。
			/// </remarks>
			//---------------------------------------------------------------
			public int MAX_PROCESS_COUNT { get; init; } = 16;

			//---------------------------------------------------------------
			/// <summary>ログ種別フィルタ</summary>
			/// <remarks>ビットマスク判定または大小判定が可能</remarks>
			//---------------------------------------------------------------
#if DEBUG
			public LogTypeFilter LOG_TYPE_FILTER { get; init; } = (lt) => lt <= LogType.T;
			//public LogTypeFilter LOG_TYPE_FILTER { get; init; } = (lt) => true;
			//public LogTypeFilter LOG_TYPE_FILTER { get; init; } = (lt) => (LogType.FILTER_ALL & lt) != 0;
#else
			public LogTypeFilter LOG_TYPE_FILTER { get; init; } = (lt) => lt <= LogType.I;
			//public LogTypeFilter LOG_TYPE_FILTER { get; init; } = (lt) => (LogType.FILTER_RELEASE & lt) != 0;
#endif
			//---------------------------------------------------------------
			/// <summary>デバッグ出力フラグ</summary>
			/// <remarks>ログファイル出力の内容をデバッグ用にも出力する場合はtrueを指定(DEBUGシンボル定義時のみ有効）</remarks>
			//---------------------------------------------------------------
			public bool DEUBG_WRITE { get; init; } = true;

			//---------------------------------------------------------------
			/// <summary>開始終了ログメッセージ</summary>
			/// <remarks>
			/// Logger自身が書き出すメッセージは以下の５つがあり、
			/// １．ログ開始
			/// ２．ログ終了
			/// ３．ログ引継ぎ開始
			/// ４．ログ引継ぎ終了
			/// ５．期限切れログファイル削除に関するメッセージ
			/// １～４のOn/Offを制御します。
			/// </remarks>
			//---------------------------------------------------------------
			public bool WRITE_START_AND_STOP_MESSAGE { get; init; } = true;

			//---------------------------------------------------------------
			/// <summary>言語</summary>
			/// <remarks>
			/// Logger自身が出力する（例外やログ）メッセージの言語。
			/// null でOSの環境依存になります。
			/// あえて現在の環境であることを明示的に指定することも可能です。
			/// CFLogでは日本語と英語のテキストリソースが定義されていて、日本以外は英語になります。
			/// </remarks>
			//---------------------------------------------------------------
			public CultureInfo? CULTURE_INFO { get; init; } = CultureInfo.CurrentCulture;
			//public CultureInfo? CULTURE_INFO { get; init; } = new("ja-JP");

			//---------------------------------------------------------------
			/// <summary>ログテキストのエンコード</summary>
			//---------------------------------------------------------------
			public Encoding LOG_TEXT_ENCODING { get; init; } = Encoding.UTF8;
			//public Encoding LOG_TEXT_ENCODING  { get; init; } = Encoding.Unicode;
			//public Encoding LOG_TEXT_ENCODING  { get; init; } = CodePagesEncodingProvider.Instance.GetEncoding(932);


			//---------------------------------------------------------------
			/// <summary>ログに表示する日時やファイル管理に使用する時間地域情報</summary>
			/// <remarks>
			/// ログの行ヘッダに記録する以外に、ログファイル名や保管期限切れログファイル名の削除、
			/// および、日付越えによるログファイル切替に影響します。
			/// Time Zone ID 一覧についてはレジストリ
			/// "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones"
			/// を参照してください。
			/// </remarks>
			//---------------------------------------------------------------
			public TimeZoneInfo TIME_ZONE_INFO { get; init; } = TimeZoneInfo.Local;
			//public TimeZoneInfo TIME_ZONE_INFO { get; init; } = TimeZoneInfo.Utc;
			//public TimeZoneInfo TIME_ZONE_INFO { get; init; } = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
			//public TimeZoneInfo TIME_ZONE_INFO { get; init; } = TimeZoneInfo.FindSystemTimeZoneById("Easter Island Standard Time");

			//---------------------------------------------------------------
			/// <summary>行ヘッダの日時データフォーマット</summary>
			/// <remarks>
			/// DateTime.ToString()のパラメータとして使用
			/// </remarks>
			//---------------------------------------------------------------
			public string DATETIME_FORMAT { get; init; } = "yyyy-MM-dd HH:mm:ss.fff";
			//public string DATETIME_FORMAT { get; init; } = "yyyyMMdd HHmmss.fff";

			//---------------------------------------------------------------
			/// <summary>行ヘッダのプロセスIDフォーマット</summary>
			//---------------------------------------------------------------
			public string PROCESS_ID_FORMAT { get; init; } = "00000";

			//---------------------------------------------------------------
			/// <summary>行ヘッダのスレッドID（.NET由来）フォーマット</summary>
			//---------------------------------------------------------------
			public string THREAD_ID_FORMAT { get; init; } = "00";

			//---------------------------------------------------------------
			/// <summary>インデントスペース</summary>
			/// <remarks>
			/// DATETIME_FORMAT, PROCESS_ID_FORMAT, THREAD_ID_FORMAT
			/// 変更の際、文字数の変更分を反映させてください
			/// </remarks>
			//---------------------------------------------------------------
			public string INDENT_SPACE { get; init; } = string.Empty.PadRight(35);
			//public string INDENT_SPACE { get; init; } = string.Empty.PadRight(31);

#if DEBUG
			//***************************************************************
			// 試験用項目
			//***************************************************************

			/// <summary>
			/// Loggerコンストラクタ排他制御内のSleep()時間（ミリ秒）
			/// </summary>
			public int TEST_OPENWAIT { get; init; } = 0;
#endif
		}
#pragma warning restore CA1805 // 不必要に初期化しません
#pragma warning restore CA1707 // 識別子はアンダースコアを含むことはできません

		//===================================================================
		// Logger インスタンス管理
		//===================================================================

		//-------------------------------------------------------------------
		// 唯一のインスタンス（シングルトン）
		//-------------------------------------------------------------------
		static private Logger? _logger;

		//-------------------------------------------------------------------
		// コンストラクタ（Loggerユーザは CreateLogger() を使用します）
		//-------------------------------------------------------------------
		private Logger(
			in LoggerDef loggerDef)		// in: Logger設定情報
		{
			// 定義保持
			_def = loggerDef;

			// タイムゾーンの反映
			DT.SetTimeZoneInfo(_def.TIME_ZONE_INFO);

			checkDirPath();

			// プロセスIDは変化しないのでここで取得しておく
			_processID = Environment.ProcessId;

			// マルチプロセスが許可されている場合はプロセス間排他制御が必要
			using(Mutex? mutex = _def.ALLOW_MULTIPLE_PROCESSES ? new Mutex(false, MutexName) : null)
			{
				mutex?.WaitOne();
#if DEBUG
				// 排他制御試験のためのSleep
				Thread.Sleep(_def.TEST_OPENWAIT);
#endif
				try
				{
					// ファイルオープン(例外発生あり）
					Open();
				} finally
				{
					mutex?.ReleaseMutex();
				}
			}

			// 保管期間の過ぎたログファイルの削除
			DeleteOldLog();

			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			// Log出力フォルダのFULLパスへの変換とチェック
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			void checkDirPath()
			{
				try
				{
					// 定義で指定されたログ出力先Dirをフルパスに変換する
					_def.log_dir_full_path = Path.GetFullPath(_def.LOG_DIR_PATH);
				} catch
				{
					_def.log_dir_full_path = string.Empty;
				}

				// Path.GetPathRoot() はネットワークドフォルダに対して期待通りの動作をしないため処理を分ける
				string pathRoot; 
				// 先頭が @"\\" で始まるか？（ネットワークフォルダか？）
				if(_def.log_dir_full_path.Substring(0, 2) == @"\\")
				{
					pathRoot = @"\\";
				} else
				{
					pathRoot = Path.GetPathRoot(_def.log_dir_full_path) ?? "";
				}

				Debug.WriteLine($"DIR FULL PATH = [{_def.log_dir_full_path}]");
				Debug.WriteLine($"DIR PATH ROOT = [{pathRoot}]");

				// ログ出力先フォルダのチェック
				if(
					// 空白が指定されいる場合 または
					string.IsNullOrWhiteSpace(_def.LOG_DIR_PATH) || string.IsNullOrWhiteSpace(_def.log_dir_full_path) ||
					// ルートフォルダ（ネットドライブを含む）が指定されている
					(_def.log_dir_full_path.Length <= pathRoot.Length))
				{
					// ルートディレクトリへのログ書き出しはサポートしません
					throw new LoggerInitException(MessageResource.NoDirectoryError);
				}
			}
		}

		//-------------------------------------------------------------------
		/// <summary>
		/// シングルトンのインスタンス取得
		/// </summary>
		/// <exception cref="LoggerInitException">初期化されていない</exception>
		/// <remarks>使用例：Logger.LOG.Write(～)</remarks>
		//-------------------------------------------------------------------
		static public Logger LOG
		{
			get
			{
				if(_logger != null)
				{
					return _logger;
				} else
				{
					// 未初期化エラー
					throw new LoggerInitException(MessageResource.UninitializedError);
				}
			}
		}

		//-------------------------------------------------------------------
		/// <summary>
		/// シングルトンのLoggerインスタンスを作成する
		/// </summary>
		/// <param name="loggerDef?">Loggerへの設定値(null で既定値)</param>
		/// <returns>Loggerインスタンス</returns>
		/// <exception cref="LoggerInitException">既に初期化済み</exception>
		//-------------------------------------------------------------------
		static public Logger CreateLogger(in LoggerDef? loggerDef = null)
		{
			if(loggerDef?.CULTURE_INFO != null)
			{
				MessageResource.Culture = loggerDef.CULTURE_INFO;
			}

			if(_logger != null)
			{
				// 二重初期化エラー
				throw new LoggerInitException(MessageResource.AlreadyInitializedError);
			}

			_logger = new Logger(loggerDef?? new LoggerDef());
			return _logger;
		}

		//===================================================================
		// 内部メンバ
		//===================================================================
		private LoggerDef _def;								// 定義パラメータ
		private FileStream? _fs;							// ログファイルストリーム
		private StreamWriter? _sw;							// ログファイルストリームライター
		private object _lockObject = new object();			// ログ書き出し排他制御用オブジェクト
		private readonly int _processID;					// プロセスID
		private DateTime _openDate;                         // ログファイルオープン日
		private RepeatableTask _deleteOldLogTask = new();	// 期限切れファイル削除タスク

		private string _dirPath = string.Empty;             // ログ出力フォルダ
		private string _fileName = string.Empty;            // ファイル名（フォルダ抜き）

		// Mutex名フォーマット
		private const string _MUTEX_NAME_FORMAT = @"Global\{0}_mutex_4FFDEDBFC8F741C6907CA9FCD7526942";

		private string MutexName							// ミューテックス名
		{
#if !MUTEX_NAME_BY_ONLYFILENAME
			// 同一ログ出力フォルダ＆ファイル名（日付部分を除く）を使用するプロセス間で
			// 排他制御を行うため必要な情報をまとめてハッシュ化してミューテックス名に使用する
#pragma warning disable CA1304 // CultureInfo を指定します
#pragma warning disable CA1305 // IFormatProvider を指定します
			get
			{
				using HashAlgorithm hashProvider = SHA512.Create();
				byte[] shaParam = Encoding.UTF8.GetBytes(
					Path.Combine(
						_def.log_dir_full_path.ToLower(),
						_def.FILE_PREFIX.ToLower() + _def.FILE_SUFFIX
					).ToLower()
				);
				byte[] hash = hashProvider.ComputeHash(shaParam);
                string hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();

				Debug.WriteLine(hashString);

				return string.Format(_MUTEX_NAME_FORMAT, hashString);
			}
#pragma warning restore CA1305 // IFormatProvider を指定します
#pragma warning restore CA1304 // CultureInfo を指定します
#else
			// 同一ログファイル名（日付部分を除く）を使用するプロセス間で
			// 排他制御を行うため必要な情報をそのまま文字列の一部としてミューテックス名とする
			get	{ return string.Format(_MUTEX_NAME_FORMAT, _def.FILE_PREFIX + _def.FILE_SUFFIX); }
#endif
		}

		//===================================================================
		// 内部メソッド
		//===================================================================

		//-------------------------------------------------------------------
		// ファイル名の作成
		//-------------------------------------------------------------------
		private string MakeFileName(			// ret:ファイル名
			in DateTime dateTime)				// in: ファイル名の一部に使用する日付（最小値の場合はワイルドカードを作成する）
		{
			string dateStr;
			if (dateTime != DateTime.MinValue)
			{
				// 日付を文字列にする
#pragma warning disable CA1305 // IFormatProvider を指定します
				dateStr = dateTime.ToString("yyyyMMdd");    // ファイル削除のファイル名ソートに影響するため、年月日の順位を変更してはならない
#pragma warning restore CA1305 // IFormatProvider を指定します
			} else
			{
				// ワイルドカードフィルター
				dateStr = "????????";
			}

			return _def.FILE_PREFIX + dateStr + _def.FILE_SUFFIX;
		}

		//-------------------------------------------------------------------
		// ログファイルオープン（この処理はスレッドセーフではない）
		// 同一プロセスからの複数オープンも当然非対応（シングルトンのため）
		//-------------------------------------------------------------------
		private void Open(
			in bool isForSwitch = false)		// in: ログファイル切替オープンならtrue
		{
			// オープン日の保存
			_openDate = DT.Today;

			// 今日の日付でファイル名を作成する
			_fileName = MakeFileName(_openDate);

			string dirPath = string.Empty;
			try
			{
				// 通常のオープン？
				if(!isForSwitch)
				{
					bool isRetry;
					int processNumber = 0;
					do
					{
						isRetry = false;

						// プロセスカウントが限界値に達したか？
						if(_def.ALLOW_MULTIPLE_PROCESSES && (++processNumber > _def.MAX_PROCESS_COUNT))
						{
							throw new LoggerInitException(string.Format(_def.CULTURE_INFO, MessageResource.MaxProcess, _def.MAX_PROCESS_COUNT));
						}

						// ディレクトリ名の作成
						dirPath = makeDirPath(processNumber);
						// 実際にディレクトリを作成
						bool dirCreated = makeDirectory(dirPath);

						try
						{
							openStream(isForSwitch, dirPath);
						} catch(IOException)
						{
							if(dirCreated || !_def.ALLOW_MULTIPLE_PROCESSES)
							{
								// 新たにディレクトリが作成されたにもかかわらずオープンエラーになったり、
								// マルチプロセスが許可されていない場合、オープンエラーで即終了
								throw;
							}
							isRetry = true;
						}

					} while(isRetry);   // マルチプロセスが許可されている場合、プロセス番号1から順にオープン可能なファイルを探索する（低パフォーマンス処理）

					// ここは正常処理なのでディレクトリパスを保存
					_dirPath = dirPath;
				} else
				{   // 切替によるオープン
					openStream(isForSwitch, _dirPath);
				}
			} catch(LoggerWriteException)
			{
				throw;
			} catch(LoggerInitException)
			{
				_fs = null;
				_sw = null;

				throw;
			} catch(Exception ex)
			{
				_fs = null;
				_sw = null;

				throw new LoggerInitException(string.Format(_def.CULTURE_INFO, MessageResource.CannotOpenLogfileError, Path.Combine(dirPath, _fileName)), ex);
			}

			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			// ディレクトリパスの作成
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			string makeDirPath(				// ret:ログ出力ディレクトリ
				int processNumber)			// in: プロセス番号
			{
				string dirPath = _def.log_dir_full_path;

				if(_def.ALLOW_MULTIPLE_PROCESSES)
				{
#pragma warning disable CA1305 // IFormatProvider を指定します
					// マルチプロセスが許可されている場合は、フォルダにプロセス番号を付加する
					dirPath = Path.Combine(dirPath, processNumber.ToString());
#pragma warning restore CA1305 // IFormatProvider を指定します
				}

				return dirPath;
			}

			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			// ストリームオープンとオープンメッセージの出力
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			void openStream(
				bool isForSwitch,			// in: 切替時オープンの場合 true
				string dirPath)
			{
				_fs = new FileStream(Path.Combine(dirPath, _fileName), FileMode.Append, FileAccess.Write, FileShare.Read);
				_sw = new StreamWriter(_fs, _def.LOG_TEXT_ENCODING);

				// 開始メッセージ出力
				WriteInternalMessage((isForSwitch ? MessageResource.SwitchStartLogging : MessageResource.StartLogging) +
					$"  [{_def.TIME_ZONE_INFO.Id}]");
			}

			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			// 権限設定処理付きのディレクトリ作成
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			bool makeDirectory(				// ret:	実際にファイルシステムにディレクトリを作成した場合は true
				in string dir)				// in:  作成するディレクトリパス
			{
				// ディレクトリはまだ存在していないか？
				if(!Directory.Exists(dir))
				{
					// ディレクトリの作成
					Directory.CreateDirectory(dir);

					if(_def.DIR_RIGHTS_TARGET != null)
					{
						// 作成したディレクトリにアクセス権を付与する
						/*
						 * これを行って"Users"に権限を設定しない場合、同一ログ出力フォルダを使用する別の
						 * ユーザがログファイルをオープンできなくなる（Admin権限がない通常ユーザの場合）。
						 */
						FileSystemAccessRule rule = new(
							_def.DIR_RIGHTS_TARGET,
							_def.DIR_RIGHTS,
							InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
							PropagationFlags.None,
							AccessControlType.Allow);

						// 作成されたディレクトリの情報を取得し、ディレクトリ情報にアクセス権を追加する
						DirectoryInfo dirInfo = new DirectoryInfo(dir);
						DirectorySecurity dirSecurity = dirInfo.GetAccessControl();
						dirSecurity.AddAccessRule(rule);
#if DEUBG_LIGHT
						Debug.Write($">> {(rule.IdentityReference as NTAccount)?.Value}");
						Debug.WriteLine($": {Convert.ToString((uint)rule.FileSystemRights, 2)}");
						foreach(FileSystemAccessRule r in dirSecurity.GetAccessRules(true,true,typeof(NTAccount)))
						{
							Debug.Write((r.IdentityReference as NTAccount)?.Value ?? "(null)");
							Debug.WriteLine($": {Convert.ToString((uint)r.FileSystemRights,2)}");
						}
#endif
						// 実際にファイルシステムへアクセス権を設定する
						FileSystemAclExtensions.SetAccessControl(dirInfo, dirSecurity);

						// プロセス番号付きのフォルダを作成した場合、その上のフォルダにも同一の権限を設定しておく
						// これは、別ユーザが新たにサブフォルダを作成できない可能性があるための処理
						if(dir != _def.log_dir_full_path)
						{
							// 作成されたディレクトリの情報を取得し、ディレクトリ情報にアクセス権を追加する
							DirectoryInfo dirInfoParent = new DirectoryInfo(_def.log_dir_full_path);
							DirectorySecurity dirSecurityParent = dirInfoParent.GetAccessControl();
							dirSecurityParent.AddAccessRule(rule);

							// 実際にファイルシステムへアクセス権を設定する
							FileSystemAclExtensions.SetAccessControl(dirInfoParent, dirSecurityParent);
						}
					}

					return true;
				} else
				{
					return false;
				}
			}
		}

		//-------------------------------------------------------------------
		// ログクローズ
		//-------------------------------------------------------------------
		private void Close(
			in bool isForSwitch = false)		// in: 日付変更による引継ぎ時のクローズの場合 true
		{
			try
			{
				// 期限切れファイル削除タスク中でもログを書き出す場合があるため、終了を待ってからClose()する
				_deleteOldLogTask.Wait();

				// 終了メッセージ出力
				WriteInternalMessage(isForSwitch ? MessageResource.SwitchStopLogging : MessageResource.StopLogging);

				_sw?.Close();
				_fs?.Close();
			} finally
			{
				_sw = null;
				_fs = null;
			}
		}

		//-------------------------------------------------------------------
		// 保管期限が過ぎたログファイルを削除する
		//-------------------------------------------------------------------
		private void DeleteOldLog()
		{
			if(_def.STORAGE_DAYS <= 0)
			{
				Write(LogType.I, MessageResource.DoNotDeleteInfo);
				return;
			}

			_deleteOldLogTask.Run(() => { deleteOldLog(); });

			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			// 削除処理
			// ファイル名文字列の大小比較をして削除するファイルを判定する（日付がファイル名の一部であることを利用）
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			void deleteOldLog()
			{
				try
				{
					// ディレクトリとワイルドカードファイル名の取得
					string filter = MakeFileName(DateTime.MinValue);

					// 保存期間内最後（最古）のファイル名の作成
					DateTime oldistDate = _openDate.AddDays(-_def.STORAGE_DAYS);
					string oldistDateFilename = MakeFileName(oldistDate);

					var files = Directory.EnumerateFiles(_dirPath, filter);
					foreach(string filePath in files)
					{
						// filePathはフルパスなのでファイル名だけ抜き出す
						string delFileName = Path.GetFileName(filePath);

						Write(LogType.T, string.Format(_def.CULTURE_INFO, MessageResource.CompareFilesDebug, delFileName, oldistDateFilename));

						// 削除対象のファイル？ 
						if(string.Compare(delFileName, oldistDateFilename, StringComparison.Ordinal) < 0)
						{
							Write(LogType.T, string.Format(_def.CULTURE_INFO, MessageResource.DeleteTargetDebug, delFileName));

							try
							{
								// ファイルを削除する
								File.Delete(filePath);

								Write(LogType.I, string.Format(_def.CULTURE_INFO, MessageResource.DeletedFileInfo, delFileName));
							} catch(Exception ex)
							{
								Write(LogType.E, string.Format(_def.CULTURE_INFO, MessageResource.DeleteFailedError, delFileName), ex);
							}
						} else
						{
							Write(LogType.T, string.Format(_def.CULTURE_INFO, MessageResource.ToBeSaved, delFileName));
						}
					}
				} catch(LoggerWriteException)
				{
					// ここではどうにもできないため、何もしない
				} catch(Exception ex)   // Directory.EnumerateFiles() の例外キャッチが主目的
				{
					try
					{
						Write(LogType.E, string.Format(_def.CULTURE_INFO, MessageResource.EnumrateFilesError), ex, true);
					} catch {/* 無視 */}
				}
			}
		}

		//-------------------------------------------------------------------
		// ログ開始／終了メッセージ用出力（内部使用）
		//-------------------------------------------------------------------
		private void WriteInternalMessage(
			in string text)                 // in: ログメッセージ
		{
			if(_def.WRITE_START_AND_STOP_MESSAGE)
			{
				try
				{
					// スレッドIDの取得（1～）
					int threadID = Environment.CurrentManagedThreadId;

					if(_fs != null && _sw != null)
					{
						lock(_lockObject)   // 日付変更によるファイル切替時は lock が入れ子になりますが問題ありません
						{
							// 開始または終了メッセージ
							string mess = $"{DT.Now.ToString(_def.DATETIME_FORMAT,_def.CULTURE_INFO)} <{_processID.ToString(_def.PROCESS_ID_FORMAT,_def.CULTURE_INFO)}:{threadID.ToString(_def.THREAD_ID_FORMAT,_def.CULTURE_INFO)}> {text}";
							_sw.WriteLine(mess);
							_sw.Flush();

							Debug.WriteLineIf(_def.DEUBG_WRITE,mess);
						}
					}
				} catch(Exception ex)
				{
					throw new LoggerWriteException(ex.Message, ex);
				}
			}
		}

		//===================================================================
		// サービスメソッド
		//===================================================================

		//-------------------------------------------------------------------
		/// <summary>
		/// ログ出力（スレッドセーフ） 
		/// </summary>
		/// <param name="logType">ログ種別</param>
		/// <param name="text">ログメッセージ</param>
		/// <param name="data">ログメッセージ２行目</param>
		/// <param name="sourceFilePath"></param>
		/// <param name="sourceLineNumber"></param>
		//-------------------------------------------------------------------
		public void Write(
			in LogType logType,             // in: ログ種別
			in string text,                 // in: ログメッセージ
			in string? data = null,         // in: ２行目に出力するデータ
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
			[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
		{
			try
			{
				// スレッドIDの取得（1～）
				int threadID = Environment.CurrentManagedThreadId;
				string file = Path.GetFileName(sourceFilePath);

				if(_fs != null && _sw != null)
				{
					lock(_lockObject)
					{
						DateTime now = DT.Now;

						// 日付が変わり、かつ期限切れファイル削除タスクが実行中でない場合にファイル切替処理を行う
						if((_openDate != now.Date)&&(!_deleteOldLogTask.IsRunning))
						{
							// 日付変更時のファイル切替
							switchLogFiles();
						}

						// 種別によるログ出力判定
						if(_def.LOG_TYPE_FILTER(logType))
						{
							string mess = $"{now.ToString(_def.DATETIME_FORMAT,_def.CULTURE_INFO)} <{_processID.ToString(_def.PROCESS_ID_FORMAT,_def.CULTURE_INFO)}:{threadID.ToString(_def.THREAD_ID_FORMAT,_def.CULTURE_INFO)}> {logType} [{file}(L.{sourceLineNumber})]: {text}{Environment.NewLine}";

							if(data != null)   // ２行目？
							{
								// 各行にインデントを追加する

								var sr = new StringReader(data);
								var sb = new StringBuilder(mess);
								string? s;
								while((s = sr.ReadLine()) != null)
								{
									sb.Append(_def.INDENT_SPACE);
									sb.AppendLine(s);
								}

								mess = sb.ToString();
							}

							// ファイル書き出し
							_sw.Write(mess);
							_sw.Flush();

							Debug.WriteIf(_def.DEUBG_WRITE,mess);
						}
					}
				}
			} catch(Exception ex)
			{
				throw new LoggerWriteException(ex.Message, ex);
			}

			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			// ログファイルの切替（日付をまたがって実行を継続した場合の対応）
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			void switchLogFiles()
			{
				// マルチプロセスが許可されている場合はプロセス間排他制御が必要
				using(Mutex? mutex = _def.ALLOW_MULTIPLE_PROCESSES ? new Mutex(false, MutexName) : null)
				{
					mutex?.WaitOne();

					try
					{
						Close(true);		// クローズメッセージがログファイルに出力されますが、そのメッセージは日付を超えた日時となります
						Open(true);         // 翌日のファイルをオープン
					} finally
					{
						mutex?.ReleaseMutex();
					}
				}

				DeleteOldLog();     // 保存期限切れログファイルの削除
			}
		}

		//-------------------------------------------------------------------
		/// <summary>
		/// ログ出力（例外情報特化版） 
		/// </summary>
		/// <param name="logType">ログ種別</param>
		/// <param name="text">ログメッセージ</param>
		/// <param name="exData">例外情報</param>
		/// <param name="enableOtherExceptionData">例外メッセージ以外の情報（ToString()による）を出力する場合true</param>
		/// <param name="sourceFilePath"></param>
		/// <param name="sourceLineNumber"></param>
		//-------------------------------------------------------------------
		public void Write(
			in LogType logType,							// in: ログ種別
			in string text,								// in: 通常ログデータ
			in Exception exData,						// in: 例外情報
			in bool enableOtherExceptionData = true,    // in: 例外メッセージ以外の情報（ToString()による）を出力する場合true
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
			[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
		{
			try
			{
				// 例外クラスのメッセージ展開
				string exMessages = ExpandExceptionMessage(exData);

				if(enableOtherExceptionData)
				{
					// ３行目以降にその他の情報を追記
					exMessages += Environment.NewLine + exData.ToString();
				}

				// 2行目以降に例外メッセージを出力させる
				this.Write(logType, text, exMessages, sourceFilePath, sourceLineNumber);

			} catch(LoggerWriteException)	// LoggerWriteException が入れ子にならないように
			{
				throw;
			} catch(Exception ex)
			{
				throw new LoggerWriteException(ex.Message, ex);
			}

			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			// 入れ子になった例外のメッセージの展開
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			static string ExpandExceptionMessage(	// ret: 展開したメッセージ
				in Exception exData)				// in: メッセージを展開する例外オブジェクト
			{
				string exMessages = string.Empty;
				Exception? currentEx = exData;
				do
				{
					exMessages += $"[{currentEx!.Message}]";
					currentEx = currentEx?.InnerException;
					if(currentEx != null)
					{
						exMessages += "<-";
					}
				}
				while(currentEx != null);

				return exMessages;
			}
		}

		//-------------------------------------------------------------------
		/// <summary>
		/// ログ終了処理
		/// </summary>
		//-------------------------------------------------------------------
		public void Dispose()
		{
			Close();    // Close()処理中で _deleteOldLogTask.Wait() を行っているのでここでは行わない
			_logger = null;
		}

		//===================================================================
		/// <summary>
		/// Logger初期化例外クラス 
		/// </summary>
		//===================================================================
#pragma warning disable CA1034 // 入れ子にされた型を参照可能にすることはできません
		[Serializable()]
		public class LoggerInitException :Exception
#pragma warning restore CA1034 // 入れ子にされた型を参照可能にすることはできません
		{
#pragma warning disable CS1591
			public LoggerInitException() : base() { }
			public LoggerInitException(in string mess) : base(mess) { }
			public LoggerInitException(in string mess, Exception exp) : base(mess, exp) { }
#pragma warning restore CS1591
		}

		//===================================================================
		/// <summary>
		/// Logger書出し処理例外クラス
		/// </summary>
		/// <remarks>
		/// Logger.Write()で例外が発生した場合、その例外をLogger.Write()で書き出そう
		/// として無限ループに陥る可能性がある。 
		/// Logger.Write()の例外を区別できるように本クラスの存在意義がある
		/// </remarks>
		//===================================================================
#pragma warning disable CA1034 // 入れ子にされた型を参照可能にすることはできません
		[Serializable()]
		public class LoggerWriteException :Exception
#pragma warning restore CA1034 // 入れ子にされた型を参照可能にすることはできません
		{
#pragma warning disable CS1591
			public LoggerWriteException() : base() { }
			public LoggerWriteException(in string mess) : base(mess) { }
			public LoggerWriteException(in string mess, Exception exp) : base(mess, exp) { }
#pragma warning restore CS1591
		}
	}
}
