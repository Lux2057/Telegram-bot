namespace TelegramBot.App;

#region << Using >>

using CRUD.CQRS;
using Telegram.Bot;
using Telegram.Bot.Types;

#endregion

public abstract class BotMessageCommandBase : CommandBase
{
    #region Properties

    protected ITelegramBotClient Client { get; }

    protected Update Update { get; }

    #endregion

    #region Constructors

    protected BotMessageCommandBase(ITelegramBotClient client, Update update)
    {
        Client = client;
        Update = update;
    }

    #endregion
}