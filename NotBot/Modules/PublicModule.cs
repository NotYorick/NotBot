using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace NotBot.Modules
{
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        public CommandService commandService { get; set; }

        [Command("ping")]
        [Alias("pong", "hello")]
        [Summary("E.g. n!ping")]
        public Task PingAsync() => ReplyAsync("pong");        
    }
}
