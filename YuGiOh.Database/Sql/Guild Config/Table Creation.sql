create table guild_config(
	id bigint primary key,
	prefix varchar,
	minimal boolean,
	guesstime integer,
	autodelete boolean,
	inline boolean
);