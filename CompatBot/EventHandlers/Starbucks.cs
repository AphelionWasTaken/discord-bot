﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompatBot.Utils;
using CompatBot.Utils.Extensions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace CompatBot.EventHandlers
{
    internal static class Starbucks
    {
        private static readonly Dictionary<DiscordEmoji, string> TextMap = new Dictionary<DiscordEmoji, string>
        {
            [DiscordEmoji.FromUnicode("Ⓜ")] = "M",
            [DiscordEmoji.FromUnicode("🅰")] = "A",
            [DiscordEmoji.FromUnicode("🅱")] = "B",
            [DiscordEmoji.FromUnicode("🆎")] = "AB",
            [DiscordEmoji.FromUnicode("🅾")] = "O",

            [DiscordEmoji.FromUnicode("🇦")] = "A",
            [DiscordEmoji.FromUnicode("🇧")] = "B",
            [DiscordEmoji.FromUnicode("🇨")] = "C",
            [DiscordEmoji.FromUnicode("🇩")] = "D",
            [DiscordEmoji.FromUnicode("🇪")] = "E",
            [DiscordEmoji.FromUnicode("🇫")] = "F",
            [DiscordEmoji.FromUnicode("🇬")] = "G",
            [DiscordEmoji.FromUnicode("🇭")] = "H",
            [DiscordEmoji.FromUnicode("🇮")] = "I",
            [DiscordEmoji.FromUnicode("🇯")] = "G",
            [DiscordEmoji.FromUnicode("🇰")] = "K",
            [DiscordEmoji.FromUnicode("🇱")] = "L",
            [DiscordEmoji.FromUnicode("🇲")] = "M",
            [DiscordEmoji.FromUnicode("🇳")] = "N",
            [DiscordEmoji.FromUnicode("🇴")] = "O",
            [DiscordEmoji.FromUnicode("🇵")] = "P",
            [DiscordEmoji.FromUnicode("🇶")] = "Q",
            [DiscordEmoji.FromUnicode("🇷")] = "R",
            [DiscordEmoji.FromUnicode("🇸")] = "S",
            [DiscordEmoji.FromUnicode("🇹")] = "T",
            [DiscordEmoji.FromUnicode("🇺")] = "U",
            [DiscordEmoji.FromUnicode("🇻")] = "V",
            [DiscordEmoji.FromUnicode("🇼")] = "W",
            [DiscordEmoji.FromUnicode("🇽")] = "X",
            [DiscordEmoji.FromUnicode("🇾")] = "Y",
            [DiscordEmoji.FromUnicode("🇿")] = "Z",

            [DiscordEmoji.FromUnicode("0⃣️")] = "0",
            [DiscordEmoji.FromUnicode("1️⃣")] = "1",
            [DiscordEmoji.FromUnicode("2️⃣")] = "2",
            [DiscordEmoji.FromUnicode("3️⃣")] = "3",
            [DiscordEmoji.FromUnicode("4️⃣")] = "4",
            [DiscordEmoji.FromUnicode("5️⃣")] = "5",
            [DiscordEmoji.FromUnicode("6️⃣")] = "6",
            [DiscordEmoji.FromUnicode("7️⃣")] = "7",
            [DiscordEmoji.FromUnicode("8️⃣")] = "8",
            [DiscordEmoji.FromUnicode("9️⃣")] = "9",
            [DiscordEmoji.FromUnicode("🔟")] = "10",

            [DiscordEmoji.FromUnicode("🆑")] = "CL",
            [DiscordEmoji.FromUnicode("🆐")] = "DJ",
            [DiscordEmoji.FromUnicode("🆒")] = "COOL",
            [DiscordEmoji.FromUnicode("🆓")] = "FREE",
            [DiscordEmoji.FromUnicode("🆔")] = "ID",
            [DiscordEmoji.FromUnicode("🆕")] = "NEW",
            [DiscordEmoji.FromUnicode("🆖")] = "NG",
            [DiscordEmoji.FromUnicode("🆗")] = "OK",
            [DiscordEmoji.FromUnicode("🆘")] = "SOS",
            [DiscordEmoji.FromUnicode("🆙")] = "UP",
            [DiscordEmoji.FromUnicode("🆚")] = "VS",
            [DiscordEmoji.FromUnicode("⭕")] = "O",
            [DiscordEmoji.FromUnicode("🔄")] = "O",
            [DiscordEmoji.FromUnicode("✝")] = "T",
            [DiscordEmoji.FromUnicode("❌")] = "X",
            [DiscordEmoji.FromUnicode("✖")] = "X",
            [DiscordEmoji.FromUnicode("❎")] = "X",
            [DiscordEmoji.FromUnicode("🅿")] = "P",
            [DiscordEmoji.FromUnicode("🚾")] = "WC",
            [DiscordEmoji.FromUnicode("ℹ")] = "i",
            [DiscordEmoji.FromUnicode("〰")] = "W",
        };

        public static Task Handler(MessageReactionAddEventArgs args)
        {
            return CheckMessageAsync(args.Client, args.Channel, args.User, args.Message, args.Emoji);
        }

        public static async Task CheckBacklogAsync(DiscordClient client, DiscordGuild guild)
        {
            try
            {
                var after = DateTime.UtcNow - Config.ModerationTimeThreshold;
                var checkTasks = new List<Task>();
                foreach (var channel in guild.Channels.Values.Where(ch => Config.Moderation.Channels.Contains(ch.Id)))
                {
                    var messages = await channel.GetMessagesAsync().ConfigureAwait(false);
                    var messagesToCheck = from msg in messages
                        where msg.CreationTimestamp > after && msg.Reactions.Any(r => r.Emoji == Config.Reactions.Starbucks && r.Count >= Config.Moderation.StarbucksThreshold)
                        select msg;
                    foreach (var message in messagesToCheck)
                    {
                        var reactionUsers = await message.GetReactionsAsync(Config.Reactions.Starbucks).ConfigureAwait(false);
                        if (reactionUsers.Count > 0)
                            checkTasks.Add(CheckMessageAsync(client, channel, reactionUsers[0], message, Config.Reactions.Starbucks));
                    }
                }
                await Task.WhenAll(checkTasks).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Config.Log.Error(e);
            }
        }

        private static async Task CheckMessageAsync(DiscordClient client, DiscordChannel channel, DiscordUser user, DiscordMessage message, DiscordEmoji emoji)
        {
            try
            {
                if (user.IsBotSafeCheck() || channel.IsPrivate)
                    return;

                // in case it's not in cache and doesn't contain any info, including Author
                message = await channel.GetMessageAsync(message.Id).ConfigureAwait(false);
                if (emoji == Config.Reactions.Starbucks)
                    await CheckMediaTalkAsync(client, channel, message, emoji).ConfigureAwait(false);

                await CheckGameFansAsync(client, channel, message).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Config.Log.Error(e);
            }
        }

        private static async Task CheckMediaTalkAsync(DiscordClient client, DiscordChannel channel, DiscordMessage message, DiscordEmoji emoji)
        {
            if (!Config.Moderation.Channels.Contains(channel.Id))
                return;

            // message.Timestamp throws if it's not in the cache AND is in local time zone
            if (DateTime.UtcNow - message.CreationTimestamp > Config.ModerationTimeThreshold)
                return;

            if (message.Reactions.Any(r => r.Emoji == emoji && (r.IsMe || r.Count < Config.Moderation.StarbucksThreshold)))
                return;

            if (message.Author.IsWhitelisted(client, channel.Guild))
                return;

            var users = await message.GetReactionsAsync(emoji).ConfigureAwait(false);
            var members = users
                .Select(u => channel.Guild
                            .GetMemberAsync(u.Id)
                            .ContinueWith(ct => ct.IsCompletedSuccessfully ? ct : Task.FromResult((DiscordMember)null), TaskScheduler.Default))
                .ToList() //force eager task creation
                .Select(t => t.Unwrap().ConfigureAwait(false).GetAwaiter().GetResult())
                .Where(m => m != null)
                .ToList();
            var reporters = new List<DiscordMember>(Config.Moderation.StarbucksThreshold);
            foreach (var member in members)
            {
                if (member.IsCurrent)
                    return;

                if (member.Roles.Any())
                    reporters.Add(member);
            }
            if (reporters.Count < Config.Moderation.StarbucksThreshold)
                return;

            await message.ReactWithAsync(client, emoji).ConfigureAwait(false);
            await client.ReportAsync(Config.Reactions.Starbucks + " Media talk report", message, reporters, null, ReportSeverity.Medium).ConfigureAwait(false);
        }

        private static async Task CheckGameFansAsync(DiscordClient client, DiscordChannel channel, DiscordMessage message)
        {
            var bot = client.GetMember(channel.Guild, client.CurrentUser);
            if (!channel.PermissionsFor(bot).HasPermission(Permissions.AddReactions))
            {
                Config.Log.Debug($"No permissions to react in #{channel.Name}");
                return;
            }

            var mood = client.GetEmoji(":sqvat:", "😒");
            if (message.Reactions.Any(r => r.Emoji == mood && r.IsMe))
                return;

            var reactionMsg = string.Concat(message.Reactions.Select(r => TextMap.TryGetValue(r.Emoji, out var txt) ? txt : " ")).Trim();
            if (string.IsNullOrEmpty(reactionMsg))
                return;

            Config.Log.Debug("Emoji text: " + reactionMsg);
            if (reactionMsg.ToUpperInvariant().Contains("MGS"))
            {
                await message.ReactWithAsync(client, mood).ConfigureAwait(false);
                await message.ReactWithAsync(client, DiscordEmoji.FromUnicode("🇳")).ConfigureAwait(false);
                await message.ReactWithAsync(client, DiscordEmoji.FromUnicode("🇴")).ConfigureAwait(false);
            }
        }
    }
}
