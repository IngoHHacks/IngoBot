using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using IngoBot.Command;
using IngoBot.MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace IngoBot.Interaction
{
    public class SlashCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("help", "Help for using IngoBot")]
        public async Task Help()
        {
            await RespondAsync("You can find the documentation at https://github.com/IngoHHacks/IngoBot/blob/main/DOCUMENTATION.md", ephemeral: true);
        }

        [SlashCommand("setprefix", "Sets the IngoBot command prefix")]
        [RequireUserPermission(Discord.GuildPermission.ManageGuild)]
        public async Task SetPrefix(string prefix)
        {
            var guild = ((SocketGuildUser)Context.User).Guild;

            var guildMetas = Mongo.Database.GetCollection<BsonDocument>("guild");
            await guildMetas.FindOneAndReplaceAsync(Builders<BsonDocument>.Filter.Eq("_id", (long)guild.Id), new BsonDocument { { "prefix", prefix } }, new FindOneAndReplaceOptions<BsonDocument>() { IsUpsert = true });

            await RespondAsync("Set prefix to `" + prefix + "`");
        }

        [SlashCommand("enablecommand", "Enables a command")]
        [RequireUserPermission(Discord.GuildPermission.ManageGuild)]
        public async Task EnableCommand([Choice("sticker", "sticker"), Choice("c", "c"), Choice("fs", "fs"), Choice("s", "s"), Choice("furry", "furry")] string command)
        {
            var user = (SocketGuildUser)Context.User;
            if (user.GuildPermissions.ManageGuild)
            {
                var guild = user.Guild;

                var guildMetas = Mongo.Database.GetCollection<BsonDocument>("guild");

                BsonDocument cDoc = null;

                var filter = Builders<BsonDocument>.Filter.Eq("_id", (long)guild.Id);
                var doc = await (await guildMetas.FindAsync(filter)).FirstOrDefaultAsync();

                if (doc.Contains("commands"))
                {
                    cDoc = doc.GetValue("commands").AsBsonDocument;
                    cDoc.Set(command, true);
                }
                else
                {
                    cDoc = new BsonDocument { { command, true } };
                }
                await guildMetas.FindOneAndReplaceAsync(filter, new BsonDocument { { "commands", cDoc } }, new FindOneAndReplaceOptions<BsonDocument>() { IsUpsert = true });

                await RespondAsync("Enabled " + "`" + command + "`");
            }
            else
            {
                await RespondAsync("You do not have permission to use this command.");
            }
        }

        [SlashCommand("disablecommand", "Disables a command")]
        [RequireUserPermission(Discord.GuildPermission.ManageGuild)]
        public async Task DisableCommand([Choice("sticker", "sticker"), Choice("c", "c"), Choice("fs", "fs"), Choice("s", "s"), Choice("furry", "furry")] string command)
        {
            var user = (SocketGuildUser)Context.User;
            if (user.GuildPermissions.ManageGuild)
            {
                var guild = user.Guild;

                var guildMetas = Mongo.Database.GetCollection<BsonDocument>("guild");

                BsonDocument cDoc = null;

                var filter = Builders<BsonDocument>.Filter.Eq("_id", (long)guild.Id);
                var doc = await (await guildMetas.FindAsync(filter)).FirstOrDefaultAsync();

                if (doc.Contains("commands"))
                {
                    cDoc = doc.GetValue("commands").AsBsonDocument;
                    cDoc.Set(command, false);
                }
                else
                {
                    cDoc = new BsonDocument { { command, false } };
                }
                await guildMetas.FindOneAndReplaceAsync(filter, new BsonDocument { { "commands", cDoc } }, new FindOneAndReplaceOptions<BsonDocument>() { IsUpsert = true });

                await RespondAsync("Disabled " + "`" + command + "`");
            }
            else
            {
                await RespondAsync("You do not have permission to use this command.");
            }
        }
    }

    public class UserCommands : InteractionModuleBase<SocketInteractionContext>
    {

        [UserCommand("Get Avatar")]
        public async Task GetAvatar(IUser user)
        {
            var avatar = user.GetAvatarUrl(size: 2048) ?? ((SocketUserCommand)Context.Interaction).Data.Member.GetDefaultAvatarUrl();
            await RespondAsync(avatar, ephemeral: true);
        }
    }

    public class MessageCommands : InteractionModuleBase<SocketInteractionContext>
    {
        
        [MessageCommand("Stickerify")]
        [RequireContext(ContextType.Guild)]
        public async Task Stickerify(IMessage message)
        {
            if (Context.Channel is SocketGuildChannel gChannel) {
                if (!await Commands.CommandIsEnabled("sticker", gChannel.Guild))
                {
                    await Context.Channel.SendMessageAsync("This command is not enabled.");
                    return;
                }
            }

            string txt = message.CleanContent;
            if (txt.Length > 100) { txt = txt.Substring(0, 97) + "..."; }
            else if (txt.Length == 0) { txt = "HI!"; }

            var mb = new ModalBuilder()
                .WithTitle("Stickerify")
                .WithCustomId("stickerifymodal")
                .AddTextInput("Sticker Name", "stickername", required: true, placeholder: "IngoDog_hi", value: "IngoDog_hi")
                .AddTextInput("Sticker Text", "stickertext", required: true, placeholder: txt, value: txt, maxLength: 100);
            await Context.Interaction.RespondWithModalAsync(mb.Build());
        }
    }
}
