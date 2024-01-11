namespace TelegramBot.App;

#region << Using >>

using CRUD.CQRS;
using JetBrains.Annotations;
using Telegram.Bot;
using Telegram.Bot.Types;

#endregion

public class EchoCommand : BotMessageCommandBase
{
    #region Constructors

    public EchoCommand(ITelegramBotClient client, Update update) : base(client, update) { }

    #endregion

    #region Nested Classes

    [UsedImplicitly]
    class Handler : CommandHandlerBase<EchoCommand>
    {
        #region Constructors

        public Handler(IServiceProvider serviceProvider) : base(serviceProvider) { }

        #endregion

        protected override async Task Execute(EchoCommand command, CancellationToken cancellationToken)
        {
            if (command.Update.Message is not { Text: { } messageText } message)
                return;

            var chatId = message.Chat.Id;

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

            await command.Client.SendTextMessageAsync(chatId: chatId,
                                                      text: $"Echo:{Environment.NewLine}{messageText}",
                                                      cancellationToken: cancellationToken);
        }
    }

    #endregion
}