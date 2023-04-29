using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amqp.Listener;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MS_Base.Helpers;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace MS_Base.Policies;
public class CircuitBreakPolicy
{
    public AsyncCircuitBreakerPolicy SqlExceptionTimeoutPolicy;

    public CircuitBreakPolicy(IConfiguration configuration, ILogger logger)
    {

        SqlExceptionTimeoutPolicy = Policy.Handle<SqlException>(ex => ex.Message.Contains("Timeout"))
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(10),
                (ex, t) =>
                {
                    logger.LogCriticalException(configuration.GetValue<int>("Cliente_ID"), configuration.GetValue<string>("SerialLog"), "Circuit broken! (ABERTO)",
                          $"CircuitBreakPolicy", $"SqlExceptionTimeoutPolicy");
                },
                () =>
                {
                    logger.LogException(configuration.GetValue<int>("Cliente_ID"), configuration.GetValue<string>("SerialLog"), "Circuit Reset! (FECHADO)",
                        $"CircuitBreakPolicy", $"SqlExceptionTimeoutPolicy");
                });
    }


}
