// Copyright (c) 2019 Michael Perez (ME3Tweaks)
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Linq;
using System.Text;

namespace ME2IniCompiler
{
    /**
     * The Mass Effect 2 Coalesced.ini file format is as follows:
     * Header: 0x1E integer (unknown purpose)
     * 
     * while (until end of file) {
     *  Unreal String Filepath (4 bytes length, length in ascii string data (null terminated)
     *  Unreal String File contents (4 bytes length, length in ascii string data (null terminated)
     * }
     *
     *
     * Usage of this program:
     * ME2IniCompiler.exe folderpath
     *      Compiles all ini files (except one named Coalesced.ini) into a Coalesced.ini file.
     * ME2INiCompiler.exe fileplath
     *      Dumps all ini file sin the specified Coalesced.ini filepath. The file must be named Coalesced.ini
     * 
     */
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Requires single argument: Directory (for compiling) or Coalesced.ini file (for decompiling)");
                Environment.Exit(1);
                return;
            }

            string arg = args[0];
            if (Directory.Exists(arg))
            {
                //Directory, compiler
                var files = Directory.GetFiles(arg, "*.ini").Where(x => Path.GetFileName(x) != "Coalesced.ini").ToList();
                MemoryStream ms = new MemoryStream();
                Console.WriteLine("Number of files to compile into this coalesced: " + files.Count);
                ms.WriteInt32(0x1E); //Unknown header but seems to just be 1E. Can't find any documentation on what this is.
                foreach (var file in files)
                {
                    Console.WriteLine("Coalescing " + Path.GetFileName(file));
                    var filename = Path.GetFileName(file);
                    filename = @"..\BIOGame\Config\PC\Cooked\" + filename;
                    ms.WriteUnrealStringASCII(filename);
                    ms.WriteUnrealStringASCII(File.ReadAllText(file));
                }
                File.WriteAllBytes(Path.Combine(arg, "Coalesced.ini"), ms.ToArray());
            }
            else
            {
                //File, decompiler
                if (Path.GetFileName(arg) == "Coalesced.ini")
                {
                    using FileStream fs = new FileStream(arg, FileMode.Open);
                    int unknownInt = fs.ReadInt32();
                    if (unknownInt != 0x1E)
                    {
                        Console.WriteLine("First 4 bytes were not 0x1E. This does not appear to be a Coalesced file.");
                        Environment.Exit(1);
                    }
                    while (fs.Position < fs.Length)
                    {
                        long pos = fs.Position;
                        string filename = fs.ReadUnrealString();
                        string contents = fs.ReadUnrealString();
                        Console.WriteLine("Writing out file " + Path.GetFileName(filename) + " from position 0x" + pos.ToString("X6"));
                        File.WriteAllText(Path.Combine(Directory.GetParent(arg).FullName, Path.GetFileName(filename)), contents);
                    }
                }
                else
                {
                    Console.WriteLine("Can only decompile files named Coalesced.ini.");
                    Environment.Exit(1);
                    return;
                }
            }
            Console.ReadKey();
        }


    }
    static class Extensions
    {
        public static void WriteInt32(this Stream stream, int data)
        {
            stream.Write(BitConverter.GetBytes(data), 0, sizeof(int));
        }

        /// <summary>
        /// Writes a null terminated string to the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="str"></param>
        public static void WriteStringASCIINull(this Stream stream, string str)
        {
            stream.WriteStringASCII(str + "\0");
        }

        /// <summary>
        /// Writes a string to the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="str"></param>
        public static void WriteStringASCII(this Stream stream, string str)
        {
            stream.Write(Encoding.ASCII.GetBytes(str), 0, Encoding.ASCII.GetByteCount(str));
        }

        public static void WriteUnrealStringASCII(this Stream stream, string value)
        {
            if (value?.Length > 0)
            {
                stream.WriteInt32(value.Length + 1);
                stream.WriteStringASCIINull(value);
            }
            else
            {
                stream.WriteInt32(0);
            }
        }

        public static int ReadInt32(this Stream stream)
        {
            var buffer = new byte[sizeof(int)];
            if (stream.Read(buffer, 0, sizeof(int)) != sizeof(int))
                throw new Exception();
            return BitConverter.ToInt32(buffer, 0);
        }

        public static string ReadUnrealString(this Stream stream)
        {
            int length = stream.ReadInt32();
            if (length == 0)
            {
                return "";
            }

            if (length < 0)
            {
                Console.WriteLine("Unicode coded files are not supported by this application.");
                Environment.Exit(1);
            }

            return stream.ReadStringASCIINull(length);
        }

        public static string ReadStringASCIINull(this Stream stream)
        {
            string str = "";
            for (; ; )
            {
                char c = (char)stream.ReadByte();
                if (c == 0)
                    break;
                str += c;
            }
            return str;
        }

        public static string ReadStringASCIINull(this Stream stream, int count)
        {
            return stream.ReadStringASCII(count).Trim('\0');
        }

        public static string ReadStringASCII(this Stream stream, int count)
        {
            byte[] buffer = stream.ReadToBuffer(count);
            return Encoding.ASCII.GetString(buffer);
        }

        public static byte[] ReadToBuffer(this Stream stream, int count)
        {
            var buffer = new byte[count];
            if (stream.Read(buffer, 0, count) != count)
                throw new Exception("Stream read error!");
            return buffer;
        }
    }
}
