using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace iwm_DirFileDialog
{
	internal class Program
	{
		private const string COPYRIGHT = "(C)2025 iwm-iwama";
		private const string NAME = "iwm_DirFileDialog";
		private const string VERSION = "20250120";

		private static readonly string NL = Environment.NewLine;
		private static readonly string LN = new string('-', 80) + NL;

		// private static readonly string IESC_CLEAR = "\u001b[2J\u001b[1;1H";
		private static readonly string IESC_RESET = "\u001b[0m";
		private static readonly string IESC_TITLE1 = "\u001b[38;2;250;250;250m\u001b[104m"; // 白／青
		private static readonly string IESC_OPT1 = "\u001b[38;2;250;150;150m";  // 赤
		private static readonly string IESC_OPT2 = "\u001b[38;2;150;150;250m";  // 青
		private static readonly string IESC_OPT21 = "\u001b[38;2;25;225;235m";  // 水
		private static readonly string IESC_OPT22 = "\u001b[38;2;250;100;250m"; // 紅紫
		private static readonly string IESC_LBL1 = "\u001b[38;2;250;250;100m";  // 黄
		private static readonly string IESC_LBL2 = "\u001b[38;2;100;100;250m";  // 青
		private static readonly string IESC_STR1 = "\u001b[38;2;225;225;225m";  // 白
		private static readonly string IESC_STR2 = "\u001b[38;2;200;200;200m";  // 銀
		private static readonly string IESC_TRUE1 = "\u001b[38;2;0;250;250m";   // 水
		private static readonly string IESC_FALSE1 = "\u001b[38;2;250;50;50m";  // 紅

		private static void SubPrintVersion()
		{
			Console.Write(
				IESC_STR2 +
				LN +
				$"\u001b[2G{COPYRIGHT}{NL}" +
				$"\u001b[5G{NAME}_{VERSION}{NL}" +
				LN +
				IESC_RESET
			);
		}

		private static void SubPrintHelp()
		{
			SubPrintVersion();
			Console.Write(
				$"\u001b[1G{IESC_TITLE1} フォルダ／ファイル選択ダイアログ {IESC_RESET}{NL}{NL}" +
				$"\u001b[5G{IESC_STR1}{NAME}{IESC_OPT2} [Option]{NL}{NL}" +
				$"\u001b[2G{IESC_LBL1}(例){NL}" +
				$"\u001b[5G{IESC_STR1}{NAME}{IESC_OPT2} -type=m -dir=\"c:\\windows\" -codepage=932{NL}{NL}" +
				$"\u001b[2G{IESC_OPT2}[Option]{NL}" +
				$"\u001b[5G{IESC_OPT21}-type=STR | -t=STR{NL}" +
				$"\u001b[9G{IESC_STR1}ダイアログ{IESC_LBL1}（必須）{NL}" +
				IESC_STR2 +
				$"\u001b[13Gd : 単一フォルダ選択{NL}" +
				$"\u001b[13Gf : 単一ファイル選択{NL}" +
				$"\u001b[13Gm : 複数ファイル選択{NL}" +
				$"\u001b[13GF : 保存ファイル選択{NL}{NL}" +
				$"\u001b[5G{IESC_OPT21}-dir=STR | -d=STR{NL}" +
				$"\u001b[9G{IESC_STR1}初期フォルダ指定{IESC_LBL1}（初期値 : Current Directory）{NL}{NL}" +
				$"\u001b[5G{IESC_OPT21}-nameonly | -no{NL}" +
				$"\u001b[9G{IESC_STR1}ファイル名のみ出力{NL}{NL}" +
				IESC_STR2 +
				LN +
				IESC_RESET
			);
		}

		private const int STD_OUTPUT_HANDLE = -11;
		private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
		private const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;

		[DllImport("kernel32.dll")]
		private static extern bool GetConsoleMode(
			IntPtr hConsoleHandle,
			out uint lpMode
		);

		[DllImport("kernel32.dll")]
		private static extern bool SetConsoleMode(
			IntPtr hConsoleHandle,
			uint dwMode
		);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr GetStdHandle(
			int nStdHandle
		);

		[DllImport("kernel32.dll")]
		private static extern uint GetLastError();

		[STAThread]
		private static void Main()
		{
			IntPtr iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
			_ = GetConsoleMode(iStdOut, out uint outConsoleMode);
			outConsoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING | DISABLE_NEWLINE_AUTO_RETURN;
			_ = SetConsoleMode(iStdOut, outConsoleMode);

			string[] ARGV = Environment.GetCommandLineArgs();

			if (ARGV.Length == 1 || (ARGV.Length >= 2 && (ARGV[1] == "-h" || ARGV[1] == "-help")))
			{
				SubPrintHelp();
				Thread.Sleep(2000);
				Environment.Exit(0);
			}
			else if ((ARGV.Length >= 2 && (ARGV[1] == "-v" || ARGV[1] == "-version")))
			{
				SubPrintVersion();
				Environment.Exit(0);
			}

			// 初期化
			int TYPE = 0;
			string CUR_DIR = Environment.CurrentDirectory;
			bool NAME_ONLY = false;
			StringBuilder OUTPUT = new StringBuilder("", 256);

			// [0] は読み飛ばす
			for (int _i1 = 1; _i1 < ARGV.Length; _i1++)
			{
				string _s1 = ARGV[_i1];

				if (_s1.StartsWith("-t=") || _s1.StartsWith("-type="))
				{
					string[] _as1 = _s1.Split('=');

					switch (_as1[1])
					{
						case "d": TYPE = 1; break;
						case "f": TYPE = 11; break;
						case "m": TYPE = 12; break;
						case "F": TYPE = 13; break;
						default: TYPE = 0; break;
					}
				}
				else if (_s1.StartsWith("-d=") || _s1.StartsWith("-dir="))
				{
					string[] _as1 = _s1.Split('=');

					if (Directory.Exists(_as1[1]))
					{
						CUR_DIR = _as1[1];
					}
				}
				else if (_s1 == "-no" || _s1 == "-nameonly")
				{
					NAME_ONLY = true;
				}
			}

			// Err
			if (TYPE == 0)
			{
				SubPrintHelp();
				Environment.Exit(0);
			}
			// Dir
			else if (TYPE == 1)
			{
				FolderBrowserDialog _fbd = new FolderBrowserDialog
				{
					Description = "フォルダーを指定してください。",
					RootFolder = Environment.SpecialFolder.MyComputer,
					SelectedPath = CUR_DIR,
					ShowNewFolderButton = true,
				};

				if (_fbd.ShowDialog() == DialogResult.OK)
				{
					_ = OUTPUT.Append(_fbd.SelectedPath.TrimEnd('\\'));
					_ = OUTPUT.Append("\\");
				}
			}
			// File
			else if (TYPE >= 11 && TYPE <= 12)
			{
				OpenFileDialog _ofd = new OpenFileDialog()
				{
					InitialDirectory = CUR_DIR,
					Filter = "すべてのファイル (*.*)|*.*",
					FilterIndex = 2,
					RestoreDirectory = true,
					Multiselect = TYPE == 12,
				};

				if (_ofd.ShowDialog() == DialogResult.OK)
				{
					foreach (string _s1 in _ofd.FileNames)
					{
						_ = OUTPUT.Append(NAME_ONLY ? Path.GetFileName(_s1) : _s1);
						_ = OUTPUT.Append(NL);
					}
				}
			}
			// Save File
			else if (TYPE == 13)
			{
				SaveFileDialog _sfd = new SaveFileDialog()
				{
					InitialDirectory = CUR_DIR,
					Filter = "すべてのファイル (*.*)|*.*",
					FilterIndex = 2,
					RestoreDirectory = true,
				};

				if (_sfd.ShowDialog() == DialogResult.OK)
				{
					string sFn = _sfd.FileNames[0];
					_ = OUTPUT.Append(NAME_ONLY ? Path.GetFileName(sFn) : sFn);
					_ = OUTPUT.Append(NL);
				}
			}

			Console.Write(OUTPUT.ToString());

			_ = OUTPUT.Clear();
		}
	}
}
