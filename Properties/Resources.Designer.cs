﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CRTQL {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("CRTQL.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
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
        ///   Looks up a localized string similar to Failed to back up file: {0}{1} Skipping formatting for this file..
        /// </summary>
        internal static string BackupFailureWarningMessage {
            get {
                return ResourceManager.GetString("BackupFailureWarningMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to write reformatted contents: {0}.
        /// </summary>
        internal static string ContentWriteFailureWarningMessage {
            get {
                return ResourceManager.GetString("ContentWriteFailureWarningMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error detail: {0}.
        /// </summary>
        internal static string ErrorDetailMessageFragment {
            get {
                return ResourceManager.GetString("ErrorDetailMessageFragment", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to read file contents (aborted): {0}.
        /// </summary>
        internal static string FileReadFailureWarningMessage {
            get {
                return ResourceManager.GetString("FileReadFailureWarningMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to create target folder: {0}.
        /// </summary>
        internal static string FolderCreationFailureWarningMessage {
            get {
                return ResourceManager.GetString("FolderCreationFailureWarningMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No files found matching filename/pattern ({0}) and extension ({1}).
        /// </summary>
        internal static string NoFilesFoundWarningMessage {
            get {
                return ResourceManager.GetString("NoFilesFoundWarningMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No input (filename(s) or piped input) has been provided..
        /// </summary>
        internal static string NoInputErrorMessage {
            get {
                return ResourceManager.GetString("NoInputErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The requested output file could not be created. Error detail: {0}.
        /// </summary>
        internal static string OutputFileCreationErrorMessage {
            get {
                return ResourceManager.GetString("OutputFileCreationErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to --WARNING! ERRORS ENCOUNTERED DURING SQL PARSING!.
        /// </summary>
        internal static string ParseErrorWarningPrefix {
            get {
                return ResourceManager.GetString("ParseErrorWarningPrefix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Encountered error when parsing or formatting file contents (aborted): {0}.
        /// </summary>
        internal static string ParsingErrorWarningMessage {
            get {
                return ResourceManager.GetString("ParsingErrorWarningMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error processing requested filename/pattern. Error detail: {0}.
        /// </summary>
        internal static string PathPatternErrorMessage {
            get {
                return ResourceManager.GetString("PathPatternErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to NOTE: this file may have been overwritten with partial content!.
        /// </summary>
        internal static string PossiblePartialWriteWarningMessage {
            get {
                return ResourceManager.GetString("PossiblePartialWriteWarningMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Poor Man&apos;s T-SQL Formatter - a small free Transact-SQL formatting 
        ///library for .Net 2.0 and JS, written in C#. Distributed under AGPL v3.
        ///Copyright (C) 2011-2017 Tao Klerks.
        /// </summary>
        internal static string ProgramSummary {
            get {
                return ResourceManager.GetString("ProgramSummary", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Usage notes: 
        ///
        ///SqlFormatter &lt;filename or pattern&gt; &lt;options&gt;
        ///
        ///is  indentString (default: \t)
        ///st  spacesPerTab (default: 4)
        ///mw  maxLineWidth (default: 999)
        ///sb  statementBreaks (default: 2)
        ///cb  clauseBreaks (default: 1)
        ///tc  trailingCommas (default: false)
        ///sac spaceAfterExpandedComma (default: false)
        ///ebc expandBetweenConditions (default: true)
        ///ebe expandBooleanExpressions (default: true)
        ///ecs expandCaseStatements (default: true)
        ///ecl expandCommaLists (default: true)
        ///eil expandInLists (default: true [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string ProgramUsageNotes {
            get {
                return ResourceManager.GetString("ProgramUsageNotes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unrecognized arguments found!.
        /// </summary>
        internal static string UnrecognizedArgumentsErrorMessage {
            get {
                return ResourceManager.GetString("UnrecognizedArgumentsErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The provided Language Code is not supported..
        /// </summary>
        internal static string UnrecognizedLanguageErrorMessage {
            get {
                return ResourceManager.GetString("UnrecognizedLanguageErrorMessage", resourceCulture);
            }
        }
    }
}
