﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.PowerBI.Common.Api.Datasets
{
    public class Row
    {
        public Guid Id { get; set; }

        public static implicit operator Row(PowerBI.Api.V2.Models.Row row)
        {
            if(row == null)
            {
                return null;
            }

            return new Row
            {
                Id = new Guid(row.Id)
            };
        }

        public static implicit operator PowerBI.Api.V2.Models.Row(Row row)
        {
            if (row == null)
            {
                return null;
            }

            return new PowerBI.Api.V2.Models.Row
            {
                Id = row.Id.ToString()
            };
        }
    }
}
