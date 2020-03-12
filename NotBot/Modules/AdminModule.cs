using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NotBot.Modules
{
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        [Command("test")]
        public async Task TestAsync()
        {
            await Context.Channel.SendMessageAsync("Nice test " + Context.User.Mention);
        }

        [Command("setpollchannel")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetPollChannelAsync()
        {
            try
            {
                // Set channel in database (override if already set?)
                //Context.Channel.Id;
            }
            catch (System.Exception)
            {
                var errorReply = await Context.Channel.SendMessageAsync("Something went wrong and the channel did not get set as poll channel, try again later");
                Thread.Sleep(5000);
                await Context.Channel.DeleteMessageAsync(errorReply.Id);
                await Context.Channel.DeleteMessageAsync(Context.Message.Id);
                throw;
            }
        }

        [Command("setreactionrole")]
        [Summary("E.g. n!setreactionrole <Emote> <@role>")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetReactionRole([Remainder] string message)
        {
            var parameters = message.Split(" ");
            if (parameters.Length < 2)
            {
                await Context.Channel.SendMessageAsync("Please provide an Emote and mention a role");
            }
            //Emote e = new Emote(parameters[0]);

        }

        // Kick a user
        [Command("kick")]
        [RequireContext(ContextType.Guild)]
        // make sure the user invoking the command can ban
        [RequireUserPermission(GuildPermission.KickMembers)]
        // make sure the bot itself has the correct permissions
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task KickUserAsync(IGuildUser user, string reason = null)
        {
            await user.KickAsync(reason);
            await ReplyAsync("ok!");
        }
    }
}
