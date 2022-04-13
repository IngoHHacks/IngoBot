using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using IngoBot.MongoDB;

namespace IngoBot.Command
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("sticker"), RequireUserPermission(ChannelPermission.AttachFiles)]
        public async Task ExecuteStickerCommand(string name, [Remainder] string text)
        {
            if (await CommandIsEnabled("sticker"))
            {
                await Context.Channel.SendMessageAsync($"https://ingoh.net/createsticker/{HttpUtility.UrlEncode(name)}/{HttpUtility.UrlEncode(text)}");
            }
            else
            {
                await Context.Channel.SendMessageAsync("This command is not enabled.");
            }
        }

        [Command("c")]
        public async Task ExecuteConverseCommand([Remainder] string text)
        {
            if (await CommandIsEnabled("c", false))
            {
                var msg = await Context.Channel.SendMessageAsync($"Generating...", messageReference: new MessageReference(messageId: Context.Message.Id), allowedMentions: new AllowedMentions(AllowedMentionTypes.None));

                var output = await RunAIPrompt("gpt2", text, OutputType.SECOND_LINE);

                var builder = new ComponentBuilder()
                    .WithButton("Retry", "c-retry");

                await Context.Channel.ModifyMessageAsync(msg.Id, message => {
                    message.Content = output;
                    message.Components = builder.Build();
                    message.AllowedMentions = new AllowedMentions(AllowedMentionTypes.None);
                });
            }
            else
            {
                await Context.Channel.SendMessageAsync("This command is not enabled.");
            }
        }

        [Command("fs")]
        public async Task ExecuteFinishSentenceCommand([Remainder] string text)
        {
            if (await CommandIsEnabled("fs", false))
            {
                var msg = await Context.Channel.SendMessageAsync($"Generating...", messageReference: new MessageReference(messageId: Context.Message.Id), allowedMentions: new AllowedMentions(AllowedMentionTypes.None));

                var output = await RunAIPrompt("gpt2", text);

                var builder = new ComponentBuilder()
                    .WithButton("Retry", "fs-retry");

                await Context.Channel.ModifyMessageAsync(msg.Id, message => {
                    message.Content = output;
                    message.Components = builder.Build();
                    message.AllowedMentions = new AllowedMentions(AllowedMentionTypes.None);
                });
            }
            else
            {
                await Context.Channel.SendMessageAsync("This command is not enabled.");
            }
        }

        [Command("s")]
        public async Task ExecuteStoryCommand([Remainder] string text)
        {
            if (await CommandIsEnabled("s", false))
            {
                var msg = await Context.Channel.SendMessageAsync($"Generating...", messageReference: new MessageReference(messageId: Context.Message.Id), allowedMentions: new AllowedMentions(AllowedMentionTypes.None));

                var output = await RunAIPrompt("gptneo", text, OutputType.ALL_LINES);

                var builder = new ComponentBuilder()
                    .WithButton("Retry", "s-retry");

                await Context.Channel.ModifyMessageAsync(msg.Id, message => { 
                    message.Content = output;
                    message.Components = builder.Build();
                    message.AllowedMentions = new AllowedMentions(AllowedMentionTypes.None);
                });
            }
            else
            {
                await Context.Channel.SendMessageAsync("This command is not enabled.");
            }
        }

        [Command("furry")]
        public async Task ExecuteFurryCommand([Remainder] string text)
        {
            if (await CommandIsEnabled("furry", false))
            {
                var msg = await Context.Channel.SendMessageAsync($"Generating...", messageReference: new MessageReference(messageId: Context.Message.Id), allowedMentions: new AllowedMentions(AllowedMentionTypes.None));

                var output = await RunAIPrompt("gpt2furry", text, OutputType.FIRST_LINE);

                var builder = new ComponentBuilder()
                    .WithButton("Retry", "furry-retry");

                await Context.Channel.ModifyMessageAsync(msg.Id, message => {
                    message.Content = output;
                    message.Components = builder.Build();
                    message.AllowedMentions = new AllowedMentions(AllowedMentionTypes.None);
                });
            }
            else
            {
                await Context.Channel.SendMessageAsync("This command is not enabled.");
            }
        }

        public async Task<bool> CommandIsEnabled(string command, bool enabledByDefault = true)
        {
            var guildMeta = await GetGuildMeta();
            if (guildMeta != null && guildMeta.Contains("commands"))
            {
                if (guildMeta.GetElement("commands").Value.AsBsonDocument.Contains(command)) return guildMeta.GetElement("commands").Value.AsBsonDocument.GetElement(command).Value.AsBoolean;
                else return enabledByDefault;
            }
            return enabledByDefault;
        }

        public async Task<BsonDocument> GetGuildMeta()
        {
            var channel = Context.Channel as SocketGuildChannel;
            var guild = channel.Guild;

            var guildMetas = Mongo.Database.GetCollection<BsonDocument>("guild");

            var guildMeta = await (await guildMetas.FindAsync(Builders<BsonDocument>.Filter.Eq("_id", (long)guild.Id))).FirstOrDefaultAsync();

            return guildMeta;
        }

        public static async Task<string> RunAIPrompt(string model, string prompt, OutputType outputType = OutputType.FIRST_LINE)
        {
            bool ok = false;
            int attempts = 0;
            string output = null;
            while (!ok)
            {

                var httpClient = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://ingoh.net/capi/{model}/{HttpUtility.UrlEncode(prompt)}");
                var response = await httpClient.SendAsync(request);
                var reader = new StreamReader(response.Content.ReadAsStream());
                var outputText = reader.ReadToEnd();
                if (outputText.EndsWith("<br>")) outputText = outputText[..^4];
                attempts++;
                if (outputType == OutputType.FIRST_LINE)
                {
                    var lines = outputText.Split("\r\n<br>");
                    output = lines[0];
                    if (output.Length > 3 || attempts == 3)
                    {

                        ok = true;
                    }
                }
                else if (outputType == OutputType.SECOND_LINE)
                {
                    var lines = outputText.Split("\r\n<br>");
                    attempts++;
                    if (lines.Length > 1 && lines[1].Length > 1)
                    {
                        output = lines[1];
                        ok = true;
                    }
                    if (attempts == 3)
                    {
                        output = lines[0];
                        ok = true;
                    }
                }
                else if (outputType == OutputType.ALL_LINES)
                {
                    output = outputText.Replace("\r\n<br>","\r\n");
                    ok = true;
                }
            }
            return output;
        }

        public static async Task Retry(string command, SocketMessageComponent component)
        {
            var msg = component.Message;
            var original = component.Message.Reference;
            var channel = msg.Channel;
            var mChannel = (SocketGuildChannel)channel;
            var guild = mChannel.Guild;

            var guildMetas = Mongo.Database.GetCollection<BsonDocument>("guild");

            var guildMeta = await (await guildMetas.FindAsync(Builders<BsonDocument>.Filter.Eq("_id", (long)guild.Id))).FirstOrDefaultAsync();

            var prefix = "!";
            if (guildMeta != null && guildMeta.Contains("prefix"))
            {
                prefix = guildMeta.GetElement("prefix").Value.AsString;
            }

            var commandLen = prefix.Length + command.Length;

            if (await CommandIsEnabled(command, guild, false))
            {

                var orig = await channel.GetMessageAsync(original.MessageId.Value);
                if (orig == null)
                {
                    await channel.ModifyMessageAsync(msg.Id, message => {
                        message.Content = "[ERROR] The original message was deleted!";
                        message.AllowedMentions = new AllowedMentions(AllowedMentionTypes.None);
                    });
                    return;
                }
                var text = orig.Content;
                text = text[(commandLen + 1)..];

                string output = null;
                switch (command) {
                    case "c":
                        output = await RunAIPrompt("gpt2", text, OutputType.SECOND_LINE);
                        break;
                    case "fs":
                        output = await RunAIPrompt("gpt2", text);
                        break;
                    case "s":
                        output = await RunAIPrompt("gptneo", text, OutputType.ALL_LINES);
                        break;
                    case "furry":
                        output = await RunAIPrompt("gpt2furry", text);
                        break;
                }

                var builder = new ComponentBuilder()
                    .WithButton("Retry", command + "-retry");

                await channel.ModifyMessageAsync(msg.Id, message => {
                    message.Content = output;
                    message.Components = builder.Build();
                    message.AllowedMentions = new AllowedMentions(AllowedMentionTypes.None);
                });
            }
            else
            {
                await channel.SendMessageAsync("This command is not enabled.");
            }
        }

        public static async Task<bool> CommandIsEnabled(string command, SocketGuild guild, bool enabledByDefault = true)
        {
            var guildMeta = await GetGuildMeta(guild);
            if (guildMeta != null && guildMeta.Contains("commands"))
            {
                if (guildMeta.GetElement("commands").Value.AsBsonDocument.Contains(command)) return guildMeta.GetElement("commands").Value.AsBsonDocument.GetElement(command).Value.AsBoolean;
                else return enabledByDefault;
            }
            return enabledByDefault;
        }

        public static async Task<BsonDocument> GetGuildMeta(SocketGuild guild)
        {

            var guildMetas = Mongo.Database.GetCollection<BsonDocument>("guild");

            var guildMeta = await (await guildMetas.FindAsync(Builders<BsonDocument>.Filter.Eq("_id", (long)guild.Id))).FirstOrDefaultAsync();

            return guildMeta;
        }

        public enum OutputType
        {
            FIRST_LINE,
            SECOND_LINE,
            ALL_LINES
        }
    }
}
