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
using Nop.Core.Infrastructure;
using System.Data.Entity;

namespace Nop.Plugin.Payments.PayPalAdvanced.Data
{    
    public class EfStartUpTask : IStartupTask
    {        
        public void Execute()
        {
            //It's required to set initializer to null (for SQL Server Compact).
            //otherwise, you'll get something like "The model backing the 'your context name' context has changed since the database was created. Consider using Code First Migrations to update the database"
            Database.SetInitializer<PayPalAdvancedObjectContext>(null);
        }

        public int Order
        {
            //ensure that this task is run first 
            get { return 0; }
        }
    }
}

