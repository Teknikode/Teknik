﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.ViewModels;

namespace Teknik.Areas.Users.ViewModels
{
    public class ResetPasswordViewModel : ViewModelBase
    {
        public string Username { get; set; }
    }
}