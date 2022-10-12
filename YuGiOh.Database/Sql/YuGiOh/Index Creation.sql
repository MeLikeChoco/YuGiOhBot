create unique index if not exists antisupports_id_name_unique_pair_idx
    on antisupports (id, name);

create index if not exists antisupports_name_fuzzy_idx
    on antisupports using gin (name gin_trgm_ops);

create unique index if not exists archetypes_id_name_pair_unique_idx
    on archetypes (id, name);

create index if not exists archetypes_name_fuzzy_idx
    on archetypes using gin (name gin_trgm_ops);

create index if not exists card_to_antisupports_antisupports_id_idx
    on card_to_antisupports (antisupportsid);

create index if not exists card_to_antisupports_card_antisupports_id_idx
    on card_to_antisupports (cardantisupportsid);

create index if not exists card_to_archetypes_archetypes_id_idx
    on card_to_archetypes (archetypesid);

create index if not exists card_to_archetypes_card_archetypes_id_idx
    on card_to_archetypes (cardarchetypesid);

create index if not exists cards_name_asc_idx
    on cards (name);

create index if not exists cards_name_fuzzy_idx
    on cards using gin (name gin_trgm_ops);

create index if not exists cards_name_idx
    on cards (name);

create index if not exists cards_name_include_id_idx
    on cards (name) include (id);

create index if not exists cards_name_real_name_idx
    on cards using gin (name gin_trgm_ops, realname gin_trgm_ops);

create index if not exists cards_real_name_idx
    on cards using gin (realname gin_trgm_ops);

create index if not exists ocgstatus_cards_idx
    on cards using gin (ocgstatus gin_trgm_ops);

create index if not exists tcgadvstatus_cards_idx
    on cards using gin (tcgadvstatus gin_trgm_ops);

create index if not exists tcgtrnstatus_cards_idx
    on cards using gin (tcgtrnstatus gin_trgm_ops);

create index if not exists test
    on cards using gin (name gin_trgm_ops);

create index if not exists card_to_supports_card_supports_id_idx
    on card_to_supports (cardsupportsid);

create index if not exists card_to_supports_supports_id_idx
    on card_to_supports (supportsid);

create unique index if not exists "supports_id_name_unique pair_idx"
    on supports (id, name);

create index if not exists supports_name_fuzzy_idx
    on supports using gin (name gin_trgm_ops);

create index if not exists translations_cardid_idx
    on translations (cardid);

create index if not exists boosterpack_cards_boosterpackid_idx
    on boosterpack_cards (boosterpackcardsid);

create unique index if not exists boosterpack_cards_rarities_idx
    on boosterpack_cards (rarities);

create index if not exists boosterpack_dates_boosterpackid_idx
    on boosterpack_dates (boosterpackdatesid);

create index if not exists boosterpack_rarities_boosterpackcardid_idx
    on boosterpack_rarities (boosterpackraritiesid);

create index if not exists booster_packs_name_idx
    on boosterpacks using gin (name gin_trgm_ops);

create unique index if not exists boosterpacks_cards_uindex
    on boosterpacks (cards);

create unique index if not exists boosterpacks_dates_uindex
    on boosterpacks (dates);

create index if not exists anime_cards_name_fuzzy_idx
    on anime_cards using gin (name gin_trgm_ops);

create index if not exists anime_cards_name_idx
    on anime_cards (name);

create index if not exists anime_cards_name_include_id_idx
    on anime_cards (name) include (id);