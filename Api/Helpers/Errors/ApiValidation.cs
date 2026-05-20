using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Helpers.Errors;

public class ApiValidation : ApiResponse
{
    public ApiValidation() : base(400)
    {
        Errors = Array.Empty<string>();
    }
    public IEnumerable<string> Errors { get; set; }
}

