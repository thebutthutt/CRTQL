using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using NDesk.Options;
using System.Resources;

namespace CRTQL {
    class Program {

        private static ResourceManager rm = null;

        static int Main(string[] args) {
            rm = new ResourceManager("CRTQL.Properties.Resources", typeof(Program).Assembly);

            //formatter engine option defaults
            var options = new PoorMansTSqlFormatterLib.Formatters.TSqlStandardFormatterOptions {
                KeywordStandardization = false,
                IndentString = "    ",
                SpacesPerTab = 4,
                MaxLineWidth = 999,
                NewStatementLineBreaks = 4,
                NewClauseLineBreaks = 1,
                TrailingCommas = true,
                SpaceAfterExpandedComma = false,
                ExpandBetweenConditions = true,
                ExpandBooleanExpressions = true,
                ExpandCaseStatements = true,
                ExpandCommaLists = true,
                BreakJoinOnSections = true,
                UppercaseKeywords = true,
                ExpandInLists = true,
                WrapNamesInBraces = true
            };

            //bulk formatter options
            bool allowParsingErrors = false;
            List<string> extensions = new List<string>();
            bool backups = true;
            bool recursiveSearch = false;
            string outputFileOrFolder = null;
            string uiLangCode = null;

            //flow/tracking switches
            bool showUsageFriendly = false;
            bool showUsageError = false;

            OptionSet p = new OptionSet()
                .Add("is|indentString=", delegate (string v) { options.IndentString = v; })
                .Add("st|spacesPerTab=", delegate (string v) { options.SpacesPerTab = int.Parse(v); })
                .Add("mw|maxLineWidth=", delegate (string v) { options.MaxLineWidth = int.Parse(v); })
                .Add("sb|statementBreaks=", delegate (string v) { options.NewStatementLineBreaks = int.Parse(v); })
                .Add("cb|clauseBreaks=", delegate (string v) { options.NewClauseLineBreaks = int.Parse(v); })
                .Add("tc|trailingCommas", delegate (string v) { options.TrailingCommas = v != null; })
                .Add("sac|spaceAfterExpandedComma", delegate (string v) { options.SpaceAfterExpandedComma = v != null; })
                .Add("ebc|expandBetweenConditions", delegate (string v) { options.ExpandBetweenConditions = v != null; })
                .Add("ebe|expandBooleanExpressions", delegate (string v) { options.ExpandBooleanExpressions = v != null; })
                .Add("ecs|expandCaseStatements", delegate (string v) { options.ExpandCaseStatements = v != null; })
                .Add("ecl|expandCommaLists", delegate (string v) { options.ExpandCommaLists = v != null; })
                .Add("eil|expandInLists", delegate (string v) { options.ExpandInLists = v != null; })
                .Add("wnb|wrapNamesInBraces", delegate (string v) { options.WrapNamesInBraces = v != null; })
                .Add("bjo|breakJoinOnSections", delegate (string v) { options.BreakJoinOnSections = v != null; })
                .Add("uk|uppercaseKeywords", delegate (string v) { options.UppercaseKeywords = v != null; })
                .Add("sk|standardizeKeywords", delegate (string v) { options.KeywordStandardization = v != null; })
                .Add("ae|allowParsingErrors", delegate (string v) { allowParsingErrors = v != null; })
                .Add("e|extensions=", delegate (string v) { extensions.Add((v.StartsWith(".") ? "" : ".") + v); })
                .Add("r|recursive", delegate (string v) { recursiveSearch = v != null; })
                .Add("b|backups", delegate (string v) { backups = v != null; })
                .Add("o|outputFileOrFolder=", delegate (string v) { outputFileOrFolder = v; })
                .Add("l|languageCode=", delegate (string v) { uiLangCode = v; })
                .Add("h|?|help", delegate (string v) { showUsageFriendly = v != null; });

            //first parse the args
            List<string> remainingArgs = p.Parse(args);


            //nasty trick to figure out whether we're in a pipeline or not
            bool throwAwayValue;
            string stdInput = null;

            try {
                throwAwayValue = Console.KeyAvailable;
            }
            catch (InvalidOperationException) {
                Console.InputEncoding = Encoding.UTF8;
                stdInput = Console.In.ReadToEnd();
            }


            //then complain about missing input or unrecognized args
            if (string.IsNullOrEmpty(stdInput) && remainingArgs.Count == 0) {
                showUsageError = true;
                Console.Error.WriteLine(rm.GetString("NoInputErrorMessage"));
            }
            else if ((!string.IsNullOrEmpty(stdInput) && remainingArgs.Count == 1) || remainingArgs.Count > 1) {
                showUsageError = true;
                Console.Error.WriteLine(rm.GetString("UnrecognizedArgumentsErrorMessage"));
            }


            if (extensions.Count == 0) extensions.Add(".sql");


            if (showUsageFriendly || showUsageError) {
                TextWriter outStream = showUsageFriendly ? Console.Out : Console.Error;
                outStream.WriteLine(rm.GetString("ProgramSummary"));
                outStream.WriteLine("v" + Assembly.GetExecutingAssembly().GetName().Version.ToString());
                outStream.WriteLine(rm.GetString("ProgramUsageNotes"));
                return 1;
            }

            /* Actually starting now! */
            bool warningEncountered = false;

            // set up a formatter
            var formatter = new PoorMansTSqlFormatterLib.Formatters.TSqlStandardFormatter(options);
            formatter.ErrorOutputPrefix = rm.GetString("ParseErrorWarningPrefix") + Environment.NewLine;

            // create the manager
            var formattingManager = new PoorMansTSqlFormatterLib.SqlFormattingManager(formatter);

            // if we have cmd input
            if (!string.IsNullOrEmpty(stdInput)) {
                string formattedOutput = null;
                bool parsingError = false;
                Exception parseException = null;
                try {
                    formattedOutput = formattingManager.Format(stdInput, ref parsingError);

                    //hide any handled parsing issues if they were requested to be allowed
                    if (allowParsingErrors) parsingError = false;
                }
                catch (Exception ex) {
                    parseException = ex;
                    parsingError = true;
                }

                if (parsingError) {
                    Console.Error.WriteLine(string.Format(rm.GetString("ParsingErrorWarningMessage"), "STDIN"));
                    if (parseException != null) Console.Error.WriteLine(string.Format(rm.GetString("ErrorDetailMessageFragment"), parseException.Message));
                    warningEncountered = true;
                }
                else {
                    if (!string.IsNullOrEmpty(outputFileOrFolder)) {
                        WriteResultFile(outputFileOrFolder, null, null, ref warningEncountered, formattedOutput);
                    }
                    else {
                        Console.OutputEncoding = Encoding.UTF8;
                        Console.Out.WriteLine(formattedOutput);
                    }
                }

            }

            // if we have file input
            else {
                DirectoryInfo baseDirectory = null;
                string searchPattern = Path.GetFileName(remainingArgs[0]);
                string baseDirectoryName = Path.GetDirectoryName(remainingArgs[0]);

                if (baseDirectoryName.Length == 0) {
                    baseDirectoryName = ".";
                    if (searchPattern.Equals("."))
                        searchPattern = "";
                }

                /* collect the matching files to be formatted */
                FileSystemInfo[] matchingObjects = null;
                try {
                    baseDirectory = new DirectoryInfo(baseDirectoryName);
                    if (searchPattern.Length > 0) {
                        if (recursiveSearch)
                            matchingObjects = baseDirectory.GetFileSystemInfos(searchPattern);
                        else
                            matchingObjects = baseDirectory.GetFiles(searchPattern);
                    }
                    else {
                        if (recursiveSearch)
                            matchingObjects = baseDirectory.GetFileSystemInfos();
                        else
                            matchingObjects = new FileSystemInfo[0];
                    }
                }
                catch (Exception e) {
                    Console.Error.WriteLine(string.Format(rm.GetString("PathPatternErrorMessage"), e.Message));
                    return 2;
                }

                /* setup the output - either replace in-file or poop into some folder */
                StreamWriter singleFileWriter = null;
                string replaceFromFolderPath = null;
                string replaceToFolderPath = null;
                if (!string.IsNullOrEmpty(outputFileOrFolder)) {
                    //ignore the backups setting - wouldn't make sense to back up the source files if we're writing to another file anyway...
                    backups = false;

                    if (Directory.Exists(outputFileOrFolder)
                    && (File.GetAttributes(outputFileOrFolder) & FileAttributes.Directory) == FileAttributes.Directory
                    ) {
                        replaceFromFolderPath = baseDirectory.FullName;
                        replaceToFolderPath = new DirectoryInfo(outputFileOrFolder).FullName;
                    }
                    else {
                        try {
                            //let's not worry too hard about releasing this resource - this is a command-line program, when it ends or dies all will be released anyway.
                            singleFileWriter = new StreamWriter(outputFileOrFolder);
                        }
                        catch (Exception e) {
                            Console.Error.WriteLine(string.Format(rm.GetString("OutputFileCreationErrorMessage"), e.Message));
                            return 3;
                        }
                    }
                }

                if (!ProcessSearchResults(
                        extensions, 
                        backups, 
                        allowParsingErrors, 
                        formattingManager, 
                        matchingObjects, 
                        singleFileWriter, 
                        replaceFromFolderPath, 
                        replaceToFolderPath, 
                        ref warningEncountered)) {
                    Console.Error.WriteLine(string.Format(rm.GetString("NoFilesFoundWarningMessage"), remainingArgs[0], string.Join(",", extensions.ToArray())));
                    return 4;
                }

                if (singleFileWriter != null) {
                    singleFileWriter.Flush();
                    singleFileWriter.Close();
                    singleFileWriter.Dispose();
                }
            }

            if (warningEncountered)
                return 5; //general "there were warnings" return code
            else
                return 0; //we got there, did something, and received no (handled) errors!
        }



        /// <summary>
        /// Given a list of files, call the ReFormatFile method on each sql file
        /// </summary>
        private static bool ProcessSearchResults(List<string> extensions, bool backups, bool allowParsingErrors,
            PoorMansTSqlFormatterLib.SqlFormattingManager formattingManager, FileSystemInfo[] matchingObjects,
            StreamWriter singleFileWriter, string replaceFromFolderPath, string replaceToFolderPath,
            ref bool warningEncountered) {

            bool fileFound = false;

            foreach (var fsEntry in matchingObjects) {
                if (fsEntry is FileInfo) {
                    if (extensions.Contains(fsEntry.Extension)) {
                        ReFormatFile(
                            (FileInfo)fsEntry, 
                            formattingManager, 
                            backups, 
                            allowParsingErrors, 
                            singleFileWriter, 
                            replaceFromFolderPath, 
                            replaceToFolderPath, 
                            ref warningEncountered);
                        fileFound = true;
                    }
                }
                else {
                    if (ProcessSearchResults(
                            extensions, 
                            backups, 
                            allowParsingErrors, 
                            formattingManager, 
                            ((DirectoryInfo)fsEntry).GetFileSystemInfos(), 
                            singleFileWriter, 
                            replaceFromFolderPath, 
                            replaceToFolderPath, 
                            ref warningEncountered))
                        fileFound = true;
                }
            }

            return fileFound;
        }

        /// <summary>
        /// Actually do the logic of formatting one file
        /// </summary>
        private static void ReFormatFile(
            FileInfo fileInfo, PoorMansTSqlFormatterLib.SqlFormattingManager formattingManager, bool backups,
            bool allowParsingErrors, StreamWriter singleFileWriter, string replaceFromFolderPath,
            string replaceToFolderPath, ref bool warningEncountered) {

            bool failedBackup = false;
            string oldFileContents = "";
            string newFileContents = "";
            bool parsingError = false;
            bool failedFolder = false;
            Exception parseException = null;

            //TODO: play with / test encoding complexities
            //TODO: consider using auto-detection - read binary, autodetect, convert.
            //TODO: consider whether to keep same output encoding as source file, or always use same, and if so whether to make parameter-based.
            try {
                oldFileContents = File.ReadAllText(fileInfo.FullName);
            }
            catch (Exception ex) {
                Console.Error.WriteLine(string.Format(rm.GetString("FileReadFailureWarningMessage"), fileInfo.FullName));
                Console.Error.WriteLine(string.Format(rm.GetString("ErrorDetailMessageFragment"), ex.Message));
                warningEncountered = true;
            }

            if (oldFileContents.Length > 0) {
                try {
                    newFileContents = formattingManager.Format(oldFileContents, ref parsingError);

                    //hide any handled parsing issues if they were requested to be allowed
                    if (allowParsingErrors) parsingError = false;
                }
                catch (Exception ex) {
                    parseException = ex;
                    parsingError = true;
                }

                if (parsingError) {
                    Console.Error.WriteLine(string.Format(rm.GetString("ParsingErrorWarningMessage"), fileInfo.FullName));
                    if (parseException != null) Console.Error.WriteLine(string.Format(rm.GetString("ErrorDetailMessageFragment"), parseException.Message));
                    warningEncountered = true;
                }
            }

            if (
                !parsingError
                && (
                    (newFileContents.Length > 0 && !oldFileContents.Equals(newFileContents)) // newfile has text + not the same as old file
                    || singleFileWriter != null
                    || (replaceFromFolderPath != null && replaceToFolderPath != null)
                )
            ) {
                if (backups) {
                    try { fileInfo.CopyTo(fileInfo.FullName + ".bak", true); }
                    catch (Exception ex) {
                        Console.Error.WriteLine(string.Format(rm.GetString("BackupFailureWarningMessage"), fileInfo.FullName, Environment.NewLine));
                        Console.Error.WriteLine(string.Format(rm.GetString("ErrorDetailMessageFragment"), ex.Message));
                        failedBackup = true;
                        warningEncountered = true;
                    }
                }
                if (!failedBackup) {
                    if (singleFileWriter != null) {
                        //we'll assume that running out of disk space, and other while-you-are-writing errors, and not worth worrying about
                        singleFileWriter.WriteLine(newFileContents);
                        singleFileWriter.WriteLine("GO");
                    }
                    else {
                        string fullTargetPath = fileInfo.FullName;
                        if (replaceFromFolderPath != null && replaceToFolderPath != null) {
                            fullTargetPath = fullTargetPath.Replace(replaceFromFolderPath, replaceToFolderPath);
                            string targetFolder = Path.GetDirectoryName(fullTargetPath);

                            try {
                                if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);
                            }
                            catch (Exception ex) {
                                Console.Error.WriteLine(string.Format(rm.GetString("FolderCreationFailureWarningMessage"), targetFolder));
                                Console.Error.WriteLine(string.Format(rm.GetString("ErrorDetailMessageFragment"), ex.Message));
                                failedFolder = true;
                                warningEncountered = true;
                            }
                        }

                        if (!failedFolder) {
                            WriteResultFile(fullTargetPath, replaceFromFolderPath, replaceToFolderPath, ref warningEncountered, newFileContents);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Write text to the specified file
        /// </summary>
        private static void WriteResultFile(
            string targetFilePath, string replaceFromFolderPath, string replaceToFolderPath, ref bool warningEncountered,
            string newFileContents) {

            try {
                File.WriteAllText(targetFilePath, newFileContents);
            }
            catch (Exception ex) {
                Console.Error.WriteLine(string.Format(rm.GetString("ContentWriteFailureWarningMessage"), targetFilePath));
                Console.Error.WriteLine(string.Format(rm.GetString("ErrorDetailMessageFragment"), ex.Message));
                if (replaceFromFolderPath == null || replaceToFolderPath == null) Console.Error.WriteLine(rm.GetString("PossiblePartialWriteWarningMessage"));
                warningEncountered = true;
            }
        }
    }
}
