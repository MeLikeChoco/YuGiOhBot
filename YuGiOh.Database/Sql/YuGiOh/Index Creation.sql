create index if not exists cards_name_idx on cards (name);
create index if not exists cards_name_fuzzy_idx on cards using gin (name gin_trgm_ops);
create index if not exists cards_real_name_idx on cards using gin (realname gin_trgm_ops);
create index if not exists cards_name_real_name_idx on cards using gin (
	name gin_trgm_ops,
	realname gin_trgm_ops
);

create index if not exists archetypes_name_fuzzy_idx on archetypes using gin (name gin_trgm_ops);
create index if not exists supports_name_fuzzy_idx on supports using gin (name gin_trgm_ops);
create index if not exists antisupports_name_fuzzy_idx on antisupports using gin (name gin_trgm_ops);

create index if not exists card_to_archetypes_card_archetypes_id_idx on card_to_archetypes (cardarchetypesid);
create index if not exists card_to_supports_card_supports_id_idx on card_to_supports (cardsupportsid);
create index if not exists card_to_antisupports_card_antisupports_id_idx on card_to_antisupports (cardantisupportsid);

create index if not exists card_to_archetypes_archetypes_id_idx on card_to_archetypes (archetypesid);
create index if not exists card_to_supports_supports_id_idx on card_to_supports (supportsid);
create index if not exists card_to_antisupports_antisupports_id_idx on card_to_antisupports (antisupportsid);

create index if not exists booster_packs_name_idx on boosterpacks using gin (name gin_trgm_ops);