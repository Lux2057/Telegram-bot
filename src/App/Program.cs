#region << Using >>

using CRUD.Core;
using CRUD.CQRS;
using CRUD.DAL.EntityFramework;
using CRUD.Logging.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using TelegramBot.App;

#endregion

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEntityFrameworkDAL<BotDbContext>(dbContextOptions: options =>
                                                                       {
                                                                           options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")!);
                                                                           options.UseLazyLoadingProxies();
                                                                       });

builder.Services.AddCQRS(mediatorAssemblies: new[]
                                             {
                                                     typeof(AddLogCommand).Assembly,
                                                     typeof(ReadEntitiesQuery<,,>).Assembly,
                                                     typeof(Program).Assembly
                                             },
                         validatorAssemblies: new[]
                                              {
                                                      typeof(AddLogCommand).Assembly,
                                                      typeof(ReadEntitiesQuery<,,>).Assembly,
                                                      typeof(Program).Assembly
                                              },
                         automapperAssemblies: new[]
                                               {
                                                       typeof(LogEntity).Assembly,
                                                       typeof(Program).Assembly
                                               });

builder.Services.AddSingleton(new TelegramBotClient(builder.Configuration.GetSection("BotKey").Value!));

var app = builder.Build();

TelegramBotClient botClient;

using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    await using (var context = serviceScope.ServiceProvider.GetService<BotDbContext>()!)
    {
        context.Database.EnsureCreated();
        context.Database.Migrate();
    }

    botClient = serviceScope.ServiceProvider.GetService<TelegramBotClient>()!;
}

ReceiverOptions receiverOptions = new() { AllowedUpdates = Array.Empty<UpdateType>() };

botClient.StartReceiving(updateHandler: async (client, update, _) =>
                                        {
                                            using var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
                                            var dispatcher = serviceScope.ServiceProvider.GetService<IDispatcher>()!;

                                            await dispatcher.PushAsync(new EchoCommand(client, update));
                                        },
                         pollingErrorHandler: async (_, exception, _) =>
                                              {
                                                  using var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

                                                  var dispatcher = serviceScope.ServiceProvider.GetService<IDispatcher>()!;

                                                  await dispatcher.PushAsync(new AddLogCommand
                                                                             {
                                                                                     Exception = exception,
                                                                                     LogLevel = LogLevel.Error,
                                                                                     Message = exception.Message
                                                                             });
                                              },
                         receiverOptions: receiverOptions,
                         cancellationToken: CancellationToken.None);

app.Run();