﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core.Exceptions
{
    public class ConfigurationMissingException : CustomException
    {
        public ConfigurationMissingException(string sectionName) : base($"{sectionName} Missing in Configurations", HttpStatusCode.NotFound)
        {
        }
        public ConfigurationMissingException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, statusCode)
        {
        }
    }
}
