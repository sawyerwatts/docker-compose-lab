create table subscriber
(
    ck   bigint generated always as identity primary key,
    id   text not null unique,
    name text not null
)
