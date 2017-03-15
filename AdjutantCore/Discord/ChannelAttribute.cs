using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdjutantCore.Discord
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ChannelAttribute : PreconditionAttribute
    {
        public ulong? GuildChannelId { get; }

        /// <summary>
        /// Require that the bot account has a specified GuildPermission
        /// </summary>
        /// <remarks>This precondition will always fail if the command is being invoked in a private channel.</remarks>
        /// <param name="permission">The GuildPermission that the bot must have. Multiple permissions can be specified by ORing the permissions together.</param>
        //public ChannelAttribute(GuildPermission permission)
        //{
        //    GuildPermission = permission;
        //    ChannelPermission = null;
        //}
        public ChannelAttribute(ulong channelId)
        {
            GuildChannelId = channelId;
        }
        /// <summary>
        /// Require that the bot account has a specified ChannelPermission.
        /// </summary>
        /// <param name="permission">The ChannelPermission that the bot must have. Multiple permissions can be specified by ORing the permissions together.</param>
        /// <example>
        /// <code language="c#">
        ///     [Command("permission")]
        ///     [RequireBotPermission(ChannelPermission.ManageMessages)]
        ///     public async Task Purge()
        ///     {
        ///     }
        /// </code>
        /// </example>
        //public ChannelAttribute(ChannelPermission permission)
        //{
        //    ChannelPermission = permission;
        //    GuildPermission = null;
        //}

        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            var channelUser = context.Channel as IGuildChannel;
            var GuildChannel = await context.Guild.GetChannelAsync(GuildChannelId.Value) as IGuildChannel;


            if (channelUser.Id == GuildChannelId)
                return PreconditionResult.FromSuccess();
            else
                return PreconditionResult.FromError($"This command can only be used in {GuildChannel.Name }.");
            

        }
    }
}
