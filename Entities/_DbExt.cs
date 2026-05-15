using Database.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace Api.Entities;

public partial class Db
{
#if DEBUG
    partial void CustomizeConfiguration(ref DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.LogTo(LogManager.GetCurrentClassLogger().Trace).EnableSensitiveDataLogging();
#endif
}