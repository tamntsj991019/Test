using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    public class ResultModel
    {
        public string ErrorMessage { get; set; }
        public bool Succeed { get; set; } = false;
        public object Data { get; set; }
    }

    public class PagingModel
    {
        public object Data { get; set; }
        public int Total { get; set; }
    }
}
