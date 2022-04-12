using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Bson;
using MongoDB.Driver;

using IngoBot.MongoDB;
using IngoBot.Command;
using Discord.Interactions;
using System.Web;

namespace IngoBot;

public static class Program
{

    public static DiscordSocketClient client = new DiscordSocketClient();

    public static async Task Main(string[] args)
    {
        client.Log += static async e =>
        {
            await Console.Out.WriteLineAsync("[DISCORD] " + e.ToString());
        };

        var commands = new CommandService(new CommandServiceConfig
        {
            DefaultRunMode = Discord.Commands.RunMode.Async
        });
        _ = commands.AddModulesAsync(typeof(Program).Assembly, null);

        client.Ready += async () =>
        {

            var interactions = new InteractionService(client, new InteractionServiceConfig
            {
                DefaultRunMode = Discord.Interactions.RunMode.Async,
                UseCompiledLambda = true
            });
            _ = interactions.AddModulesAsync(typeof(Program).Assembly, null);
            var regcommands = await interactions.RegisterCommandsGloballyAsync();
            regcommands.ToList().ForEach(async command => await Console.Out.WriteLineAsync("[INTERACTIONS] Registered: " + command.Name));
            client.InteractionCreated += async interaction =>
            {
                var context = new SocketInteractionContext(client, interaction);
                await interactions.ExecuteCommandAsync(context, null);
            };
            client.ModalSubmitted += async modal =>
            {
                if (modal.Data.CustomId == "stickerifymodal")
                {

                    List<SocketMessageComponentData> components =
                        modal.Data.Components.ToList();
                    string stickername = components
                        .First(x => x.CustomId == "stickername").Value;
                    string stickertext = components
                        .First(x => x.CustomId == "stickertext").Value;

                    await modal.RespondAsync($"https://ingoh.net/stickers/createsticker.php?image={HttpUtility.UrlEncode(stickername)}&text={HttpUtility.UrlEncode(stickertext)}");
                }
            };
        };

        client.MessageReceived += async msg =>
        {

            if (msg is not SocketUserMessage message || message.Channel is not SocketGuildChannel channel || msg.Author.Id == client.CurrentUser.Id)
                return;

            var guild = channel.Guild;

            var guildMetas = Mongo.Database.GetCollection<BsonDocument>("guild");

            var guildMeta = await (await guildMetas.FindAsync(Builders<BsonDocument>.Filter.Eq("_id", (long)guild.Id))).FirstOrDefaultAsync();

            var prefix = "!";
            if (guildMeta != null && guildMeta.Contains("prefix"))
            {
                prefix = guildMeta.GetElement("prefix").Value.AsString;
            }

            int argPos = 0;
            if (message.HasStringPrefix(prefix, ref argPos))
            {
                var result = await commands.ExecuteAsync(new SocketCommandContext(client, message), argPos, null);

                if (!result.IsSuccess)
                {
                    switch (result.Error)
                    {
                        case CommandError.UnknownCommand:
                            await message.Channel.SendMessageAsync($"{result.Error}: {result.ErrorReason}");
                            break;
                        default:
                            await message.Channel.SendMessageAsync($"{result.Error}: {result.ErrorReason}");
                            break;
                    }
                }
            }
        };

        client.ButtonExecuted += ButtonHandler;

        var meta = Mongo.Database.GetCollection<BsonDocument>("meta");
        var discordMeta = await (await meta.FindAsync(Builders<BsonDocument>.Filter.Eq("_id", "discord"))).FirstOrDefaultAsync();
        
        if (discordMeta != null && discordMeta.Contains("activity"))
        {
            await client.SetGameAsync(discordMeta.GetValue("activity").AsString);
        }

        var token = File.ReadAllText("config/DISCORDTOKEN");
        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();
        await Task.Delay(-1);
    }

    public static async Task ButtonHandler(SocketMessageComponent component)
    {
        await component.Message.ModifyAsync(message =>
        {
            message.Content = $"Generating again...";
            message.AllowedMentions = new AllowedMentions(AllowedMentionTypes.None);
            message.Components = null;
        });
        await component.DeferAsync();
        switch (component.Data.CustomId)
        {
            case "c-retry":
                _ = Task.Factory.StartNew(() => Commands.Retry("c", component));
                break;
            case "fs-retry":
                _ = Task.Factory.StartNew(() => Commands.Retry("fs", component));
                break;
            case "s-retry":
                _ = Task.Factory.StartNew(() => Commands.Retry("s", component));
                break;
            case "furry-retry":
                _ = Task.Factory.StartNew(() => Commands.Retry("furry", component));
                break;
        }
    }
}
