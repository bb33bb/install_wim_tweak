#define DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Win32;
using Microsoft.Win32.Security;

namespace install_wim_tweak
{
	internal class Program
	{
		private const string HIVE_MOUNT_DIR = "windows6_x_software";

		private static string _pkgDirectory = "windows6_x_software\\Microsoft\\Windows\\CurrentVersion\\Component Based Servicing\\";

		private const string HIVE_MOUNT_POINT = "HKLM\\windows6_x_software";

		private const string REGISTRY_PATH = "Windows\\system32\\config\\SOFTWARE";

		private static readonly string ProgramHeader = string.Concat("-------------------------------------------\n--------Registry Tweak Tool v", Assembly.GetExecutingAssembly().GetName().Version, "-------\n---------------for Windows 6.x-------------\n---------Created by Michał Wnuowski--------\n-----Concept by Aviv00@msfn / lite8@MDL----\n-----------Modified by Legolash2o----------\n-------------------------------------------\n\n");

		private const string PROGRAM_HELP_INFO = "USAGE : \n   install_wim_tweak [/p <Path>] [/c <PackageName> (optional)] [/?]\n\nREMARKS : \n   /p<Path>     Use '/p' switch to provide path to mounted install.wim\n   /o           Use '/o' to run on current Windows\n   /c <ComponentName>  Use '/c' to show a specific package\n   /?           Use '/?' switch to display this info\n   /l           Outputs all packages to \"Packages.txt\"\nEXAMPLE : \n    install_wim_tweak /p C:\\temp files\\mount\n    install_wim_tweak /c Microsoft-Hyper-V-Common-Drivers-Package";

		private static bool _failed;

		private static string _bkpFile;

		private static string _hiveFileInfo;

		private static string _comp = "";

		private static Dictionary<char, string> _cmdLineArgs;

		private static bool _online;

		private static readonly string PackLog = Environment.CurrentDirectory + "\\Packages.txt";

		private static string _cr = "";

		private static bool _vis;

		private static void Main(string[] args)
		{
			Console.ForegroundColor = ConsoleColor.White;
			Console.Write(ProgramHeader);
			Console.ResetColor();
			try
			{
				_cmdLineArgs = ProcessCmdArgs(args, new char[9] { 'p', '?', 'c', 'o', 'l', 'r', 'n', 'h', 'd' });
				if (_cmdLineArgs.ContainsKey('?'))
				{
					Console.Write("USAGE : \n   install_wim_tweak [/p <Path>] [/c <PackageName> (optional)] [/?]\n\nREMARKS : \n   /p<Path>     Use '/p' switch to provide path to mounted install.wim\n   /o           Use '/o' to run on current Windows\n   /c <ComponentName>  Use '/c' to show a specific package\n   /?           Use '/?' switch to display this info\n   /l           Outputs all packages to \"Packages.txt\"\nEXAMPLE : \n    install_wim_tweak /p C:\\temp files\\mount\n    install_wim_tweak /c Microsoft-Hyper-V-Common-Drivers-Package");
					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.Write("\nPlease make sure you use lowercase for the /p, /c, /o and /l");
					Console.ResetColor();
					Environment.Exit(1);
				}
				if (_cmdLineArgs.ContainsKey('c'))
				{
					if (!string.IsNullOrEmpty(_cmdLineArgs['c']))
					{
						_comp = Path.Combine(_cmdLineArgs['c'], "");
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.White;
						Console.WriteLine("Type the name of the package, if nothing is entered all packages will be made visible :");
						Console.ForegroundColor = ConsoleColor.Cyan;
						_comp = Path.Combine(Console.ReadLine(), "");
					}
					Console.ResetColor();
				}
				if (_cmdLineArgs.ContainsKey('o'))
				{
					_hiveFileInfo = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "Windows\\system32\\config\\SOFTWARE");
					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.WriteLine("MountPath : Online");
					Console.ResetColor();
					_pkgDirectory = _pkgDirectory.Replace("windows6_x_software", "Software");
					_online = true;
				}
				if (_cmdLineArgs.ContainsKey('h'))
				{
					_vis = true;
				}
				if (!_cmdLineArgs.ContainsKey('o'))
				{
					if (!_cmdLineArgs.ContainsKey('p'))
					{
						Console.ForegroundColor = ConsoleColor.White;
						Console.WriteLine("Type path to mounted install.wim :");
						Console.ForegroundColor = ConsoleColor.Cyan;
						_hiveFileInfo = Path.Combine(Console.ReadLine(), "Windows\\system32\\config\\SOFTWARE");
						if (_hiveFileInfo.Substring(0, _hiveFileInfo.Length - "Windows\\system32\\config\\SOFTWARE".Length).Length == 3)
						{
							Console.WriteLine("MountPath : Online");
							_pkgDirectory = _pkgDirectory.Replace("windows6_x_software", "Software");
							_online = true;
						}
						else
						{
							Console.WriteLine("MountPath : {0}", "\"" + _hiveFileInfo.Substring(0, _hiveFileInfo.Length - "Windows\\system32\\config\\SOFTWARE".Length) + "\"");
							_online = false;
						}
						Console.ResetColor();
					}
					else
					{
						_hiveFileInfo = Path.Combine(_cmdLineArgs['p'], "Windows\\system32\\config\\SOFTWARE");
						Console.ForegroundColor = ConsoleColor.Cyan;
						if (_cmdLineArgs['p'].Length == 3)
						{
							Console.WriteLine("MountPath : Online");
							_pkgDirectory = _pkgDirectory.Replace("windows6_x_software", "Software");
							_online = true;
						}
						else
						{
							Console.WriteLine("MountPath : {0}", "\"" + _cmdLineArgs['p'] + "\"");
							_online = false;
						}
						Console.ResetColor();
					}
				}
				if (string.IsNullOrEmpty(_hiveFileInfo))
				{
					Environment.Exit(-2);
				}
				if (!File.Exists(_hiveFileInfo))
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Registry file not found, please make sure your mount path is correct!");
					Console.ResetColor();
					_failed = true;
					Environment.Exit(-532459699);
				}
				if (!string.IsNullOrEmpty(_comp))
				{
					string text = _comp;
					while (text.Contains("~"))
					{
						text = text.Substring(0, text.Length - 1);
					}
					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.WriteLine("Component : \"" + text + "\"");
					Console.ResetColor();
				}
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("\n------------------Starting-----------------");
				Console.ResetColor();
				if (!_online)
				{
					if (!_cmdLineArgs.ContainsKey('l') && !_cmdLineArgs.ContainsKey('n'))
					{
						Console.Write("Creating BKP of registry file...         ");
						_bkpFile = Path.Combine(Environment.CurrentDirectory, "SOFTWAREBKP");
						if (!File.Exists(_bkpFile))
						{
							File.Copy(_hiveFileInfo, _bkpFile, overwrite: true);
						}
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine("OK");
						Console.ResetColor();
					}
					Console.Write("Mounting registry file...                ");
					if (!Contains(Registry.LocalMachine.GetSubKeyNames(), "windows6_x_software") && !LoadHive(_hiveFileInfo, "HKLM\\windows6_x_software"))
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("FAIL");
						Console.ResetColor();
						_failed = true;
						Ending();
					}
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("OK");
					Console.ResetColor();
				}
				if (_cmdLineArgs.ContainsKey('l'))
				{
					Console.Write("Writing to Log (Packages.txt)         ");
					if (File.Exists(PackLog))
					{
						File.Delete(PackLog);
					}
					ListComponentSubkeys(_pkgDirectory + "Packages\\");
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write("OK");
					Console.ResetColor();
					Ending();
				}
				Console.Write("Taking Ownership...                      ");
				new AccessTokenProcess(Process.GetCurrentProcess().Id, TokenAccessType.TOKEN_ALL_ACCESS).EnablePrivilege(new TokenPrivilege("SeTakeOwnershipPrivilege", enabled: true));
				if (Win32.GetLastError() != 0)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("FAIL");
					Console.WriteLine("You must be logged as Administrator.");
					Console.ResetColor();
					_failed = true;
					Ending();
				}
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("OK");
				Console.ResetColor();
				Console.Write("Editing 'Packages' subkeys            ");
				try
				{
					if (CleanComponentSubkeys(_pkgDirectory + "Packages\\", _comp))
					{
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine("OK");
						Console.ResetColor();
					}
				}
				catch
				{
				}
				if (!_online)
				{
					Console.Write("Editing 'PackagesPending' subkeys     ");
					try
					{
						if (CleanComponentSubkeys(_pkgDirectory + "PackagesPending\\", _comp))
						{
							Console.ForegroundColor = ConsoleColor.Green;
							Console.WriteLine("OK");
							Console.ResetColor();
						}
					}
					catch
					{
					}
				}
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Modifying registry completed sucessfully.");
				Console.ResetColor();
				if (_cmdLineArgs.ContainsKey('r'))
				{
					if (Contains(Registry.LocalMachine.GetSubKeyNames(), "windows6_x_software"))
					{
						Console.Write("Unmounting key...                        ");
						if (!UnloadHive("HKLM\\windows6_x_software"))
						{
							Console.ForegroundColor = ConsoleColor.Red;
							Console.WriteLine("FAIL");
							Console.WriteLine("You must unmount registry hive manually.");
							Console.WriteLine("Hit any key to close.");
							Console.ResetColor();
							Console.ReadKey();
							Environment.Exit(-3);
						}
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine("OK");
						Console.ResetColor();
					}
					Console.Write("Removing 'Packages'...                ");
					if (RemoveComponentSubkeys(_pkgDirectory + "Packages\\", _comp))
					{
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine("OK");
						Console.WriteLine("Removed packages successfully.");
						Console.ResetColor();
					}
					Console.Write("Removing 'PackagesPending'...         ");
					if (RemoveComponentSubkeys(_pkgDirectory + "Packages\\", _comp))
					{
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine("OK");
						Console.WriteLine("Removed packages successfully.");
						Console.ResetColor();
					}
				}
				Ending();
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("FAIL");
				Console.WriteLine("Unhandled error occured.");
				Console.ResetColor();
				Console.WriteLine(ex.Message);
				_failed = true;
				Ending();
			}
		}

		private static bool RemoveComponentSubkeys(string registryPath, string Comp)
		{
			int consoleX = 0;
			int consoleY = 0;
			try
			{
				consoleX = Console.CursorLeft;
				consoleY = Console.CursorTop;
			}
			catch
			{
			}
			int num = 1;
			int num2 = 0;
			string text = _hiveFileInfo;
			while (!text.EndsWith("\\Windows\\"))
			{
				text = text.Substring(0, text.Length - 1);
			}
			text = text.Replace("Windows\\", "");
			string[] array = _cr.Split(Environment.NewLine.ToCharArray());
			foreach (string text2 in array)
			{
				if (Comp != null && text2.StartsWith(Comp) && !string.IsNullOrEmpty(Comp))
				{
					num2++;
				}
			}
			array = _cr.Split(Environment.NewLine.ToCharArray());
			foreach (string text3 in array)
			{
				if (!text3.StartsWith(Comp) || string.IsNullOrEmpty(Comp))
				{
					continue;
				}
				CorrectConsolePostion(num2, consoleY, consoleX);
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Write("{0}/{1}", num, num2);
				Console.ResetColor();
				try
				{
					Process process = new Process();
					process.StartInfo.FileName = "pkgmgr.exe";
					if (!_online)
					{
						process.StartInfo.Arguments = "/o:\"" + text + ";" + text + "Windows\" /up:" + text3 + " /norestart /quiet";
					}
					else
					{
						process.StartInfo.Arguments = "/up:" + text3 + " /norestart /quiet";
					}
					process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
					process.Start();
					process.WaitForExit();
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
				num++;
			}
			return true;
		}

		private static bool ListComponentSubkeys(string registryPath)
		{
			int consoleX = 0;
			int consoleY = 0;
			try
			{
				consoleX = Console.CursorLeft;
				consoleY = Console.CursorTop;
			}
			catch
			{
			}
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(registryPath);
			int num = 1;
			int num2 = 0;
			string text = "";
			string[] subKeyNames = registryKey.GetSubKeyNames();
			for (int i = 0; i < subKeyNames.Length; i++)
			{
				if (!subKeyNames[i].StartsWith("Package"))
				{
					num2++;
				}
			}
			subKeyNames = registryKey.GetSubKeyNames();
			foreach (string text2 in subKeyNames)
			{
				if (!text2.StartsWith("Package"))
				{
					CorrectConsolePostion(num2, consoleY, consoleX);
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Write("{0}/{1}", num, num2);
					Console.ResetColor();
					text = text + text2 + Environment.NewLine;
					num++;
				}
			}
			registryKey.Close();
			try
			{
				StreamWriter streamWriter = new StreamWriter(PackLog, append: true);
				streamWriter.WriteLine(text);
				streamWriter.Close();
			}
			catch
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write("FAIL");
				Console.ResetColor();
			}
			return true;
		}

		private static bool CleanComponentSubkeys(string registryPath, string CN)
		{
			int consoleX = 0;
			int consoleY = 0;
			try
			{
				consoleX = Console.CursorLeft;
				consoleY = Console.CursorTop;
			}
			catch
			{
			}
			try
			{
				RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(registryPath);
				IdentityReference user = WindowsIdentity.GetCurrent().User;
				int num = 1;
				int num2 = 0;
				string[] subKeyNames = registryKey.GetSubKeyNames();
				for (int i = 0; i < subKeyNames.Length; i++)
				{
					if (subKeyNames[i].Contains(CN))
					{
						num2++;
					}
				}
				Debug.Assert(registryKey != null, "myKey != null");
				subKeyNames = registryKey.GetSubKeyNames();
				foreach (string text in subKeyNames)
				{
					if (!text.Contains(CN))
					{
						continue;
					}
					try
					{
						if (!_cr.Contains(text))
						{
							_cr = _cr + text + Environment.NewLine;
						}
						CorrectConsolePostion(num2, consoleY, consoleX);
						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.Write("{0}/{1}", num, num2);
						Console.ResetColor();
						if (!RegSetOwneship(registryKey, text, user))
						{
							registryKey.Close();
							Console.ForegroundColor = ConsoleColor.Red;
							Console.WriteLine("FAIL");
							Console.WriteLine("Error at setting key privileges.");
							Console.ResetColor();
							_failed = true;
							Ending();
						}
						RegistryAccessRule nacc = RegSetFullAccess(registryKey, text, user);
						RegistryKey registryKey2 = registryKey.OpenSubKey(text, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
						try
						{
							if (Contains(registryKey2.GetValueNames(), "Visibility"))
							{
								if (!_vis)
								{
									if (!Contains(registryKey2.GetValueNames(), "DefVis"))
									{
										registryKey2.SetValue("DefVis", registryKey2.GetValue("Visibility"), RegistryValueKind.DWord);
									}
									registryKey2.SetValue("Visibility", 1, RegistryValueKind.DWord);
								}
								else if (Contains(registryKey2.GetValueNames(), "DefVis"))
								{
									registryKey2.SetValue("Visibility", registryKey2.GetValue("DefVis"), RegistryValueKind.DWord);
								}
							}
							if (!_cmdLineArgs.ContainsKey('d') && Contains(registryKey2.GetSubKeyNames(), "Owners"))
							{
								RegSetOwneship(registryKey, text + "\\Owners", user);
								RegSetFullAccess(registryKey, text + "\\Owners", user);
								registryKey2.DeleteSubKey("Owners");
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.Message);
						}
						registryKey2.Close();
						RegRemoveAccess(registryKey, text, user, nacc);
					}
					catch
					{
					}
					num++;
				}
				registryKey.Close();
			}
			catch
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("   FAIL - Key not exist");
				Console.ResetColor();
				return false;
			}
			return true;
		}

		private static void Ending()
		{
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("\n-------------------Ending------------------");
			Console.ResetColor();
			if (!_online)
			{
				if (Contains(Registry.LocalMachine.GetSubKeyNames(), "windows6_x_software"))
				{
					Console.Write("Unmounting key...                        ");
					if (!UnloadHive("HKLM\\windows6_x_software"))
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("FAIL");
						Console.WriteLine("You must unmount registry hive manually.");
						Console.WriteLine("Hit any key to close.");
						Console.ResetColor();
						Console.ReadKey();
						Environment.Exit(-1);
					}
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("OK");
					Console.ResetColor();
				}
				if (File.Exists(_bkpFile) && _failed && !_cmdLineArgs.ContainsKey('n'))
				{
					Console.Write("Restoring Backup...                      ");
					File.Copy(_bkpFile, _hiveFileInfo, overwrite: true);
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("OK");
					Console.ResetColor();
					try
					{
						Console.Write("Removing Backup file...                  ");
						File.Delete(_bkpFile);
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine("OK");
						Console.ResetColor();
						if (_cmdLineArgs.Count == 0)
						{
							Console.WriteLine("Hit any key to close.");
							Console.ReadKey();
						}
					}
					catch
					{
					}
				}
			}
			Environment.Exit(0);
		}

		private static void RegRemoveAccess(RegistryKey nParentKey, string nkey, IdentityReference nuser, RegistryAccessRule nacc)
		{
			RegistryKey registryKey = nParentKey.OpenSubKey(nkey, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.ExecuteKey | RegistryRights.ChangePermissions);
			RegistrySecurity accessControl = registryKey.GetAccessControl(AccessControlSections.Access);
			accessControl.RemoveAccessRule(nacc);
			registryKey.SetAccessControl(accessControl);
			registryKey.Close();
		}

		private static RegistryAccessRule RegSetFullAccess(RegistryKey nParentKey, string nkey, IdentityReference nuser)
		{
			RegistryKey registryKey = null;
			try
			{
				registryKey = nParentKey.OpenSubKey(nkey, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.ExecuteKey | RegistryRights.ChangePermissions);
				RegistrySecurity accessControl = registryKey.GetAccessControl(AccessControlSections.Access);
				RegistryAccessRule registryAccessRule = new RegistryAccessRule(nuser, RegistryRights.FullControl, AccessControlType.Allow);
				accessControl.AddAccessRule(registryAccessRule);
				registryKey.SetAccessControl(accessControl);
				registryKey.Close();
				return registryAccessRule;
			}
			catch
			{
				registryKey?.Close();
				return null;
			}
		}

		private static bool RegSetOwneship(RegistryKey nParentKey, string nkey, IdentityReference nuser)
		{
			RegistryKey registryKey = null;
			try
			{
				registryKey = nParentKey.OpenSubKey(nkey, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.ExecuteKey | RegistryRights.TakeOwnership);
				RegistrySecurity accessControl = registryKey.GetAccessControl(AccessControlSections.Owner);
				accessControl.SetOwner(nuser);
				registryKey.SetAccessControl(accessControl);
				registryKey.Close();
				return true;
			}
			catch
			{
				registryKey?.Close();
				return false;
			}
		}

		private static bool Contains<typeColl, typeKey>(typeColl collection, typeKey val) where typeColl : IEnumerable<typeKey> where typeKey : IComparable
		{
			foreach (typeKey item in collection)
			{
				if (item.CompareTo(val) == 0)
				{
					return true;
				}
			}
			return false;
		}

		private static void InitProcess(Process nproc)
		{
			nproc.StartInfo.UseShellExecute = false;
			nproc.StartInfo.RedirectStandardError = true;
			nproc.StartInfo.RedirectStandardOutput = true;
			nproc.StartInfo.RedirectStandardInput = true;
			nproc.StartInfo.CreateNoWindow = true;
		}

		private static bool LoadHive(string nfile, string nkeyname)
		{
			return RunReg(string.Format("LOAD {0} {1}", nkeyname, "\"" + nfile + "\""));
		}

		private static bool UnloadHive(string nkeyname)
		{
			return RunReg($"UNLOAD {nkeyname}");
		}

		private static bool RunReg(string nArguments)
		{
			Process process = new Process();
			InitProcess(process);
			process.StartInfo.FileName = "reg.exe";
			process.StartInfo.Arguments = nArguments;
			process.Start();
			process.WaitForExit();
			string text = process.StandardOutput.ReadToEnd();
			string text2 = process.StandardError.ReadToEnd();
			if (text.Length < 1 || text2.Length > 1)
			{
				return false;
			}
			return true;
		}

		private static void CorrectConsolePostion(int tot, int consoleY, int consoleX)
		{
			try
			{
				Console.CursorLeft = consoleX;
				Console.CursorTop = consoleY;
				if (tot < 10)
				{
					Console.CursorLeft = consoleX;
					Console.CursorTop = consoleY;
				}
				if (tot > 9 && tot < 100)
				{
					Console.CursorLeft = consoleX - 2;
					Console.CursorTop = consoleY;
				}
				if (tot > 99 && tot < 1000)
				{
					Console.CursorLeft = consoleX - 4;
					Console.CursorTop = consoleY;
				}
				if (tot > 999 && tot < 10000)
				{
					Console.CursorLeft = consoleX - 6;
					Console.CursorTop = consoleY;
				}
			}
			catch
			{
			}
		}

		private static Dictionary<char, string> ProcessCmdArgs(string[] args, char[] allowedArgs)
		{
			Dictionary<char, string> dictionary = new Dictionary<char, string>();
			string text = "";
			char c = ' ';
			foreach (string text2 in args)
			{
				string text3 = text2.Trim();
				if (text3[0] == '/')
				{
					if (!Contains(allowedArgs, text3[1]))
					{
						dictionary.Clear();
						dictionary.Add('?', "");
						return dictionary;
					}
					if (c != ' ')
					{
						dictionary.Add(c, text.Trim());
					}
					c = text2[1];
					text = "";
				}
				else
				{
					if (c == ' ')
					{
						dictionary.Clear();
						dictionary.Add('?', "");
						return dictionary;
					}
					text = text + " " + text3;
				}
			}
			dictionary.Add(c, text.Trim());
			return dictionary;
		}
	}
}
