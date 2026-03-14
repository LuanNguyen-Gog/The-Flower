using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.DTOs.Response
{
    public class ResponseDto
    {
        public bool isSuccess { get; set; } = true;

        public string? Message { get; set; }

        public object? Data { get; set; }
    }
}
