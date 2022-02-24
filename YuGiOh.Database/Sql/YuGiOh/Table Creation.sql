create table if not exists antisupports
(
    id serial primary key,
    name varchar unique
);

create table if not exists archetypes
(
    id serial primary key,
    name varchar
        constraint archetypes_name_idx unique
);

create table if not exists supports
(
    id serial primary key,
    name varchar unique
);

create table if not exists cards
(
    id integer not null primary key,
    name varchar,
    realname varchar,
    cardtype varchar,
    property varchar,
    types varchar,
    attribute varchar,
    materials varchar,
    lore text,
    archetypes serial unique,
    supports serial unique,
    antisupports serial unique,
    link integer,
    linkarrows varchar,
    atk varchar,
    def varchar,
    level integer,
    pendulumscale integer,
    rank integer,
    tcgexists boolean,
    ocgexists boolean,
    img varchar,
    url varchar,
    passcode varchar,
    ocgstatus varchar,
    tcgadvstatus varchar,
    tcgtrnstatus varchar,
    cardtrivia varchar
);

create table if not exists card_to_antisupports
(
    cardantisupportsid integer references cards (antisupports),
    antisupportsid integer references antisupports,
    constraint cardantisupportsid_antisupportsid_pair_unique unique (cardantisupportsid, antisupportsid)
);

create table if not exists card_to_archetypes
(
    cardarchetypesid integer
        references cards (archetypes),
    archetypesid integer
        references archetypes,
    constraint cardarchetypesid_archetypesid_pair_unique
        unique (cardarchetypesid, archetypesid)
);

create table if not exists card_to_supports
(
    cardsupportsid integer
        references cards (supports),
    supportsid integer
        references supports,
    constraint cardsupportsid_supportsid_pair_unique unique (cardsupportsid, supportsid)
);

create table if not exists translations
(
    id serial
        constraint translations_pk primary key,
    cardid integer not null
        constraint translations_fk references cards,
    language varchar,
    name varchar,
    lore text,
    constraint cardid_language_pair_unique unique (cardid, language)
);

create table if not exists boosterpacks
(
    id integer not null primary key,
    name varchar,
    dates integer generated always as identity
        constraint boosterpacks_dates_unique unique,
    cards integer generated always as identity
        constraint boosterpacks_cards_unique unique,
    url varchar,
    tcgexists boolean,
    ocgexists boolean
);

create table if not exists boosterpack_cards
(
    id integer generated always as identity
        constraint boosterpack_cards_pk primary key,
    name varchar not null,
    boosterpackcardsid integer not null
        constraint boosterpack_cards_fk references boosterpacks (cards),
    rarities integer generated always as identity
        constraint boosterpack_cards_rarities_unique unique,
    constraint boosterpack_cards_boosterpackcardsid_name_unique_pair unique (boosterpackcardsid, name)
);

create table if not exists boosterpack_dates
(
    id integer generated always as identity
        constraint boosterpack_dates_pk primary key,
    boosterpackdatesid integer not null
        constraint boosterpack_dates_fk references boosterpacks (dates),
    name varchar,
    date varchar,
    constraint boosterpack_dates_boosterpackid_name_unique_pair unique (boosterpackdatesid, name)
);

create table if not exists boosterpack_rarities
(
    id integer generated always as identity
        constraint boosterpack_rarities_pk primary key,
    boosterpackraritiesid integer not null
        constraint boosterpack_rarities_fk references boosterpack_cards (rarities),
    name varchar not null,
    constraint boosterpack_rarities_boosterpackraritiesid_name_unique_pair unique (boosterpackraritiesid, name)
);

create table if not exists card_hashes
(
    id integer not null primary key,
    hash varchar
);

create table if not exists errors
(
    id serial primary key,
    name varchar,
    message varchar,
    stacktrace text,
    url varchar,
    type varchar(12)
);