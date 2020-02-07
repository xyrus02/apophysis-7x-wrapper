//////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Console Utilities
//////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  Requirements
//  |
//  + { sys="System.Core" }
//  |
//  + { framework="4.0_client" }
//
//////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Apophysis.Interop
{
    static class ConsoleUtil
    {
        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        struct ProgressInfo {
            public readonly string Line;
            public string Info;
            public double Progress;
            public int X, Y;
            
            public ProgressInfo(string line) {
                Line = line;
                Info = string.Empty;
                Progress = 0;
                X = Console.CursorLeft;
                Y = Console.CursorTop;
            }
        }
        private static readonly Dictionary<IntPtr, ProgressInfo> _progressInfos = new Dictionary<IntPtr,ProgressInfo>();

        private static string[] _args = { };
        private static readonly Random _random;

        static ConsoleUtil()
        {
            _random = (new Random((int)DateTime.Now.Ticks));
        }
        static T ParseOption<T>(int position, T defaultValue)
        {
            if (position + 1 >= _args.Length) return defaultValue;
            string data = _args[position + 1];
            if (data.StartsWith("-") && !data.Contains(' '))
                return defaultValue;

            if (typeof(T) == typeof(sbyte))
            {
                sbyte x; if (!sbyte.TryParse(data, NumberStyles.Integer, CultureInfo.InvariantCulture, out x)) return defaultValue;
                return (T)(object)x;
            }

            if (typeof(T) == typeof(byte))
            {
                byte x; if (!byte.TryParse(data, NumberStyles.Integer, CultureInfo.InvariantCulture, out x)) return defaultValue;
                return (T)(object)x;
            }

            if (typeof(T) == typeof(short))
            {
                short x; if (!short.TryParse(data, NumberStyles.Integer, CultureInfo.InvariantCulture, out x)) return defaultValue;
                return (T)(object)x;
            }
            if (typeof(T) == typeof(ushort))
            {
                ushort x; if (!ushort.TryParse(data, NumberStyles.Integer, CultureInfo.InvariantCulture, out x)) return defaultValue;
                return (T)(object)x;
            }
            if (typeof(T) == typeof(int))
            {
                int x; if (!int.TryParse(data, NumberStyles.Integer, CultureInfo.InvariantCulture, out x)) return defaultValue;
                return (T)(object)x;
            }
            if (typeof(T) == typeof(uint))
            {
                uint x; if (!uint.TryParse(data, NumberStyles.Integer, CultureInfo.InvariantCulture, out x)) return defaultValue;
                return (T)(object)x;
            }
            if (typeof(T) == typeof(long))
            {
                long x; if (!long.TryParse(data, NumberStyles.Integer, CultureInfo.InvariantCulture, out x)) return defaultValue;
                return (T)(object)x;
            }
            if (typeof(T) == typeof(ulong))
            {
                ulong x; if (!ulong.TryParse(data, NumberStyles.Integer, CultureInfo.InvariantCulture, out x)) return defaultValue;
                return (T)(object)x;
            }
            if (typeof(T) == typeof(float))
            {
                float x; if (!float.TryParse(data, NumberStyles.Float, CultureInfo.InvariantCulture, out x)) return defaultValue;
                return (T)(object)x;
            }
            if (typeof(T) == typeof(double))
            {
                double x; if (!double.TryParse(data, NumberStyles.Float, CultureInfo.InvariantCulture, out x)) return defaultValue;
                return (T)(object)x;
            }
            if (typeof(T) == typeof(decimal))
            {
                decimal x; if (!decimal.TryParse(data, NumberStyles.Float, CultureInfo.InvariantCulture, out x)) return defaultValue;
                return (T)(object)x;
            }
            if (typeof(T) == typeof(bool))
            {
                if (data.ToLower() == "yes") return (T)(object)true;
                if (data.ToLower() == "no") return (T)(object)false;
                return defaultValue;
            }
            if (typeof(T) == typeof(string))
            {
                return (T)(object)data;
            }
            return defaultValue;
        }
        static T InternalGetOption<T>(string[] names, T defaultValue)
        {
            if (names.Length == 0) throw new ArgumentException();

            string[] existingArguments = _args;
            string[] primaryNames = names.Take(1).Select((x) => x.ToLower()).ToArray();
            string[] secondaryNames = names.Skip(1).Select((x) => x.ToLower()).ToArray();

            for (int i = 0; i < existingArguments.Length; i++)
            {
                if (existingArguments[i].StartsWith("--"))
                {
                    string currentName = existingArguments[i].Substring(2).ToLower();
                    if (primaryNames.Contains(currentName))
                    {
                        if (typeof(T) == typeof(bool)) return (T)(object)true;
                        return ParseOption(i, defaultValue);
                    }
                }
                else if (existingArguments[i].StartsWith("-"))
                {
                    string currentName = existingArguments[i].Substring(1).ToLower();
                    if (secondaryNames.Contains(currentName))
                    {
                        if (typeof(T) == typeof(bool)) return (T)(object)true;
                        return ParseOption(i, defaultValue);
                    }
                }
            }

            return defaultValue;
        }

        static string TrimText(int offset, string text)
        {
            var needsTrimming = text.Length + offset > Console.BufferWidth - 1;
            if (needsTrimming)
                text = text.Substring(0, Console.BufferWidth - offset - 1);
            if (needsTrimming && text.Length > 3)
                text = text.Substring(0, text.Length - 3) + "...";
            return text;
        }
        static void ClearLine(int y)
        {
            WriteLine(new string(' ', Console.BufferWidth), y);
        }
        static void WriteLine(string text, int py)
        {
            int x = Console.CursorLeft;
            int y = Console.CursorTop;
            var str = text;

            Console.CursorLeft = 0;
            Console.CursorTop = py;
            Console.Write(str);
            Console.CursorLeft = x;
            Console.CursorTop = y;
        }

        public static T GetOption<T>(string name)
        {
            return InternalGetOption<T>(new string[] { name }, default(T));
        }
        public static T GetOption<T>(string name, T defaultValue)
        {
            return InternalGetOption(new[] { name }, defaultValue);
        }
        public static T GetOption<T>(string name, params string[] altNames)
        {
            return InternalGetOption(new[] { name }.Concat(altNames).ToArray(), default(T));
        }
        public static T GetOption<T>(string name, T defaultValue, params string[] altNames)
        {
            return InternalGetOption(new[] { name }.Concat(altNames).ToArray(), defaultValue);
        }
        public static void SetOptions(string[] args)
        {
            _args = args ?? new string[] { };
        }

        public static void Banner()
        {
            var currentAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        
            var titleAttributes = currentAssembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            var copyrightAttributes = currentAssembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            var versionAttributes = currentAssembly.GetCustomAttributes(typeof(AssemblyVersionAttribute), false);

            var title = "";
            var copyright = "";
            var version = "";

            foreach (var attr in titleAttributes) title = (attr as AssemblyTitleAttribute ?? new AssemblyTitleAttribute("")).Title;
            foreach (var attr in copyrightAttributes) copyright = (attr as AssemblyCopyrightAttribute ?? new AssemblyCopyrightAttribute("")).Copyright;
            foreach (var attr in versionAttributes) version = (attr as AssemblyVersionAttribute ?? new AssemblyVersionAttribute("")).Version;

            if (!string.IsNullOrEmpty(title)) Console.WriteLine(title);
            if (!string.IsNullOrEmpty(version)) Console.WriteLine("Version {0}", version);
            if (!string.IsNullOrEmpty(copyright)) Console.WriteLine(copyright);
            if (!string.IsNullOrEmpty(title + version + copyright)) Console.WriteLine("");
        }

        public static void ConditionalExit(int code)
        {
#if DEBUG
            if (Console.KeyAvailable) Console.ReadKey(true);
            Console.Write("\nPress any key to continue...");
            Console.ReadKey(true);
#endif
            Environment.Exit(code);
        }
        public static void ConditionalExecute(Action action)
        {
#if DEBUG
            action();
#else
            try { action(); }
            catch (Exception exception) {
                Console.WriteLine(exception.Message);
                ConsoleUtil.ConditionalExit(-1);
            }
#endif
        }

        public static IntPtr ProgressLineStart(string text)
        {
            return ProgressLineStart(text, null);
        }
        public static IntPtr ProgressLineStart(string text, string infoText)
        {
            var ptr = new IntPtr(_random.Next());
            var progress = new ProgressInfo(text);

            _progressInfos.Add(ptr, progress);
            ProgressLineUpdate(ptr, 0, infoText);

            return ptr;
        }
        
        public static void ProgressLineUpdate(IntPtr line, double progress)
        {
            ProgressLineUpdate(line, progress, null);
        }
        public static void ProgressLineUpdate(IntPtr line, double value, string infoText)
        {
            var progress = _progressInfos[line];
            var str = progress.Line + "..." + ((int)Math.Round(value * 100)).ToString() + "%";

            if (infoText != null) progress.Info = infoText;
            progress.Progress = value;

            if (!string.IsNullOrEmpty(progress.Info)) {
                var str2 = " (" + progress.Info + ")";
                str += TrimText(str.Length, str2);
            }

            ClearLine(progress.Y);
            WriteLine(str, progress.Y);
            Console.CursorLeft = 0;
            Console.CursorTop = progress.Y + 1;
        }
        public static void ProgressLineDone(IntPtr line)
        {
            ProgressLineUpdate(line, 1, string.Empty);
            _progressInfos.Remove(line);
        }
    }
}