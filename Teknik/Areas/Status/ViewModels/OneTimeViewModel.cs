﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.ViewModels;

namespace Teknik.Areas.Status.ViewModels
{
    public class OneTimeViewModel : TransactionViewModel
    {
        public string Recipient { get; set; }
    }
}