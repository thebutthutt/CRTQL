/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0 and JS, written in C#. 
Copyright (C) 2011-2017 Tao Klerks

Additional Contributors:
 * Timothy Klenke, 2012

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PoorMansTSqlFormatterLib.Interfaces;
using PoorMansTSqlFormatterLib.ParseStructure;

namespace PoorMansTSqlFormatterLib.Formatters {
    public partial class TSqlStandardFormatter : ISqlTreeFormatter {
        public TSqlStandardFormatter() : this(new TSqlStandardFormatterOptions()) { }

        public TSqlStandardFormatter(TSqlStandardFormatterOptions options) {
            if (options == null)
                throw new ArgumentNullException("options");

            Options = options;

            if (options.KeywordStandardization)
                KeywordMapping = StandardKeywordRemapping.Instance;
            ErrorOutputPrefix = MessagingConstants.FormatErrorDefaultMessage + Environment.NewLine;
        }

        public TSqlStandardFormatterOptions Options { get; private set; }

        public IDictionary<string, string> KeywordMapping = new Dictionary<string, string>();

        public string ErrorOutputPrefix { get; set; }


        /// <summary>
        /// Start here
        /// </summary>
        public string FormatSQLTree(Node sqlTreeDoc) {
            //thread-safe - each call to FormatSQLTree() gets its own independent state object
            TSqlStandardFormattingState state = new TSqlStandardFormattingState(
                                                                                Options.IndentString,
                                                                                Options.SpacesPerTab,
                                                                                Options.MaxLineWidth,
                                                                                0);

            if (sqlTreeDoc.Name == SqlElemNames.SQL_ROOT && sqlTreeDoc.GetAttributeValue(SqlElemNames.ANAME_ERRORFOUND) == "1")
                state.AddOutputContent(ErrorOutputPrefix);

            ProcessSqlNodeList(sqlTreeDoc.Children, state);

            WhiteSpace_BreakAsExpected(state);

            //someone forgot to close a "[noformat]" or "[minify]" region... we'll assume that's ok
            if (state.SpecialRegionActive == SpecialRegionType.NoFormat) {
                Node skippedXml = NodeExtensions.ExtractStructureBetween(state.RegionStartNode, sqlTreeDoc);
                TSqlIdentityFormatter tempFormatter = new TSqlIdentityFormatter();
                state.AddOutputContent(tempFormatter.FormatSQLTree(skippedXml));
            }
            else if (state.SpecialRegionActive == SpecialRegionType.Minify) {
                Node skippedXml = NodeExtensions.ExtractStructureBetween(state.RegionStartNode, sqlTreeDoc);
                TSqlIdentityFormatter tempFormatter = new TSqlIdentityFormatter();
                state.AddOutputContent(tempFormatter.FormatSQLTree(skippedXml));
            }
            return state.DumpOutput();
        }

        private void ProcessSqlNodeList(IEnumerable<Node> rootList, TSqlStandardFormattingState state) {
            foreach (Node contentElement in rootList)
                ProcessSqlNode(contentElement, state);
        }

        private void ProcessSqlNode(Node contentElement, TSqlStandardFormattingState state) {
            int initialIndent = state.IndentLevel;


            switch (contentElement.Name) {
                case SqlElemNames.SQL_STATEMENT:
                    WhiteSpace_SeparateStatements(contentElement, state);
                    state.ResetKeywords();
                    ProcessSqlNodeList(contentElement.Children, state);
                    state.StatementBreakExpected = true;
                    break;

                case SqlElemNames.SQL_CLAUSE:
                    state.UnIndentInitialBreak = true;
                    ProcessSqlNodeList(contentElement.Children, state.IncrementIndent());
                    state.DecrementIndent();
                    if (Options.NewClauseLineBreaks > 0)
                        state.BreakExpected = true;
                    if (Options.NewClauseLineBreaks > 1)
                        state.AdditionalBreaksExpected = Options.NewClauseLineBreaks - 1;
                    break;

                case SqlElemNames.SET_OPERATOR_CLAUSE:
                    state.DecrementIndent();
                    state.WhiteSpace_BreakToNextLine(); //this is the one already recommended by the start of the clause
                    state.WhiteSpace_BreakToNextLine(); //this is the one we additionally want to apply
                    ProcessSqlNodeList(contentElement.Children, state.IncrementIndent());
                    state.BreakExpected = true;
                    state.AdditionalBreaksExpected = 1;
                    break;

                case SqlElemNames.BATCH_SEPARATOR:
                    //newline regardless of whether previous element recommended a break or not.
                    state.WhiteSpace_BreakToNextLine();
                    ProcessSqlNodeList(contentElement.Children, state);
                    state.BreakExpected = true;
                    break;

                case SqlElemNames.DDL_PROCEDURAL_BLOCK:
                case SqlElemNames.DDL_OTHER_BLOCK:
                case SqlElemNames.DDL_DECLARE_BLOCK:
                case SqlElemNames.CURSOR_DECLARATION:
                case SqlElemNames.BEGIN_TRANSACTION:
                case SqlElemNames.SAVE_TRANSACTION:
                case SqlElemNames.COMMIT_TRANSACTION:
                case SqlElemNames.ROLLBACK_TRANSACTION:
                case SqlElemNames.CONTAINER_OPEN:
                case SqlElemNames.CONTAINER_CLOSE:
                case SqlElemNames.WHILE_LOOP:
                case SqlElemNames.IF_STATEMENT:
                case SqlElemNames.CONTAINER_GENERALCONTENT:
                case SqlElemNames.CTE_WITH_CLAUSE:
                case SqlElemNames.PERMISSIONS_BLOCK:
                case SqlElemNames.PERMISSIONS_DETAIL:
                case SqlElemNames.MERGE_CLAUSE:
                case SqlElemNames.MERGE_TARGET:
                    ProcessSqlNodeList(contentElement.Children, state);
                    break;

                case SqlElemNames.SELECTIONTARGET:
                    state.BreakExpected = true;
                    //state.IncrementIndent();
                    ProcessSqlNodeList(contentElement.Children, state);
                    //state.DecrementIndent();
                    break;

                case SqlElemNames.CASE_INPUT:
                case SqlElemNames.BOOLEAN_EXPRESSION:
                case SqlElemNames.BETWEEN_LOWERBOUND:
                case SqlElemNames.BETWEEN_UPPERBOUND:
                    WhiteSpace_SeparateWords(state);
                    ProcessSqlNodeList(contentElement.Children, state);
                    break;

                case SqlElemNames.CONTAINER_SINGLESTATEMENT:
                case SqlElemNames.CONTAINER_MULTISTATEMENT:
                case SqlElemNames.MERGE_ACTION:

                    bool singleStatementIsIf = false;
                    foreach (Node statement in contentElement.ChildrenByName(SqlElemNames.SQL_STATEMENT))
                        foreach (Node clause in statement.ChildrenByName(SqlElemNames.SQL_CLAUSE))
                            foreach (Node ifStatement in clause.ChildrenByName(SqlElemNames.IF_STATEMENT))
                                singleStatementIsIf = true;

                    if (singleStatementIsIf && contentElement.Parent.Name.Equals(SqlElemNames.ELSE_CLAUSE)) {
                        //artificially decrement indent and skip new statement break for "ELSE IF" constructs
                        state.DecrementIndent();
                    }
                    else {
                        state.BreakExpected = true;
                    }
                    ProcessSqlNodeList(contentElement.Children, state);
                    if (singleStatementIsIf && contentElement.Parent.Name.Equals(SqlElemNames.ELSE_CLAUSE)) {
                        //bring indent back to symmetrical level
                        state.IncrementIndent();
                    }

                    state.StatementBreakExpected = false; //the responsibility for breaking will be with the OUTER statement; there should be no consequence propagating out from statements in this container;
                    state.UnIndentInitialBreak = false; //if there was no word spacing after the last content statement's clause starter, doesn't mean the unIndent should propagate to the following content!
                    break;

                case SqlElemNames.PERMISSIONS_TARGET:
                case SqlElemNames.PERMISSIONS_RECIPIENT:
                case SqlElemNames.DDL_WITH_CLAUSE:
                case SqlElemNames.MERGE_CONDITION:
                case SqlElemNames.MERGE_THEN:
                    state.BreakExpected = true;
                    state.UnIndentInitialBreak = true;
                    ProcessSqlNodeList(contentElement.Children, state.IncrementIndent());
                    state.DecrementIndent();
                    break;

                case SqlElemNames.JOIN_TARGET:
                    ProcessSqlNodeList(contentElement.ChildrenByName(SqlElemNames.CONTAINER_OPEN), state);
                    state.IncrementIndent();
                    ProcessSqlNodeList(contentElement.ChildrenByName(SqlElemNames.CONTAINER_GENERALCONTENT), state);
                    break;

                case SqlElemNames.JOIN_ON_SECTION:
                    state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.ChildrenByName(SqlElemNames.CONTAINER_OPEN), state);
                    //state.IncrementIndent();
                    ProcessSqlNodeList(contentElement.ChildrenByName(SqlElemNames.CONTAINER_GENERALCONTENT), state);
                    state.DecrementIndent();
                    break;

                case SqlElemNames.CTE_ALIAS:
                    state.UnIndentInitialBreak = true;
                    ProcessSqlNodeList(contentElement.Children, state);
                    break;

                case SqlElemNames.ELSE_CLAUSE:
                    ProcessSqlNodeList(contentElement.ChildrenByName(SqlElemNames.CONTAINER_OPEN), state.DecrementIndent());
                    ProcessSqlNodeList(contentElement.ChildrenByName(SqlElemNames.CONTAINER_SINGLESTATEMENT), state.IncrementIndent());
                    break;

                case SqlElemNames.DDL_AS_BLOCK:
                case SqlElemNames.CURSOR_FOR_BLOCK:
                    state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.ChildrenByName(SqlElemNames.CONTAINER_OPEN), state.DecrementIndent());
                    state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.ChildrenByName(SqlElemNames.CONTAINER_GENERALCONTENT), state);
                    state.IncrementIndent();
                    break;

                case SqlElemNames.TRIGGER_CONDITION:
                    state.DecrementIndent();
                    state.WhiteSpace_BreakToNextLine();
                    ProcessSqlNodeList(contentElement.Children, state.IncrementIndent());
                    break;

                case SqlElemNames.CURSOR_FOR_OPTIONS:
                case SqlElemNames.CTE_AS_BLOCK:
                    state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.ChildrenByName(SqlElemNames.CONTAINER_OPEN), state.DecrementIndent());
                    ProcessSqlNodeList(contentElement.ChildrenByName(SqlElemNames.CONTAINER_GENERALCONTENT), state.IncrementIndent());
                    break;

                case SqlElemNames.DDL_RETURNS:
                case SqlElemNames.MERGE_USING:
                case SqlElemNames.MERGE_WHEN:
                    state.BreakExpected = true;
                    state.UnIndentInitialBreak = true;
                    ProcessSqlNodeList(contentElement.Children, state);
                    break;

                case SqlElemNames.BETWEEN_CONDITION:
                    ProcessSqlNodeList(contentElement.ChildrenByName(SqlElemNames.CONTAINER_OPEN), state);
                    state.IncrementIndent();
                    ProcessSqlNodeList(contentElement.ChildrenByName(SqlElemNames.BETWEEN_LOWERBOUND), state.IncrementIndent());
                    if (Options.ExpandBetweenConditions)
                        state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.ChildrenByName(SqlElemNames.CONTAINER_CLOSE), state.DecrementIndent());
                    ProcessSqlNodeList(contentElement.ChildrenByName(SqlElemNames.BETWEEN_UPPERBOUND), state.IncrementIndent());
                    state.DecrementIndent();
                    state.DecrementIndent();
                    break;

                case SqlElemNames.DDLDETAIL_PARENS:
                case SqlElemNames.FUNCTION_PARENS:
                    //simply process sub-nodes - don't add space or expect any linebreaks (but respect linebreaks if necessary)
                    state.WordSeparatorExpected = false;
                    WhiteSpace_BreakAsExpected(state);
                    state.AddOutputContent(FormatOperator("("));
                    ProcessSqlNodeList(contentElement.Children, state.IncrementIndent());
                    state.DecrementIndent();
                    WhiteSpace_BreakAsExpected(state);
                    state.AddOutputContent(FormatOperator(")"));
                    state.WordSeparatorExpected = true;
                    break;

                case SqlElemNames.DDL_PARENS:
                case SqlElemNames.EXPRESSION_PARENS:
                case SqlElemNames.SELECTIONTARGET_PARENS:
                case SqlElemNames.IN_PARENS:
                    WhiteSpace_SeparateWords(state);
                    if (contentElement.Name.Equals(SqlElemNames.EXPRESSION_PARENS) || contentElement.Name.Equals(SqlElemNames.IN_PARENS))
                        state.IncrementIndent();
                    state.AddOutputContent(FormatOperator("("));
                    TSqlStandardFormattingState innerState = new TSqlStandardFormattingState(state);
                    ProcessSqlNodeList(contentElement.Children, innerState);
                    //if there was a linebreak in the parens content, or if it wanted one to follow, then put linebreaks before and after.
                    if (innerState.BreakExpected || innerState.OutputContainsLineBreak) {
                        if (!innerState.StartsWithBreak)
                            state.WhiteSpace_BreakToNextLine();
                        state.Assimilate(innerState);
                        state.WhiteSpace_BreakToNextLine();
                    }
                    else {
                        state.Assimilate(innerState);
                    }
                    state.AddOutputContent(FormatOperator(")"));
                    if (contentElement.Name.Equals(SqlElemNames.EXPRESSION_PARENS) || contentElement.Name.Equals(SqlElemNames.IN_PARENS))
                        state.DecrementIndent();
                    state.WordSeparatorExpected = true;
                    break;

                case SqlElemNames.BEGIN_END_BLOCK:
                case SqlElemNames.TRY_BLOCK:
                case SqlElemNames.CATCH_BLOCK:
                    if (contentElement.Parent.Name.Equals(SqlElemNames.SQL_CLAUSE)
                        && contentElement.Parent.Parent.Name.Equals(SqlElemNames.SQL_STATEMENT)
                        && contentElement.Parent.Parent.Parent.Name.Equals(SqlElemNames.CONTAINER_SINGLESTATEMENT)
                        )
                        state.DecrementIndent();
                    ProcessSqlNodeList(contentElement.ChildrenByName(SqlElemNames.CONTAINER_OPEN), state);
                    ProcessSqlNodeList(contentElement.ChildrenByName(SqlElemNames.CONTAINER_MULTISTATEMENT), state);
                    state.DecrementIndent();
                    state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.ChildrenByName(SqlElemNames.CONTAINER_CLOSE), state);
                    state.IncrementIndent();
                    if (contentElement.Parent.Name.Equals(SqlElemNames.SQL_CLAUSE)
                        && contentElement.Parent.Parent.Name.Equals(SqlElemNames.SQL_STATEMENT)
                        && contentElement.Parent.Parent.Parent.Name.Equals(SqlElemNames.CONTAINER_SINGLESTATEMENT)
                        )
                        state.IncrementIndent();
                    break;

                case SqlElemNames.CASE_STATEMENT:
                    ProcessSqlNodeList(contentElement.ChildrenByName(SqlElemNames.CONTAINER_OPEN), state);
                    state.IncrementIndent();
                    ProcessSqlNodeList(contentElement.ChildrenByName(SqlElemNames.CASE_INPUT), state);
                    ProcessSqlNodeList(contentElement.ChildrenByName(SqlElemNames.CASE_WHEN), state);
                    ProcessSqlNodeList(contentElement.ChildrenByName(SqlElemNames.CASE_ELSE), state);
                    if (Options.ExpandCaseStatements)
                        state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.ChildrenByName(SqlElemNames.CONTAINER_CLOSE), state);
                    state.DecrementIndent();
                    break;

                case SqlElemNames.CASE_WHEN:
                case SqlElemNames.CASE_THEN:
                case SqlElemNames.CASE_ELSE:
                    if (Options.ExpandCaseStatements)
                        state.BreakExpected = true;
                    ProcessSqlNodeList(contentElement.ChildrenByName(SqlElemNames.CONTAINER_OPEN), state);
                    ProcessSqlNodeList(contentElement.ChildrenByName(SqlElemNames.CONTAINER_GENERALCONTENT), state.IncrementIndent());
                    ProcessSqlNodeList(contentElement.ChildrenByName(SqlElemNames.CASE_THEN), state);
                    state.DecrementIndent();
                    break;

                case SqlElemNames.AND_OPERATOR:
                case SqlElemNames.OR_OPERATOR:
                    if (Options.ExpandBooleanExpressions)
                        state.BreakExpected = true;
                    ProcessSqlNode(contentElement.ChildByName(SqlElemNames.OTHERKEYWORD), state);
                    break;

                case SqlElemNames.COMMENT_MULTILINE:
                    if (state.SpecialRegionActive == SpecialRegionType.NoFormat && contentElement.TextValue.ToUpperInvariant().Contains("[/NOFORMAT]")) {
                        Node skippedXml = NodeExtensions.ExtractStructureBetween(state.RegionStartNode, contentElement);
                        if (skippedXml != null) {
                            TSqlIdentityFormatter tempFormatter = new TSqlIdentityFormatter();
                            state.AddOutputContent(tempFormatter.FormatSQLTree(skippedXml));
                            state.WordSeparatorExpected = false;
                            state.BreakExpected = false;
                        }
                        state.SpecialRegionActive = null;
                        state.RegionStartNode = null;
                    }
                    else if (state.SpecialRegionActive == SpecialRegionType.Minify && contentElement.TextValue.ToUpperInvariant().Contains("[/MINIFY]")) {
                        Node skippedXml = NodeExtensions.ExtractStructureBetween(state.RegionStartNode, contentElement);
                        if (skippedXml != null) {
                            TSqlIdentityFormatter tempFormatter = new TSqlIdentityFormatter();
                            state.AddOutputContent(tempFormatter.FormatSQLTree(skippedXml));
                            state.WordSeparatorExpected = false;
                            state.BreakExpected = false;
                        }
                        state.SpecialRegionActive = null;
                        state.RegionStartNode = null;
                    }

                    WhiteSpace_SeparateComment(contentElement, state);
                    state.AddOutputContent("/*" + contentElement.TextValue + "*/");
                    if (contentElement.Parent.Name.Equals(SqlElemNames.SQL_STATEMENT)
                        || (contentElement.NextSibling() != null
                            && contentElement.NextSibling().Name.Equals(SqlElemNames.WHITESPACE)
                            && Regex.IsMatch(contentElement.NextSibling().TextValue, @"(\r|\n)+")
                            )
                        )
                        //if this block comment is at the start or end of a statement, or if it was followed by a 
                        // linebreak before any following content, then break here.
                        state.BreakExpected = true;
                    else {
                        state.WordSeparatorExpected = true;
                    }

                    if (state.SpecialRegionActive == null && contentElement.TextValue.ToUpperInvariant().Contains("[NOFORMAT]")) {
                        //state.AddOutputLineBreak();
                        state.SpecialRegionActive = SpecialRegionType.NoFormat;
                        state.RegionStartNode = contentElement;
                    }
                    else if (state.SpecialRegionActive == null && contentElement.TextValue.ToUpperInvariant().Contains("[MINIFY]")) {
                        //state.AddOutputLineBreak();
                        state.SpecialRegionActive = SpecialRegionType.Minify;
                        state.RegionStartNode = contentElement;
                    }
                    break;

                case SqlElemNames.COMMENT_SINGLELINE:
                case SqlElemNames.COMMENT_SINGLELINE_CSTYLE:
                    if (state.SpecialRegionActive == SpecialRegionType.NoFormat && contentElement.TextValue.ToUpperInvariant().Contains("[/NOFORMAT]")) {
                        Node skippedXml = NodeExtensions.ExtractStructureBetween(state.RegionStartNode, contentElement);
                        if (skippedXml != null) {
                            TSqlIdentityFormatter tempFormatter = new TSqlIdentityFormatter();
                            state.AddOutputContent(tempFormatter.FormatSQLTree(skippedXml));
                            state.WordSeparatorExpected = false;
                            state.BreakExpected = false;
                        }
                        state.SpecialRegionActive = null;
                        state.RegionStartNode = null;
                    }
                    else if (state.SpecialRegionActive == SpecialRegionType.Minify && contentElement.TextValue.ToUpperInvariant().Contains("[/MINIFY]")) {
                        Node skippedXml = NodeExtensions.ExtractStructureBetween(state.RegionStartNode, contentElement);
                        if (skippedXml != null) {
                            TSqlIdentityFormatter tempFormatter = new TSqlIdentityFormatter();
                            state.AddOutputContent(tempFormatter.FormatSQLTree(skippedXml));
                            state.WordSeparatorExpected = false;
                            state.BreakExpected = false;
                        }
                        state.SpecialRegionActive = null;
                        state.RegionStartNode = null;
                    }

                    WhiteSpace_SeparateComment(contentElement, state);
                    state.AddOutputContent((contentElement.Name == SqlElemNames.COMMENT_SINGLELINE ? "--" : "//") + contentElement.TextValue.Replace("\r", "").Replace("\n", ""));
                    state.BreakExpected = true;
                    state.SourceBreakPending = true;

                    if (state.SpecialRegionActive == null && contentElement.TextValue.ToUpperInvariant().Contains("[NOFORMAT]")) {
                        state.AddOutputLineBreak();
                        state.SpecialRegionActive = SpecialRegionType.NoFormat;
                        state.RegionStartNode = contentElement;
                    }
                    else if (state.SpecialRegionActive == null && contentElement.TextValue.ToUpperInvariant().Contains("[MINIFY]")) {
                        state.AddOutputLineBreak();
                        state.SpecialRegionActive = SpecialRegionType.Minify;
                        state.RegionStartNode = contentElement;
                    }
                    break;

                case SqlElemNames.STRING:
                case SqlElemNames.NSTRING:
                    WhiteSpace_SeparateWords(state);
                    string outValue = null;
                    if (contentElement.Name.Equals(SqlElemNames.NSTRING))
                        outValue = "N'" + contentElement.TextValue.Replace("'", "''") + "'";
                    else
                        outValue = "'" + contentElement.TextValue.Replace("'", "''") + "'";
                    state.AddOutputContent(outValue);
                    state.WordSeparatorExpected = true;
                    break;

                case SqlElemNames.BRACKET_QUOTED_NAME:
                    WhiteSpace_SeparateWords(state);
                    state.AddOutputContent("[" + contentElement.TextValue.Replace("]", "]]") + "]");
                    state.WordSeparatorExpected = true;
                    break;

                case SqlElemNames.QUOTED_STRING:
                    WhiteSpace_SeparateWords(state);
                    state.AddOutputContent("\"" + contentElement.TextValue.Replace("\"", "\"\"") + "\"");
                    state.WordSeparatorExpected = true;
                    break;

                case SqlElemNames.COMMA:
                    //comma always ignores requested word spacing
                    if (Options.TrailingCommas) {
                        WhiteSpace_BreakAsExpected(state);
                        state.AddOutputContent(FormatOperator(","));

                        if ((Options.ExpandCommaLists
                                && !(contentElement.Parent.Name.Equals(SqlElemNames.DDLDETAIL_PARENS)
                                    || contentElement.Parent.Name.Equals(SqlElemNames.FUNCTION_PARENS)
                                    || contentElement.Parent.Name.Equals(SqlElemNames.IN_PARENS)))
                            ||
                            (Options.ExpandInLists
                                && contentElement.Parent.Name.Equals(SqlElemNames.IN_PARENS))
                        ) {
                            state.BreakExpected = true;
                        }
                        else { state.WordSeparatorExpected = true; }
                    }
                    else {
                        if ((Options.ExpandCommaLists
                                && !(contentElement.Parent.Name.Equals(SqlElemNames.DDLDETAIL_PARENS)
                                    || contentElement.Parent.Name.Equals(SqlElemNames.FUNCTION_PARENS)
                                    || contentElement.Parent.Name.Equals(SqlElemNames.IN_PARENS)))
                            ||
                            (Options.ExpandInLists
                                && contentElement.Parent.Name.Equals(SqlElemNames.IN_PARENS))
                        ) {
                            state.WhiteSpace_BreakToNextLine();
                            state.AddOutputContent(FormatOperator(","));
                            if (Options.SpaceAfterExpandedComma)
                                state.WordSeparatorExpected = true;
                        }
                        else {
                            WhiteSpace_BreakAsExpected(state);
                            state.AddOutputContent(FormatOperator(","));
                            state.WordSeparatorExpected = true;
                        }

                    }
                    break;

                case SqlElemNames.PERIOD:
                case SqlElemNames.SEMICOLON:
                case SqlElemNames.SCOPERESOLUTIONOPERATOR:
                    //always ignores requested word spacing, and doesn't request a following space either.
                    state.WordSeparatorExpected = false;
                    WhiteSpace_BreakAsExpected(state);
                    state.AddOutputContent(FormatOperator(contentElement.TextValue));
                    break;

                case SqlElemNames.ASTERISK:
                case SqlElemNames.EQUALSSIGN:
                case SqlElemNames.ALPHAOPERATOR:
                case SqlElemNames.OTHEROPERATOR:
                    WhiteSpace_SeparateWords(state);
                    state.AddOutputContent(FormatOperator(contentElement.TextValue));
                    state.WordSeparatorExpected = true;
                    break;

                // this is like INNER JOIN etc
                case SqlElemNames.COMPOUNDKEYWORD:
                    WhiteSpace_SeparateWords(state);
                    state.SetRecentKeyword(contentElement.GetAttributeValue(SqlElemNames.ANAME_SIMPLETEXT));
                    // words I want to indent a little
                    if (contentElement.GetAttributeValue(SqlElemNames.ANAME_SIMPLETEXT).Contains("JOIN"))
                        state.Indent(state.IndentLevel - 1);

                    state.AddOutputContent(FormatKeyword(contentElement.GetAttributeValue(SqlElemNames.ANAME_SIMPLETEXT)));
                    state.WordSeparatorExpected = true;
                    ProcessSqlNodeList(contentElement.ChildrenByNames(SqlElemNames.ENAMELIST_COMMENT), state.IncrementIndent());
                    state.DecrementIndent();
                    state.WordSeparatorExpected = true;
                    break;

                case SqlElemNames.OTHERKEYWORD:
                case SqlElemNames.DATATYPE_KEYWORD:
                    WhiteSpace_SeparateWords(state);
                    state.SetRecentKeyword(contentElement.TextValue);
                    state.AddOutputContent(FormatKeyword(contentElement.TextValue));
                    AddLineBreakIfAfterSpecifiedKeyword(contentElement, state);
                    break;

                case SqlElemNames.PSEUDONAME:
                    WhiteSpace_SeparateWords(state);
                    state.AddOutputContent(FormatKeyword(contentElement.TextValue));
                    state.WordSeparatorExpected = true;
                    break;

                case SqlElemNames.FUNCTION_KEYWORD:
                    WhiteSpace_SeparateWords(state);
                    state.SetRecentKeyword(contentElement.TextValue);
                    state.AddOutputContent(contentElement.TextValue);
                    state.WordSeparatorExpected = true;
                    break;

                // this is like unbracketed names of things
                case SqlElemNames.OTHERNODE:
                    WhiteSpace_SeparateWords(state);

                    // if the first character is a letter, we assume it is NOT a variable
                    if (Options.WrapNamesInBraces && Char.IsLetter(contentElement.TextValue, 0))
                        state.AddOutputContent("[" + contentElement.TextValue.Replace("]", "]]") + "]");
                    else
                        state.AddOutputContent(contentElement.TextValue);

                    state.WordSeparatorExpected = true;
                    break;

                case SqlElemNames.MONETARY_VALUE:
                case SqlElemNames.LABEL:
                    WhiteSpace_SeparateWords(state);
                    state.AddOutputContent(contentElement.TextValue);
                    state.WordSeparatorExpected = true;
                    break;

                case SqlElemNames.NUMBER_VALUE:
                    WhiteSpace_SeparateWords(state);
                    state.AddOutputContent(contentElement.TextValue.ToLowerInvariant());
                    state.WordSeparatorExpected = true;
                    break;

                case SqlElemNames.BINARY_VALUE:
                    WhiteSpace_SeparateWords(state);
                    state.AddOutputContent("0x");
                    state.AddOutputContent(contentElement.TextValue.Substring(2).ToUpperInvariant());
                    state.WordSeparatorExpected = true;
                    break;

                case SqlElemNames.WHITESPACE:
                    //take note if it's a line-breaking space, but don't DO anything here
                    if (Regex.IsMatch(contentElement.TextValue, @"(\r|\n)+"))
                        state.SourceBreakPending = true;
                    break;
                default:
                    throw new Exception("Unrecognized element in SQL Xml!");
            }


            //if (initialIndent != state.IndentLevel)
            //    throw new Exception("Messed up the indenting!! Check code/stack or panic!");
        }


        private string FormatKeyword(string keyword) {
            string outputKeyword;
            if (!KeywordMapping.TryGetValue(keyword.ToUpperInvariant(), out outputKeyword))
                outputKeyword = keyword;

            if (Options.UppercaseKeywords)
                return outputKeyword.ToUpperInvariant();
            else
                return outputKeyword.ToLowerInvariant();
        }

        private string FormatOperator(string operatorValue) {
            if (Options.UppercaseKeywords)
                return operatorValue.ToUpperInvariant();
            else
                return operatorValue.ToLowerInvariant();
        }

        private void WhiteSpace_SeparateStatements(Node contentElement, TSqlStandardFormattingState state) {
            if (state.StatementBreakExpected) {
                //check whether this is a DECLARE/SET clause with similar precedent, and therefore exempt from double-linebreak.
                Node thisClauseStarter = FirstSemanticElementChild(contentElement);
                if (!(thisClauseStarter != null
                  && thisClauseStarter.Name.Equals(SqlElemNames.OTHERKEYWORD)
                  && state.GetRecentKeyword() != null
                  && ((thisClauseStarter.TextValue.ToUpperInvariant().Equals("SET")
                      && state.GetRecentKeyword().Equals("SET")
                      )
                    || (thisClauseStarter.TextValue.ToUpperInvariant().Equals("DECLARE")
                      && state.GetRecentKeyword().Equals("DECLARE")
                      )
                    || (thisClauseStarter.TextValue.ToUpperInvariant().Equals("PRINT")
                      && state.GetRecentKeyword().Equals("PRINT")
                      )
                    )
                  )) {
                    for (int i = Options.NewStatementLineBreaks; i > 0; i--)
                        state.AddOutputLineBreak();
                }
                else {
                    for (int i = Options.NewClauseLineBreaks; i > 0; i--)
                        state.AddOutputLineBreak();
                }

                state.Indent(state.IndentLevel);
                state.BreakExpected = false;
                state.AdditionalBreaksExpected = 0;
                state.SourceBreakPending = false;
                state.StatementBreakExpected = false;
                state.WordSeparatorExpected = false;
            }
        }

        private Node FirstSemanticElementChild(Node contentElement) {
            Node target = null;
            while (contentElement != null) {
                target = contentElement.ChildrenExcludingNames(SqlElemNames.ENAMELIST_NONCONTENT).FirstOrDefault();

                if (target != null && SqlElemNames.ENAMELIST_NONSEMANTICCONTENT.Contains(target.Name))
                    contentElement = target;
                else
                    contentElement = null;
            }

            return target;
        }

        private void AddLineBreakIfAfterSpecifiedKeyword(Node contentElement, TSqlStandardFormattingState state) {
            if (string.Compare(contentElement.TextValue, "select", StringComparison.OrdinalIgnoreCase) == 0
             || string.Compare(contentElement.TextValue, "where", StringComparison.OrdinalIgnoreCase) == 0) {
                state.BreakExpected = true;
            }
            else {
                state.WordSeparatorExpected = true;
            }
        }

        private void WhiteSpace_SeparateWords(TSqlStandardFormattingState state) {
            if (state.BreakExpected || state.AdditionalBreaksExpected > 0) {
                bool wasUnIndent = state.UnIndentInitialBreak;
                if (wasUnIndent) state.DecrementIndent();
                WhiteSpace_BreakAsExpected(state);
                if (wasUnIndent) state.IncrementIndent();
            }
            else if (state.WordSeparatorExpected) {
                state.AddOutputSpace();
            }
            state.UnIndentInitialBreak = false;
            state.SourceBreakPending = false;
            state.WordSeparatorExpected = false;
        }

        private void WhiteSpace_SeparateComment(Node contentElement, TSqlStandardFormattingState state) {
            if (state.CurrentLineHasContent && state.SourceBreakPending) {
                state.BreakExpected = true;
                WhiteSpace_BreakAsExpected(state);
            }
            else if (state.WordSeparatorExpected)
                state.AddOutputSpace();
            state.SourceBreakPending = false;
            state.WordSeparatorExpected = false;
        }

        private void WhiteSpace_BreakAsExpected(TSqlStandardFormattingState state) {
            if (state.BreakExpected)
                state.WhiteSpace_BreakToNextLine();
            while (state.AdditionalBreaksExpected > 0) {
                state.WhiteSpace_BreakToNextLine();
                state.AdditionalBreaksExpected--;
            }
        }

        public enum SpecialRegionType {
            NoFormat = 1,
            Minify = 2
        }
    }
}
