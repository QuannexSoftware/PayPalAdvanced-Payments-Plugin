/*
    PayPalAdvanced Payments Plugin
    Copyright(C) 2016  Quan Luu

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, version 3 of the License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program. If not, see http://www.gnu.org/licenses.
*/
using System;

namespace Nop.Plugin.Payments.PayPalAdvanced.Common
{
    public class SecurityHelper
    {
        private const string k = "Xg79YVSc2Blcjf0mtWtb5LeqcxAKCFR6VvWnmamwpp4EuvtiMraB2W14S7zb0kDjTE4FxQ00tEecpYDvnYTcCV5InP";

        public static bool IsBase64(string txt)
        {
            try
            {
                if (string.IsNullOrEmpty(txt) || txt.Trim().Length == 0 || txt.Length % 4 != 0)
                    return false;

                Convert.FromBase64String(txt);
                return true;
            }
            catch {
                return false;
            }
        }

        internal static string GetPrivateKey()
        {
            return k.Substring(k[69] - ' ', 0x10);
        }
    }
}
