create table configs
(
    id                bigint primary key,
    prefix            varchar not null default 'y!',
    minimal           boolean          default true,
    guesstime         integer          default 60,
    autodelete        boolean          default true,
    inline            boolean          default true,
    hangmantime       integer          default 300,
    hangmanallowwords boolean          default true
);