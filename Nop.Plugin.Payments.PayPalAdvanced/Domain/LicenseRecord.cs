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
using Nop.Core;

namespace Nop.Plugin.Payments.PayPalAdvanced.Domain
{    
    /// <summary>
    /// Represents a License Key product record
    /// </summary>
    public class LicenseRecord : BaseEntity
    {
        public virtual string LicenseKey { get; set; }
    }
}

