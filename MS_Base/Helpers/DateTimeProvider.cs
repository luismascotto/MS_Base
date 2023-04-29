using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace MS_Base.Helpers;

public interface IDateTimeProvider
{
    DateTime Now { get; }
}

public  class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
}

public class PreValidacaoDateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now.AddMinutes(2);
}

public enum DateTimeProviderType
{
    System,
    PreValidacao
}

public delegate IDateTimeProvider ServiceResolver(ServiceType serviceType);