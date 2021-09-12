create index cards_name_idx on cards (name);
create index cards_name_fuzzy_idx on cards using gin (name gin_trgm_ops);
create index cards_real_name_idx on cards using gin (realname gin_trgm_ops);
create index cards_name_real_name_idx on cards using gin (
	name gin_trgm_ops,
	realname gin_trgm_ops
);

create index archetypes_name_idx on archetypes using gin (name gin_trgm_ops);
create index supports_name_idx on supports using gin (name gin_trgm_ops);
create index antisupports_name_idx on antisupports using gin (name gin_trgm_ops);

create index card_to_archetypes_card_archetypes_id_idx on card_to_archetypes (cardarchetypesid);
create index card_to_supports_card_supports_id_idx on card_to_supports (cardsupportsid);
create index card_to_antisupports_card_antisupports_id_idx on card_to_antisupports (cardantisupportsid);

create index card_to_archetypes_archetypes_id_idx on card_to_archetypes (archetypesid);
create index card_to_supports_supports_id_idx on card_to_supports (supportsid);
create index card_to_antisupports_antisupports_id_idx on card_to_antisupports (antisupportsid);

create index booster_packs_name_idx on boosterpacks using gin (name gin_trgm_ops);