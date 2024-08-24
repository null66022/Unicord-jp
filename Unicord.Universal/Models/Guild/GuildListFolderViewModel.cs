﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DSharpPlus.Entities;

namespace Unicord.Universal.Models.Guild
{
    internal class GuildListFolderViewModel : ViewModelBase, IGuildListViewModel
    {
        public DiscordGuildFolder _folder;
        private bool _isExpanded;

        public GuildListFolderViewModel(DiscordGuildFolder folder, IEnumerable<DiscordGuild> guilds)
            : base(null)
        {
            _folder = folder;
            Children = new ObservableCollection<GuildListViewModel>();

            foreach (var guild in guilds)
                Children.Add(new GuildListViewModel(guild, this));
        }

        public string Name => _folder.Name;

        public DiscordColor Color => _folder.Color ?? default;

        public bool Unread => Children.Any(g => g.Unread);

        public int MentionCount
        {
            get
            {
                var v = Children.Sum(r => r.AccessibleChannels.Sum(r => r.ReadState.MentionCount));
                return v == 0 ? -1 : v;
            }
        }

        public bool IsExpanded { get => _isExpanded; set => OnPropertySet(ref _isExpanded, value); }

        public ObservableCollection<GuildListViewModel> Children { get; set; }

        public string Icon1 => Children.ElementAtOrDefault(0)?.IconUrl;
        public string Icon2 => Children.ElementAtOrDefault(1)?.IconUrl;
        public string Icon3 => Children.ElementAtOrDefault(2)?.IconUrl;
        public string Icon4 => Children.ElementAtOrDefault(3)?.IconUrl;

        public bool TryGetModelForGuild(DiscordGuild guild, out GuildListViewModel model)
        {
            foreach (var child in Children)
            {
                if (child.TryGetModelForGuild(guild, out model))
                    return true;
            }

            model = null;
            return false;
        }
    }
}
