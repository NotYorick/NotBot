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

        [Command("help")]
        public async Task HelpAsync()
        {
            List<CommandInfo> commands = commandService.Commands.ToList();
            EmbedBuilder embedBuilder = new EmbedBuilder();
            var user = (IGuildUser)Context.User;

            foreach (CommandInfo command in commands)
            {
                // Only give the commands from available modules
                if ((command.Module.Name != "PublicModule" && !user.GuildPermissions.Administrator) || command.Name == "help") continue;

                var summary = command.Summary ?? "No description found";
                string aliasesString = string.Join(", ", command.Aliases);
                embedBuilder.AddField(aliasesString, summary);
            }
            await Context.User.SendMessageAsync("Commandlist (prefix = n!): ", false, embedBuilder.Build()); // TODO: Prefix var in JSON or something
        }

        [Command("ping")]
        [Alias("pong", "hello")]
        [Summary("E.g. n!ping")]
        public Task PingAsync() => ReplyAsync("pong");

        // Reply with users activity
        [Command("activity")]
        [Alias("song", "game", "stream")]
        [Summary("E.g. n!activity <optional targetuser>")]
        public async Task ActivityAsync(SocketUser user = null)
        {
            if (user == null)
                user = Context.User;

            if (user.Activity != null)
            {
                if (user.Activity is SpotifyGame)
                {
                    var spotify = (SpotifyGame)user.Activity;
                    await Context.Channel.SendMessageAsync("Listening to: " + spotify.TrackTitle + " By " + spotify.Artists.First());
                }
                else if (user.Activity is StreamingGame)
                {
                    var stream = (StreamingGame)user.Activity;
                    await Context.Channel.SendMessageAsync("Live streaming: " + stream.Url);
                }
                else
                {
                    Game game = (Game)user.Activity;
                    await Context.Channel.SendMessageAsync("Playing: " + game.Name);
                }
            }
            else
            {
                var m = (IUserMessage)await Context.Channel.GetMessageAsync(Context.Message.Id);
                await m.AddReactionAsync(new Emoji("🖕"));
                await Context.Channel.SendMessageAsync("Not doing shit");
            }
        }

        // Reply with a meme
        [Command("reddit")]
        [Summary("E.g. n!reddit <subreddit>")]
        public async Task MemeAsync(string subreddit = "memes")
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Async = true;

            //TODO: Optimize the XML reading
            using (XmlReader x = XmlReader.Create("https://www.reddit.com/r/" + subreddit + "/.rss", settings))
            {
                while (await x.ReadAsync())
                {
                    if (x.NodeType == XmlNodeType.Text)
                    {
                        string value = await x.GetValueAsync();

                        Regex regex = new Regex("https://i.redd.it/([a-zA-Z0-9])*.(jpg|png)");
                        Match match = regex.Match(value);
                        if (match.Success)
                        {
                            await Context.Channel.SendMessageAsync(match.Value);
                            break;
                        }
                    }
                }
            }
        }

        [Command("poll")]
        [Alias("createpoll")]
        [Summary("E.g. n!poll <polltitle>, <option1>, <option2>, <option3>...")]
        public async Task CreatePollAsync([Remainder]string message)
        {
            string[] reactions = { "0️⃣", "1️⃣", "2️⃣", "3️⃣", "4️⃣", "5️⃣", "6️⃣", "7️⃣", "8️⃣", "9️⃣", "🔟" };

            // Get an array of the different parameters, if there is only a title, only 1 option or more than 10 options => abort
            var parameters = message.Split(",");
            if (parameters.Length < 3 || parameters.Length > 10)
            {
                await Context.Channel.SendMessageAsync("To create a poll you need a title and 2-10 options");
                return;
            }

            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.WithTitle("Poll: " + parameters[0])
                .WithAuthor(Context.User)
                .WithCurrentTimestamp()
                .WithColor(Color.Magenta);

            IEmote[] emotes = new IEmote[parameters.Length - 1];
            int i;
            for (i = 1; i < parameters.Length; i++)
            {
                embedBuilder.Description += i + ".\t" + parameters[i] + "\n";
                emotes[i - 1] = new Emoji(reactions[i]);
            }

            var pollMessage = await Context.Channel.SendMessageAsync("", false, embedBuilder.Build());
            await pollMessage.AddReactionsAsync(emotes);
        }

        [Command("suggestion")]
        [Alias("createsuggestion", "suggest")]
        [Summary("E.g. n!suggestion <suggestion>")]
        public async Task CreateSuggestionAsync([Remainder]string message)
        {
            //string[] updown = { "👍", "👎" };
            IEmote[] emotes = { new Emoji("👍"), new Emoji("👎") };

            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.WithTitle("Suggestion: " + message)
                .WithDescription("React with 👍 if you like it and 👎 if you don't")
                .WithAuthor(Context.User)
                .WithCurrentTimestamp()
                .WithColor(Color.Teal);

            var pollMessage = await Context.Channel.SendMessageAsync("", false, embedBuilder.Build());
            await pollMessage.AddReactionsAsync(emotes);
        }
    }
}
