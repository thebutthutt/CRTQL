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

namespace PoorMansTSqlFormatterLib.Interfaces {
    public static class SqlElemNames {

        public const string SQL_ROOT = "SqlRoot";
        public const string SQL_STATEMENT = "SqlStatement";
        public const string SQL_CLAUSE = "Clause";
        public const string SET_OPERATOR_CLAUSE = "SetOperatorClause";
        public const string INSERT_CLAUSE = "InsertClause";
        public const string BEGIN_END_BLOCK = "BeginEndBlock";
        public const string TRY_BLOCK = "TryBlock";
        public const string CATCH_BLOCK = "CatchBlock";
        public const string BATCH_SEPARATOR = "BatchSeparator";
        public const string CASE_STATEMENT = "CaseStatement";
        public const string CASE_INPUT = "Input";
        public const string CASE_WHEN = "When";
        public const string CASE_THEN = "Then";
        public const string CASE_ELSE = "CaseElse";
        public const string IF_STATEMENT = "IfStatement";
        public const string ELSE_CLAUSE = "ElseClause";
        public const string BOOLEAN_EXPRESSION = "BooleanExpression";
        public const string WHILE_LOOP = "WhileLoop";
        public const string CURSOR_DECLARATION = "CursorDeclaration";
        public const string CURSOR_FOR_BLOCK = "CursorForBlock";
        public const string CURSOR_FOR_OPTIONS = "CursorForOptions";
        public const string CTE_WITH_CLAUSE = "CTEWithClause";
        public const string CTE_ALIAS = "CTEAlias";
        public const string CTE_AS_BLOCK = "CTEAsBlock";
        public const string BEGIN_TRANSACTION = "BeginTransaction";
        public const string COMMIT_TRANSACTION = "CommitTransaction";
        public const string ROLLBACK_TRANSACTION = "RollbackTransaction";
        public const string SAVE_TRANSACTION = "SaveTransaction";
        public const string DDL_DECLARE_BLOCK = "DDLDeclareBlock";
        public const string DDL_PROCEDURAL_BLOCK = "DDLProceduralBlock";
        public const string DDL_OTHER_BLOCK = "DDLOtherBlock";
        public const string DDL_AS_BLOCK = "DDLAsBlock";
        public const string DDL_PARENS = "DDLParens";
        public const string DDL_SUBCLAUSE = "DDLSubClause";
        public const string DDL_RETURNS = "DDLReturns";
        public const string DDLDETAIL_PARENS = "DDLDetailParens";
        public const string DDL_WITH_CLAUSE = "DDLWith";
        public const string PERMISSIONS_BLOCK = "PermissionsBlock";
        public const string PERMISSIONS_DETAIL = "PermissionsDetail";
        public const string PERMISSIONS_TARGET = "PermissionsTarget";
        public const string PERMISSIONS_RECIPIENT = "PermissionsRecipient";
        public const string TRIGGER_CONDITION = "TriggerCondition";
        public const string SELECTIONTARGET_PARENS = "SelectionTargetParens";
        public const string EXPRESSION_PARENS = "ExpressionParens";
        public const string FUNCTION_PARENS = "FunctionParens";
        public const string IN_PARENS = "InParens";
        public const string FUNCTION_KEYWORD = "FunctionKeyword";
        public const string DATATYPE_KEYWORD = "DataTypeKeyword";
        public const string COMPOUNDKEYWORD = "CompoundKeyword";
        public const string OTHERKEYWORD = "OtherKeyword";
        public const string LABEL = "Label";
        public const string CONTAINER_OPEN = "ContainerOpen";
        public const string CONTAINER_MULTISTATEMENT = "ContainerMultiStatementBody";
        public const string CONTAINER_SINGLESTATEMENT = "ContainerSingleStatementBody";
        public const string CONTAINER_GENERALCONTENT = "ContainerContentBody";
        public const string CONTAINER_CLOSE = "ContainerClose";
        public const string SELECTIONTARGET = "SelectionTarget";
        public const string MERGE_CLAUSE = "MergeClause";
        public const string MERGE_TARGET = "MergeTarget";
        public const string MERGE_USING = "MergeUsing";
        public const string MERGE_CONDITION = "MergeCondition";
        public const string MERGE_WHEN = "MergeWhen";
        public const string MERGE_THEN = "MergeThen";
        public const string MERGE_ACTION = "MergeAction";
        public const string JOIN_TARGET = "JoinTarget";
        public const string JOIN_ON_SECTION = "JoinOn";

        public const string PSEUDONAME = "PseudoName";
        public const string WHITESPACE = "WhiteSpace";
        public const string OTHERNODE = "Other";
        public const string COMMENT_SINGLELINE = "SingleLineComment";
        public const string COMMENT_SINGLELINE_CSTYLE = "SingleLineCommentCStyle";
        public const string COMMENT_MULTILINE = "MultiLineComment";
        public const string STRING = "String";
        public const string NSTRING = "NationalString";
        public const string QUOTED_STRING = "QuotedString";
        public const string BRACKET_QUOTED_NAME = "BracketQuotedName";
        public const string COMMA = "Comma";
        public const string PERIOD = "Period";
        public const string SEMICOLON = "Semicolon";
        public const string SCOPERESOLUTIONOPERATOR = "ScopeResolutionOperator";
        public const string ASTERISK = "Asterisk";
        public const string EQUALSSIGN = "EqualsSign";
        public const string ALPHAOPERATOR = "AlphaOperator";
        public const string OTHEROPERATOR = "OtherOperator";

        public const string AND_OPERATOR = "And";
        public const string OR_OPERATOR = "Or";
        public const string BETWEEN_CONDITION = "Between";
        public const string BETWEEN_LOWERBOUND = "LowerBound";
        public const string BETWEEN_UPPERBOUND = "UpperBound";

        public const string NUMBER_VALUE = "NumberValue";
        public const string MONETARY_VALUE = "MonetaryValue";
        public const string BINARY_VALUE = "BinaryValue";

        //attribute names
        public const string ANAME_ERRORFOUND = "errorFound";
        public const string ANAME_HASERROR = "hasError";
        public const string ANAME_DATALOSS = "dataLossLimitation";
        public const string ANAME_SIMPLETEXT = "simpleText";

        public static string[] ENAMELIST_COMMENT = new string[] {
                        COMMENT_MULTILINE,
                        COMMENT_SINGLELINE,
                        COMMENT_SINGLELINE_CSTYLE
            };

        public static string[] ENAMELIST_NONCONTENT = new string[] {
                        WHITESPACE,
                        COMMENT_MULTILINE,
                        COMMENT_SINGLELINE,
                        COMMENT_SINGLELINE_CSTYLE
            };

        public static string[] ENAMELIST_NONSEMANTICCONTENT = new string[] {
                        SQL_CLAUSE,
                        DDL_PROCEDURAL_BLOCK,
                        DDL_OTHER_BLOCK,
                        DDL_DECLARE_BLOCK
            };

    }
}
