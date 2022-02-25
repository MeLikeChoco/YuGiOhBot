create table configs
(
    id numeric(20) not null primary key,
    prefix varchar default 'y!'::character varying not null,
    minimal boolean default true,
    guesstime integer default 60,
    autodelete boolean default false,
    inline boolean default true,
    hangmantime integer default 300,
    hangmanallowwords boolean default true
);