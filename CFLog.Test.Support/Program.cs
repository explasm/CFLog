//***************************************************************************
// Copyright (c) Takahiro Fukushima All rights reserved.
// Licensed under the MIT license.
//***************************************************************************

using static CFLog.Logger;
using static CFLog.Logger.LogType;

//===========================================================================
// xUnit 試験の補助に使用するもので、xUnit の試験プログラムから起動されます
// ★本プロセスは DEBUG モードでビルドされたときだけ機能します
//===========================================================================

if(args.Length > 0)
{
	for(int i = 0; i < args.Length; i++)
	{
		switch(args[i])
		{
		case "-test01":
			if((i+1) < args.Length)
			{
				Test01(args[i + 1]);
			}
			break;
		case "-test02":
			if((i + 1) < args.Length)
			{
				Test02(args[i + 1]);
			}
			break;
		}
	}
}

return 0;

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
// ミューテックス排他制御試験
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
void Test01(string sleepTime)
{
	LoggerDef logDef = new()
	{
		LOG_TYPE_FILTER = (lt) => lt <= LogType.I,

		FILE_PREFIX = "CFLogTests",
		ALLOW_MULTIPLE_PROCESSES = true,
#if DEBUG
		TEST_OPENWAIT = int.Parse(sleepTime),
#endif
	};

	using(CreateLogger(logDef))
	{
#if DEBUG
		LOG.Write(I,"Support 1",$"Sleep({logDef.TEST_OPENWAIT})");
		Thread.Sleep(logDef.TEST_OPENWAIT);
		LOG.Write(I, "Support 2");
#endif
	}
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
// マルチプロセス試験
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
void Test02(string sleepTime)
{
	LoggerDef logDef = new()
	{
		LOG_TYPE_FILTER = (lt) => lt <= LogType.I,

		FILE_PREFIX = "CFLogTests",
		ALLOW_MULTIPLE_PROCESSES = true,
	};

	int sleepTimeNum = int.Parse(sleepTime);

	using(CreateLogger(logDef))
	{
#if DEBUG
		LOG.Write(I, "Support 1", $"Sleep({sleepTimeNum})");
		Thread.Sleep(sleepTimeNum);
		LOG.Write(I, "Support 2");
#endif
	}
}

