/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0 and JS, written in C#. 
Copyright (C) 2011-2017 Tao Klerks

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
using System.Text;
using System.Collections.Generic;
using PoorMansTSqlFormatterLib.ParseStructure;
using PoorMansTSqlFormatterLib.Interfaces;

namespace PoorMansTSqlFormatterLib.Formatters {
    /// <summary>
    /// This formatter is intended to output *exactly the same content as initially parsed*
    /// </summary>
    public class TSqlIdentityFormatter : ISqlTokenFormatter, ISqlTreeFormatter {
        public TSqlIdentityFormatter() {
            ErrorOutputPrefix = MessagingConstants.FormatErrorDefaultMessage + Environment.NewLine;
        }

        public string ErrorOutputPrefix { get; set; }

        public string FormatSQLTree(Node sqlTreeDoc) {
            BaseFormatterState state = new BaseFormatterState();

            if (sqlTreeDoc.Name == SqlElemNames.SQL_ROOT && sqlTreeDoc.GetAttributeValue(SqlElemNames.ANAME_ERRORFOUND) == "1")
                state.AddOutputContent(ErrorOutputPrefix);

            //pass "doc" itself into process: useful/necessary when formatting NOFORMAT sub-regions from standard formatter
            ProcessSqlNodeList(new[] { sqlTreeDoc }, state);
            return state.DumpOutput();
        }

        private static void ProcessSqlNodeList(IEnumerable<Node> rootList, BaseFormatterState state) {
            foreach (Node contentElement in rootList)
                ProcessSqlNode(state, contentElement);
        }

        private static void ProcessSqlNode(BaseFormatterState state, Node contentElement) {

            switch (contentElement.Name) {
                case SqlElemNames.DDLDETAIL_PARENS:
                case SqlElemNames.DDL_PARENS:
                case SqlElemNames.FUNCTION_PARENS:
                case SqlElemNames.IN_PARENS:
                case SqlElemNames.EXPRESSION_PARENS:
                case SqlElemNames.SELECTIONTARGET_PARENS:
                    state.AddOutputContent("(");
                    ProcessSqlNodeList(contentElement.Children, state);
                    state.AddOutputContent(")");
                    break;

                case SqlElemNames.SQL_ROOT:
                case SqlElemNames.SQL_STATEMENT:
                case SqlElemNames.SQL_CLAUSE:
                case SqlElemNames.BOOLEAN_EXPRESSION:
                case SqlElemNames.DDL_PROCEDURAL_BLOCK:
                case SqlElemNames.DDL_OTHER_BLOCK:
                case SqlElemNames.DDL_DECLARE_BLOCK:
                case SqlElemNames.CURSOR_DECLARATION:
                case SqlElemNames.BEGIN_END_BLOCK:
                case SqlElemNames.TRY_BLOCK:
                case SqlElemNames.CATCH_BLOCK:
                case SqlElemNames.CASE_STATEMENT:
                case SqlElemNames.CASE_INPUT:
                case SqlElemNames.CASE_WHEN:
                case SqlElemNames.CASE_THEN:
                case SqlElemNames.CASE_ELSE:
                case SqlElemNames.IF_STATEMENT:
                case SqlElemNames.ELSE_CLAUSE:
                case SqlElemNames.WHILE_LOOP:
                case SqlElemNames.DDL_AS_BLOCK:
                case SqlElemNames.BETWEEN_CONDITION:
                case SqlElemNames.BETWEEN_LOWERBOUND:
                case SqlElemNames.BETWEEN_UPPERBOUND:
                case SqlElemNames.CTE_WITH_CLAUSE:
                case SqlElemNames.CTE_ALIAS:
                case SqlElemNames.CTE_AS_BLOCK:
                case SqlElemNames.CURSOR_FOR_BLOCK:
                case SqlElemNames.CURSOR_FOR_OPTIONS:
                case SqlElemNames.TRIGGER_CONDITION:
                case SqlElemNames.COMPOUNDKEYWORD:
                case SqlElemNames.BEGIN_TRANSACTION:
                case SqlElemNames.ROLLBACK_TRANSACTION:
                case SqlElemNames.SAVE_TRANSACTION:
                case SqlElemNames.COMMIT_TRANSACTION:
                case SqlElemNames.BATCH_SEPARATOR:
                case SqlElemNames.SET_OPERATOR_CLAUSE:
                case SqlElemNames.CONTAINER_OPEN:
                case SqlElemNames.CONTAINER_MULTISTATEMENT:
                case SqlElemNames.CONTAINER_SINGLESTATEMENT:
                case SqlElemNames.CONTAINER_GENERALCONTENT:
                case SqlElemNames.CONTAINER_CLOSE:
                case SqlElemNames.SELECTIONTARGET:
                case SqlElemNames.PERMISSIONS_BLOCK:
                case SqlElemNames.PERMISSIONS_DETAIL:
                case SqlElemNames.PERMISSIONS_TARGET:
                case SqlElemNames.PERMISSIONS_RECIPIENT:
                case SqlElemNames.DDL_WITH_CLAUSE:
                case SqlElemNames.MERGE_CLAUSE:
                case SqlElemNames.MERGE_TARGET:
                case SqlElemNames.MERGE_USING:
                case SqlElemNames.MERGE_CONDITION:
                case SqlElemNames.MERGE_WHEN:
                case SqlElemNames.MERGE_THEN:
                case SqlElemNames.MERGE_ACTION:
                case SqlElemNames.JOIN_TARGET:
                case SqlElemNames.JOIN_ON_SECTION:
                case SqlElemNames.DDL_RETURNS:
                    foreach (Node childNode in contentElement.Children)
                        ProcessSqlNode(state, childNode);
                    break;

                case SqlElemNames.COMMENT_MULTILINE:
                    state.AddOutputContent("/*" + contentElement.TextValue + "*/");
                    break;
                case SqlElemNames.COMMENT_SINGLELINE:
                    state.AddOutputContent("--" + contentElement.TextValue);
                    break;
                case SqlElemNames.COMMENT_SINGLELINE_CSTYLE:
                    state.AddOutputContent("//" + contentElement.TextValue);
                    break;
                case SqlElemNames.STRING:
                    state.AddOutputContent("'" + contentElement.TextValue.Replace("'", "''") + "'");
                    break;
                case SqlElemNames.NSTRING:
                    state.AddOutputContent("N'" + contentElement.TextValue.Replace("'", "''") + "'");
                    break;
                case SqlElemNames.QUOTED_STRING:
                    state.AddOutputContent("\"" + contentElement.TextValue.Replace("\"", "\"\"") + "\"");
                    break;
                case SqlElemNames.BRACKET_QUOTED_NAME:
                    state.AddOutputContent("[" + contentElement.TextValue.Replace("]", "]]") + "]");
                    break;

                case SqlElemNames.COMMA:
                case SqlElemNames.PERIOD:
                case SqlElemNames.SEMICOLON:
                case SqlElemNames.ASTERISK:
                case SqlElemNames.EQUALSSIGN:
                case SqlElemNames.SCOPERESOLUTIONOPERATOR:
                case SqlElemNames.ALPHAOPERATOR:
                case SqlElemNames.OTHEROPERATOR:
                    state.AddOutputContent(contentElement.TextValue);
                    break;

                case SqlElemNames.AND_OPERATOR:
                case SqlElemNames.OR_OPERATOR:
                    state.AddOutputContent(contentElement.ChildByName(SqlElemNames.OTHERKEYWORD).TextValue);
                    break;

                case SqlElemNames.FUNCTION_KEYWORD:
                    state.AddOutputContent(contentElement.TextValue);
                    break;

                case SqlElemNames.OTHERKEYWORD:
                case SqlElemNames.DATATYPE_KEYWORD:
                case SqlElemNames.PSEUDONAME:
                    state.AddOutputContent(contentElement.TextValue);
                    break;

                case SqlElemNames.OTHERNODE:
                case SqlElemNames.WHITESPACE:
                case SqlElemNames.NUMBER_VALUE:
                case SqlElemNames.MONETARY_VALUE:
                case SqlElemNames.BINARY_VALUE:
                case SqlElemNames.LABEL:
                    state.AddOutputContent(contentElement.TextValue);
                    break;
                default:
                    throw new Exception("Unrecognized element in SQL Xml!");
            }

        }


        public string FormatSQLTokens(ITokenList sqlTokenList) {
            StringBuilder outString = new StringBuilder();

            if (sqlTokenList.HasUnfinishedToken)
                outString.Append(ErrorOutputPrefix);

            foreach (var entry in sqlTokenList) {
                switch (entry.Type) {
                    case SqlTokenType.MultiLineComment:
                        outString.Append("/*");
                        outString.Append(entry.Value);
                        outString.Append("*/");
                        break;
                    case SqlTokenType.SingleLineComment:
                        outString.Append("--");
                        outString.Append(entry.Value);
                        break;
                    case SqlTokenType.SingleLineCommentCStyle:
                        outString.Append("//");
                        outString.Append(entry.Value);
                        break;
                    case SqlTokenType.String:
                        outString.Append("'");
                        outString.Append(entry.Value.Replace("'", "''"));
                        outString.Append("'");
                        break;
                    case SqlTokenType.NationalString:
                        outString.Append("N'");
                        outString.Append(entry.Value.Replace("'", "''"));
                        outString.Append("'");
                        break;
                    case SqlTokenType.QuotedString:
                        outString.Append("\"");
                        outString.Append(entry.Value.Replace("\"", "\"\""));
                        outString.Append("\"");
                        break;
                    case SqlTokenType.BracketQuotedName:
                        outString.Append("[");
                        outString.Append(entry.Value.Replace("]", "]]"));
                        outString.Append("]");
                        break;

                    case SqlTokenType.OpenParens:
                    case SqlTokenType.CloseParens:
                    case SqlTokenType.Comma:
                    case SqlTokenType.Period:
                    case SqlTokenType.Semicolon:
                    case SqlTokenType.Colon:
                    case SqlTokenType.Asterisk:
                    case SqlTokenType.EqualsSign:
                    case SqlTokenType.OtherNode:
                    case SqlTokenType.WhiteSpace:
                    case SqlTokenType.OtherOperator:
                    case SqlTokenType.Number:
                    case SqlTokenType.BinaryValue:
                    case SqlTokenType.MonetaryValue:
                    case SqlTokenType.PseudoName:
                        outString.Append(entry.Value);
                        break;
                    default:
                        throw new Exception("Unrecognized Token Type in Token List!");
                }
            }

            return outString.ToString();
        }
    }
}
