using System;
using System.Collections.Generic;

namespace VerifyV2Quickstart.Services
{
    public class VerificationResult
    {
        public VerificationResult(string sid)
        {
            Sid = sid;
            IsValid = true;
        }

        public VerificationResult(List<string> errors)
        {
            Errors = errors;
            IsValid = false;
        }

        public bool IsValid { get; set; }

        public string Sid { get; set; }
        
        public List<string> Errors { get; set; }
    }
}