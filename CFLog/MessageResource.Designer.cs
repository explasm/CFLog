﻿//------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン:4.0.30319.42000
//
//     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
//     コードが再生成されるときに損失したりします。
// </auto-generated>
//------------------------------------------------------------------------------

namespace CFLog {
    using System;
    
    
    /// <summary>
    ///   ローカライズされた文字列などを検索するための、厳密に型指定されたリソース クラスです。
    /// </summary>
    // このクラスは StronglyTypedResourceBuilder クラスが ResGen
    // または Visual Studio のようなツールを使用して自動生成されました。
    // メンバーを追加または削除するには、.ResX ファイルを編集して、/str オプションと共に
    // ResGen を実行し直すか、または VS プロジェクトをビルドし直します。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class MessageResource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal MessageResource() {
        }
        
        /// <summary>
        ///   このクラスで使用されているキャッシュされた ResourceManager インスタンスを返します。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("CFLog.MessageResource", typeof(MessageResource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   すべてについて、現在のスレッドの CurrentUICulture プロパティをオーバーライドします
        ///   現在のスレッドの CurrentUICulture プロパティをオーバーライドします。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   CreateLogger() has already been executed. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string AlreadyInitializedError {
            get {
                return ResourceManager.GetString("AlreadyInitializedError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Could not open log file &quot;{0}&quot;. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string CannotOpenLogfileError {
            get {
                return ResourceManager.GetString("CannotOpenLogfileError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Compare file [{0}] in folder with longest stored file name [{1}]. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string CompareFilesDebug {
            get {
                return ResourceManager.GetString("CompareFilesDebug", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Deleted log file [{0}]. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string DeletedFileInfo {
            get {
                return ResourceManager.GetString("DeletedFileInfo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Failed to delete log file [{0}]. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string DeleteFailedError {
            get {
                return ResourceManager.GetString("DeleteFailedError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   File [{0}] in the folder is subject to deletion. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string DeleteTargetDebug {
            get {
                return ResourceManager.GetString("DeleteTargetDebug", resourceCulture);
            }
        }
        
        /// <summary>
        ///   The deletion process was not performed because the specified number of log retention days was less than 0. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string DoNotDeleteInfo {
            get {
                return ResourceManager.GetString("DoNotDeleteInfo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Mainly, it failed to get the file list. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string EnumrateFilesError {
            get {
                return ResourceManager.GetString("EnumrateFilesError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   The number of processes has reached the maximum number ({0}). に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string MaxProcess {
            get {
                return ResourceManager.GetString("MaxProcess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Root directory specified or no directory specified. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string NoDirectoryError {
            get {
                return ResourceManager.GetString("NoDirectoryError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   === Start logging === に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string StartLogging {
            get {
                return ResourceManager.GetString("StartLogging", resourceCulture);
            }
        }
        
        /// <summary>
        ///   === Stop logging === に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string StopLogging {
            get {
                return ResourceManager.GetString("StopLogging", resourceCulture);
            }
        }
        
        /// <summary>
        ///   --- Continued logging from previous file --- に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SwitchStartLogging {
            get {
                return ResourceManager.GetString("SwitchStartLogging", resourceCulture);
            }
        }
        
        /// <summary>
        ///   --- Continued logging to next file --- に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SwitchStopLogging {
            get {
                return ResourceManager.GetString("SwitchStopLogging", resourceCulture);
            }
        }
        
        /// <summary>
        ///   The file [{0}] in the folder is to be saved. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string ToBeSaved {
            get {
                return ResourceManager.GetString("ToBeSaved", resourceCulture);
            }
        }
        
        /// <summary>
        ///   CreateLogger() has not been executed. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string UninitializedError {
            get {
                return ResourceManager.GetString("UninitializedError", resourceCulture);
            }
        }
    }
}