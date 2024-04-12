using System;
using CopyTableFunction.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CopyTableFunction
{
    public class Function1
    {
        private readonly ILogger _logger;
        private readonly DataTransferService _transferService;
        public Function1(ILoggerFactory loggerFactory,DataTransferService transferService)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
            _transferService = transferService;
        }

        [Function("Function1")]
        public void Run([TimerTrigger("0 * * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            

            if (myTimer.ScheduleStatus is not null)
            {
                _transferService.TransferData();
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
