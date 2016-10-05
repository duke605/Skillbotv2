using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fclp.Internals.Extensions;
using SkillBotv2.Extensions;

namespace SkillBotv2.Util
{
    class CommandLineUtil
    {
        [DllImport("shell32.dll", SetLastError = true)]
        private static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        /// <summary>
        /// Splits a string into
        /// </summary>
        /// <param name="commandLine">The string to split</param>
        /// <returns></returns>
        public static string[] CommandLineToArgs(string commandLine)
        {
            if (commandLine.IsNullOrEmpty())
                return new string[0];

            int argc;
            var argv = CommandLineToArgvW(NormalizeCommandLine(commandLine), out argc);
            if (argv == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception();
            try
            {
                var args = new string[argc];
                for (var i = 0; i < args.Length; i++)
                {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }

                return args;
            } finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }

        /// <summary>
        /// Removes = from the command line that arn't in quotes
        /// </summary>
        /// <param name="commandLine">The command lise string to be normalized</param>
        /// <returns></returns>
        public static string NormalizeCommandLine(string commandLine)
        {
            return Regex.Replace(commandLine, "=+(?=([^\"]*\"[^\"]*\")*[^\"]*$)", " ");
        }
    }
}
