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
using Nop.Plugin.Payments.PayPalAdvanced.Domain;
using System.Collections.Generic;

namespace Nop.Plugin.Payments.PayPalAdvanced.Services
{
    public partial interface ILicenseService
    {        
        void Insert(LicenseRecord lic);
        void Update(LicenseRecord lic);
        void Delete(LicenseRecord lic);
        IList<LicenseRecord> GetAll();        
        LicenseRecord GetById(int id); 

        string GetLicenseHost(string licKey);
        string GetLicenseType(string licKey);
        bool IsLicensed(string host);
    }
}
