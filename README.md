# ME2CoalescedCompiler
Compiler and decompiler for the ME2 Coalesced.ini file format.

This program serves as a documentation reference source for how the Coalesced.ini file is built, as most sources in 2019 have disappeared.

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
