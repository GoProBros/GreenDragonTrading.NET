using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenDragonTrading.Application.Common.Options
{
    public class FinscApiOptions
    {
        /// <summary>
        /// Configuration options for Finsc API
        /// </summary>
        public const string SectionName = "FinscApi";
        /// <summary>
        /// Base URL for Finsc API
        /// </summary>
        public string FinscBaseUrl { get; set; }
        /// <summary>
        /// HTTP request timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; }

    }
}
