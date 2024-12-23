﻿using System.Collections.Generic;
using System.Linq;
using Discord;
using YuGiOh.Bot.Extensions;

namespace YuGiOh.Bot.Models.Cards
{
    public abstract class Card
    {

        #region Icon Urls

        protected const string DefaultIconUrl = "http://3.bp.blogspot.com/-12VDHRVnjYk/VHdt3uHWbdI/AAAAAAAACyA/fOgzigv-9XU/s1600/Level.png"; //its a star, rofl
        private const string FooterIconUrl = "http://1.bp.blogspot.com/-a3KasYvDBaY/VCQXuTjmb2I/AAAAAAAACZM/oQ6Hw71kLQQ/s1600/Cursed%2BHexagram.png";

        #endregion Icon Urls

        #region Properties

        public string Name { get; set; }
        public string RealName { get; set; }
        public CardType CardType { get; set; }
        public string Lore { get; set; }

        public List<string> Archetypes { get; set; }
        public List<string> Supports { get; set; }
        public List<string> AntiSupports { get; set; }

        public bool OcgExists { get; set; }
        public bool TcgExists { get; set; }

        public string Img { get; set; }
        public string Url { get; set; }
        public string Passcode { get; set; }

        public CardStatus OcgStatus { get; set; }
        public CardStatus TcgAdvStatus { get; set; }
        public CardStatus TcgTrnStatus { get; set; }

        public abstract bool HasEffect { get; }

        #endregion Properties

        public virtual EmbedBuilder GetEmbedBuilder()
        {

            var author = new EmbedAuthorBuilder()
                .WithIconUrl(GetIconUrl())
                .WithName(Name)
                .WithUrl(Url);

            var footer = new EmbedFooterBuilder()
                .WithIconUrl(FooterIconUrl)
                .WithText("Yu-Gi-Oh!");

            var body = new EmbedBuilder()
                .WithColor(GetColor())
                .WithAuthor(author)
                .WithFooter(footer)
                .WithDescription(GetDescription().Join("\n"));

            if (!string.IsNullOrWhiteSpace(Img))
                body.ImageUrl = Img;

            return AddAdditionalFields(AddLore(body));

        }

        protected abstract string GetIconUrl();

        protected abstract Color GetColor();

        protected virtual List<string> GetDescription()
        {

            var desc = new List<string>();

            if (!string.IsNullOrEmpty(RealName))
                desc.Add($"**Real Name:** {RealName}");

            var format = "**Format:** ";

            if (OcgExists)
                format += $"OCG({OcgStatus})";

            if (TcgExists)
                format += $"{(OcgExists ? "/" : "")}TCG({TcgAdvStatus})/Traditional({TcgTrnStatus})";

            desc.Add(format);

            if (!string.IsNullOrEmpty(Passcode))
                desc.Add($"**Passcode:** {Passcode}");

            desc.Add($"**Card Type:** {CardType}");

            return desc;

        }

        protected virtual EmbedBuilder AddLore(EmbedBuilder body)
            => string.IsNullOrEmpty(Lore) ? body.AddField("Not released yet", "\u200B") : body.AddField("Effect", Lore);

        protected virtual EmbedBuilder AddAdditionalFields(EmbedBuilder body)
        {

            var hasArchetypes = Archetypes?.Any() == true;
            var hasSupports = Supports?.Any() == true;
            var hasAntisupports = AntiSupports?.Any() == true;

            body = hasArchetypes ? body.AddField("Archetypes", Archetypes.Join(", "), true) : body;
            body = hasSupports ? body.AddField("Supports", Supports.Join(", "), true) : body;
            body = hasAntisupports ? body.AddField("Antisupports", AntiSupports.Join(", "), hasArchetypes ^ hasSupports) : body; //xor, when it only has one of either

            return body;

        }

        public string GetArtUrl()
            => $"{Constants.Url.ArtBaseUrl}{Passcode}.{Constants.ArtFileType}";

    }

    public enum CardStatus
    {

        Forbidden,
        Illegal,
        Legal,
        Limited,
        SemiLimited,
        Unreleased,
        Unlimited,
        NA

    }

    public enum CardType
    {

        Unknown,
        Monster,
        Trap,
        Spell

    }
}