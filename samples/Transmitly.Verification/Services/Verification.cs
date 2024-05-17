using System.Collections.Generic;
using System.Threading.Tasks;
using Twilio.Exceptions;
using Transmitly;

namespace VerifyV2Quickstart.Services
{
    public interface IVerification
    {
        Task<VerificationResult> StartVerificationAsync(string phoneNumber, string channel);

        Task<VerificationResult> CheckVerificationAsync(string phoneNumber, string code);
    }

    public class Verification : IVerification
    {

        private readonly IChannelVerificationCommunicationsClient _communicationsClient;

        public Verification(IChannelVerificationCommunicationsClient communicationsClient)
        {
            _communicationsClient = communicationsClient ?? throw new System.ArgumentNullException(nameof(communicationsClient));
        }

        public async Task<VerificationResult> StartVerificationAsync(string phoneNumber, string channel)
        {
            try
            {
                var verificationResource = await _communicationsClient.StartChannelVerificationAsync(phoneNumber, channel == "sms" ? Id.Channel.Sms() : Id.Channel.Voice());
               
                return new VerificationResult(string.Empty);
            }
            catch (TwilioException e)
            {
                return new VerificationResult(new List<string> { e.Message });
            }
        }

        public async Task<VerificationResult> CheckVerificationAsync(string phoneNumber, string code)
        {
            try
            {
                var verificationCheckResource = await _communicationsClient.CheckChannelVerificationAsync(phoneNumber, code);
               
                return verificationCheckResource.IsSuccessful && verificationCheckResource.IsVerified ?
                    new VerificationResult(string.Empty) :
                    new VerificationResult(new List<string> { "Wrong code. Try again." });
            }
            catch (TwilioException e)
            {
                return new VerificationResult(new List<string> { e.Message });
            }
        }
    }
}