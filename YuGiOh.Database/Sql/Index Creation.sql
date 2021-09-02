create index cards_name_idx on cards (lower(name));
create index cards_real_name_idx on cards (lower(realname));

create index cards_name_fuzzy_idx on cards using gin (lower(name) gin_trgm_ops);
create index cards_real_name_fuzzy_idx on cards using gin (lower(realname) gin_trgm_ops);
create index cards_name_real_name_fuzzy_idx on cards using gin (
	lower(name) gin_trgm_ops,
	lower(realname) gin_trgm_ops
);

create index archetypes_name_idx on archetypes (lower(name));
create index supports_name_idx on supports (lower(name));
create index antisupports_name_idx on antisupports (lower(name));

create index archetypes_name_fuzzy_idx on archetypes using gin (lower(name) gin_trgm_ops);
create index supports_name_fuzzy_idx on supports using gin (lower(name) gin_trgm_ops);
create index antisupports_name_fuzzy_idx on antisupports using gin (lower(name) gin_trgm_ops);

create index card_to_archetypes_card_archetypes_id_idx on card_to_archetypes (cardarchetypesid);
create index card_to_supports_card_supports_id_idx on card_to_supports (cardsupportsid);
create index card_to_antisupports_card_antisupports_id_idx on card_to_antisupports (cardantisupportsid);

create index card_to_archetypes_archetypes_id_idx on card_to_archetypes (archetypesid);
create index card_to_supports_supports_id_idx on card_to_supports (supportsid);
create index card_to_antisupports_antisupports_id_idx on card_to_antisupports (antisupportsid);

create index booster_packs_name_fuzzy_idx on boosterpacks using gin (lower(name) gin_trgm_ops);