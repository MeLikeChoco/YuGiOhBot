create table cards (
	id integer primary key,
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

create table archetypes (
	id serial primary key,
	name varchar unique
);

create table supports (
	id serial primary key,
	name varchar unique
);

create table antisupports (
	id serial primary key,
	name varchar unique
);

create table card_to_archetypes (
	cardarchetypesid integer references cards (archetypes),
	archetypesid integer references archetypes (id),
	constraint cardarchetypesid_archetypesid_pair_unique unique (cardarchetypesid, archetypesid)
);

create table card_to_supports (
	cardsupportsid integer references cards (supports),
	supportsid integer references supports (id),
	constraint cardsupportsid_supportsid_pair_unique unique (cardsupportsid, supportsid)
);

create table card_to_antisupports (
	cardantisupportsid integer references cards (antisupports),
	antisupportsid integer references antisupports (id),
	constraint cardantisupportsid_antisupportsid_pair_unique unique (cardantisupportsid, antisupportsid)
);

create table boosterpacks (
	id integer primary key,
	name varchar,
	dates varchar,
	cards varchar,
	url varchar,
	tcgexists boolean,
	ocgexists boolean
);

create table errors (
	id serial primary key,
	name varchar,
	message varchar,
	stacktrace text,
	url varchar,
	type varchar(12)
);

create table configs (
	id bigint primary key,
	prefix varchar not null default 'y!',
	minimal boolean default true,
	guess_time integer default 60,
	autodelete boolean default true,
	inline boolean default true
);