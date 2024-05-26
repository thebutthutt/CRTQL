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
using System.Collections.Generic;
using System.Text;

namespace PoorMansTSqlFormatterLib {
    internal class BaseFormatterState {
        public BaseFormatterState() { }

        protected StringBuilder _outBuilder = new StringBuilder();

        public virtual void DEBUG_PRINT(string content) {
            _outBuilder.Append(content);
        }

        public virtual void DEBUG_NEWLINE() {
            _outBuilder.Append(Environment.NewLine);
        }

        public virtual void AddOutputContent(string content) {
            _outBuilder.Append(content);
        }

        public virtual void AddOutputLineBreak() {
            _outBuilder.Append(Environment.NewLine);
        }

        public string DumpOutput() {
            return _outBuilder.ToString();
        }

    }
}
