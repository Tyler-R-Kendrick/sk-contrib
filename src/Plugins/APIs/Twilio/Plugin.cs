using System.ComponentModel;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace SKHelpers.Plugins.Twilio
{
    public class TwilioPlugin
    {
        public TwilioPlugin(string accountSid, string authToken)
            => TwilioClient.Init(accountSid, authToken);

        [KernelFunction]
        [Description("Sends an SMS message.")]
        [return: Description("The SID of the sent message.")]
        public async Task<string> SendSmsAsync(
            [Description("The phone number to send the SMS to.")]
            string toPhoneNumber,
            [Description("The phone number to send the SMS from.")]
            string fromPhoneNumber,
            [Description("The body of the SMS message.")]
            string message)
        {
            var messageResource = await MessageResource.CreateAsync(
                to: new PhoneNumber(toPhoneNumber),
                from: new PhoneNumber(fromPhoneNumber),
                body: message);

            return messageResource.Sid;
        }

        [KernelFunction]
        [Description("Makes a phone call.")]
        [return: Description("The SID of the initiated call.")]
        public async Task<string> MakeCallAsync(
            [Description("The phone number to call.")]
            string toPhoneNumber,
            [Description("The phone number to call from.")]
            string fromPhoneNumber,
            [Description("The URL for the call's TwiML.")]
            Uri url)
        {
            var callResource = await CallResource.CreateAsync(
                to: new PhoneNumber(toPhoneNumber),
                from: new PhoneNumber(fromPhoneNumber),
                url: url);

            return callResource.Sid;
        }

        [KernelFunction]
        [Description("Fetches details of a specific message.")]
        [return: Description("Details of the specified message.")]
        public async Task<string?> FetchMessageAsync(
            [Description("The SID of the message to fetch.")]
            string messageSid)
        {
            var message = await MessageResource.FetchAsync(messageSid);
            return message.ToString();
        }

        [KernelFunction]
        [Description("Fetches details of a specific call.")]
        [return: Description("Details of the specified call.")]
        public async Task<string?> FetchCallAsync(
            [Description("The SID of the call to fetch.")]
            string callSid)
        {
            var call = await CallResource.FetchAsync(callSid);
            return call.ToString();
        }
    }
}
